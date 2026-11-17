<#
.SYNOPSIS
    Restores, builds and starts the Calendar API, then opens Swagger in the default browser.

.EXAMPLE
    .\run.ps1           Build and run.
    .\run.ps1 -Test     Run all tests first; stop if any fail.
    .\run.ps1 -Reset    Delete the local database first, so you start with no data.
#>
param(
    [switch]$Test,
    [switch]$Reset
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $root 'src\Calendar.Api'
$port = 5055
$baseUrl = "http://localhost:$port"
$swaggerUrl = "$baseUrl/swagger"

function Write-Step([string]$message) {
    Write-Host ""
    Write-Host "==> $message" -ForegroundColor Cyan
}

function Invoke-DotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "'dotnet $($args -join ' ')' failed with exit code $LASTEXITCODE."
    }
}

function Assert-DotNetInstalled {
    # A terminal opened before the SDK was installed doesn't see it on PATH yet.
    $env:Path = [Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' +
        [Environment]::GetEnvironmentVariable('Path', 'User')

    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw '.NET SDK not found. Install the .NET 10 SDK from https://dotnet.microsoft.com/download'
    }

    $sdkMajor = [int]((& dotnet --version) -split '\.')[0]
    if ($sdkMajor -lt 10) {
        throw ".NET SDK 10 or newer is required (found $(& dotnet --version))."
    }
}

function Assert-PortFree {
    $listener = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if ($listener) {
        throw "Port $port is already in use. Is the API already running in another window? Stop it with Ctrl+C and try again."
    }
}

function Remove-Database {
    Get-ChildItem -Path $apiProject -Filter 'calendar.db*' | Remove-Item -Force
}

function Start-BrowserWhenReady {
    Start-Job -ArgumentList "$baseUrl/swagger/v1/swagger.json", $swaggerUrl -ScriptBlock {
        param($healthUrl, $swaggerUrl)
        for ($attempt = 0; $attempt -lt 60; $attempt++) {
            try {
                Invoke-WebRequest $healthUrl -UseBasicParsing -TimeoutSec 2 | Out-Null
                Start-Process $swaggerUrl
                return
            }
            catch {
                Start-Sleep -Seconds 1
            }
        }
    }
}

Push-Location $root
try {
    Write-Step 'Checking .NET SDK'
    Assert-DotNetInstalled
    Write-Host "Using .NET SDK $(& dotnet --version)"

    Assert-PortFree

    if ($Reset) {
        Write-Step 'Deleting local database'
        Remove-Database
    }

    Write-Step 'Restoring packages'
    Invoke-DotNet restore

    Write-Step 'Building'
    Invoke-DotNet build --no-restore

    if ($Test) {
        Write-Step 'Running tests'
        Invoke-DotNet test --no-build
    }

    Write-Step "Starting API on $baseUrl (Swagger opens in your browser; press Ctrl+C to stop)"
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_URLS = $baseUrl
    $browserJob = Start-BrowserWhenReady
    try {
        & dotnet run --project $apiProject --no-build --no-launch-profile
    }
    finally {
        Stop-Job $browserJob
        Remove-Job $browserJob -Force
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
