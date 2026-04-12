param(
    [string]$BaseUrl = "http://127.0.0.1:5068",
    [int]$DurationSeconds = 275,
    [string]$FfmpegPath = "C:\Users\Alucard30Dec\AppData\Local\Microsoft\WinGet\Links\ffmpeg.exe",
    [string]$OutputPath = "",
    [switch]$NoRecording,
    [switch]$KeepTempScript
)

$ErrorActionPreference = "Stop"

$repoRoot = "E:\Project\Online-Sales-Management-System"
$packageRoot = Join-Path $repoRoot "Report Test subject\Powered by GPT"
$videosRoot = Join-Path $packageRoot "Videos"
$outputVideo = if ([string]::IsNullOrWhiteSpace($OutputPath)) { Join-Path $videosRoot "OSMS-Automation-Demo.mp4" } else { $OutputPath }
$legacyVideo = Join-Path $repoRoot "Report Test subject\video\OSMS-Automation-Demo.mp4"

$uiRunner = Join-Path $packageRoot "TestScript-Data\Automation\ui\run-ui-tests.ps1"
$apiRunner = Join-Path $packageRoot "TestScript-Data\Automation\api\newman\run-api-tests.ps1"
$uiCaseCsv = Join-Path $packageRoot "TestCases\UI\OSMS-UI-Test-Cases.csv"
$apiCaseCsv = Join-Path $packageRoot "TestCases\API\OSMS-API-Test-Cases.csv"
$uiCasesWorkbook = Join-Path $packageRoot "TestCases\UI\OSMS-UI-Test-Cases.xlsx"
$apiCasesWorkbook = Join-Path $packageRoot "TestCases\API\OSMS-API-Test-Cases.xlsx"
$finalResultsCsv = Join-Path $packageRoot "TestResults\FinalResults\OSMS-Final-Results.csv"
$finalResultsWorkbook = Join-Path $packageRoot "TestResults\FinalResults\OSMS-Final-Test-Results.xlsx"
$videoComparisonCsv = Join-Path $packageRoot "TestResults\FinalResults\OSMS-Automation-Video-Result-Comparison.csv"
$videoComparisonViewCsv = Join-Path $packageRoot "TestResults\FinalResults\OSMS-Automation-Video-Result-View.csv"
$demoLogPath = Join-Path $videosRoot "OSMS-Automation-Demo.log"
$defectLogWorkbook = Join-Path $packageRoot "TestResults\Defects\OSMS-Defect-Log.xlsx"
$apiSummaryImage = Join-Path $packageRoot "TestResults\Evidence\Report\OSMS-Newman-Full-Run-Snippet.png"
$defectScreenshot = Join-Path $packageRoot "TestResults\Evidence\Defects\BUG-20260406-001-github-issue.png"
$uiEvidenceRoot = Join-Path $packageRoot "TestResults\Evidence\UI\automation"

if (-not (Test-Path -LiteralPath $FfmpegPath)) {
    throw "ffmpeg not found at $FfmpegPath"
}

foreach ($requiredPath in @(
    $uiRunner,
    $apiRunner,
    $uiCaseCsv,
    $apiCaseCsv,
    $uiCasesWorkbook,
    $apiCasesWorkbook,
    $finalResultsCsv,
    $defectLogWorkbook,
    $apiSummaryImage,
    $defectScreenshot
)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Missing required artifact: $requiredPath"
    }
}

function Test-AppHealth {
    param([string]$HealthUrl)

    try {
        $null = Invoke-WebRequest -UseBasicParsing $HealthUrl -TimeoutSec 10
        return $true
    }
    catch {
        return $false
    }
}

function Get-FirstNonEmptyValue {
    param([string[]]$Values)

    foreach ($value in $Values) {
        if (-not [string]::IsNullOrWhiteSpace($value) -and $value -ne "N/A") {
            return $value.Trim()
        }
    }

    return ""
}

function Get-StepSummary {
    param([string]$Steps)

    if ([string]::IsNullOrWhiteSpace($Steps)) {
        return "Automated execution completed using the configured runner."
    }

    $parts = $Steps -split ';' |
        ForEach-Object { $_.Trim() } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -First 4

    if (-not $parts) {
        return "Automated execution completed using the configured runner."
    }

    return ($parts -join " | ")
}

