# API Automation

Framework: `Postman Collection + Newman`

## Implemented artifacts

- collection: `postman/collections/OSMS-API-Automation.postman_collection.json`
- environment: `postman/environments/OSMS-Local.postman_environment.json`
- runner: `newman/run-api-tests.ps1`

## Current coverage

- `TC-API-HLT-001`
- `TC-API-CAT-001` to `TC-API-CAT-018`

The current synchronized baseline records all `19` API cases as executed with the canonical evidence stored under:

- `SV00123-ATU-A01/TestResults/Evidence/API/newman-full-run.txt`
- `SV00123-ATU-A01/TestResults/RunnerOutput/API/newman-results.xml`
- `SV00123-ATU-A01/TestResults/Evidence/Report/OSMS-Newman-Full-Run-Snippet.png`

## Run

```powershell
pwsh "Report Test subject/SV00123-ATU-A01/TestScript-Data/Automation/api/newman/run-api-tests.ps1"
```

To override the base URL:

```powershell
pwsh "Report Test subject/SV00123-ATU-A01/TestScript-Data/Automation/api/newman/run-api-tests.ps1" -BaseUrl "http://127.0.0.1:5068"
```

## Video alignment note

The submission video maps the visible API automation step to `TC-API-HLT-001`, then shows the Newman summary artifact and the final comparison workbook that links the API run back to the synchronized final results.

## Preconditions

- the OSMS application is running locally
- `npx` is available
- internet access is available the first time `newman` is resolved through `npx`
