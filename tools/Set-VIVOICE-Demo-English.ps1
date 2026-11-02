# Purpose: Replaces the demo's Thai translations with English content from the shared POI catalog.
# Responsibility: Backs up affected translations and changes only language content in VivoiceDemo in one transaction.
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$taskRepositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$taskCatalog = [IO.File]::ReadAllText((Join-Path $taskRepositoryRoot 'src\Vivoice.Web\Data\BaselineContent.json')) | ConvertFrom-Json
if (@($taskCatalog).Count -eq 0) { throw 'The English POI catalog must not be empty.' }
foreach ($taskPoi in $taskCatalog) {
    if (-not $taskPoi.translations.PSObject.Properties['en'] -or [string]::IsNullOrWhiteSpace($taskPoi.translations.en.name) -or [string]::IsNullOrWhiteSpace($taskPoi.translations.en.summary)) { throw 'An English POI translation is missing.' }
    foreach ($taskTopic in $taskPoi.topics) {
        if (-not $taskTopic.translations.PSObject.Properties['en'] -or [string]::IsNullOrWhiteSpace($taskTopic.translations.en.title) -or [string]::IsNullOrWhiteSpace($taskTopic.translations.en.content)) { throw 'An English topic translation is missing.' }
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

function Invoke-EnglishSql {
    param([string]$Statement, [hashtable]$Values = @{}, [switch]$Scalar)
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.Transaction = $taskTransaction
    $taskCommand.CommandTimeout = 60
    $taskCommand.CommandText = $Statement
    try {
        foreach ($taskKey in $Values.Keys) { $taskCommand.Parameters.AddWithValue('@' + $taskKey, $Values[$taskKey]) | Out-Null }
        if ($Scalar) { return $taskCommand.ExecuteScalar() }
        $taskCommand.ExecuteNonQuery() | Out-Null
    } finally { $taskCommand.Dispose() }
}

function Read-LanguageSnapshot {
    param([ValidateSet('PoiTranslations','TopicTranslations')][string]$TableName)
    $taskCommand = $taskConnection.CreateCommand()
    $taskCommand.Transaction = $taskTransaction
    $taskCommand.CommandText = "SELECT * FROM dbo.[$TableName] WITH (UPDLOCK, HOLDLOCK) WHERE LanguageCode IN ('th','en') ORDER BY Id"
    try {
        $taskReader = $taskCommand.ExecuteReader()
        try {
            while ($taskReader.Read()) {
                $taskRow = [ordered]@{}
                for ($taskColumn = 0; $taskColumn -lt $taskReader.FieldCount; $taskColumn++) { $taskRow[$taskReader.GetName($taskColumn)] = $taskReader.GetValue($taskColumn) }
                [pscustomobject]$taskRow
            }
        } finally { $taskReader.Close() }
    } finally { $taskCommand.Dispose() }
}

try {
    $taskConnection.Open()
    $taskTransaction = $taskConnection.BeginTransaction([Data.IsolationLevel]::Serializable)
    if ((Invoke-EnglishSql -Statement 'SELECT DB_NAME()' -Scalar) -ne 'VivoiceDemo') { throw 'Unexpected target database.' }
    $taskSnapshot = [ordered]@{
        schemaVersion = 1
        database = 'VivoiceDemo'
        capturedAtUtc = [DateTime]::UtcNow.ToString('o')
        PoiTranslations = @(Read-LanguageSnapshot -TableName 'PoiTranslations')
        TopicTranslations = @(Read-LanguageSnapshot -TableName 'TopicTranslations')
    }
    $taskBackupDirectory = [IO.Path]::GetFullPath((Join-Path $taskRepositoryRoot '.vivoice-demo\backups'))
    if (-not $taskBackupDirectory.StartsWith($taskRepositoryRoot + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Backup path is outside the repository.' }
    [IO.Directory]::CreateDirectory($taskBackupDirectory) | Out-Null
    $taskBackupPath = Join-Path $taskBackupDirectory ('translations-before-english-' + [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ') + '.json')
    $taskBackupJson = $taskSnapshot | ConvertTo-Json -Depth 20
    [IO.File]::WriteAllText($taskBackupPath, $taskBackupJson, [Text.UTF8Encoding]::new($false))
    if ([IO.File]::ReadAllText($taskBackupPath) -cne $taskBackupJson) { throw 'Backup write verification failed.' }
    $taskBackupCheck = [IO.File]::ReadAllText($taskBackupPath) | ConvertFrom-Json
    if (@($taskBackupCheck.PoiTranslations).Count -ne $taskSnapshot.PoiTranslations.Count -or @($taskBackupCheck.TopicTranslations).Count -ne $taskSnapshot.TopicTranslations.Count) { throw 'Backup row count verification failed.' }

    foreach ($taskPoi in $taskCatalog) {
        $taskPoiId = Invoke-EnglishSql -Scalar -Statement 'SELECT Id FROM dbo.Pois WHERE Slug = @slug' -Values @{slug=$taskPoi.slug}
        if ($null -eq $taskPoiId) { throw ('POI missing from demo database: ' + $taskPoi.slug) }
        Invoke-EnglishSql -Statement "IF NOT EXISTS (SELECT 1 FROM dbo.PoiTranslations WHERE PoiId=@poiId AND LanguageCode='en') INSERT INTO dbo.PoiTranslations (PoiId,LanguageCode,Name,Summary) VALUES (@poiId,'en',@name,@summary)" -Values @{
            poiId=$taskPoiId; name=$taskPoi.translations.en.name; summary=$taskPoi.translations.en.summary
        }
        foreach ($taskTopic in $taskPoi.topics) {
            $taskTopicId = Invoke-EnglishSql -Scalar -Statement 'SELECT Id FROM dbo.PoiTopics WHERE PoiId=@poiId AND Category=@category' -Values @{poiId=$taskPoiId;category=$taskTopic.category}
            if ($null -eq $taskTopicId) { throw ('Topic missing from demo database: ' + $taskPoi.slug + '/' + $taskTopic.category) }
            Invoke-EnglishSql -Statement "IF NOT EXISTS (SELECT 1 FROM dbo.TopicTranslations WHERE PoiTopicId=@topicId AND LanguageCode='en') INSERT INTO dbo.TopicTranslations (PoiTopicId,LanguageCode,Title,Content,IsPublished) VALUES (@topicId,'en',@title,@content,1)" -Values @{
                topicId=$taskTopicId; title=$taskTopic.translations.en.title; content=$taskTopic.translations.en.content
            }
        }
    }
    $taskUntranslatedPois = Invoke-EnglishSql -Scalar -Statement "SELECT COUNT(*) FROM dbo.PoiTranslations t WHERE t.LanguageCode='th' AND NOT EXISTS (SELECT 1 FROM dbo.PoiTranslations e WHERE e.PoiId=t.PoiId AND e.LanguageCode='en')"
    $taskUntranslatedTopics = Invoke-EnglishSql -Scalar -Statement "SELECT COUNT(*) FROM dbo.TopicTranslations t WHERE t.LanguageCode='th' AND NOT EXISTS (SELECT 1 FROM dbo.TopicTranslations e WHERE e.PoiTopicId=t.PoiTopicId AND e.LanguageCode='en')"
    if ($taskUntranslatedPois -gt 0 -or $taskUntranslatedTopics -gt 0) { throw 'Some custom Thai content has no English replacement; add English translations before retrying.' }
    Invoke-EnglishSql -Statement "DELETE FROM dbo.TopicTranslations WHERE LanguageCode='th'; DELETE FROM dbo.PoiTranslations WHERE LanguageCode='th';"
    $taskEnglishPoiCount = Invoke-EnglishSql -Scalar -Statement "SELECT COUNT(*) FROM dbo.PoiTranslations WHERE LanguageCode='en'"
    $taskEnglishTopicCount = Invoke-EnglishSql -Scalar -Statement "SELECT COUNT(*) FROM dbo.TopicTranslations WHERE LanguageCode='en'"
    $taskTransaction.Commit()
    $taskCommitted = $true
    Write-Output ('English ready: ' + $taskEnglishPoiCount + ' POI translations and ' + $taskEnglishTopicCount + ' topic translations. Thai translations removed; existing English edits preserved.')
    Write-Output ('Backup: ' + $taskBackupPath)
    Write-Output ('Backup SHA256: ' + (Get-FileHash -LiteralPath $taskBackupPath -Algorithm SHA256).Hash)
} catch {
    if ($taskTransaction -and -not $taskCommitted) { try { $taskTransaction.Rollback() } catch {} }
    throw
} finally {
    if ($taskTransaction) { $taskTransaction.Dispose() }
    $taskConnection.Dispose()
}
