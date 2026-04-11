# BUG-20260411-004 Manual GitHub Issue Checklist

## Completion status

- Issue created: `Yes`
- GitHub issue number: `#4`
- GitHub URL: `https://github.com/Alucard30Dec/Online-Sales-Management-System/issues/4`
- Current package status: internal confirmed defect only

## Recommended issue title

- `Invoice cancellation fails and does not return stock for unpaid invoice`

## Recommended labels

- `severity:high`
- `priority:high`
- `status:open`
- `module:invoices`
- `interface:web-ui`
- `type:defect`

## Required body sections

1. Environment
   - `Local test environment`
   - `http://127.0.0.1:5068`
   - `Google Chrome`
2. Preconditions
   - admin user is logged in
   - an unpaid or partially paid invoice exists
   - invoice detail page is reachable
3. Steps to reproduce
   - open an unpaid or partially paid invoice detail page
   - click `Há»§y Ä‘Æ¡n`
   - accept the confirmation dialog if shown
   - observe the toast and invoice state
4. Expected result
   - the invoice should become `Cancelled`
   - related stock should be returned and a stock movement entry should exist
5. Actual result
   - the toast shows `Failed to cancel invoice. Please try again.`
   - the invoice remains active
   - stock-return behavior does not occur

## Required attachments

- `TestResults/Evidence/UI/automation/20260411_075908_TC-UI-INV-005-failure.png`
- `TestResults/RunnerOutput/UI/reporting-public-invoice-coverage.trx`

## After opening the issue

1. Copy the issue URL into:
   - `TestResults/Defects/OSMS-Defect-Register.csv`
   - `TestResults/FinalResults/execution-evidence-mapping.csv`
2. Capture one screenshot of the issue page with labels visible.
3. Save the screenshot under:
   - `TestResults/Evidence/Defects/BUG-20260411-004-github-issue.png`