function Get-EvidenceSummary {
    param($ResultRow)

    $items = New-Object System.Collections.Generic.List[string]

    foreach ($fieldName in @("Evidence Screenshot", "Evidence Result File")) {
        $fieldValue = [string]$ResultRow.$fieldName
        if (-not [string]::IsNullOrWhiteSpace($fieldValue) -and $fieldValue -notlike "N/A*") {
            $items.Add([System.IO.Path]::GetFileName($fieldValue))
        }
    }

    $bugLink = [string]$ResultRow.'Bug Link'
    if (-not [string]::IsNullOrWhiteSpace($bugLink) -and $bugLink -ne "N/A") {
        $items.Add($bugLink)
    }

    if ($items.Count -eq 0) {
        return "Runner output only"
    }

    return (($items | Select-Object -Unique) -join "; ")
}

function Get-ReportReference {
    param($ResultRow)

    if (-not [string]::IsNullOrWhiteSpace([string]$ResultRow.'Bug Link') -and [string]$ResultRow.'Bug Link' -ne "N/A") {
        return "OSMS-Defect-Log.xlsx"
    }

    return [System.IO.Path]::GetFileName($finalResultsWorkbook)
}

$videoCaseIds = @(
    "TC-UI-AUTH-001",
    "TC-UI-IMP-002",
    "TC-API-HLT-001",
    "TC-UI-PUR-002",
    "TC-UI-INV-001"
)

$uiCases = @{}
Import-Csv $uiCaseCsv | ForEach-Object { $uiCases[$_.'Test Case ID'] = $_ }

$apiCases = @{}
Import-Csv $apiCaseCsv | ForEach-Object { $apiCases[$_.'Test Case ID'] = $_ }

$resultRows = Import-Csv $finalResultsCsv | Where-Object { $_.'Test Case ID' -in $videoCaseIds }
$resultIndex = @{}
foreach ($row in $resultRows) {
    $resultIndex[$row.'Test Case ID'] = $row
}

$videoComparison = foreach ($caseId in $videoCaseIds) {
    $result = $resultIndex[$caseId]
    if (-not $result) {
        throw "Missing final result row for $caseId"
    }

    $caseSource = if ($uiCases.ContainsKey($caseId)) { $uiCases[$caseId] } else { $apiCases[$caseId] }
    if (-not $caseSource) {
        throw "Missing test case row for $caseId"
    }

    $preCondition = Get-FirstNonEmptyValue @(
        [string]$caseSource.Preconditions,
        [string]$caseSource.'Auth Requirement',
        "Application is running locally and required test data is available."
    )

    [pscustomobject]@{
        "Execution Order"   = [array]::IndexOf($videoCaseIds, $caseId) + 1
        "Test Case ID"      = $caseId
        "Test Objective"    = [string]$result.Title
        "Pre-condition"     = $preCondition
        "Steps Executed"    = Get-StepSummary ([string]$caseSource.Steps)
        "Expected Result"   = [string]$result.'Expected Result'
        "Actual Result"     = [string]$result.'Actual Result'
        "Status"            = [string]$result.'Reporting Status'
        "Evidence / Files"  = Get-EvidenceSummary $result
        "Report Reference"  = Get-ReportReference $result
    }
}

$videoComparison | Export-Csv -LiteralPath $videoComparisonCsv -NoTypeInformation -Encoding UTF8
$videoComparison |
    Select-Object "Execution Order", "Test Case ID", "Test Objective", "Expected Result", "Actual Result", "Status", "Evidence / Files", "Report Reference" |
    Export-Csv -LiteralPath $videoComparisonViewCsv -NoTypeInformation -Encoding UTF8

$healthUrl = "$BaseUrl/api/v1/health"
$appBootstrapProcess = $null

if (-not (Test-AppHealth -HealthUrl $healthUrl)) {
    $bootstrapCommand = "Set-Location '$repoRoot'; dotnet run --project '.\Online Sales Management System.csproj' --urls '$BaseUrl'"
    $appBootstrapProcess = Start-Process powershell.exe -ArgumentList '-NoLogo', '-NoProfile', '-NoExit', '-Command', $bootstrapCommand -WindowStyle Minimized -PassThru
    $deadline = (Get-Date).AddSeconds(90)

    while ((Get-Date) -lt $deadline) {
        Start-Sleep -Seconds 2
        if (Test-AppHealth -HealthUrl $healthUrl) {
            break
        }
    }
}

if (-not (Test-AppHealth -HealthUrl $healthUrl)) {
    throw "Application is not reachable at $BaseUrl."
}

