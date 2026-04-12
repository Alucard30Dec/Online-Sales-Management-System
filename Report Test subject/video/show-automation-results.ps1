param(
    [string]$BaseUrl = "http://127.0.0.1:5068"
)

$ErrorActionPreference = "Stop"

$repoRoot = "E:\Project\Online-Sales-Management-System"
$apiResultsFolder = Join-Path $repoRoot "Report Test subject\results\automation-api"
$uiCommand = "powershell -ExecutionPolicy Bypass -File `"Report Test subject/automation/ui/run-ui-tests.ps1`" -BaseUrl `"$BaseUrl`" -Filter `"FullyQualifiedName~AdminLoginSmokeSucceeds`""
$apiCommand = "powershell -ExecutionPolicy Bypass -File `"Report Test subject/automation/api/newman/run-api-tests.ps1`" -BaseUrl `"$BaseUrl`""

Set-Location $repoRoot
$Host.UI.RawUI.WindowTitle = "OSMS Result Review"

$latestUiEvidence = Get-ChildItem "Report Test subject\evidence\ui\automation" -Filter "*TC-UI-AUTH-001-success.png" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $latestUiEvidence) {
    throw "Could not find a generated UI screenshot evidence file for TC-UI-AUTH-001."
}

Write-Host "UI screenshot evidence:"
Write-Host ("Report Test subject/evidence/ui/automation/" + $latestUiEvidence.Name)
Write-Host ""
Get-Item $latestUiEvidence.FullName | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize

Start-Sleep -Seconds 4

Write-Host "API automation command:"
Write-Host $apiCommand
Write-Host ""

& powershell -ExecutionPolicy Bypass -File "Report Test subject/automation/api/newman/run-api-tests.ps1" -BaseUrl $BaseUrl

if ($LASTEXITCODE -is [int] -and $LASTEXITCODE -ne 0) {
    throw ("API automation exited with code " + $LASTEXITCODE)
}

Write-Host ""
Write-Host "Saved API result files:"
Get-ChildItem $apiResultsFolder | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize

Start-Sleep -Seconds 8
