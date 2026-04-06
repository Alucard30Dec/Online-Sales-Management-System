# Report Assembly Map

## Objective

Use this file while converting the Markdown report source into the final Word and PDF deliverables.

## Target Final Files

- final Word file:
  - `report/Powered by GPT - Software Quality Verification - Final Report.docx`
- final PDF file:
  - `report/Powered by GPT - Software Quality Verification - Final Report.pdf`

## Section-To-Artifact Mapping

### Cover Page

- source text:
  - `report/Powered by GPT - Software Quality Verification - Final Report Content.md`
- no screenshot required

### Record Of Changes

- source text:
  - `report/Powered by GPT - Software Quality Verification - Final Report Content.md`

### I. Overview

- use text from the report content file
- optional supporting table:
  - `test-cases/ui/OSMS-UI-Test-Cases.xlsx`
  - summarize owner counts:
    - `Hoang Van Thien = 15`
    - `Nguyen Thanh Dat = 12`
    - `Le Quang Duy = 13`

### II. Test Plan

- source text:
  - `docs/test-plan.md`
- recommended table:
  - environment summary table
- no screenshot required

### III. Test Design And Execution

- scenario summary:
  - `docs/test-scenarios.md`
  - `metrics/OSMS-Scenario-Coverage.csv`
- UI cases:
  - `test-cases/ui/OSMS-UI-Test-Cases.xlsx`
- API cases:
  - `test-cases/api/OSMS-API-Test-Cases.xlsx`
- test data:
  - `test-data/accounts/OSMS-Test-Accounts.md`
  - `test-data/ui/OSMS-UI-Test-Data.xlsx`
  - `test-data/api/OSMS-API-Test-Data.json`
- automation design:
  - `docs/phase-7-automation-design.md`
  - `automation/README.md`
- execution tables:
  - `results/OSMS-Final-Results.xlsx`
  - `metrics/OSMS-Test-Metrics.xlsx`

### Mandatory figures for Section III

- insert now:
  - `evidence/ui/automation/20260405_115322_TC-UI-AUTH-001-success.png`
  - screenshot or clipped output from `results/automation-api/newman-health-smoke.txt`
- insert later when available:
  - purchase success screenshot
  - invoice success screenshot
  - manual permission-denied screenshot
  - import preview-count screenshot

### IV. Defect Report And Metrics

- defect log source:
  - `defects/exports/OSMS-Defect-Register.csv`
- defect workflow source:
  - `docs/phase-10-defect-log-and-bug-management.md`
  - `defects/github-issues/github-label-taxonomy.md`
- metrics source:
  - `metrics/OSMS-Test-Metrics.xlsx`
  - `metrics/OSMS-Interface-Results.csv`
  - `metrics/OSMS-Module-Wise-Results.csv`
  - `metrics/OSMS-Scenario-Coverage.csv`

### Mandatory figures for Section IV

- insert later when available:
  - GitHub Issue screenshot showing severity and priority
  - metrics workbook summary screenshot

### V. Conclusion And Future Work

- source text:
  - `docs/phase-12-analysis-and-insights.md`
  - final conclusion paragraph in the report content file

### References

- source list:
  - `report/Powered by GPT - Software Quality Verification - Final Report Content.md`

### Appendix

- GitHub appendix link:
  - `https://github.com/Alucard30Dec/Online-Sales-Management-System/tree/main/Report%20Test%20subject`
- list the exact artifacts already committed in:
  - `test-cases/`
  - `test-data/`
  - `automation/`
  - `results/`
  - `metrics/`
  - `defects/`
  - `evidence/`
  - `video/`

## High-Risk Missing Items Before Final Export

- `PENDING REAL EXECUTION`: automation demo video in `video/OSMS-Automation-Demo.mp4`
- `PENDING REAL EXECUTION`: purchase and invoice success screenshots
- `PENDING REAL EXECUTION`: manual confirmation screenshots for the two observations
- `PENDING REAL EXECUTION`: GitHub Issue screenshot if a confirmed defect is found
- `PENDING REAL EXECUTION`: full catalog API execution outputs

## Final Export Checklist

- copy the Markdown report content into the Word report draft
- apply `Times New Roman`, `12 pt`, and `1.5` line spacing
- generate the automatic table of contents
- insert all tables and figures referenced in the report content
- keep every missing artifact labeled as `PENDING REAL EXECUTION` if it still does not exist
- export the final Word file to PDF
