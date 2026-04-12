param(
    [string]$BaseUrl = "http://127.0.0.1:5068"
)

$ErrorActionPreference = "Stop"

$apiRoot = Split-Path -Parent $PSScriptRoot
$collection = Join-Path $apiRoot "postman\collections\OSMS-API-Automation.postman_collection.json"
$environment = Join-Path $apiRoot "postman\environments\OSMS-Local.postman_environment.json"
$resultsRoot = Join-Path (Resolve-Path (Join-Path $apiRoot "..\..\results")).Path "automation-api"
$junitPath = Join-Path $resultsRoot "newman-results.xml"
$fullRunPath = Join-Path $resultsRoot "newman-full-run.txt"

New-Item -ItemType Directory -Path $resultsRoot -Force | Out-Null

$npxCmd = (Get-Command "npx.cmd" -ErrorAction Stop).Source
$output = & $npxCmd newman run $collection `
    -e $environment `
    --env-var "baseUrl=$BaseUrl" `
    --color off `
    --disable-unicode `
    --reporters "cli,junit" `
    --reporter-junit-export $junitPath 2>&1 | ForEach-Object { $_.ToString() }
$exitCode = $LASTEXITCODE

$output | Tee-Object -FilePath $fullRunPath

if ($exitCode -ne 0) {
    throw "Newman run failed with exit code $exitCode. See $fullRunPath"
}
