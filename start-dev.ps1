param(
    [switch]$NoBrowser,
    [switch]$SkipInstall
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$backendDirectory = Join-Path $projectRoot 'backend'
$frontendDirectory = Join-Path $projectRoot 'frontend'
$backendProcess = $null
$frontendProcess = $null

function Assert-CommandExists {
    param([Parameter(Mandatory)][string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found in PATH."
    }
}

function Test-UrlReady {
    param([Parameter(Mandatory)][string]$Url)

    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2
        return $response.StatusCode -ge 200 -and $response.StatusCode -lt 500
    }
    catch {
        return $false
    }
}

function Stop-DevProcess {
    param([System.Diagnostics.Process]$Process)

    if ($null -ne $Process -and -not $Process.HasExited) {
        if ($env:OS -eq 'Windows_NT' -and (Get-Command 'taskkill.exe' -ErrorAction SilentlyContinue)) {
            & taskkill.exe /PID $Process.Id /T /F 2>$null | Out-Null
        }
        else {
            Stop-Process -Id $Process.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

try {
    Assert-CommandExists 'dotnet'
    Assert-CommandExists 'npm.cmd'

    if (-not $SkipInstall -and -not (Test-Path (Join-Path $frontendDirectory 'node_modules'))) {
        Write-Host 'Frontend dependencies are missing. Running npm ci...' -ForegroundColor Yellow
        Push-Location $frontendDirectory
        try {
            & npm.cmd ci
            if ($LASTEXITCODE -ne 0) { throw 'npm ci failed.' }
        }
        finally {
            Pop-Location
        }
    }

    Write-Host ''
    Write-Host 'Starting MIS development environment...' -ForegroundColor Cyan
    Write-Host 'Backend:  http://localhost:5000' -ForegroundColor DarkCyan
    Write-Host 'Frontend: http://localhost:5173' -ForegroundColor DarkCyan
    Write-Host 'Press Ctrl+C once to stop both services.' -ForegroundColor Yellow
    Write-Host ''

    $backendProcess = Start-Process -FilePath 'dotnet' `
        -ArgumentList @('watch', 'run', '--project', 'MIS.API', '--launch-profile', 'http') `
        -WorkingDirectory $backendDirectory `
        -NoNewWindow `
        -PassThru

    $frontendProcess = Start-Process -FilePath 'npm.cmd' `
        -ArgumentList @('run', 'dev', '--', '--host', '127.0.0.1') `
        -WorkingDirectory $frontendDirectory `
        -NoNewWindow `
        -PassThru

    $browserOpened = $NoBrowser
    $startupDeadline = (Get-Date).AddSeconds(90)

    while (-not $backendProcess.HasExited -and -not $frontendProcess.HasExited) {
        if (-not $browserOpened -and
            (Test-UrlReady 'http://localhost:5000/api/health') -and
            (Test-UrlReady 'http://localhost:5173')) {
            Start-Process 'http://localhost:5173'
            $browserOpened = $true
        }
        elseif (-not $browserOpened -and (Get-Date) -gt $startupDeadline) {
            Write-Warning 'Services are still starting; open http://localhost:5173 when they are ready.'
            $browserOpened = $true
        }

        Start-Sleep -Milliseconds 500
    }

    if ($backendProcess.HasExited) {
        throw "Backend stopped unexpectedly with exit code $($backendProcess.ExitCode)."
    }
    if ($frontendProcess.HasExited) {
        throw "Frontend stopped unexpectedly with exit code $($frontendProcess.ExitCode)."
    }
}
catch {
    Write-Host ''
    Write-Error $_ -ErrorAction Continue
    exit 1
}
finally {
    Write-Host ''
    Write-Host 'Stopping MIS services...' -ForegroundColor Yellow
    Stop-DevProcess $frontendProcess
    Stop-DevProcess $backendProcess
}