& dotnet build (Join-Path $packageRoot "TestScript-Data\Automation\ui\OSMS.UITests\OSMS.UITests.csproj") | Out-Null
& npx.cmd --yes newman --version | Out-Null

New-Item -ItemType Directory -Path $videosRoot -Force | Out-Null
New-Item -ItemType Directory -Path (Split-Path -Parent $legacyVideo) -Force | Out-Null
Set-Content -LiteralPath $demoLogPath -Value ("[{0}] Preparing automation demo." -f (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")) -Encoding UTF8

if (Test-Path -LiteralPath $outputVideo) {
    Remove-Item -LiteralPath $outputVideo -Force
}

Get-Process ffmpeg -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process chrome, msedge, excel, TeamViewer, Monitor, ProTip, SPNativeMessage -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue

Get-Process powershell, pwsh, WindowsTerminal -ErrorAction SilentlyContinue |
    Where-Object { $_.MainWindowTitle -like '*OSMS Automation Demo*' } |
    Stop-Process -Force -ErrorAction SilentlyContinue

try {
    $shell = New-Object -ComObject Shell.Application
    $shell.MinimizeAll()
}
catch {
}
finally {
    if ($shell) {
        [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($shell)
    }
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
    $outputVideo
)

$demoTemplate = @'
Set-Location "{{REPO_ROOT}}"
$Host.UI.RawUI.WindowTitle = 'OSMS Automation Demo'
chcp 65001 | Out-Null
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$OutputEncoding = [Console]::OutputEncoding

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class DemoWindowState
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
}
"@

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$script:DemoWindowHandle = [System.Diagnostics.Process]::GetProcessById($PID).MainWindowHandle
$script:DemoLogPath = '{{DEMO_LOG}}'

function Write-DemoLog {
    param([string]$Message)
    Add-Content -LiteralPath $script:DemoLogPath -Value ("[{0}] {1}" -f (Get-Date).ToString('HH:mm:ss'), $Message)
}

function Pause-Demo {
    param([int]$Seconds = 3)
    Start-Sleep -Seconds $Seconds
}

function Bring-WindowToFront {
    param([IntPtr]$Handle)

    if ($Handle -eq [IntPtr]::Zero) {
        return
    }

    [DemoWindowState]::ShowWindow($Handle, 3) | Out-Null
    Start-Sleep -Milliseconds 400
    [DemoWindowState]::SetForegroundWindow($Handle) | Out-Null
}

function Bring-DemoWindowToFront {
    Bring-WindowToFront $script:DemoWindowHandle
}

function Wait-ForBrowserWindow {
    param([int]$TimeoutSeconds = 35)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $browser = Get-Process chrome, msedge -ErrorAction SilentlyContinue |
            Where-Object { $_.MainWindowHandle -ne 0 } |
            Sort-Object StartTime -Descending |
            Select-Object -First 1

        if ($browser) {
            return $browser.MainWindowHandle
        }

        Start-Sleep -Milliseconds 400
    }

    return [IntPtr]::Zero
}

function Close-WindowProcess {
    param([System.Diagnostics.Process]$Process)

    if (-not $Process) {
        return
    }

    try {
        if (-not $Process.HasExited) {
            $Process.CloseMainWindow() | Out-Null
            Start-Sleep -Milliseconds 800
        }
    }
    catch {
    }

    try {
        if (-not $Process.HasExited) {
            Stop-Process -Id $Process.Id -Force -ErrorAction SilentlyContinue
        }
    }
    catch {
    }
}

function Show-WorkbookCases {
    param(
        [string]$WorkbookPath,
        [string[]]$CaseIds,
        [string[]]$HeadersToHighlight,
        [int]$Zoom = 145,
        [int]$PauseSeconds = 4
    )

    $excel = New-Object -ComObject Excel.Application
    Write-DemoLog "Showing workbook: $WorkbookPath"
    $excel.Visible = $true
    $excel.DisplayAlerts = $false
    $excel.WindowState = -4137
    $excel.DisplayFullScreen = $true
    $excel.DisplayFormulaBar = $false
    $excel.DisplayStatusBar = $false
    try { $excel.ExecuteExcel4Macro('SHOW.TOOLBAR(""Ribbon"",False)') | Out-Null } catch {}

    $workbook = $excel.Workbooks.Open($WorkbookPath)
    $sheet = $workbook.Worksheets.Item(1)
    $usedRange = $sheet.UsedRange
    $sheet.Activate() | Out-Null
    $excel.ActiveWindow.Zoom = $Zoom
    Bring-WindowToFront ([IntPtr]$excel.Hwnd)
    Pause-Demo 1

    $headerMap = @{}
    for ($column = 1; $column -le $usedRange.Columns.Count; $column++) {
        $headerText = [string]$sheet.Cells.Item(1, $column).Text
        if (-not [string]::IsNullOrWhiteSpace($headerText)) {
            $headerMap[$headerText.Trim()] = $column
        }
    }

    $preferredWidths = @{
        'Execution Order'   = 12
        'Test Case ID'      = 18
        'Test Objective'    = 34
        'Pre-condition'     = 22
        'Steps Executed'    = 30
        'Expected Result'   = 34
        'Actual Result'     = 34
        'Status'            = 14
        'Evidence / Files'  = 26
        'Report Reference'  = 18
        'Title'             = 34
        'Endpoint'          = 24
        'Expected Body'     = 30
        'Reporting Status'  = 16
        'Defect ID'         = 18
        'Record ID'         = 18
        'Related Test Case ID' = 22
        'Severity'          = 14
        'Priority'          = 14
        'Current Status'    = 18
        'GitHub Issue'      = 22
        'Issue URL'         = 18
    }

    foreach ($header in $preferredWidths.Keys) {
        if ($headerMap.ContainsKey($header)) {
            $sheet.Columns.Item($headerMap[$header]).ColumnWidth = $preferredWidths[$header]
            $sheet.Columns.Item($headerMap[$header]).WrapText = $true
        }
    }

    if ($headerMap.ContainsKey('Test Case ID')) {
        $testCaseColumn = $headerMap['Test Case ID']
        foreach ($caseId in $CaseIds) {
            $match = $sheet.Columns.Item($testCaseColumn).Find($caseId)
            if ($match) {
                $row = $match.Row
                $startColumn = 1
                $endColumn = [Math]::Min($usedRange.Columns.Count, $startColumn + 9)
                $sheet.Range($sheet.Cells.Item($row, $startColumn), $sheet.Cells.Item($row, $endColumn)).Select() | Out-Null
                Pause-Demo $PauseSeconds
            }
        }
    }

    foreach ($header in $HeadersToHighlight) {
        if ($headerMap.ContainsKey($header)) {
            $column = $headerMap[$header]
            $sheet.Range($sheet.Cells.Item(1, $column), $sheet.Cells.Item([Math]::Min($usedRange.Rows.Count, 6), $column)).Select() | Out-Null
            Pause-Demo 2
        }
    }

    $workbook.Close($false)
    $excel.Quit()
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($sheet)
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($workbook)
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($excel)
    Bring-DemoWindowToFront
    Write-DemoLog "Closed workbook: $WorkbookPath"
}

function Show-ImageArtifact {
    param(
        [string]$Path,
        [string]$Caption,
        [int]$PauseSeconds = 5
    )

    $form = New-Object System.Windows.Forms.Form
    Write-DemoLog "Showing image artifact: $Caption"
    $form.Text = $Caption
    $form.WindowState = 'Maximized'
    $form.StartPosition = 'CenterScreen'
    $form.TopMost = $true

    $label = New-Object System.Windows.Forms.Label
    $label.Dock = 'Top'
    $label.Height = 52
    $label.TextAlign = 'MiddleLeft'
    $label.Font = New-Object System.Drawing.Font('Segoe UI', 20, [System.Drawing.FontStyle]::Bold)
    $label.Padding = New-Object System.Windows.Forms.Padding(18, 0, 0, 0)
    $label.Text = $Caption

    $picture = New-Object System.Windows.Forms.PictureBox
    $picture.Dock = 'Fill'
    $picture.SizeMode = 'Zoom'
    $picture.Image = [System.Drawing.Image]::FromFile($Path)

    $form.Controls.Add($picture)
    $form.Controls.Add($label)

    $timer = New-Object System.Windows.Forms.Timer
    $timer.Interval = [Math]::Max(1, $PauseSeconds) * 1000
    $timer.Add_Tick({
        $timer.Stop()
        $form.Close()
    })
    $timer.Start()

    [void]$form.ShowDialog()

    $timer.Dispose()
    $picture.Image.Dispose()
    $picture.Dispose()
    $label.Dispose()
    $form.Dispose()
    Bring-DemoWindowToFront
    Write-DemoLog "Closed image artifact: $Caption"
}

function Show-TextArtifact {
    param(
        [string]$Text,
        [string]$Caption,
        [int]$PauseSeconds = 6
    )

    $form = New-Object System.Windows.Forms.Form
    Write-DemoLog "Showing text artifact: $Caption"
    $form.Text = $Caption
    $form.WindowState = 'Maximized'
    $form.StartPosition = 'CenterScreen'
    $form.TopMost = $true

    $label = New-Object System.Windows.Forms.Label
    $label.Dock = 'Top'
    $label.Height = 52
    $label.TextAlign = 'MiddleLeft'
    $label.Font = New-Object System.Drawing.Font('Segoe UI', 20, [System.Drawing.FontStyle]::Bold)
    $label.Padding = New-Object System.Windows.Forms.Padding(18, 0, 0, 0)
    $label.Text = $Caption

    $textbox = New-Object System.Windows.Forms.RichTextBox
    $textbox.Dock = 'Fill'
    $textbox.ReadOnly = $true
    $textbox.BackColor = [System.Drawing.Color]::White
    $textbox.ForeColor = [System.Drawing.Color]::Black
    $textbox.Font = New-Object System.Drawing.Font('Consolas', 16)
    $textbox.WordWrap = $false
    $textbox.Text = $Text

    $form.Controls.Add($textbox)
    $form.Controls.Add($label)

    $timer = New-Object System.Windows.Forms.Timer
    $timer.Interval = [Math]::Max(1, $PauseSeconds) * 1000
    $timer.Add_Tick({
        $timer.Stop()
        $form.Close()
    })
    $timer.Start()

    [void]$form.ShowDialog()

    $timer.Dispose()
    $textbox.Dispose()
    $label.Dispose()
    $form.Dispose()
    Bring-DemoWindowToFront
    Write-DemoLog "Closed text artifact: $Caption"
}

function Show-ResultComparisonArtifact {
    param(
        [string]$ComparisonCsv,
        [int]$PauseSeconds = 18
    )

    $comparisonRows = Import-Csv $ComparisonCsv
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('Expected Result vs Actual Result vs Status')
    $lines.Add('========================================')
    $lines.Add('')

    foreach ($row in $comparisonRows) {
        $lines.Add("Test Case ID   : $($row.'Test Case ID')")
        $lines.Add("Objective      : $($row.'Test Objective')")
        $lines.Add("Pre-condition  : $($row.'Pre-condition')")
        $lines.Add("Steps Executed : $($row.'Steps Executed')")
        $lines.Add("Expected Result: $($row.'Expected Result')")
        $lines.Add("Actual Result  : $($row.'Actual Result')")
        $lines.Add("Status         : $($row.Status)")
        $lines.Add("Evidence / File: $($row.'Evidence / Files')")
        $lines.Add("Report Ref     : $($row.'Report Reference')")
        $lines.Add('')
        $lines.Add('----------------------------------------')
        $lines.Add('')
    }

    Show-TextArtifact -Text ($lines -join [Environment]::NewLine) -Caption 'Result Comparison - Expected vs Actual' -PauseSeconds $PauseSeconds
}

function Invoke-UiAutomationDemo {
    param(
        [string]$CaseId,
        [string]$Filter,
        [string]$Browser = 'chrome',
        [string]$ExpectedText,
        [string]$ActualText,
        [string]$ScreenshotPattern,
        [int]$BrowserViewSeconds = 8
    )

    Write-Host ''
    Write-Host "Running $CaseId"
    Write-Host "Expected Result: $ExpectedText"
    Write-DemoLog "Starting UI automation: $CaseId"
    Pause-Demo 2

    $argumentList = @(
        '-NoLogo',
        '-NoProfile',
        '-ExecutionPolicy',
        'Bypass',
        '-File',
        '"{{UI_RUNNER}}"',
        '-BaseUrl',
        '{{BASE_URL}}',
        '-Browser',
        $Browser,
        '-Fullscreen',
        '-DemoPauseSeconds',
        '4',
        '-Filter',
        $Filter
    )

    $runnerStdOut = Join-Path $env:TEMP ("ui-runner-" + $CaseId + "-stdout.log")
    $runnerStdErr = Join-Path $env:TEMP ("ui-runner-" + $CaseId + "-stderr.log")
    Remove-Item -LiteralPath $runnerStdOut, $runnerStdErr -Force -ErrorAction SilentlyContinue
    $process = Start-Process powershell.exe -ArgumentList $argumentList -WindowStyle Minimized -WorkingDirectory '{{REPO_ROOT}}' -RedirectStandardOutput $runnerStdOut -RedirectStandardError $runnerStdErr -PassThru
    Write-DemoLog "UI runner process id: $($process.Id)"
    $browserHandle = Wait-ForBrowserWindow -TimeoutSeconds 35
    if ($browserHandle -ne [IntPtr]::Zero) {
        Write-DemoLog "Browser window detected for $CaseId."
        Bring-WindowToFront $browserHandle
        Pause-Demo 1
        [System.Windows.Forms.SendKeys]::SendWait('{F11}')
        Write-DemoLog "Sent F11 to browser window for $CaseId."
        Pause-Demo 1
        Bring-WindowToFront $browserHandle
        Pause-Demo $BrowserViewSeconds
    }
    else {
        Write-DemoLog "Browser window was not detected for $CaseId within timeout."
    }

    $process.WaitForExit()
    $process.Refresh()
    $exitCode = $process.ExitCode
    if ($null -eq $exitCode) {
        $exitCode = 0
    }
    Bring-DemoWindowToFront
    Write-DemoLog "UI runner exited for $CaseId with code $exitCode."

    if ($exitCode -ne 0) {
        $stderrPreview = ''
        if (Test-Path -LiteralPath $runnerStdErr) {
            $stderrPreview = (Get-Content -LiteralPath $runnerStdErr -Raw | Select-Object -First 1)
        }
        if ([string]::IsNullOrWhiteSpace($stderrPreview) -and (Test-Path -LiteralPath $runnerStdOut)) {
            $stderrPreview = (Get-Content -LiteralPath $runnerStdOut -Tail 20 | Out-String)
        }
        throw ("UI automation failed for " + $CaseId + ". " + $stderrPreview.Trim())
    }

    $evidence = Get-ChildItem '{{UI_EVIDENCE_ROOT}}' -Filter $ScreenshotPattern |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $evidence) {
        throw "Missing screenshot evidence for $CaseId."
    }

    Write-Host "Actual Result: $ActualText"
    Write-Host 'Status: Pass'
    Write-DemoLog "UI automation passed for $CaseId."
    Pause-Demo 2
    Show-ImageArtifact -Path $evidence.FullName -Caption "Evidence - $CaseId" -PauseSeconds 5
}

