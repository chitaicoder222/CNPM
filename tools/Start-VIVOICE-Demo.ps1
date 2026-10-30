# Purpose: Starts a loopback-only VIVOICE demo and, after readiness succeeds, creates or reuses a launcher-owned Cloudflare Quick Tunnel.
# Responsibility: Uses private Development configuration without exposing it, never manages Docker/database resources, and only stops processes recorded as launcher-owned.
[CmdletBinding()]
param(
    [ValidateRange(1024, 65535)]
    [int]$Port = 5078,
    [switch]$NoTunnel,
    [switch]$Stop,
    [switch]$OpenBrowser
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$projectRelativePath = "src\Vivoice.Web\Vivoice.Web.csproj"
$projectPath = Join-Path $repositoryRoot $projectRelativePath
$stateDirectory = Join-Path $repositoryRoot ".vivoice-demo"
$statePath = Join-Path $stateDirectory "demo-state.json"
$origin = "http://127.0.0.1:$Port"

function Write-LauncherMessage {
    param([string]$Message)
    Write-Host "[VIVOICE] $Message"
}

function Get-DemoState {
    if (-not (Test-Path -LiteralPath $statePath -PathType Leaf)) { return $null }
    try { return Get-Content -LiteralPath $statePath -Raw -Encoding utf8 | ConvertFrom-Json }
    catch { return $null }
}

function Save-DemoState {
    param([object]$State)
    if (-not (Test-Path -LiteralPath $stateDirectory -PathType Container)) {
        New-Item -ItemType Directory -Path $stateDirectory -Force | Out-Null
    }
    $json = $State | ConvertTo-Json -Depth 4
    [System.IO.File]::WriteAllText($statePath, $json, [System.Text.Encoding]::UTF8)
}

function Get-HttpStatus {
    param([string]$Url, [int]$TimeoutSeconds = 5)
    try { return [int](Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec $TimeoutSeconds -ErrorAction Stop).StatusCode }
    catch {
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) { return [int]$_.Exception.Response.StatusCode }
        return $null
    }
}

function Test-LocalReady {
    return (Get-HttpStatus -Url "$origin/ready" -TimeoutSeconds 3) -eq 200
}

function Test-LoopbackPortInUse {
    try { return [bool](Test-NetConnection -ComputerName "127.0.0.1" -Port $Port -InformationLevel Quiet -WarningAction SilentlyContinue) }
    catch { return $false }
}

function Get-ProcessCommandLine {
    param([int]$ProcessId)
    try { return Get-CimInstance Win32_Process -Filter "ProcessId = $ProcessId" -ErrorAction Stop }
    catch { return $null }
}

function Test-OwnedProcess {
    param([int]$ProcessId, [string]$NamePattern, [string]$RequiredCommandText)
    $processInfo = Get-ProcessCommandLine -ProcessId $ProcessId
    return $null -ne $processInfo -and $processInfo.Name -match $NamePattern -and $processInfo.CommandLine -like "*$RequiredCommandText*"
}

function Stop-OwnedProcess {
    param([int]$ProcessId, [string]$NamePattern, [string]$RequiredCommandText, [string]$Label)
    if (-not (Test-OwnedProcess -ProcessId $ProcessId -NamePattern $NamePattern -RequiredCommandText $RequiredCommandText)) {
        Write-LauncherMessage "$Label process is absent or is not launcher-owned; it was not stopped."
        return
    }
    Stop-Process -Id $ProcessId -ErrorAction Stop
    Write-LauncherMessage "Stopped launcher-owned $Label process."
}

function Wait-ForReady {
    param([System.Diagnostics.Process]$WebProcess)
    for ($attempt = 1; $attempt -le 60; $attempt++) {
        if (Test-LocalReady) { return }
        $WebProcess.Refresh()
        if ($WebProcess.HasExited) { throw "The VIVOICE web process exited before /ready succeeded. Inspect the ignored local log files under .vivoice-demo." }
        Start-Sleep -Seconds 1
    }
    throw "VIVOICE did not return HTTP 200 from /ready within 60 seconds. No tunnel was started."
}

function Get-QuickTunnelUrl {
    param([string[]]$LogPaths)
    foreach ($logPath in $LogPaths) {
        if (-not (Test-Path -LiteralPath $logPath -PathType Leaf)) { continue }
        $content = Get-Content -LiteralPath $logPath -Raw -Encoding utf8 -ErrorAction SilentlyContinue
        if ($content -match "https://[a-z0-9-]+\.trycloudflare\.com") { return $Matches[0] }
    }
    return $null
}

