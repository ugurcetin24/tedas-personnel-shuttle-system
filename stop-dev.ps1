$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendPort = 5284
$FrontendPort = 5173
$LogDir = Join-Path $Root "logs"

function Get-PortPid {
    param([int]$Port)

    $matches = netstat -ano -p tcp | Select-String -Pattern "LISTENING"
    foreach ($match in $matches) {
        $parts = ($match.Line -split "\s+") | Where-Object { $_ }
        if ($parts.Count -lt 5) {
            continue
        }

        $localAddress = $parts[1]
        if ($localAddress.EndsWith(":$Port")) {
            return [int]$parts[$parts.Count - 1]
        }
    }

    return $null
}

function Get-ProcessCommandLine {
    param([int]$ProcessId)

    try {
        $process = Get-CimInstance Win32_Process -Filter "ProcessId = $ProcessId" -ErrorAction Stop
        return $process.CommandLine
    }
    catch {
        return $null
    }
}

function Test-ProjectProcess {
    param(
        [System.Diagnostics.Process]$Process,
        [string]$Name
    )

    $normalizedRoot = [System.IO.Path]::GetFullPath($Root).TrimEnd("\")

    if ($Name -eq "backend") {
        if ($null -ne $Process.Path -and $Process.ProcessName -eq "Tedas.Shuttle.Api" -and $Process.Path.StartsWith($normalizedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    if ($Name -eq "frontend") {
        $commandLine = Get-ProcessCommandLine -ProcessId $Process.Id
        if ($null -ne $commandLine -and $commandLine.IndexOf($normalizedRoot, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -and $commandLine.IndexOf("vite", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            return $true
        }
    }

    return $false
}

function Test-SavedProcessMatch {
    param(
        [System.Diagnostics.Process]$Process,
        [object]$Info
    )

    if ($null -eq $Info.startTimeUtc) {
        return $false
    }

    if ($Info.root -ne $Root -or $Info.startedByScript -ne $true) {
        return $false
    }

    $savedStart = [DateTime]::Parse($Info.startTimeUtc).ToUniversalTime()
    $actualStart = $Process.StartTime.ToUniversalTime()
    return ([Math]::Abs(($savedStart - $actualStart).TotalSeconds) -lt 2)
}

function Stop-ProcessTree {
    param([int]$ProcessId)

    taskkill.exe /PID $ProcessId /T /F | Out-Null
}

function Stop-TrackedProcess {
    param(
        [string]$Name,
        [int]$Port
    )

    $infoPath = Join-Path $LogDir "$Name.process.json"
    $pid = $null
    $source = $null
    $savedInfo = $null

    if (Test-Path $infoPath) {
        try {
            $savedInfo = Get-Content -Raw $infoPath | ConvertFrom-Json
            $pid = [int]$savedInfo.pid
            $source = "metadata"
        }
        catch {
            $pid = $null
        }
    }

    if ($null -eq $pid) {
        $portPid = Get-PortPid -Port $Port
        if ($null -ne $portPid) {
            $pid = $portPid
            $source = "port"
        }
    }

    if ($null -eq $pid) {
        Write-Host "[OK] $Name is not running on port $Port."
        return
    }

    $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
    if ($null -eq $process) {
        Write-Host "[OK] $Name process is already stopped."
        Remove-Item -Path $infoPath -Force -ErrorAction SilentlyContinue
        return
    }

    $isSafe = Test-ProjectProcess -Process $process -Name $Name
    if ($source -eq "metadata" -and $null -ne $savedInfo) {
        $isSafe = $isSafe -or (Test-SavedProcessMatch -Process $process -Info $savedInfo)
    }

    if (-not $isSafe) {
        Write-Host "[SKIP] $Name port $Port is used by PID $pid, but it was not verified as this project."
        return
    }

    Stop-ProcessTree -ProcessId $pid
    Remove-Item -Path $infoPath -Force -ErrorAction SilentlyContinue
    Write-Host "[OK] Stopped $Name PID $pid."
}

Write-Host "TEDAS Personnel Shuttle System dev shutdown"
Write-Host "Root: $Root"

Stop-TrackedProcess -Name "frontend" -Port $FrontendPort
Stop-TrackedProcess -Name "backend" -Port $BackendPort