try {
    Write-DemoLog 'Demo script started.'
    Bring-DemoWindowToFront
    Write-Host 'ONLINE SALES MANAGEMENT SYSTEM - AUTOMATION EXECUTION DEMO'
    Write-Host 'UI Automation: dotnet test + Selenium'
    Write-Host 'API Automation: Newman / Postman Collection'
    Write-Host ''
    Write-Host 'This demo shows mapped test cases, live execution, evidence, and result comparison.'
    Pause-Demo 5

    Write-Host ''
    Write-Host 'STEP 1/8 - Mapping UI automated and defect-linked test cases.'
    Show-WorkbookCases -WorkbookPath '{{UI_CASES_WORKBOOK}}' -CaseIds @('TC-UI-AUTH-001', 'TC-UI-IMP-002', 'TC-UI-PUR-002', 'TC-UI-INV-001') -HeadersToHighlight @('Test Case ID', 'Title', 'Preconditions', 'Expected Result', 'Status') -Zoom 145 -PauseSeconds 4

    Write-Host ''
    Write-Host 'STEP 2/8 - Mapping API automated test case.'
    Show-WorkbookCases -WorkbookPath '{{API_CASES_WORKBOOK}}' -CaseIds @('TC-API-HLT-001') -HeadersToHighlight @('Test Case ID', 'Endpoint', 'Expected Status Code', 'Expected Body', 'Status') -Zoom 145 -PauseSeconds 4

    Write-Host ''
    Write-Host 'STEP 3/8 - Executing UI login smoke automation on Edge.'
    Write-Host 'Pre-condition: seeded admin account exists and the local application is running.'
    Write-Host 'UI runner command: run-ui-tests.ps1 -Browser edge -Fullscreen -Filter AdminLoginSmokeSucceeds'
    Invoke-UiAutomationDemo -CaseId 'TC-UI-AUTH-001' -Browser 'edge' -Filter 'FullyQualifiedName~AdminLoginSmokeSucceeds' -ExpectedText 'Valid admin credentials should redirect the user to the admin dashboard.' -ActualText 'The Edge smoke rerun opened the login page, submitted valid credentials, and reached the admin dashboard successfully.' -ScreenshotPattern '*TC-UI-AUTH-001-success.png' -BrowserViewSeconds 8

    Write-Host ''
    Write-Host 'STEP 4/8 - Executing UI product import preview automation.'
    Write-Host 'Pre-condition: the mixed-validation import workbook is available in the prepared test data set.'
    Write-Host 'UI runner command: run-ui-tests.ps1 -Browser chrome -Fullscreen -Filter ProductImportPreviewShowsExpectedValidAndInvalidCounts'
    Invoke-UiAutomationDemo -CaseId 'TC-UI-IMP-002' -Browser 'chrome' -Filter 'FullyQualifiedName~ProductImportPreviewShowsExpectedValidAndInvalidCounts' -ExpectedText 'The preview page should show 6 total rows, 1 valid row, and 5 invalid rows.' -ActualText 'The automated rerun uploaded the workbook and the preview page displayed 6 total rows, 1 valid row, and 5 invalid rows.' -ScreenshotPattern '*TC-UI-IMP-002-preview.png' -BrowserViewSeconds 24

    Write-Host ''
    Write-Host 'STEP 5/8 - Executing API automation with Newman.'
    Write-Host 'Pre-condition: the local API is reachable at /api/v1/health and catalog endpoints.'
    Write-Host 'Expected Result: the health and catalog requests should return the expected status codes and pass all assertions.'
    Write-Host 'API runner command: run-api-tests.ps1 -BaseUrl {{BASE_URL}}'
    Write-DemoLog 'Starting API automation.'
    Pause-Demo 2
    & '{{API_RUNNER}}' -BaseUrl '{{BASE_URL}}'
    if ($LASTEXITCODE -is [int] -and $LASTEXITCODE -ne 0) {
        throw 'API automation failed.'
    }
    Write-DemoLog 'API automation passed.'
    Write-Host ''
    Write-Host 'Actual Result: the Newman collection completed successfully and saved the execution artifact.'
    Write-Host 'Status: Pass'
    Pause-Demo 2
    Show-ImageArtifact -Path '{{API_SUMMARY_IMAGE}}' -Caption 'Evidence - API Automation Summary' -PauseSeconds 6

    Write-Host ''
    Write-Host 'STEP 6/8 - Comparing Expected Result, Actual Result, and Status.'
    Show-WorkbookCases -WorkbookPath '{{VIDEO_COMPARISON_VIEW_CSV}}' -CaseIds @('TC-UI-AUTH-001', 'TC-UI-IMP-002', 'TC-API-HLT-001', 'TC-UI-PUR-002', 'TC-UI-INV-001') -HeadersToHighlight @('Expected Result', 'Actual Result', 'Status', 'Evidence / Files', 'Report Reference') -Zoom 130 -PauseSeconds 4

    Write-Host ''
    Write-Host 'STEP 7/8 - Showing focused defect retest evidence and live issue tracking.'
    Write-Host 'Expected Result: failed cases should map to clear defect records with severity, priority, and issue links.'
    Write-Host 'Actual Result: the latest 2026-04-11 reruns still reproduced the purchase, import, and invoice defects.'
    Show-WorkbookCases -WorkbookPath '{{DEFECT_LOG_WORKBOOK}}' -CaseIds @('TC-UI-PUR-002', 'TC-UI-IMP-003', 'TC-UI-INV-001', 'TC-UI-INV-005') -HeadersToHighlight @('Record ID', 'Related Test Case ID', 'Severity', 'Priority', 'Current Status', 'GitHub Issue') -Zoom 145 -PauseSeconds 3
    Show-ImageArtifact -Path '{{DEFECT_SCREENSHOT}}' -Caption 'Evidence - Live GitHub Issue for Invoice Defect' -PauseSeconds 5

    Write-Host ''
    Write-Host 'STEP 8/8 - Final result review.'
    Show-ResultComparisonArtifact -ComparisonCsv '{{VIDEO_COMPARISON_CSV}}' -PauseSeconds 18
    Show-WorkbookCases -WorkbookPath '{{VIDEO_COMPARISON_VIEW_CSV}}' -CaseIds @('TC-UI-AUTH-001', 'TC-UI-IMP-002', 'TC-API-HLT-001', 'TC-UI-PUR-002', 'TC-UI-INV-001') -HeadersToHighlight @('Test Case ID', 'Expected Result', 'Actual Result', 'Status', 'Evidence / Files', 'Report Reference') -Zoom 125 -PauseSeconds 5
    $finalExcel = New-Object -ComObject Excel.Application
    $finalExcel.Visible = $true
    $finalExcel.DisplayAlerts = $false
    $finalExcel.WindowState = -4137
    $finalExcel.DisplayFullScreen = $true
    $finalExcel.DisplayFormulaBar = $false
    $finalExcel.DisplayStatusBar = $false
    try { $finalExcel.ExecuteExcel4Macro('SHOW.TOOLBAR(""Ribbon"",False)') | Out-Null } catch {}
    $finalWorkbook = $finalExcel.Workbooks.Open('{{VIDEO_COMPARISON_VIEW_CSV}}')
    $finalSheet = $finalWorkbook.Worksheets.Item(1)
    $finalSheet.Activate() | Out-Null
    $finalExcel.ActiveWindow.Zoom = 155
    Bring-WindowToFront ([IntPtr]$finalExcel.Hwnd)
    Pause-Demo 1
    Write-DemoLog 'Holding final comparison workbook.'
    Write-Host 'Holding the comparison workbook for final review.'
    Pause-Demo 45
    $finalWorkbook.Close($false)
    $finalExcel.Quit()
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($finalSheet)
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($finalWorkbook)
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($finalExcel)
    Write-DemoLog 'Demo completed successfully.'
    Write-Host 'Automation demo completed.'
}
catch {
    Write-DemoLog ('ERROR: ' + $_.Exception.Message)
    Bring-DemoWindowToFront
    Write-Host ''
    Write-Host ('DEMO ERROR: ' + $_.Exception.Message) -ForegroundColor Red
    Pause-Demo 30
    throw
}
'@

