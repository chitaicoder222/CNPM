# Purpose: Explicitly replaces only VivoiceDemo POI content with the curated Ho Chi Minh City catalog.
# Responsibility: Backs up all four POI tables, validates the catalog and commits delete/insert as one transaction.
[CmdletBinding()]
param([switch]$ReplaceExisting)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $ReplaceExisting) { throw 'This command replaces all demo POIs. Specify -ReplaceExisting only when that is intended.' }

$taskRepositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$taskCatalogPath = Join-Path $taskRepositoryRoot 'src\Vivoice.Web\Data\BaselineContent.json'
$taskCatalog = [IO.File]::ReadAllText($taskCatalogPath) | ConvertFrom-Json
$taskLanguages = @('vi','ko','ja','zh','en')
if (@($taskCatalog).Count -eq 0) { throw 'The Ho Chi Minh City POI catalog must not be empty.' }
$taskSlugs = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($taskPoi in $taskCatalog) {
    if ([string]::IsNullOrWhiteSpace($taskPoi.slug) -or -not $taskSlugs.Add($taskPoi.slug)) { throw 'POI slugs must be nonempty and unique.' }
    if ($taskPoi.latitude -lt 10.7 -or $taskPoi.latitude -gt 10.9 -or $taskPoi.longitude -lt 106.6 -or $taskPoi.longitude -gt 106.8) { throw 'A POI is outside the expected Ho Chi Minh City area.' }
    if ([double]::IsNaN($taskPoi.radiusMeters) -or [double]::IsInfinity($taskPoi.radiusMeters) -or $taskPoi.radiusMeters -le 0) { throw 'POI radius must be finite and positive.' }
    if (@($taskPoi.topics).Count -ne 3) { throw 'Each POI must have three topics.' }
    $taskCategories = @($taskPoi.topics | ForEach-Object { $_.category })
    if (($taskCategories -join ',') -ne 'culture,food,tourism') { throw 'Topic categories must be culture, food and tourism in order.' }
    foreach ($taskLanguage in $taskLanguages) {
        $taskTranslation = $taskPoi.translations.PSObject.Properties[$taskLanguage]
        if (-not $taskTranslation -or [string]::IsNullOrWhiteSpace($taskTranslation.Value.name) -or [string]::IsNullOrWhiteSpace($taskTranslation.Value.summary)) { throw 'A POI translation is missing.' }
        foreach ($taskTopic in $taskPoi.topics) {
            $taskTopicTranslation = $taskTopic.translations.PSObject.Properties[$taskLanguage]
            if (-not $taskTopicTranslation -or [string]::IsNullOrWhiteSpace($taskTopicTranslation.Value.title) -or [string]::IsNullOrWhiteSpace($taskTopicTranslation.Value.content)) { throw 'A topic translation is missing.' }
        }
    }
}

$taskPasswordLine = [IO.File]::ReadAllLines((Join-Path $taskRepositoryRoot '.env')) | Where-Object { $_ -match '^\s*MSSQL_SA_PASSWORD\s*=' } | Select-Object -Last 1
if (-not $taskPasswordLine) { throw 'MSSQL_SA_PASSWORD is missing from .env.' }
$taskSqlPassword = ($taskPasswordLine -split '=',2)[1].Trim().Trim("'",'"')
if ([string]::IsNullOrWhiteSpace($taskSqlPassword)) { throw 'MSSQL_SA_PASSWORD is empty.' }
$taskBuilder = [System.Data.SqlClient.SqlConnectionStringBuilder]::new()
$taskBuilder['Data Source'] = '127.0.0.1,14333'
$taskBuilder['Initial Catalog'] = 'VivoiceDemo'
$taskBuilder['User ID'] = 'sa'
$taskBuilder['Password'] = $taskSqlPassword
$taskBuilder['TrustServerCertificate'] = $true
$taskBuilder['Connect Timeout'] = 10
$taskConnection = [System.Data.SqlClient.SqlConnection]::new($taskBuilder.get_ConnectionString())
$taskTransaction = $null
$taskCommitted = $false

