# Report Assembly Map

## Package Note

This file is the package copy of the report-assembly guide. The canonical grading deliverables are the generated binaries in `../`.

If any referenced artifact still uses the original workspace naming, resolve it through `../Submission-Index.md`.

## Objective

Use this file while converting the Markdown report source into the final Word and PDF deliverables.

## Canonical Submission Package

- clean package root:
  - `SV00123-ATU-A01/`
- final deliverables should be reviewed from:
  - `SV00123-ATU-A01/MainReport/`

## Target Final Files

- final Word file:
  - `SV00123-ATU-A01/MainReport/Powered by GPT - Software Quality Verification - Final Report.docx`
- final PDF file:
  - `SV00123-ATU-A01/MainReport/Powered by GPT - Software Quality Verification - Final Report.pdf`

## Section-To-Artifact Mapping

### Cover Page

- source text:
  - `SV00123-ATU-A01/MainReport/SourceAssets/Powered by GPT - Software Quality Verification - Final Report Content.md`
- no screenshot required

### Record Of Changes

- source text:
  - `SV00123-ATU-A01/MainReport/SourceAssets/Powered by GPT - Software Quality Verification - Final Report Content.md`

### I. Overview

- use text from the report content file
- optional supporting table:
  - `SV00123-ATU-A01/TestCases/UI/OSMS-UI-Test-Cases.xlsx`
  - summarize owner counts:
    - `Hoang Van Thien = 17`
    - `Nguyen Thanh Dat = 13`
    - `Le Quang Duy = 14`

### II. Test Plan

- source text:
  - `SV00123-ATU-A01/MainReport/SupportingDocs/test-plan.md`
- recommended table:
  - environment summary table
- no screenshot required

### III. Test Design And Execution

- scenario summary:
  - `SV00123-ATU-A01/TestCases/Scenarios/test-scenarios.md`
  - `SV00123-ATU-A01/TestResults/Metrics/OSMS-Scenario-Coverage.csv`
- UI cases:
  - `SV00123-ATU-A01/TestCases/UI/OSMS-UI-Test-Cases.xlsx`
- API cases:
  - `SV00123-ATU-A01/TestCases/API/OSMS-API-Test-Cases.xlsx`
- test data:
  - `SV00123-ATU-A01/TestScript-Data/TestData/accounts/OSMS-Test-Accounts.md`
  - `SV00123-ATU-A01/TestScript-Data/TestData/ui/OSMS-UI-Test-Data.xlsx`
  - `SV00123-ATU-A01/TestScript-Data/TestData/api/OSMS-API-Test-Data.json`
- automation design:
  - `SV00123-ATU-A01/MainReport/SupportingDocs/phase-7-automation-design.md`
  - `SV00123-ATU-A01/TestScript-Data/Automation/README.md`
- execution tables:
  - `SV00123-ATU-A01/TestResults/FinalResults/OSMS-Final-Test-Results.xlsx`
  - `SV00123-ATU-A01/TestResults/Metrics/OSMS-Test-Metrics.xlsx`

### Mandatory figures for Section III

- insert now:
  - `SV00123-ATU-A01/TestResults/Evidence/UI/automation/20260406_053930_TC-UI-AUTH-001-success.png`
  - `SV00123-ATU-A01/TestResults/Evidence/UI/automation/20260406_054245_TC-UI-AUTH-003-access-denied.png`
  - `SV00123-ATU-A01/TestResults/Evidence/UI/automation/20260406_054115_TC-UI-IMP-002-preview.png`
  - `SV00123-ATU-A01/TestResults/Evidence/UI/automation/20260406_054004_TC-UI-PUR-001-draft-created.png`
  - `SV00123-ATU-A01/TestResults/Evidence/UI/automation/20260406_053902_TC-UI-INV-001-failure.png`
  - screenshot or clipped output from `SV00123-ATU-A01/TestResults/Evidence/API/newman-full-run.txt`

### IV. Defect Report And Metrics

- defect log source:
  - `SV00123-ATU-A01/TestResults/Defects/OSMS-Defect-Register.csv`
- defect workflow source:
  - `SV00123-ATU-A01/MainReport/SupportingDocs/phase-10-defect-log-and-bug-management.md`
  - `SV00123-ATU-A01/TestResults/Defects/GitHubIssues/github-label-taxonomy.md`
- metrics source:
  - `SV00123-ATU-A01/TestResults/Metrics/OSMS-Test-Metrics.xlsx`
  - `SV00123-ATU-A01/TestResults/Metrics/OSMS-Interface-Results.csv`
  - `SV00123-ATU-A01/TestResults/Metrics/OSMS-Module-Wise-Results.csv`
  - `SV00123-ATU-A01/TestResults/Metrics/OSMS-Scenario-Coverage.csv`

### Mandatory figures for Section IV

- insert now:
  - `SV00123-ATU-A01/TestResults/Evidence/Report/OSMS-Test-Metrics-Summary.png`
  - `SV00123-ATU-A01/TestResults/Evidence/Defects/BUG-20260406-001-invoice-create-log.txt`
  - `SV00123-ATU-A01/TestResults/Evidence/Defects/BUG-20260406-001-github-issue.png`

### V. Conclusion And Future Work

- source text:
  - `SV00123-ATU-A01/MainReport/SupportingDocs/phase-12-analysis-and-insights.md`
  - final conclusion paragraph in the report content file

### References

- source list:
  - `SV00123-ATU-A01/MainReport/SourceAssets/Powered by GPT - Software Quality Verification - Final Report Content.md`

### Appendix

- GitHub appendix link:
  - `https://github.com/Alucard30Dec/Online-Sales-Management-System/tree/main/Report%20Test%20subject/SV00123-ATU-A01`
- list the exact artifacts already committed in:
  - `SV00123-ATU-A01/TestCases/`
  - `SV00123-ATU-A01/TestScript-Data/TestData/`
  - `SV00123-ATU-A01/TestScript-Data/Automation/`
  - `SV00123-ATU-A01/TestResults/FinalResults/`
  - `SV00123-ATU-A01/TestResults/Metrics/`
  - `SV00123-ATU-A01/TestResults/Defects/`
  - `SV00123-ATU-A01/TestResults/Evidence/`
  - `SV00123-ATU-A01/Videos/`

## High-Risk Missing Items Before Final Export

- `PENDING REAL EXECUTION`: additional execution screenshots for `Stock`, `Reports`, `Products`, and `Public Catalog`

## Final Export Checklist

- copy the Markdown report content into the Word report draft
- apply `Times New Roman`, `12 pt`, and `1.5` line spacing
- generate the automatic table of contents
- insert all tables and figures referenced in the report content
- keep every missing artifact labeled as `PENDING REAL EXECUTION` if it still does not exist
- export the final Word file to PDF
