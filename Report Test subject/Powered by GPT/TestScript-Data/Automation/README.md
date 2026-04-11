# Automation Workspace

This folder contains the runnable automation assets included in the clean submission package.

## Implemented stacks

- UI automation: `.NET 8 + xUnit + Selenium WebDriver`
- API automation: `Postman Collection + Newman`

## Canonical package paths

- UI runner: `TestScript-Data/Automation/ui/run-ui-tests.ps1`
- API runner: `TestScript-Data/Automation/api/newman/run-api-tests.ps1`
- UI runner output: `TestResults/RunnerOutput/UI/`
- API runner output: `TestResults/RunnerOutput/API/`
- UI screenshots: `TestResults/Evidence/UI/automation/`
- API text evidence: `TestResults/Evidence/API/newman-full-run.txt`

## Current evidence scope

### UI automation baseline

The synchronized package contains executed UI evidence for all `44` UI test cases. Coverage is organized across focused xUnit test classes and rerun batches such as:

- `AuthenticationTests`
- `PurchaseFlowTests`
- `InvoiceFlowTests`
- `ProductImportTests`
- `AdminAndCustomerCoverageTests`
- `PurchaseProductImportCoverageTests`
- `ReportingPublicInvoiceCoverageTests`
- `ExtendedCoverageTests`
- `AdditionalCrudCoverageTests`

### API automation baseline

The Newman collection covers all `19` API cases in the final package:

- `TC-API-HLT-001`
- `TC-API-CAT-001` to `TC-API-CAT-018`

## Quick commands

### UI automation

```powershell
pwsh "Report Test subject/Powered by GPT/TestScript-Data/Automation/ui/run-ui-tests.ps1"
```

### API automation

```powershell
pwsh "Report Test subject/Powered by GPT/TestScript-Data/Automation/api/newman/run-api-tests.ps1"
```

## Submission note

For grading, treat `TestResults/RunnerOutput/` and `TestResults/Evidence/` as the canonical proof locations. The project-local build folders under the test project are not part of the submission-facing evidence set.
