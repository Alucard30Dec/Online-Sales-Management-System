# Phase 9 Execution And Evidence

## Objective

Prepare a real, traceable execution-and-evidence package for the current OSMS submission without fabricating pass results, bug reports, screenshots, or automation claims.

## Evidence basis used in this phase

### Real execution completed across 2026-04-06, 2026-04-10, and 2026-04-11

- Focused UI reruns executed from `Report Test subject/SV00123-ATU-A01/TestScript-Data/Automation/ui/run-ui-tests.ps1`
  - result files:
    - `TestResults/RunnerOutput/UI/auth-permission-rerun.trx`
    - `TestResults/RunnerOutput/UI/import-preview-rerun.trx`
    - `TestResults/RunnerOutput/UI/purchase-rerun.trx`
    - `TestResults/RunnerOutput/UI/invoice-rerun.trx`
  - covered outcomes:
    - `TC-UI-AUTH-001` passed
    - `TC-UI-AUTH-003` passed
    - `TC-UI-IMP-002` passed
    - `TC-UI-PUR-001` passed
    - `TC-UI-PUR-007` passed
    - `TC-UI-INV-001` failed and became a confirmed defect
- Full API automation run executed with Newman
  - scope: complete Postman collection
  - result: all `19` API test cases passed
  - result files:
    - `TestResults/Evidence/API/newman-full-run.txt`
    - `TestResults/RunnerOutput/API/newman-results.xml`
- Expanded UI coverage batches executed on `2026-04-10` and `2026-04-11`
  - scope: remaining authentication, admin, customer, supplier, product, import, purchase, invoice, stock, report, and public-catalog cases
  - result: UI baseline is now fully executed with `37` pass and `7` fail
  - representative result files:
    - `TestResults/RunnerOutput/UI/admin-customer-coverage.trx`
    - `TestResults/RunnerOutput/UI/purchase-product-import-coverage-rerun.trx`
    - `TestResults/RunnerOutput/UI/reporting-public-invoice-coverage.trx`
    - `TestResults/RunnerOutput/UI/extended-coverage-rerun.trx`

### Real screenshots currently available

- `TestResults/Evidence/UI/automation/20260406_053930_TC-UI-AUTH-001-success.png`
- `TestResults/Evidence/UI/automation/20260406_054245_TC-UI-AUTH-003-access-denied.png`
- `TestResults/Evidence/UI/automation/20260406_054115_TC-UI-IMP-002-preview.png`
- `TestResults/Evidence/UI/automation/20260406_054004_TC-UI-PUR-001-draft-created.png`
- `TestResults/Evidence/UI/automation/20260406_053902_TC-UI-INV-001-failure.png`

### Evidence interpretation rules applied

- `Pass` is used only when a real execution result exists and the expected behavior is visibly confirmed.
- `Fail` must be reserved for a confirmed product defect with reproducible expected-versus-actual mismatch.
- `Automation Script Failure` is used when the runner failed or timed out, but the product defect is not yet confirmed.
- `Not Run` remains a generic status in the status vocabulary, but it is no longer present in the synchronized final baseline.

## Current confirmed execution status

### Confirmed pass evidence

- `TC-UI-AUTH-001`
  - real screenshot shows successful login to the admin dashboard as `admin@osms.local`
  - focused UI rerun passed
- `TC-UI-AUTH-003`
  - direct navigation to `/Admin/Purchases` by `sales@osms.local` was rejected by redirect away from the protected route
  - server log also showed `GET /Admin/Purchases` returned `302`
- `TC-UI-IMP-002`
  - mixed-validation workbook preview opened successfully and displayed `6` total rows, `1` valid row, and `5` invalid rows
- `TC-UI-PUR-001` and `TC-UI-PUR-007`
  - a valid draft purchase was created successfully and the details page assertions passed
- all API cases `TC-API-HLT-001` to `TC-API-CAT-018`
  - full Newman collection run passed with saved text and JUnit artifacts
- expanded UI batches for admin users, customers, products, purchases, reports, stock, and public catalog
  - all corresponding executed rows are synchronized in `TestResults/FinalResults/OSMS-Final-Test-Results.xlsx`

### Confirmed product defect evidence

- `TC-UI-INV-001`
  - valid walk-in invoice creation failed in the current environment
  - UI stayed on the Create page and showed `Failed to create invoice. Please check data and try again.`
  - focused rerun TRX and extracted server log prove `BUG-20260406-001`