function Test-QuickTunnel {
    param([string]$TunnelUrl)
    if ([string]::IsNullOrWhiteSpace($TunnelUrl)) { return $false }
    $home = Get-HttpStatus -Url "$TunnelUrl/" -TimeoutSeconds 10
    $pois = Get-HttpStatus -Url "$TunnelUrl/api/public/pois?lang=vi" -TimeoutSeconds 10
    $anonymousAdmin = Get-HttpStatus -Url "$TunnelUrl/api/admin/dashboard" -TimeoutSeconds 10
    return $home -eq 200 -and $pois -eq 200 -and $anonymousAdmin -eq 404
}

function Open-VerifiedTunnelUrl {
    param([string]$TunnelUrl)
    if (-not $OpenBrowser -or [string]::IsNullOrWhiteSpace($TunnelUrl)) { return }
    try {
        Start-Process -FilePath $TunnelUrl
        Write-LauncherMessage "Opened the verified public demo URL in the default browser."
    }
    catch {
        Write-LauncherMessage "The tunnel is ready, but the browser could not be opened automatically. Copy this URL: $TunnelUrl"
    }
}

$previousPublicOnly = $null
$publicOnlyConfigured = $false
try {
    if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) { throw "VIVOICE project file was not found at the expected repository path." }
    if ($Stop) {
        $state = Get-DemoState
        if ($null -eq $state) { Write-LauncherMessage "No launcher state exists; nothing was stopped."; exit 0 }
        if ($state.TunnelProcessId) { Stop-OwnedProcess -ProcessId ([int]$state.TunnelProcessId) -NamePattern "^cloudflared(\.exe)?$" -RequiredCommandText $state.Origin -Label "Cloudflare tunnel" }
        if ($state.WebProcessId) { Stop-OwnedProcess -ProcessId ([int]$state.WebProcessId) -NamePattern "^dotnet(\.exe)?$" -RequiredCommandText "Vivoice.Web.csproj" -Label "web host" }
        if (Test-Path -LiteralPath $statePath -PathType Leaf) { Remove-Item -LiteralPath $statePath -Force }
        exit 0
    }

    $previousPublicOnly = $env:VivoiceDemo__PublicOnly
    $env:VivoiceDemo__PublicOnly = "true"
    $publicOnlyConfigured = $true
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:DOTNET_ENVIRONMENT = "Development"
    $state = Get-DemoState
    $webProcessId = $null

    if (Test-LocalReady) {
        Write-LauncherMessage "A ready VIVOICE host already responds at $origin."
        if ($state -and $state.WebProcessId -and (Test-OwnedProcess -ProcessId ([int]$state.WebProcessId) -NamePattern "^dotnet(\.exe)?$" -RequiredCommandText "Vivoice.Web.csproj")) {
            $webProcessId = [int]$state.WebProcessId
        }
    }
    elseif (Test-LoopbackPortInUse) {
        throw "Port $Port is already in use but $origin/ready is not HTTP 200. The launcher will not stop an unknown process or start a tunnel."
    }
    else {
        $appLog = Join-Path $stateDirectory "web.stdout.log"
        $appErrorLog = Join-Path $stateDirectory "web.stderr.log"
        if (-not (Test-Path -LiteralPath $stateDirectory -PathType Container)) { New-Item -ItemType Directory -Path $stateDirectory -Force | Out-Null }
        $webProcess = Start-Process -FilePath "dotnet" -WorkingDirectory $repositoryRoot -ArgumentList @("run", "--project", $projectRelativePath, "--no-launch-profile", "--", "--urls", $origin) -PassThru -WindowStyle Hidden -RedirectStandardOutput $appLog -RedirectStandardError $appErrorLog
        $webProcessId = $webProcess.Id
        Write-LauncherMessage "Started a launcher-owned VIVOICE web host; waiting for /ready."
        Wait-ForReady -WebProcess $webProcess
    }

    # Persist the web process before tunnel work so an interrupted/failed tunnel setup cannot orphan a launcher-owned host.
    Save-DemoState ([ordered]@{ Version = 1; Origin = $origin; WebProcessId = $webProcessId; TunnelProcessId = if ($state) { $state.TunnelProcessId } else { $null }; TunnelUrl = if ($state) { $state.TunnelUrl } else { $null }; UpdatedAtUtc = [DateTime]::UtcNow.ToString("o") })

    if ($NoTunnel) {
        Write-LauncherMessage "Local demo is ready at $origin. No tunnel was requested."
        exit 0
    }

    if ($state -and $state.TunnelProcessId -and $state.Origin -eq $origin -and (Test-OwnedProcess -ProcessId ([int]$state.TunnelProcessId) -NamePattern "^cloudflared(\.exe)?$" -RequiredCommandText $origin) -and (Test-QuickTunnel -TunnelUrl $state.TunnelUrl)) {
        Save-DemoState ([ordered]@{ Version = 1; Origin = $origin; WebProcessId = $webProcessId; TunnelProcessId = [int]$state.TunnelProcessId; TunnelUrl = $state.TunnelUrl; UpdatedAtUtc = [DateTime]::UtcNow.ToString("o") })
        Write-LauncherMessage "Reusing verified launcher-owned Quick Tunnel: $($state.TunnelUrl)"
        Open-VerifiedTunnelUrl -TunnelUrl $state.TunnelUrl
        exit 0
    }

    if ($state -and $state.TunnelProcessId -and $state.Origin -eq $origin) {
        Stop-OwnedProcess -ProcessId ([int]$state.TunnelProcessId) -NamePattern "^cloudflared(\.exe)?$" -RequiredCommandText $origin -Label "stale Cloudflare tunnel"
    }

    if (@(Get-Process -Name "cloudflared" -ErrorAction SilentlyContinue).Count -gt 0) {
        throw "An unowned cloudflared process already exists. The launcher will not stop it or start a duplicate tunnel. Stop or identify that process first."
    }
    if ($null -eq (Get-Command "cloudflared" -ErrorAction SilentlyContinue)) { throw "cloudflared was not found on PATH. Install/configure it first; no tunnel was started." }

    $tunnelOutLog = Join-Path $stateDirectory "tunnel.stdout.log"
    $tunnelErrorLog = Join-Path $stateDirectory "tunnel.stderr.log"
    $tunnelProcess = Start-Process -FilePath "cloudflared" -WorkingDirectory $repositoryRoot -ArgumentList @("tunnel", "--no-autoupdate", "--url", $origin) -PassThru -WindowStyle Hidden -RedirectStandardOutput $tunnelOutLog -RedirectStandardError $tunnelErrorLog
    $tunnelUrl = $null
    for ($attempt = 1; $attempt -le 30; $attempt++) {
        $tunnelUrl = Get-QuickTunnelUrl -LogPaths @($tunnelOutLog, $tunnelErrorLog)
        if ($tunnelUrl) { break }
        $tunnelProcess.Refresh()
        if ($tunnelProcess.HasExited) { throw "The launcher-owned cloudflared process exited before publishing a Quick Tunnel URL." }
        Start-Sleep -Seconds 1
    }
    if (-not $tunnelUrl) {
        Stop-OwnedProcess -ProcessId $tunnelProcess.Id -NamePattern "^cloudflared(\.exe)?$" -RequiredCommandText $origin -Label "Cloudflare tunnel"
        throw "Cloudflared did not publish a Quick Tunnel URL within 30 seconds."
    }
    if (-not (Test-QuickTunnel -TunnelUrl $tunnelUrl)) {
        Stop-OwnedProcess -ProcessId $tunnelProcess.Id -NamePattern "^cloudflared(\.exe)?$" -RequiredCommandText $origin -Label "unverified Cloudflare tunnel"
        throw "Quick Tunnel probes did not return public 200/200 and blocked Admin 404. The URL is not reported as successful."
    }

    Save-DemoState ([ordered]@{ Version = 1; Origin = $origin; WebProcessId = $webProcessId; TunnelProcessId = $tunnelProcess.Id; TunnelUrl = $tunnelUrl; UpdatedAtUtc = [DateTime]::UtcNow.ToString("o") })
    Write-LauncherMessage "Demo is ready: $tunnelUrl"
    Open-VerifiedTunnelUrl -TunnelUrl $tunnelUrl
}
catch {
    Write-Error "VIVOICE demo was not started: $($_.Exception.Message)"
    exit 1
}
finally {
    if ($publicOnlyConfigured) {
        if ($null -eq $previousPublicOnly) {
            Remove-Item -Path Env:VivoiceDemo__PublicOnly -ErrorAction SilentlyContinue
        }
        else {
            $env:VivoiceDemo__PublicOnly = $previousPublicOnly
        }
    }
}
