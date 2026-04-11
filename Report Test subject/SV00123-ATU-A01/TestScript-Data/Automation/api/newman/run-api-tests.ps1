param(
    [string]$BaseUrl = "http://127.0.0.1:5068"
)

$ErrorActionPreference = "Stop"

$apiRoot = Split-Path -Parent $PSScriptRoot
$packageRoot = (Resolve-Path (Join-Path $apiRoot "..\..\..")).Path
$collection = Join-Path $apiRoot "postman\collections\OSMS-API-Automation.postman_collection.json"
$environment = Join-Path $apiRoot "postman\environments\OSMS-Local.postman_environment.json"
$runnerOutputRoot = Join-Path $packageRoot "TestResults\RunnerOutput\API"
$evidenceRoot = Join-Path $packageRoot "TestResults\Evidence\API"
$junitPath = Join-Path $runnerOutputRoot "newman-results.xml"
$fullRunPath = Join-Path $evidenceRoot "newman-full-run.txt"

New-Item -ItemType Directory -Path $runnerOutputRoot -Force | Out-Null
New-Item -ItemType Directory -Path $evidenceRoot -Force | Out-Null

& npx.cmd --yes newman run $collection `
    -e $environment `
    --env-var "baseUrl=$BaseUrl" `
    --reporters "cli,junit" `
    --reporter-junit-export $junitPath 2>&1 | Tee-Object -FilePath $fullRunPath

if ($LASTEXITCODE -is [int] -and $LASTEXITCODE -ne 0) {
    throw ("API automation exited with code " + $LASTEXITCODE)
}
