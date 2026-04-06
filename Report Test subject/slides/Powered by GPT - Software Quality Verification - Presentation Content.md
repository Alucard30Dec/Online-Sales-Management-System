# Powered by GPT - Software Quality Verification

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
- `59` total test cases
  - `40` UI
  - `19` API
- `90.48%` scenario design coverage

**Team Allocation**

- Hoang Van Thien: `15` UI cases
- Nguyen Thanh Dat: `12` UI cases
- Le Quang Duy: `13` UI cases

### Visual to show

- bar chart or simple table for case ownership
- small note:
  - `4` scenario gaps remain: `SCN-AUTH-003`, `SCN-GOV-003`, `SCN-INV-003`, `SCN-PUB-003`

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
- `TC-API-HLT-001` passed

**Current Observation Evidence**

- `TC-UI-AUTH-003` blocked by automation instability
- `TC-UI-IMP-002` blocked by automation instability

**Important Rule**

- no unexecuted test was marked as pass
- no observation was promoted into a confirmed defect

### Visual to show

- login success screenshot:
  - `evidence/ui/automation/20260405_115322_TC-UI-AUTH-001-success.png`
- Newman output snippet for health smoke:
  - `results/automation-api/newman-health-smoke.txt`

## Slide 7. Current Metrics

### Slide content

**Execution Summary As Of 2026-04-05**

- total cases: `59`
- executed: `4`
- pass: `2`
- blocked: `2`
- fail: `0`
- not run: `55`
- execution progress: `6.78%`

**Interface View**

- UI: `3` executed, `1` pass, `2` blocked
- API: `1` executed, `1` pass

### Visual to show

- one summary chart from `metrics/OSMS-Test-Metrics.xlsx`

## Slide 8. Defect And Risk Analysis

### Slide content

**Confirmed Defects**

- `0` at current evidence state

**Observations Under Triage**

- authorization / purchases navigation
- product import preview flow

**Highest Business Risks Still Unverified**

- Purchases
- Invoices
- Stock
- Reports
- Product Import

### Visual to show

- one risk heat-style table:
  - `Verified Baseline`
  - `Observed Instability`
  - `Unverified Critical Area`

## Slide 9. Key Insights

### Slide content

**What We Can Defend**

- project is testable and automation-capable
- source-based test design is strong
- traceability from scenario -> test case -> result -> evidence is in place

**What We Do Not Overclaim**

- current evidence does not prove full system stability
- current observations are not yet confirmed product defects
- more execution is still required for high-confidence quality claims

### Visual to show

- two-column slide:
  - `Proven Today`
  - `Still Pending`

## Slide 10. Next Steps And Submission Package

### Slide content

**Highest-Value Next Steps**

1. manually retest the two observations
2. execute purchase and invoice happy paths
3. run full catalog API regression
4. close the four scenario gaps
5. record automation video

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
  - `github.com/Alucard30Dec/Online-Sales-Management-System/tree/main/Report%20Test%20subject`

## Slide 11. Q&A Backup

### Slide content

**Likely Questions**

- Why do you have `0` confirmed defects?
- Why is execution progress still low?
- Why did you choose Selenium and Newman?
- How do you distinguish automation failure from product defect?
- Which module is riskiest right now?

### Visual to show

- plain clean backup slide only
