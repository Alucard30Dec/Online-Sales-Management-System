# Phase 10 Defect Log And Bug Management

## Objective

Define a professional defect-management process for the current OSMS submission and prepare a traceable defect register that stays fully aligned with real evidence collected so far.

## Current defect state as of 2026-04-05

- Confirmed product defects: `0`
- Pending observations from automation execution: `2`
- Rejected or duplicate defects: `0`

At this point, the repository does not contain enough real evidence to open a confirmed product defect. The two existing failure records from Phase 9 remain observations because both failures originated in the automation layer and have not yet been reproduced manually as application bugs.

## Real record classification used in this project

- `Confirmed Defect`
  - reproducible application behavior
  - expected result is clear
  - actual result is proven by screenshots, logs, or API evidence
- `Observation`
  - unusual execution result exists
  - product bug is not confirmed yet
  - needs manual retest or environment validation
- `Rejected`
  - not a bug, expected behavior, duplicate, or test-script issue

## Required defect fields

Every confirmed issue in GitHub Issues and every exported defect row must contain:

- defect ID or GitHub issue number
- title
- defect type
- module
- related test case ID
- interface (`UI` or `API`)
- environment
- browser or runner
- build or execution date
- severity
- priority
- status
- assigned owner
- precondition
- steps to reproduce
- expected result
- actual result
- attachment list
- root-cause hint or notes

## Environment format to use

Use this exact environment structure in every defect report:

- Application: `Online Sales Management System`
- Repo: `Alucard30Dec/Online-Sales-Management-System`
- Environment: `Local test environment`
- URL: `http://localhost:5068`
- OS: `Windows 11`
- Browser: `Chrome`
- Database tag: `TiDB / test`
- Execution date: actual run date such as `2026-04-05`
- Evidence source: `Manual`, `Selenium xUnit`, or `Postman Newman`

## Severity definition

- `severity:critical`
  - system unavailable, data corruption, or a core business flow cannot continue
- `severity:high`
  - major business flow fails, permission leak exists, or financial or stock data becomes unreliable
- `severity:medium`
  - important function behaves incorrectly but a workaround may exist
- `severity:low`
  - minor incorrect behavior, cosmetic problem, or non-blocking validation issue

## Priority definition

- `priority:p1`
  - fix immediately before demo or submission rerun
- `priority:p2`
  - fix in the current test cycle
- `priority:p3`
  - fix after critical flows are stable
- `priority:p4`
  - optional or cosmetic backlog item

## Severity and priority guidance for this project

- Auth bypass or incorrect permission access:
  - expected label baseline: `severity:high`, `priority:p1`
- Purchase or invoice creation creates wrong totals, wrong stock movement, or cannot submit:
  - expected label baseline: `severity:critical` or `severity:high`, `priority:p1`
- Product import accepts invalid rows silently:
  - expected label baseline: `severity:high`, `priority:p1` or `priority:p2`
- Catalog API validation returns wrong status or malformed body:
  - expected label baseline: `severity:medium`, `priority:p2`
- UI alignment or non-blocking label issue:
  - expected label baseline: `severity:low`, `priority:p4`

## GitHub Issues workflow

1. Execute or re-execute the related test case.
2. Decide whether the outcome is `Confirmed Defect`, `Observation`, or `Automation Script Failure`.
3. If the issue is not yet reproducible manually, keep it in the defect register as `Observation`.
4. If the issue is reproducible manually or through stable API evidence, open a GitHub Issue.
5. Apply label groups for severity, priority, status, module, and interface.
6. Attach screenshots, runner output, and testcase linkage.
7. Update `results/execution-evidence-mapping.csv` with the GitHub issue link.
8. When fixed, re-run the original testcase and add retest evidence before closing the issue.

## GitHub status workflow

- `status:new`
- `status:triaged`
- `status:in-progress`
- `status:ready-for-retest`
- `status:closed`
- `status:rejected`

Recommended movement:

- new evidence -> `status:new`
- validated by QA lead -> `status:triaged`
- assigned to developer -> `status:in-progress`
- developer reports fix -> `status:ready-for-retest`
- QA retest passes -> `status:closed`
- not reproducible / expected behavior / script problem -> `status:rejected`

## Attachments required for each confirmed defect

- one issue screenshot from GitHub Issues showing labels and title
- one screenshot showing the page or request context before the failure if needed
- one screenshot showing the incorrect result
- related runner output file if automation detected the problem
- related API response snippet if the issue is API-related

## Exact screenshots that must appear in the final report if defects exist

- GitHub Issue page screenshot with:
  - issue number
  - title
  - severity label
  - priority label
  - status label
  - module label
- failure-state screenshot from the application or API client
- retest screenshot after fix if the issue is closed before submission

## Current observations that are not defects yet

### OBS-20260405-001

- related testcase: `TC-UI-AUTH-003`
- module: `Authorization / Purchases`
- classification: `Observation`
- current status: `Pending Manual Confirmation`
- reason:
  - Selenium timed out while waiting for the access-denied state
  - screenshot shows `sales@osms.local` on the dashboard
  - no confirmed product defect yet

### OBS-20260405-002

- related testcase: `TC-UI-IMP-002`
- module: `Products Import`
- classification: `Observation`
- current status: `Pending Manual Confirmation`
- reason:
  - Selenium timed out before preview interaction completed
  - screenshot shows the workbook already selected on the import page
  - no confirmed product defect yet

## Immediate bug-management actions

1. Retest `TC-UI-AUTH-003` manually and capture the exact post-navigation behavior.
2. Retest `TC-UI-IMP-002` manually and capture whether the preview screen loads with valid and invalid counts.
3. Open GitHub Issues only if one of the above becomes reproducible as an application bug.
4. Export issue screenshots into `evidence/defects/` after opening confirmed issues.
5. Keep `defects/exports/OSMS-Defect-Register.csv` updated after every triage decision.
