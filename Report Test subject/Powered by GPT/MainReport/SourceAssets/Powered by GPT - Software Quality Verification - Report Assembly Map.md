# Report Assembly Map

## Package Note

This file is the package copy of the report-assembly guide. The canonical grading deliverables are the generated binaries in `../`.

## Objective

Use this file while converting the Markdown report source into the final Word and PDF deliverables.

## Canonical Submission Package

- clean package root:
  - `Powered by GPT/`
- final deliverables should be reviewed from:
  - `Powered by GPT/MainReport/`

## Target Final Files

- final Word file:
  - `Powered by GPT/MainReport/Powered by GPT - Software Quality Verification - Final Report.docx`
- final PDF file:
  - `Powered by GPT/MainReport/Powered by GPT - Software Quality Verification - Final Report.pdf`

## Section-To-Artifact Mapping

### Cover Page

- source text:
  - `Powered by GPT/MainReport/SourceAssets/Powered by GPT - Software Quality Verification - Final Report Content.md`
- no screenshot required

### Record Of Changes

- source text:
  - `Powered by GPT/MainReport/SourceAssets/Powered by GPT - Software Quality Verification - Final Report Content.md`

### I. Overview

- use text from the report content file
- optional supporting table:
  - `Powered by GPT/TestCases/UI/OSMS-UI-Test-Cases.xlsx`
  - summarize owner counts:
    - `Hoang Van Thien = 17`
    - `Nguyen Thanh Dat = 13`
    - `Le Quang Duy = 14`

### II. Test Plan

- source text:
  - `Powered by GPT/MainReport/SupportingDocs/test-plan.md`
- recommended table:
  - environment summary table
- no screenshot required

### III. Test Design And Execution

- scenario summary:
  - `Powered by GPT/TestCases/Scenarios/test-scenarios.md`
  - `Powered by GPT/TestResults/Metrics/OSMS-Scenario-Coverage.csv`
- UI cases:
  - `Powered by GPT/TestCases/UI/OSMS-UI-Test-Cases.xlsx`
- API cases:
  - `Powered by GPT/TestCases/API/OSMS-API-Test-Cases.xlsx`
- test data:
  - `Powered by GPT/TestScript-Data/TestData/accounts/OSMS-Test-Accounts.md`
  - `Powered by GPT/TestScript-Data/TestData/ui/OSMS-UI-Test-Data.xlsx`
  - `Powered by GPT/TestScript-Data/TestData/api/OSMS-API-Test-Data.json`
- automation design:
  - `Powered by GPT/MainReport/SupportingDocs/phase-7-automation-design.md`
  - `Powered by GPT/TestScript-Data/Automation/README.md`
- execution tables:
  - `Powered by GPT/TestResults/FinalResults/OSMS-Final-Test-Results.xlsx`
  - `Powered by GPT/TestResults/Metrics/OSMS-Test-Metrics.xlsx`

### Mandatory figures for Section III

- insert now:
  - `Powered by GPT/TestResults/Evidence/UI/automation/20260406_053930_TC-UI-AUTH-001-success.png`
  - `Powered by GPT/TestResults/Evidence/UI/automation/20260406_054245_TC-UI-AUTH-003-access-denied.png`
  - `Powered by GPT/TestResults/Evidence/UI/automation/20260406_054115_TC-UI-IMP-002-preview.png`
  - `Powered by GPT/TestResults/Evidence/UI/automation/20260406_054004_TC-UI-PUR-001-draft-created.png`
  - `Powered by GPT/TestResults/Evidence/UI/automation/20260406_053902_TC-UI-INV-001-failure.png`
  - `Powered by GPT/TestResults/Evidence/Report/OSMS-Newman-Full-Run-Snippet.png`

### IV. Defect Report And Metrics

- defect log source:
  - `Powered by GPT/TestResults/Defects/OSMS-Defect-Register.csv`
- defect workflow source:
  - `Powered by GPT/MainReport/SupportingDocs/phase-10-defect-log-and-bug-management.md`
  - `Powered by GPT/TestResults/Defects/GitHubIssues/github-label-taxonomy.md`
- metrics source:
  - `Powered by GPT/TestResults/Metrics/OSMS-Test-Metrics.xlsx`
  - `Powered by GPT/TestResults/Metrics/OSMS-Interface-Results.csv`
  - `Powered by GPT/TestResults/Metrics/OSMS-Module-Wise-Results.csv`
  - `Powered by GPT/TestResults/Metrics/OSMS-Scenario-Coverage.csv`

### Mandatory figures for Section IV

- insert now:
  - `Powered by GPT/TestResults/Evidence/Report/OSMS-Test-Metrics-Summary.png`
  - `Powered by GPT/TestResults/Evidence/Defects/BUG-20260406-001-invoice-create-log.txt`
  - `Powered by GPT/TestResults/Evidence/Defects/BUG-20260406-001-github-issue.png`

### V. Conclusion And Future Work

- source text:
  - `Powered by GPT/MainReport/SupportingDocs/phase-12-analysis-and-insights.md`
  - final conclusion paragraph in the report content file

### References

- source list:
  - `Powered by GPT/MainReport/SourceAssets/Powered by GPT - Software Quality Verification - Final Report Content.md`

### Appendix

- GitHub appendix link:
  - `https://github.com/Alucard30Dec/Online-Sales-Management-System/tree/main/Report%20Test%20subject/Powered%20by%20GPT`
- list the exact artifacts already committed in:
  - `Powered by GPT/TestCases/`
  - `Powered by GPT/TestScript-Data/TestData/`
  - `Powered by GPT/TestScript-Data/Automation/`
  - `Powered by GPT/TestResults/FinalResults/`
  - `Powered by GPT/TestResults/Metrics/`
  - `Powered by GPT/TestResults/Defects/`
  - `Powered by GPT/TestResults/Evidence/`
  - `Powered by GPT/Videos/`

## High-Risk Missing Items Before Final Export

- mirror `BUG-20260411-002`, `BUG-20260411-003`, and `BUG-20260411-004` into GitHub Issues if you want stronger bug-management evidence
- add post-fix retest evidence after any confirmed defect is resolved

## Final Export Checklist

- copy the Markdown report content into the Word report draft
- apply `Times New Roman`, `12 pt`, and `1.5` line spacing
- generate the automatic table of contents
- insert all tables and figures referenced in the report content
- keep every missing artifact labeled as not yet executed in the current evidence baseline if it still does not exist
- export the final Word file to PDF

