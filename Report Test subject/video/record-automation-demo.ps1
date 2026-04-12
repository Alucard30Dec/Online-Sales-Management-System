param(
    [string]$BaseUrl = "http://127.0.0.1:5068",
    [int]$DurationSeconds = 125,
    [string]$FfmpegPath = "C:\Users\Alucard30Dec\AppData\Local\Microsoft\WinGet\Links\ffmpeg.exe",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = "E:\Project\Online-Sales-Management-System"
$packageRoot = Join-Path $repoRoot "Report Test subject\Powered by GPT"
$legacyVideoPath = Join-Path $repoRoot "Report Test subject\video\OSMS-Automation-Demo.mp4"
$canonicalVideoPath = Join-Path $packageRoot "Videos\OSMS-Automation-Demo.mp4"
$apiResultsFolder = Join-Path $repoRoot "Report Test subject\results\automation-api"
$apiFullRunPath = Join-Path $apiResultsFolder "newman-full-run.txt"
$apiJunitPath = Join-Path $apiResultsFolder "newman-results.xml"
$resultsWorkbook = Join-Path $repoRoot "Report Test subject\results\OSMS-Final-Test-Results.xlsx"
$uiCommand = "powershell -ExecutionPolicy Bypass -File `"Report Test subject/automation/ui/run-ui-tests.ps1`" -BaseUrl `"$BaseUrl`" -Filter `"FullyQualifiedName~AdminLoginSmokeSucceeds`""
$apiCommand = "powershell -ExecutionPolicy Bypass -File `"Report Test subject/automation/api/newman/run-api-tests.ps1`" -BaseUrl `"$BaseUrl`""

if (-not (Test-Path $FfmpegPath)) {
    throw "ffmpeg not found at $FfmpegPath"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = $canonicalVideoPath
}

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

public static class ShellWindowState
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
'@

function Test-AppHealth {
    param(
        [string]$HealthUrl,
        [int]$TimeoutSeconds = 15
    )

    try {
        $null = Invoke-WebRequest -UseBasicParsing $HealthUrl -TimeoutSec $TimeoutSeconds
        return $true
    }
    catch {
        return $false
    }
}

function Set-TaskbarVisibility {
    param(
        [bool]$Visible
    )

    foreach ($className in @("Shell_TrayWnd", "Shell_SecondaryTrayWnd")) {
        $handle = [ShellWindowState]::FindWindow($className, $null)
        if ($handle -ne [IntPtr]::Zero) {
            [ShellWindowState]::ShowWindow($handle, $(if ($Visible) { 5 } else { 0 })) | Out-Null
        }
    }
}

$healthUrl = "$BaseUrl/api/v1/health"
$appBootstrapProcess = $null

if (-not (Test-AppHealth -HealthUrl $healthUrl)) {
    $bootstrapCommand = "Set-Location '$repoRoot'; dotnet run --project '.\Online Sales Management System.csproj' --urls '$BaseUrl'"
    $appBootstrapProcess = Start-Process powershell.exe -ArgumentList '-NoLogo', '-NoProfile', '-NoExit', '-Command', $bootstrapCommand -WindowStyle Minimized -PassThru
    $deadline = (Get-Date).AddSeconds(90)
    while ((Get-Date) -lt $deadline) {
        Start-Sleep -Seconds 2
        if (Test-AppHealth -HealthUrl $healthUrl -TimeoutSeconds 5) {
            break
        }
    }
}

if (-not (Test-AppHealth -HealthUrl $healthUrl -TimeoutSeconds 5)) {
    throw "Application is not reachable at $BaseUrl. Start the app first, then rerun this recording script."
}

$teamViewerProcess = Get-Process TeamViewer -ErrorAction SilentlyContinue | Select-Object -First 1
$teamViewerPath = if ($teamViewerProcess) { $teamViewerProcess.Path } else { $null }
$iobitUiProcesses = @(
    Get-Process Monitor -ErrorAction SilentlyContinue | Select-Object -First 1
    Get-Process ProTip -ErrorAction SilentlyContinue | Select-Object -First 1
    Get-Process SPNativeMessage -ErrorAction SilentlyContinue | Select-Object -First 1
) | Where-Object { $_ }
$iobitUiProcessPaths = $iobitUiProcesses | ForEach-Object { $_.Path } | Where-Object { $_ } | Select-Object -Unique

try {
    $shellApp = New-Object -ComObject Shell.Application
    foreach ($window in @($shellApp.Windows())) {
        try {
            $folderPath = $window.Document.Folder.Self.Path
            if ($folderPath -and $folderPath.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
                $window.Quit()
            }
        }
        catch {
        }
    }
}
catch {
}
finally {
    if ($shellApp) {
        [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($shellApp)
    }
}

if ($teamViewerProcess) {
    Stop-Process -Id $teamViewerProcess.Id -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
}

if ($iobitUiProcesses) {
    foreach ($process in $iobitUiProcesses) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }

    Start-Sleep -Seconds 1
}

try {
    $desktopShell = New-Object -ComObject Shell.Application
    $desktopShell.MinimizeAll()
}
catch {
}
finally {
    if ($desktopShell) {
        [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($desktopShell)
    }
}

Set-TaskbarVisibility -Visible $false

New-Item -ItemType Directory -Path (Split-Path -Parent $OutputPath) -Force | Out-Null
New-Item -ItemType Directory -Path (Split-Path -Parent $legacyVideoPath) -Force | Out-Null

if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Force
}

$ffmpegArgs = @(
    "-y",
    "-f", "gdigrab",
    "-framerate", "12",
    "-video_size", "1920x1080",
    "-i", "desktop",
    "-t", $DurationSeconds,
    "-c:v", "libx264",
    "-preset", "ultrafast",
    "-pix_fmt", "yuv420p",
    $OutputPath
)

$demoScript = @"
Set-Location '$repoRoot'
`$Host.UI.RawUI.WindowTitle = 'OSMS Automation Demo'
`$env:OSMS_UI_TIMEOUT_SECONDS = '30'
`$env:OSMS_UI_HEADLESS = 'false'
`$env:OSMS_UI_FULLSCREEN = 'true'
`$env:OSMS_UI_DEMO_PAUSE_SECONDS = '5'
Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public static class DemoWindowState { [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] public static extern bool SetForegroundWindow(IntPtr hWnd); }'
function Get-DemoWindowHandle {
    `$process = Get-Process WindowsTerminal, powershell, pwsh -ErrorAction SilentlyContinue |
        Where-Object { `$_.MainWindowHandle -ne 0 -and `$_.MainWindowTitle -like '*OSMS Automation Demo*' } |
        Select-Object -First 1
    if (`$process) {
        return `$process.MainWindowHandle
    }
    return [IntPtr]::Zero
}
function Set-DemoWindowState {
    param([int]`$State)
    `$handle = Get-DemoWindowHandle
    if (`$handle -ne [IntPtr]::Zero) {
        [DemoWindowState]::ShowWindow(`$handle, `$State) | Out-Null
    }
}
function Restore-DemoWindow {
    `$handle = Get-DemoWindowHandle
    if (`$handle -ne [IntPtr]::Zero) {
        [DemoWindowState]::ShowWindow(`$handle, 9) | Out-Null
        Start-Sleep -Milliseconds 500
        [DemoWindowState]::SetForegroundWindow(`$handle) | Out-Null
    }
}
Write-Host 'Repository root: $repoRoot'
Write-Host ''
Write-Host 'Automation folder contents:'
Get-ChildItem 'Report Test subject/automation' | Select-Object Name, LastWriteTime | Format-Table -AutoSize
Write-Host ''
Write-Host 'UI automation command:'
Write-Host '$uiCommand'
Write-Host ''
& powershell -ExecutionPolicy Bypass -File 'Report Test subject/automation/ui/run-ui-tests.ps1' -BaseUrl '$BaseUrl' -Filter 'FullyQualifiedName~AdminLoginSmokeSucceeds'
`$uiExitCode = if (`$LASTEXITCODE -is [int]) { `$LASTEXITCODE } else { 0 }
if (`$uiExitCode -ne 0) {
    throw ('UI automation exited with code ' + `$uiExitCode)
}
Write-Host ''
`$latestUiEvidence = Get-ChildItem 'Report Test subject/evidence/ui/automation' -Filter '*TC-UI-AUTH-001-success.png' |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if (-not `$latestUiEvidence) {
    throw 'Could not find a generated UI screenshot evidence file for TC-UI-AUTH-001.'
}
Write-Host 'UI screenshot evidence:'
Write-Host ('Report Test subject/evidence/ui/automation/' + `$latestUiEvidence.Name)
Write-Host ''
Get-Item `$latestUiEvidence.FullName | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
Start-Sleep -Seconds 4
`$wshell = New-Object -ComObject WScript.Shell
Write-Host ''
Write-Host 'API automation command:'
Write-Host '$apiCommand'
Write-Host ''
& powershell -ExecutionPolicy Bypass -File 'Report Test subject/automation/api/newman/run-api-tests.ps1' -BaseUrl '$BaseUrl'
Write-Host ''
Write-Host 'Saved API result files:'
Get-ChildItem '$apiResultsFolder' | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
Start-Sleep -Seconds 5
Write-Host 'Opening final results workbook...'
Start-Sleep -Seconds 2
try {
    `$shellApp = New-Object -ComObject Shell.Application
    foreach (`$window in @(`$shellApp.Windows())) {
        try {
            `$folderPath = `$window.Document.Folder.Self.Path
            if (`$folderPath -and `$folderPath.StartsWith('$repoRoot', [System.StringComparison]::OrdinalIgnoreCase)) {
                `$window.Quit()
            }
        }
        catch {
        }
    }
}
catch {
}
finally {
    if (`$shellApp) {
        [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject(`$shellApp)
    }
}
`$excel = New-Object -ComObject Excel.Application
`$excel.Visible = `$true
`$excel.DisplayAlerts = `$false
`$excel.DisplayFormulaBar = `$false
`$excel.DisplayStatusBar = `$false
`$excel.WindowState = -4137
`$excel.DisplayFullScreen = `$true
`$excelWorkbook = `$excel.Workbooks.Open('$resultsWorkbook')
`$excelWorkbook.Activate() | Out-Null
Start-Sleep -Seconds 2
`$null = `$wshell.AppActivate('OSMS-Final-Test-Results')
Start-Sleep -Milliseconds 500
`$null = `$wshell.AppActivate('Excel')
Start-Sleep -Milliseconds 500
`$excel.ExecuteExcel4Macro('SHOW.TOOLBAR(""Ribbon"",False)')
Start-Sleep -Milliseconds 500
`$wshell.SendKeys('^{F1}')
Start-Sleep -Milliseconds 500
try {
    `$excel.ActiveWindow.DisplayWorkbookTabs = `$false
    `$excel.ActiveWindow.DisplayHeadings = `$false
}
catch {
}
Write-Host ''
Write-Host 'Automation demo finished. Holding final results workbook briefly before recording ends.'
Start-Sleep -Seconds 30
[void][System.Runtime.InteropServices.Marshal]::ReleaseComObject(`$wshell)
"@

$encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($demoScript))

$orchestrator = Start-Job -ScriptBlock {
    param($Encoded)
    Start-Sleep -Seconds 1
    Start-Process powershell.exe -ArgumentList "-NoLogo", "-NoProfile", "-EncodedCommand", $Encoded -WindowStyle Normal | Out-Null
} -ArgumentList $encoded

try {
    & $FfmpegPath @ffmpegArgs | Out-Null

    Wait-Job $orchestrator | Out-Null

    if (-not (Test-Path $OutputPath)) {
        throw "Recording did not produce $OutputPath"
    }

    if ($OutputPath -ne $legacyVideoPath) {
        Copy-Item $OutputPath $legacyVideoPath -Force
    }
}
finally {
    if ($orchestrator) {
        Remove-Job $orchestrator -Force -ErrorAction SilentlyContinue
    }

    Set-TaskbarVisibility -Visible $true

    if ($teamViewerPath -and (Test-Path $teamViewerPath) -and -not (Get-Process TeamViewer -ErrorAction SilentlyContinue)) {
        Start-Process $teamViewerPath | Out-Null
    }

    foreach ($path in $iobitUiProcessPaths) {
        if ((Test-Path $path) -and -not (Get-Process | Where-Object { $_.Path -eq $path } | Select-Object -First 1)) {
            Start-Process $path | Out-Null
        }
    }
}
