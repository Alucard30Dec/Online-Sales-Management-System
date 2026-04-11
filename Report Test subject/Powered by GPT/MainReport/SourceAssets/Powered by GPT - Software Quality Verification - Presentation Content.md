# Powered by GPT - Software Quality Verification

## Package Note

This Markdown file is the slide-source copy kept inside the clean submission package. The canonical grading deliverable is:

- `../Powered by GPT - Software Quality Verification - Presentation.pptx`

If any path below still uses the original workspace naming, resolve it through `../Submission-Index.md`.

## Presentation Goal

Use this file as the source content for the final `PPTX`. The deck is intentionally short and defense-oriented. It highlights the strongest evidence currently available and avoids overclaiming unexecuted coverage.

## Recommended Deck Size

- main deck: `10` slides
- optional backup: `1` Q&A slide
- target speaking time: `8-10 minutes`

## Slide 1. Title

### Slide content

**Powered by GPT - Software Quality Verification**

- System Under Test: `Online Sales Management System`
- Course: `Software Testing`
- Team:
  - Hoang Van Thien
  - Nguyen Thanh Dat
  - Le Quang Duy
- Repository:
  - `github.com/Alucard30Dec/Online-Sales-Management-System`

### Visual to show

- simple cover layout with team names and repository name

## Slide 2. Project And Test Scope

### Slide content

**What We Tested**

- Admin UI
- Public Product Catalog
- Health API
- Catalog API

**Highest-Risk Business Areas**

- Authentication and permissions
- Products and product import
- Purchases
- Invoices
- Stock and reports

### Visual to show

- one scope diagram or four-box layout for `Admin UI`, `Public UI`, `Health API`, `Catalog API`

## Slide 3. Test Strategy

### Slide content

**Strategy**

- source-based project audit, not generic template testing
- black-box execution + white-box-informed test design
- manual testing for business logic and visible validation
- targeted automation for high-value regression paths

**Automation Stack**

- UI: `.NET 8 + xUnit + Selenium`
- API: `Postman + Newman`
- structure: `Page Object Model + reusable helpers`

### Visual to show

- one small architecture row:
  - `Source Audit -> Test Design -> Manual + Automation -> Evidence -> Metrics`

## Slide 4. Coverage And Team Allocation

### Slide content

**Design Coverage**

- `42` documented scenarios
- `63` total test cases
  - `44` UI
  - `19` API
- `100%` scenario design coverage

**Team Allocation**

- Hoang Van Thien: `17` UI cases
- Nguyen Thanh Dat: `13` UI cases
- Le Quang Duy: `14` UI cases

### Visual to show

- bar chart or simple table for case ownership
- small note:
  - `42 / 42` scenarios are now mapped to test cases

## Slide 5. Automation Implementation

### Slide content

**Implemented Automation Assets**

- Selenium UI project with `Page Objects`
- shared config, wait helpers, screenshot capture
- Postman collection with Newman runner

**Priority Automated Flows**

- admin login smoke
- sales permission check
- purchase creation
- invoice creation
- product import preview
- API health and catalog groups

### Visual to show

- folder tree screenshot or simplified structure:
  - `Pages`
  - `Support`
  - `Tests`
  - `postman`
  - `newman`

## Slide 6. Real Execution Evidence

### Slide content

**Confirmed Pass Evidence**

- `TC-UI-AUTH-001` passed
- `TC-UI-AUTH-003` passed
- `TC-UI-IMP-002` passed
- `TC-UI-PUR-001` and `TC-UI-PUR-007` passed
- extended reruns now also pass admin-user, customer, report, stock, and public-catalog flows
- full API collection passed (`19 / 19`)

**Confirmed Fail Evidence**

- `TC-UI-INV-001`, `TC-UI-INV-003`, and `TC-UI-INV-006` fail from the same invoice-create defect
- `TC-UI-INV-005` fails in invoice cancellation
- `TC-UI-IMP-003` fails in import confirm
- `TC-UI-PUR-002` and `TC-UI-PUR-003` fail on blank validation banners

