# Presentation Source Content

## Canonical Binary

- `../Powered by GPT - Software Quality Verification - Presentation.pptx`

## Deck Goal

This deck is the final defense-oriented presentation for the Online Sales Management System Software Testing submission. It is intentionally concise, evidence-first, and synchronized with the current execution baseline:

- `63` total test cases
- `63` executed
- `56` pass
- `7` fail
- `4` confirmed defects
- `4` live GitHub issues

## Slide 1. Title

### Slide content

**Online Sales Management System**

- Software Testing Final Submission
- Team:
  - Hoang Van Thien
  - Nguyen Thanh Dat
  - Le Quang Duy
- Repository:
  - `github.com/Alucard30Dec/Online-Sales-Management-System`

### Visual to show

- cover slide with execution baseline cards:
  - `63 cases`
  - `100% executed`
  - `4 live defects`

## Slide 2. Risk-Based Scope

### Slide content

**System Surfaces**

- Admin UI
- Public Catalog UI
- Health API
- Catalog API

**Highest-Risk Modules**

- Authentication and permissions
- Products, import, and stock
- Purchases and invoices
- Reports and public catalog

### Visual to show

- four scope cards plus one short risk callout block

## Slide 3. Strategy And Evidence Model

### Slide content

**Approach**

- source-based audit instead of generic template testing
- black-box execution with white-box-informed edge-case design
- manual UI validation plus API regression plus targeted UI automation

**Evidence Chain**

- scenario -> testcase -> result -> screenshot or runner output -> defect log -> GitHub issue

### Visual to show

- left-to-right strategy pipeline with `Design`, `Execution`, `Evidence`, `Defects`

## Slide 4. Coverage Snapshot

### Slide content

**Coverage Baseline**

- `42` scenarios
- `63` test cases
- `44` UI and `19` API
- `100%` scenario-to-testcase mapping

**Team Allocation**

- Hoang Van Thien: `17` UI cases
- Nguyen Thanh Dat: `13` UI cases
- Le Quang Duy: `14` UI cases

### Visual to show

- coverage cards and owner allocation bars

## Slide 5. Automation Implementation

### Slide content

**UI Automation**

- `.NET 8`, `xUnit`, `Selenium`
- `Page Object Model`
- shared settings, waits, and screenshot helper

**API Automation**

- Postman collection and Newman runner
- text and XML artifacts stored in package

**Demonstrated Flows**

- `TC-UI-AUTH-001` on Edge
- `TC-UI-IMP-002` on Chrome
- `TC-API-HLT-001`
- focused fail-case comparison

### Visual to show

- architecture cards for `UI Suite`, `API Suite`, `Evidence`, `Results`

## Slide 6. Real Execution Evidence

### Slide content

**Confirmed Pass Evidence**

- `TC-UI-AUTH-001` login smoke passed
- `TC-UI-IMP-002` import preview passed
- API collection passed `19 / 19`

**Confirmed Fail Evidence**

- `TC-UI-PUR-002` purchase validation banner defect
- `TC-UI-IMP-003` import confirm defect
- `TC-UI-INV-001` and `TC-UI-INV-005` invoice defects

**Execution Rule**

- `63 / 63` cases are executed
- `0` test case remains `Not Run`
- no testcase was promoted to `Pass` without runtime evidence

### Visual to show

- auth success screenshot:
  - `Powered by GPT/TestResults/Evidence/UI/automation/20260406_053930_TC-UI-AUTH-001-success.png`
- import preview screenshot:
  - `Powered by GPT/TestResults/Evidence/UI/automation/20260406_054115_TC-UI-IMP-002-preview.png`
- Newman summary image:
  - `Powered by GPT/TestResults/Evidence/Report/OSMS-Newman-Full-Run-Snippet.png`

## Slide 7. Result Comparison

### Slide content

**Expected vs Actual vs Status**

- `TC-UI-AUTH-001`: valid admin login reaches dashboard -> `Pass`
- `TC-UI-IMP-002`: preview shows `6 total`, `1 valid`, `5 invalid` -> `Pass`
- `TC-UI-PUR-002`: supplier-missing validation should show readable message -> `Fail`
- `TC-UI-INV-001`: valid walk-in invoice should be created successfully -> `Fail`

**Traceability**

- the video, result workbook, and evidence mapping use the same final baseline

### Visual to show

- card-based comparison layout, not a text wall

## Slide 8. Metrics Snapshot

### Slide content

**Execution Summary As Of 2026-04-11**

- total: `63`
- pass: `56`
- fail: `7`
- not run: `0`

**Interface View**

- Admin UI: `41 executed`, `34 pass`, `7 fail`
- API: `19 executed`, `19 pass`
- Public UI: `3 executed`, `3 pass`

### Visual to show

- `Powered by GPT/TestResults/Evidence/Report/OSMS-Test-Metrics-Summary.png`

## Slide 9. Defect Management

### Slide content

**Confirmed Defects**

- `BUG-20260406-001` invoice create -> GitHub Issue `#1`
- `BUG-20260411-002` purchase validation -> GitHub Issue `#2`
- `BUG-20260411-003` import confirm -> GitHub Issue `#3`
- `BUG-20260411-004` invoice cancel -> GitHub Issue `#4`

**Current State**

- all four defects were rerun on `2026-04-11`
- all four defects still reproduced
- live GitHub issue tracking is already in place

### Visual to show

- representative issue screenshot:
  - `Powered by GPT/TestResults/Evidence/Defects/BUG-20260406-001-github-issue.png`

## Slide 10. Final Assessment And Next Steps

### Slide content

**What Is Proven Today**

- strong traceability from design to issue tracking
- automation supports both pass and fail evidence
- submission package is complete and reviewable

**What Remains Open**

- `4` product defects are still open
- post-fix retest still depends on future code fixes
- current cross-browser proof is limited to the `Edge` smoke run

**Next Actions**

1. fix invoice create and invoice cancel
2. fix purchase validation and import confirm
3. rerun failed cases and convert them to post-fix evidence

### Visual to show

- two-column `Proven Today` vs `Open After Execution` layout

## Slide 11. Q&A Backup

### Slide content

**Likely Questions**

- Why do `7` failing test cases map to `4` defects?
- How do you distinguish automation failure from product defect?
- Why does `100%` execution not mean a stable system?
- Which module is riskiest right now?
- What is the first post-fix retest priority?

### Visual to show

- clean Q&A backup slide only