- `TC-UI-PUR-002` and `TC-UI-PUR-003`
  - both failed because the purchase create banner rendered without readable validation text
  - synchronized under `BUG-20260411-002`
- `TC-UI-IMP-003`
  - import confirm failed after a valid preview and is synchronized under `BUG-20260411-003`
- `TC-UI-INV-005`
  - invoice cancellation failed and is synchronized under `BUG-20260411-004`

## Execution checklist

- Confirm application base URL is reachable before every run.
- Record execution date, tester, browser, OS, environment, and test scope.
- Use the seeded accounts from `test-data/accounts/OSMS-Test-Accounts.md`.
- Keep screenshots in timestamped subfolders or timestamped filenames.
- Export runner outputs after every execution batch.
- Update `TestResults/FinalResults/execution-evidence-mapping.csv` immediately after each run.
- If a test fails, decide first whether it is:
  - a confirmed product defect
  - a blocked environment issue
  - an automation script failure
- Only create a defect entry after reproducing the issue with a stable expected-versus-actual mismatch.

## Screenshot checklist

### Mandatory screenshots for the final submission

- successful admin login landing page
- successful API smoke runner summary
- each confirmed product defect:
  - one screenshot before action if context matters
  - one screenshot showing the triggering action or input
  - one screenshot showing the actual incorrect result
- one screenshot of any permission-denied behavior if that behavior is confirmed manually
- purchase creation success detail page
- invoice creation failure state for the currently confirmed defect, plus post-fix success evidence if the issue is resolved later
- product import preview page showing valid and invalid row counts
- one screenshot showing the final results workbook or metrics summary
- one screenshot showing the issue tracker entry for each confirmed defect

### Screenshots that already exist

- login success screenshot for `TC-UI-AUTH-001`
- permission-denial screenshot for `TC-UI-AUTH-003`
- import preview screenshot for `TC-UI-IMP-002`
- purchase details screenshot for `TC-UI-PUR-001` and `TC-UI-PUR-007`
- invoice failure screenshot for `TC-UI-INV-001`
- GitHub Issue screenshot for `BUG-20260406-001`
- metrics summary screenshot for the final report and slide deck

### Screenshots still missing

- No critical screenshot gap remains for the current executed baseline.
- The remaining missing evidence is post-fix pass proof for the four confirmed defects. Basic cross-browser smoke evidence now exists through the `Edge` rerun of `TC-UI-AUTH-001`.

## Bug evidence checklist

- Reproduce the issue with stable steps and environment details.
- Capture exact test case ID and module.
- Capture expected result and actual result in separate sentences.
- Record severity and priority with business impact rationale.
- Attach screenshots that show context and failure outcome.
- If the issue is API-related, attach request, response status, and response body snippet.
- If the issue is UI-related, attach browser, URL, account role, and visible page state.
- If the failure is from automation only, mark it as `Automation Script Failure` and do not open a product defect yet.

## Video evidence checklist

- Show the repository path and the automation folder used.
- Show the command used to run UI automation.
- Show the browser executing at least one stable automated UI flow.
- Show the final runner result summary.
- Show where screenshots and result files are stored after the run.
- Show the Newman API run or collection runner summary.
- Export final video as `mp4` or `avi`.

## Result capture mapping rule

The file `TestResults/FinalResults/execution-evidence-mapping.csv` is the source-of-truth traceability sheet for:

- `Test Case ID`
- current execution result
- evidence screenshot path
- result artifact path
- video status
- bug or defect linkage

Use these status values consistently:

- `Pass`
- `Fail`
- `Blocked`
- `Not Run`
- `Automation Script Failure`

## Immediate next evidence priority

1. Fix and retest the four confirmed defects, then capture post-fix success evidence. The latest focused reruns already captured re-failure evidence on `2026-04-11`.
2. Keep the four live GitHub issue URLs synchronized with the defect register, execution-evidence mapping, and local issue screenshots.
3. Keep the metrics summary screenshot, GitHub issue evidence, and automation video linked consistently across the report, slides, and appendix.

## Known limitations after this phase

- UI execution depth is no longer partial; all designed UI and API cases are now executed in the synchronized baseline.
- Full business confidence still depends on retesting the four confirmed defects after fixes.
- Cross-browser evidence is still not available.
