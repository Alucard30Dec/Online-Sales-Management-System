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

## High-value current evidence

- Basic cross-browser smoke evidence exists for `TC-UI-AUTH-001` on `Edge`:
  - `TestResults/Evidence/UI/automation/20260411_143741_TC-UI-AUTH-001-success.png`
  - `TestResults/RunnerOutput/UI/edge-auth-smoke.trx`
- Focused defect retest evidence from `2026-04-11` exists for the still-open UI defects:
  - `TC-UI-PUR-002`
  - `TC-UI-IMP-003`
  - `TC-UI-INV-001`
  - `TC-UI-INV-005`
- The automation demo video is regenerated from the package script:
  - `Videos/record-automation-demo.ps1`
  - `Videos/OSMS-Automation-Demo.mp4`

## Quick commands

### UI automation

```powershell
pwsh "Report Test subject/SV00123-ATU-A01/TestScript-Data/Automation/ui/run-ui-tests.ps1"
```

### API automation

```powershell
pwsh "Report Test subject/SV00123-ATU-A01/TestScript-Data/Automation/api/newman/run-api-tests.ps1"
```

## Submission note

For grading, treat `TestResults/RunnerOutput/` and `TestResults/Evidence/` as the canonical proof locations. The project-local build folders under the test project are not part of the submission-facing evidence set.