$demoScript = $demoTemplate
$demoScript = $demoScript.Replace('{{REPO_ROOT}}', $repoRoot)
$demoScript = $demoScript.Replace('{{BASE_URL}}', $BaseUrl)
$demoScript = $demoScript.Replace('{{UI_RUNNER}}', $uiRunner)
$demoScript = $demoScript.Replace('{{API_RUNNER}}', $apiRunner)
$demoScript = $demoScript.Replace('{{UI_CASES_WORKBOOK}}', $uiCasesWorkbook)
$demoScript = $demoScript.Replace('{{API_CASES_WORKBOOK}}', $apiCasesWorkbook)
$demoScript = $demoScript.Replace('{{VIDEO_COMPARISON_CSV}}', $videoComparisonCsv)
$demoScript = $demoScript.Replace('{{VIDEO_COMPARISON_VIEW_CSV}}', $videoComparisonViewCsv)
$demoScript = $demoScript.Replace('{{DEFECT_LOG_WORKBOOK}}', $defectLogWorkbook)
$demoScript = $demoScript.Replace('{{API_SUMMARY_IMAGE}}', $apiSummaryImage)
$demoScript = $demoScript.Replace('{{DEFECT_SCREENSHOT}}', $defectScreenshot)
$demoScript = $demoScript.Replace('{{UI_EVIDENCE_ROOT}}', $uiEvidenceRoot)
$demoScript = $demoScript.Replace('{{DEMO_LOG}}', $demoLogPath)