function Invoke-ContentSql {
    param([string]$Statement, [hashtable]$Values = @{}, [switch]$Scalar)
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.Transaction = $taskTransaction
    $taskCommand.CommandTimeout = 60
    $taskCommand.CommandText = $Statement
    try {
        foreach ($taskKey in $Values.Keys) {
            $taskValue = $Values[$taskKey]
            if ($null -eq $taskValue) { $taskValue = [DBNull]::Value }
            $taskCommand.Parameters.AddWithValue('@' + $taskKey, $taskValue) | Out-Null
        }
        if ($Scalar) { return $taskCommand.ExecuteScalar() }
        $taskCommand.ExecuteNonQuery() | Out-Null
    } finally { $taskCommand.Dispose() }
}

function Read-PoiSnapshot {
    param([ValidateSet('Pois','PoiTranslations','PoiTopics','TopicTranslations')][string]$TableName)
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.Transaction = $taskTransaction
    $taskCommand.CommandTimeout = 60
    $taskCommand.CommandText = 'SELECT * FROM dbo.[' + $TableName + '] WITH (UPDLOCK, HOLDLOCK) ORDER BY Id'
    try {
        $taskReader = $taskCommand.ExecuteReader()
        try {
            while ($taskReader.Read()) {
                $taskRow = [ordered]@{}
                for ($taskColumn = 0; $taskColumn -lt $taskReader.FieldCount; $taskColumn++) {
                    $taskValue = $taskReader.GetValue($taskColumn)
                    if ($taskValue -is [DBNull]) { $taskValue = $null }
                    elseif ($taskValue -is [DateTime]) { $taskValue = [DateTime]::SpecifyKind($taskValue, [DateTimeKind]::Utc).ToString('o') }
                    $taskRow[$taskReader.GetName($taskColumn)] = $taskValue
                }
                [pscustomobject]$taskRow
            }
        } finally { $taskReader.Close() }
    } finally { $taskCommand.Dispose() }
}

