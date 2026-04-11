param(
    [string]$BaseUrl = "http://127.0.0.1:5068",
    [string]$Browser = "chrome",
    [switch]$Headless,
    [switch]$Fullscreen,
    [int]$DemoPauseSeconds = 0,
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"

$uiRoot = $PSScriptRoot
$packageRoot = (Resolve-Path (Join-Path $uiRoot "..\..\..")).Path
$project = Join-Path $uiRoot "OSMS.UITests\OSMS.UITests.csproj"
$resultsRoot = Join-Path $packageRoot "TestResults\RunnerOutput\UI"
$screenshotsDirectory = "Report Test subject/SV00123-ATU-A01/TestResults/Evidence/UI/automation"

New-Item -ItemType Directory -Path $resultsRoot -Force | Out-Null

$env:OSMS_UI_BASE_URL = $BaseUrl
$env:OSMS_UI_BROWSER = $Browser
$env:OSMS_UI_HEADLESS = if ($Headless.IsPresent) { "true" } else { "false" }
$env:OSMS_UI_FULLSCREEN = if ($Fullscreen.IsPresent) { "true" } else { "false" }
$env:OSMS_UI_DEMO_PAUSE_SECONDS = [string][Math]::Max(0, $DemoPauseSeconds)
$env:OSMS_UI_SCREENSHOTS_DIR = $screenshotsDirectory

$arguments = @(
    "test",
    $project,
    "--results-directory",
    $resultsRoot,
    "--logger",
    "trx;LogFileName=ui-tests.trx"
)

if (-not [string]::IsNullOrWhiteSpace($Filter)) {
    $arguments += @("--filter", $Filter)
}

& dotnet @arguments

if ($LASTEXITCODE -is [int] -and $LASTEXITCODE -ne 0) {
    throw ("UI automation exited with code " + $LASTEXITCODE)
}
