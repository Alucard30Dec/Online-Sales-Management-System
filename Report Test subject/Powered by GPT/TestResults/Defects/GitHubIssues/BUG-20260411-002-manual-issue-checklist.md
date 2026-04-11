# BUG-20260411-002 Manual GitHub Issue Checklist

## Completion status

- Issue created: `Yes`
- GitHub issue number: `#2`
- GitHub URL: `https://github.com/Alucard30Dec/Online-Sales-Management-System/issues/2`
- Current package status: internal confirmed defect only

## Recommended issue title

- `Purchase create validation banner renders without readable text for missing supplier or items`

## Recommended labels

- `severity:medium`
- `priority:medium`
- `status:open`
- `module:purchases`
- `interface:web-ui`
- `type:defect`

## Required body sections

1. Environment
   - `Local test environment`
   - `http://127.0.0.1:5068`
   - `Google Chrome`
2. Preconditions
   - admin user is logged in
   - purchase create page is reachable
   - seeded supplier and product data are available
3. Steps to reproduce
   - open `/Admin/Purchases/Create`
   - submit with product lines but no supplier
   - repeat with supplier selected but no valid items
   - observe the red validation banner
4. Expected result
   - the form should remain on the page
   - the validation banner should display readable validation text explaining the missing supplier or missing item requirement
5. Actual result
   - the form remains blocked
   - the red validation banner renders without readable validation text

## Required attachments

- `TestResults/Evidence/UI/automation/20260410_113408_TC-UI-PUR-002-failure.png`
- `TestResults/Evidence/UI/automation/20260410_113214_TC-UI-PUR-003-failure.png`
- `TestResults/RunnerOutput/UI/purchase-product-import-coverage-rerun.trx`

## After opening the issue

1. Copy the issue URL into:
   - `TestResults/Defects/OSMS-Defect-Register.csv`
   - `TestResults/FinalResults/execution-evidence-mapping.csv`
2. Capture one screenshot of the issue page with labels visible.
3. Save the screenshot under:
   - `TestResults/Evidence/Defects/BUG-20260411-002-github-issue.png`

