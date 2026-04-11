# Source Of Truth Mapping

This file identifies the canonical source for each submission-facing artifact in the `Powered by GPT` package.

| Artifact area | Canonical source file | Notes |
|---|---|---|
| UI test cases | `TestCases/UI/OSMS-UI-Test-Cases.xlsx` | Primary UI case workbook; CSV mirror exists for Git diff and quick review. |
| API test cases | `TestCases/API/OSMS-API-Test-Cases.xlsx` | Primary API case workbook; CSV mirror exists for Git diff and quick review. |
| Scenario coverage | `TestCases/Scenarios/test-scenarios.md` | Scenario count and IDs used by both UI and API case files. |
| Final execution results | `TestResults/FinalResults/OSMS-Final-Test-Results.xlsx` | Primary execution workbook used for report, slides, and grading review. |
| Final-results CSV mirror | `TestResults/FinalResults/OSMS-Final-Results.csv` | Text mirror of the canonical final-results workbook. |
| Execution traceability | `TestResults/FinalResults/execution-evidence-mapping.csv` | Source-of-truth linkage between test case, evidence, runner output, video, and defect. |
| Metrics | `TestResults/Metrics/OSMS-Test-Metrics.xlsx` | Summary workbook; CSV files in the same folder are the text mirrors per sheet. |
| Defect log | `TestResults/Defects/OSMS-Defect-Log.xlsx` | Canonical defect workbook aligned to GitHub Issue evidence. |
| Defect register CSV | `TestResults/Defects/OSMS-Defect-Register.csv` | Text mirror of the defect log and observation history. |
| UI evidence | `TestResults/Evidence/UI/automation/` | Timestamped screenshots for executed UI cases. |
| API evidence | `TestResults/Evidence/API/newman-full-run.txt` | Human-readable full Newman output referenced by the report and video. |
| API machine-readable runner output | `TestResults/RunnerOutput/API/newman-results.xml` | XML result artifact for automation traceability. |
| UI runner output | `TestResults/RunnerOutput/UI/*.trx` | Focused rerun artifacts for executed UI automation cases. |
| Automation source | `TestScript-Data/Automation/` | Canonical folder for UI and API automation scripts. |
| Test data | `TestScript-Data/TestData/` | Canonical folder for datasets, accounts, and import samples. |
| Main report source | `MainReport/SourceAssets/Powered by GPT - Software Quality Verification - Final Report Content.md` | Authoring source used to regenerate the report binaries. |
| Main report binary | `MainReport/Powered by GPT - Software Quality Verification - Final Report.pdf` | Primary grading copy of the report. |
| Presentation source | `MainReport/SourceAssets/Powered by GPT - Software Quality Verification - Presentation Content.md` | Authoring source used to regenerate the slide deck. |
| Presentation binary | `MainReport/Powered by GPT - Software Quality Verification - Presentation.pptx` | Primary grading copy of the slide deck. |
| Automation video | `Videos/OSMS-Automation-Demo.mp4` | Canonical demo video referenced in the report, slides, and traceability sheet. |

## Synchronization Rule

When a test case, result, defect, or evidence item changes, update the canonical file above first, then refresh any mirrored CSV, derived metrics, and submission-facing report or slide content that references it.
