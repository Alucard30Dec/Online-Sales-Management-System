# BUG-20260411-003 Manual GitHub Issue Checklist

## Completion status

- Issue created: `Yes`
- GitHub issue number: `#3`
- GitHub URL: `https://github.com/Alucard30Dec/Online-Sales-Management-System/issues/3`
- Current package status: internal confirmed defect only

## Recommended issue title

- `Product import confirm fails after a valid preview row is accepted`

## Recommended labels

- `severity:high`
- `priority:high`
- `status:open`
- `module:product-import`
- `interface:web-ui`
- `type:defect`

## Required body sections

1. Environment
   - `Local test environment`
   - `http://127.0.0.1:5068`
   - `Google Chrome`
2. Preconditions
   - admin user is logged in
   - import page is reachable
   - mixed-validation workbook previews successfully with one valid row
3. Steps to reproduce
   - open `/Admin/Products/ImportExcel`
   - upload the mixed-validation workbook
   - verify the preview opens successfully
   - click the confirm import action
   - observe the page and toast
4. Expected result
   - the valid preview row should be imported successfully
   - the product list should show the imported or updated product
5. Actual result
   - the page returns to import with the toast `Import tháº¥t báº¡i. Vui lÃ²ng thá»­ láº¡i.`
   - the expected product is not imported

## Required attachments

- `TestResults/Evidence/UI/automation/20260410_113328_TC-UI-IMP-003-failure.png`
- `TestResults/RunnerOutput/UI/purchase-product-import-coverage-rerun.trx`

## After opening the issue

1. Copy the issue URL into:
   - `TestResults/Defects/OSMS-Defect-Register.csv`
   - `TestResults/FinalResults/execution-evidence-mapping.csv`
2. Capture one screenshot of the issue page with labels visible.
3. Save the screenshot under:
   - `TestResults/Evidence/Defects/BUG-20260411-003-github-issue.png`

