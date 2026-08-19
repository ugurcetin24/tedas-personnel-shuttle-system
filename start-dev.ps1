$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendPort = 5284
$FrontendPort = 5173
$BackendUrl = "http://localhost:$BackendPort"
$FrontendUrl = "http://127.0.0.1:$FrontendPort"
$BackendProject = Join-Path $Root "backend\Tedas.Shuttle.Api\Tedas.Shuttle.Api.csproj"
$FrontendDir = Join-Path $Root "frontend\tedas-shuttle-web"
$ViteCli = Join-Path $FrontendDir "node_modules\vite\bin\vite.js"
$LogDir = Join-Path $Root "logs"

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

function Test-PortOpen {
    param(
        [string]$ComputerName,
        [int]$Port,
        [int]$TimeoutMilliseconds = 750
    )

    $client = [System.Net.Sockets.TcpClient]::new()
    try {
        $connect = $client.ConnectAsync($ComputerName, $Port)
        if (-not $connect.Wait($TimeoutMilliseconds)) {
            return $false
        }

        return $client.Connected
    }
    catch {
        return $false
    }
    finally {
        $client.Dispose()
    }
}

function Wait-PortOpen {
    param(
        [string]$ComputerName,
        [int]$Port,
        [int]$TimeoutSeconds = 8
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        if (Test-PortOpen -ComputerName $ComputerName -Port $Port) {
            return $true
        }

        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    return $false
}

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

function Save-ProcessInfo {
    param(
        [string]$Name,
        [int]$Port,
        [int]$ProcessId,
        [bool]$StartedByScript
    )

    $process = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue
    if ($null -eq $process) {
        return
    }

    $infoPath = Join-Path $LogDir "$Name.process.json"
    [PSCustomObject]@{
        name = $Name
        port = $Port
        pid = $ProcessId
        processName = $process.ProcessName
        startTimeUtc = $process.StartTime.ToUniversalTime().ToString("o")
        root = $Root
        startedByScript = $StartedByScript
        savedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    } | ConvertTo-Json | Set-Content -Path $infoPath -Encoding UTF8
}

function Start-Backend {
    $outLog = Join-Path $LogDir "backend.out.log"
    $errLog = Join-Path $LogDir "backend.err.log"

    $oldAspNetEnvironment = $env:ASPNETCORE_ENVIRONMENT
    $oldConnectionString = $env:ConnectionStrings__Default

    try {
        $env:ASPNETCORE_ENVIRONMENT = "Development"
        Remove-Item Env:\ConnectionStrings__Default -ErrorAction SilentlyContinue

        Start-Process `
            -FilePath "dotnet" `
            -ArgumentList @("run", "--no-build", "--project", $BackendProject, "--urls", $BackendUrl) `
            -WorkingDirectory $Root `
            -WindowStyle Hidden `
            -RedirectStandardOutput $outLog `
            -RedirectStandardError $errLog | Out-Null
    }
    finally {
        if ($null -ne $oldAspNetEnvironment) {
            $env:ASPNETCORE_ENVIRONMENT = $oldAspNetEnvironment
        }
        else {
            Remove-Item Env:\ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
        }

        if ($null -ne $oldConnectionString) {
            $env:ConnectionStrings__Default = $oldConnectionString
        }
        else {
            Remove-Item Env:\ConnectionStrings__Default -ErrorAction SilentlyContinue
        }
    }
}

function Start-Frontend {
    if (-not (Test-Path $ViteCli)) {
        throw "Vite CLI was not found at $ViteCli. Run npm install in $FrontendDir first."
    }

    $outLog = Join-Path $LogDir "frontend.out.log"
    $errLog = Join-Path $LogDir "frontend.err.log"
    $oldApiBaseUrl = $env:VITE_API_BASE_URL

    try {
        $env:VITE_API_BASE_URL = $BackendUrl

        Start-Process `
            -FilePath "node" `
            -ArgumentList @($ViteCli, "--host", "127.0.0.1", "--port", "$FrontendPort") `
            -WorkingDirectory $FrontendDir `
            -WindowStyle Hidden `
            -RedirectStandardOutput $outLog `
            -RedirectStandardError $errLog | Out-Null
    }
    finally {
        if ($null -ne $oldApiBaseUrl) {
            $env:VITE_API_BASE_URL = $oldApiBaseUrl
        }
        else {
            Remove-Item Env:\VITE_API_BASE_URL -ErrorAction SilentlyContinue
        }
    }
}

Write-Host "TEDAS Personnel Shuttle System dev startup"
Write-Host "Root: $Root"

if (Test-PortOpen -ComputerName "127.0.0.1" -Port $BackendPort) {
    $backendPid = Get-PortPid -Port $BackendPort
    if ($null -ne $backendPid) {
        $backendProcess = Get-Process -Id $backendPid -ErrorAction SilentlyContinue
        if ($null -ne $backendProcess -and (Test-ProjectProcess -Process $backendProcess -Name "backend")) {
            Save-ProcessInfo -Name "backend" -Port $BackendPort -ProcessId $backendPid -StartedByScript $false
        }
    }

    Write-Host "[OK] Backend already running: $BackendUrl"
}
else {
    Write-Host "[..] Starting backend on port $BackendPort"
    Start-Backend

    if (Wait-PortOpen -ComputerName "127.0.0.1" -Port $BackendPort -TimeoutSeconds 8) {
        $backendPid = Get-PortPid -Port $BackendPort
        if ($null -ne $backendPid) {
            Save-ProcessInfo -Name "backend" -Port $BackendPort -ProcessId $backendPid -StartedByScript $true
        }

        Write-Host "[OK] Backend started: $BackendUrl"
    }
    else {
        Write-Host "[!!] Backend did not open port $BackendPort within the short startup window."
        Write-Host "     Logs: $(Join-Path $LogDir 'backend.out.log')"
        Write-Host "     Errors: $(Join-Path $LogDir 'backend.err.log')"
    }
}

if (Test-PortOpen -ComputerName "127.0.0.1" -Port $FrontendPort) {
    $frontendPid = Get-PortPid -Port $FrontendPort
    if ($null -ne $frontendPid) {
        $frontendProcess = Get-Process -Id $frontendPid -ErrorAction SilentlyContinue
        if ($null -ne $frontendProcess -and (Test-ProjectProcess -Process $frontendProcess -Name "frontend")) {
            Save-ProcessInfo -Name "frontend" -Port $FrontendPort -ProcessId $frontendPid -StartedByScript $false
        }
    }

    Write-Host "[OK] Frontend already running: $FrontendUrl"
}
else {
    Write-Host "[..] Starting frontend on port $FrontendPort"
    Start-Frontend

    if (Wait-PortOpen -ComputerName "127.0.0.1" -Port $FrontendPort -TimeoutSeconds 8) {
        $frontendPid = Get-PortPid -Port $FrontendPort
        if ($null -ne $frontendPid) {
            Save-ProcessInfo -Name "frontend" -Port $FrontendPort -ProcessId $frontendPid -StartedByScript $true
        }

        Write-Host "[OK] Frontend started: $FrontendUrl"
    }
    else {
        Write-Host "[!!] Frontend did not open port $FrontendPort within the short startup window."
        Write-Host "     Logs: $(Join-Path $LogDir 'frontend.out.log')"
        Write-Host "     Errors: $(Join-Path $LogDir 'frontend.err.log')"
    }
}

Write-Host ""
Write-Host "Frontend: $FrontendUrl"
Write-Host "Backend:  $BackendUrl"
Write-Host "Swagger:  $BackendUrl/swagger"