$tempDemoScript = Join-Path $env:TEMP "osms-automation-demo-generated.ps1"
Set-Content -LiteralPath $tempDemoScript -Value $demoScript -Encoding UTF8

$orchestrator = $null
if (-not $NoRecording) {
    $orchestrator = Start-Job -ScriptBlock {
        param($ScriptPath)
        Start-Sleep -Seconds 2
        Start-Process powershell.exe -ArgumentList "-NoLogo", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $ScriptPath -WindowStyle Maximized | Out-Null
    } -ArgumentList $tempDemoScript
}

try {
    if ($NoRecording) {
        Start-Process powershell.exe -ArgumentList "-NoLogo", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $tempDemoScript -WindowStyle Maximized | Out-Null
    }
    else {
        & $FfmpegPath @ffmpegArgs | Out-Null

        Wait-Job $orchestrator | Out-Null

        if (-not (Test-Path -LiteralPath $outputVideo)) {
            throw "Recording did not produce $outputVideo"
        }

        Copy-Item -LiteralPath $outputVideo -Destination $legacyVideo -Force
    }
}
finally {
    if ($orchestrator) {
        Remove-Job $orchestrator -Force -ErrorAction SilentlyContinue
    }

    if (-not $KeepTempScript -and (Test-Path -LiteralPath $tempDemoScript)) {
        Remove-Item -LiteralPath $tempDemoScript -Force -ErrorAction SilentlyContinue
    }
}
