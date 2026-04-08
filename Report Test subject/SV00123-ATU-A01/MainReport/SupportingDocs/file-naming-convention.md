# File Naming Convention

## Objective

Use short, stable, professional filenames so the examiner can identify each artifact immediately.

## General Rules

- Use `Title Case` for the main report and slide deck.
- Use `OSMS-` prefix for testing artifacts tied to the system under test.
- Use hyphens instead of spaces for workbook, JSON, and media artifacts.
- Keep one final canonical filename for each deliverable category.
- Do not commit temporary Office lock files such as `~$*.docx`.

## Canonical Filenames

| Artifact type | Canonical filename |
|---|---|
| Final report DOCX | `Powered by GPT - Software Quality Verification - Final Report.docx` |
| Final report PDF | `Powered by GPT - Software Quality Verification - Final Report.pdf` |
| Presentation | `Powered by GPT - Software Quality Verification - Presentation.pptx` |
| UI test cases | `OSMS-UI-Test-Cases.xlsx` |
| API test cases | `OSMS-API-Test-Cases.xlsx` |
| UI test data | `OSMS-UI-Test-Data.xlsx` |
| API test data | `OSMS-API-Test-Data.json` |
| Test accounts | `OSMS-Test-Accounts.md` |
| Defect log | `OSMS-Defect-Log.xlsx` |
| Final results | `OSMS-Final-Test-Results.xlsx` |
| Metrics | `OSMS-Test-Metrics.xlsx` |
| Automation video | `OSMS-Automation-Demo.mp4` |

## Canonical Package Folders

Use these paths in the clean submission package `SV00123-ATU-A01/`:

- `MainReport/`
- `TestCases/UI/`
- `TestCases/API/`
- `TestResults/FinalResults/`
- `TestResults/Metrics/`
- `TestResults/Defects/`
- `TestResults/Evidence/`
- `TestScript-Data/Automation/`
- `TestScript-Data/TestData/`
- `Videos/`

## Screenshot Naming

Use this pattern for screenshots:

`<TestCaseID>__<Module>__<StepOrOutcome>__<YYYYMMDD>.png`

Examples:

- `TC-UI-AUTH-001__Auth__InvalidLogin__20260405.png`
- `TC-API-CAT-004__CatalogApi__PriceValidation__20260405.png`
- `BUG-INV-001__Invoices__DraftCancelStockIssue__20260405.png`

## Automation File Naming

| Folder | Naming rule |
|---|---|
| `TestScript-Data/Automation/ui/` | keep framework-native project and page-object names such as `OSMS.UITests.csproj`, `AuthenticationTests.cs`, `LoginPage.cs` |
| `TestScript-Data/Automation/api/` | `OSMS-API-Automation.postman_collection.json`, `OSMS-Local.postman_environment.json`, `run-api-tests.ps1` |
| `TestScript-Data/Automation/` | reusable helpers, config, run notes, and shared utilities should stay in stable framework-specific filenames |

## Workbook Sheet Naming

- UI test cases: `AUTH`, `CUSTOMERS`, `PRODUCTS`, `PURCHASES`, `INVOICES`, `STOCK`, `REPORTS`, `PUBLIC-CATALOG`
- API test cases: `HEALTH`, `CATALOG-LIST`, `CATALOG-DETAIL`, `TRENDING`, `FILTERS`
- Results workbook should reuse the same module naming so cross-reference remains simple.

## Member Allocation Naming

Use member initials in ownership columns instead of changing filenames:

- `HVT` = Hoang Van Thien
- `NTD` = Nguyen Thanh Dat
- `LQD` = Le Quang Duy

This prevents duplicated workbooks while still proving ownership and non-overlap.