**Important Rule**

- no unexecuted test was marked as pass
- UI suite no longer contains `Not Run`
- `4` confirmed defects are recorded, and `1` of them is already tracked in GitHub Issue `#1`

### Visual to show

- login success screenshot:
  - `Powered by GPT/TestResults/Evidence/UI/automation/20260406_053930_TC-UI-AUTH-001-success.png`
- purchase success screenshot:
  - `Powered by GPT/TestResults/Evidence/UI/automation/20260406_054004_TC-UI-PUR-001-draft-created.png`
- invoice defect screenshot:
  - `Powered by GPT/TestResults/Evidence/UI/automation/20260406_053902_TC-UI-INV-001-failure.png`
- Newman output snippet for full collection:
  - `Powered by GPT/TestResults/Evidence/Report/OSMS-Newman-Full-Run-Snippet.png`

## Slide 7. Current Metrics

### Slide content

**Execution Summary As Of 2026-04-11**

- total cases: `63`
- executed: `63`
- pass: `56`
- fail: `7`
- blocked: `0`
- not run: `0`
- execution progress: `100%`

**Interface View**

- Admin UI: `41` executed, `34` pass, `7` fail
- API: `19` executed, `19` pass
- Public UI: `3` executed, `3` pass

### Visual to show

- `Powered by GPT/TestResults/Evidence/Report/OSMS-Test-Metrics-Summary.png`

## Slide 8. Defect And Risk Analysis

### Slide content

**Confirmed Defects**

- `4` confirmed defects
  - `BUG-20260406-001` invoice-create defect, tracked in GitHub Issue `#1`
  - `BUG-20260411-002` purchase validation banner defect, tracked in GitHub Issue `#2`
  - `BUG-20260411-003` import-confirm defect, tracked in GitHub Issue `#3`
  - `BUG-20260411-004` invoice-cancel defect, tracked in GitHub Issue `#4`

**Closed Observations**

- authorization redirect behavior
- product import preview locator issue

**Highest Remaining Risks**

- unresolved invoice create and cancel flows
- purchase validation rendering defect
- import-confirm defect after valid preview
- no post-fix retest evidence yet for the confirmed defects

### Visual to show

- one risk heat-style table:
  - `Verified`
  - `Confirmed Defect`
  - `Unverified`
- `Powered by GPT/TestResults/Evidence/Defects/BUG-20260406-001-github-issue.png`

## Slide 9. Key Insights

### Slide content

**What We Can Defend**

- project is testable and automation-capable
- source-based test design is strong
- traceability from scenario -> test case -> result -> evidence -> defect is in place

**What We Do Not Overclaim**

- current evidence still does not prove full system stability
- four confirmed defects are currently open
- post-fix retest is still required for maximum confidence

### Visual to show

- two-column slide:
  - `Proven Today`
  - `Open Defects`

## Slide 10. Next Steps And Submission Package

### Slide content

**Highest-Value Next Steps**

1. fix and retest invoice creation through GitHub Issue `#1`
2. attach the saved package evidence to GitHub Issues `#2`, `#3`, and `#4`
3. add post-fix retest evidence for all confirmed defects
4. add optional cross-browser evidence if Edge is available

**Submission Package**

- Final report
- PPTX
- test cases
- test data
- automation scripts
- final results
- metrics
- evidence
- video

### Visual to show

- GitHub appendix link:
  - `github.com/Alucard30Dec/Online-Sales-Management-System/tree/main/Report%20Test%20subject/Powered%20by%20GPT`

## Slide 11. Q&A Backup

### Slide content

**Likely Questions**

- Why do you have `4` confirmed defects?
- Why is execution progress now `100%`?
- Why did you choose Selenium and Newman?
- How do you distinguish automation failure from product defect?
- Which module is riskiest right now?

### Visual to show

- plain clean backup slide only