try {
    $taskConnection.Open()
    $taskTransaction = $taskConnection.BeginTransaction([Data.IsolationLevel]::Serializable)
    if ((Invoke-ContentSql -Statement 'SELECT DB_NAME()' -Scalar) -ne 'VivoiceDemo') { throw 'Unexpected target database.' }
    $taskAdminCount = [long](Invoke-ContentSql -Statement 'SELECT COUNT_BIG(*) FROM dbo.AdminUsers' -Scalar)
    $taskTables = [ordered]@{}
    foreach ($taskTableName in 'Pois','PoiTranslations','PoiTopics','TopicTranslations') { $taskTables[$taskTableName] = @(Read-PoiSnapshot -TableName $taskTableName) }
    $taskBackup = [ordered]@{
        schemaVersion = 1
        database = 'VivoiceDemo'
        capturedAtUtc = [DateTime]::UtcNow.ToString('o')
        catalogSha256 = (Get-FileHash -LiteralPath $taskCatalogPath -Algorithm SHA256).Hash
        tables = $taskTables
    }
    $taskBackupDirectory = [IO.Path]::GetFullPath((Join-Path $taskRepositoryRoot '.vivoice-demo\backups'))
    if (-not $taskBackupDirectory.StartsWith($taskRepositoryRoot + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Backup path is outside the repository.' }
    [IO.Directory]::CreateDirectory($taskBackupDirectory) | Out-Null
    $taskBackupPath = Join-Path $taskBackupDirectory ('pois-before-hcmc-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ') + '.json')
    $taskBackupJson = $taskBackup | ConvertTo-Json -Depth 30
    [IO.File]::WriteAllText($taskBackupPath, $taskBackupJson, [Text.UTF8Encoding]::new($false))
    if ([IO.File]::ReadAllText($taskBackupPath) -cne $taskBackupJson) { throw 'Backup write verification failed.' }
    $taskBackupCheck = [IO.File]::ReadAllText($taskBackupPath) | ConvertFrom-Json
    foreach ($taskTableName in 'Pois','PoiTranslations','PoiTopics','TopicTranslations') {
        if (@($taskBackupCheck.tables.PSObject.Properties[$taskTableName].Value).Count -ne $taskTables[$taskTableName].Count) { throw 'Backup row count verification failed.' }
    }

    Invoke-ContentSql -Statement 'DELETE FROM dbo.TopicTranslations; DELETE FROM dbo.PoiTopics; DELETE FROM dbo.PoiTranslations; DELETE FROM dbo.Pois;'
    $taskNow = [DateTime]::UtcNow
    foreach ($taskPoi in $taskCatalog) {
        $taskPoiId = [long](Invoke-ContentSql -Scalar -Statement 'INSERT INTO dbo.Pois (Slug,Latitude,Longitude,RadiusMeters,ImageUrl,DisplayOrder,IsActive,CreatedAtUtc,UpdatedAtUtc) OUTPUT INSERTED.Id VALUES (@slug,@latitude,@longitude,@radius,NULL,@displayOrder,1,@now,@now)' -Values @{
            slug=$taskPoi.slug; latitude=[decimal]$taskPoi.latitude; longitude=[decimal]$taskPoi.longitude; radius=[double]$taskPoi.radiusMeters; displayOrder=[int]$taskPoi.displayOrder; now=$taskNow
        })
        foreach ($taskLanguage in $taskLanguages) {
            $taskTranslation = $taskPoi.translations.PSObject.Properties[$taskLanguage].Value
            Invoke-ContentSql -Statement 'INSERT INTO dbo.PoiTranslations (PoiId,LanguageCode,Name,Summary) VALUES (@poiId,@language,@name,@summary)' -Values @{
                poiId=$taskPoiId; language=$taskLanguage; name=$taskTranslation.name; summary=$taskTranslation.summary
            }
        }
        foreach ($taskTopic in $taskPoi.topics) {
            $taskTopicId = [long](Invoke-ContentSql -Scalar -Statement 'INSERT INTO dbo.PoiTopics (PoiId,Category,DisplayOrder,IsActive) OUTPUT INSERTED.Id VALUES (@poiId,@category,@displayOrder,1)' -Values @{
                poiId=$taskPoiId; category=$taskTopic.category; displayOrder=[int]$taskTopic.displayOrder
            })
            foreach ($taskLanguage in $taskLanguages) {
                $taskTranslation = $taskTopic.translations.PSObject.Properties[$taskLanguage].Value
                Invoke-ContentSql -Statement 'INSERT INTO dbo.TopicTranslations (PoiTopicId,LanguageCode,Title,Content,IsPublished) VALUES (@topicId,@language,@title,@content,1)' -Values @{
                    topicId=$taskTopicId; language=$taskLanguage; title=$taskTranslation.title; content=$taskTranslation.content
                }
            }
        }
    }
    $taskExpectedPoiCount = @($taskCatalog).Count
    $taskExpectedTopicCount = @($taskCatalog | ForEach-Object { $_.topics }).Count
    $taskExpectedPoiTranslationCount = $taskExpectedPoiCount * $taskLanguages.Count
    $taskExpectedTopicTranslationCount = $taskExpectedTopicCount * $taskLanguages.Count
    foreach ($taskCountCheck in @(@('Pois',$taskExpectedPoiCount),@('PoiTranslations',$taskExpectedPoiTranslationCount),@('PoiTopics',$taskExpectedTopicCount),@('TopicTranslations',$taskExpectedTopicTranslationCount))) {
        $taskActual = [long](Invoke-ContentSql -Scalar -Statement ('SELECT COUNT_BIG(*) FROM dbo.[' + $taskCountCheck[0] + ']'))
        if ($taskActual -ne [long]$taskCountCheck[1]) { throw ('Unexpected final row count for ' + $taskCountCheck[0]) }
    }
    if ([long](Invoke-ContentSql -Scalar -Statement 'SELECT COUNT_BIG(*) FROM dbo.AdminUsers') -ne $taskAdminCount) { throw 'Admin account count changed unexpectedly.' }
    $taskTransaction.Commit()
    $taskCommitted = $true
    Write-Output ('Replaced ' + $taskTables['Pois'].Count + ' previous POIs with ' + $taskExpectedPoiCount + ' Ho Chi Minh City POIs.')
    Write-Output ('Verified ' + $taskExpectedPoiTranslationCount + ' POI translations, ' + $taskExpectedTopicCount + ' topics and ' + $taskExpectedTopicTranslationCount + ' topic translations. Admin accounts preserved.')
    Write-Output ('Backup: ' + $taskBackupPath)
    Write-Output ('Backup SHA256: ' + (Get-FileHash -LiteralPath $taskBackupPath -Algorithm SHA256).Hash)
} catch {
    if ($taskTransaction -and -not $taskCommitted) { try { $taskTransaction.Rollback() } catch {} }
    throw
} finally {
    if ($taskTransaction) { $taskTransaction.Dispose() }
    $taskConnection.Dispose()
}
