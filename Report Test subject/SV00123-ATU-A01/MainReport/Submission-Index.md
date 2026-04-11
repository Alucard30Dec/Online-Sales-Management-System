# Submission Index

## Package Root

- `SV00123-ATU-A01/`

This folder is the clean final submission package aligned to the requested 5-folder structure:

- `MainReport`
- `TestCases`
- `TestResults`
- `TestScript-Data`
- `Videos`

## Fast Grading Route

1. Open `MainReport/Powered by GPT - Software Quality Verification - Final Report.pdf`
2. Open `MainReport/Powered by GPT - Software Quality Verification - Presentation.pptx`
3. Review `TestCases/UI/OSMS-UI-Test-Cases.xlsx`
4. Review `TestCases/API/OSMS-API-Test-Cases.xlsx`
5. Review `TestResults/FinalResults/OSMS-Final-Test-Results.xlsx`
6. Review `TestResults/Metrics/OSMS-Test-Metrics.xlsx`
7. Review `TestResults/Defects/OSMS-Defect-Log.xlsx`
8. Review `TestResults/Evidence/Defects/BUG-20260406-001-github-issue.png`
9. Review `TestScript-Data/Automation/`
10. Review `Videos/OSMS-Automation-Demo.mp4`

## Folder Meaning

### MainReport

- final report `docx`
- final report `pdf`
- final presentation `pptx`
- supporting docs and source assets used to produce the final binaries

### TestCases

- UI and API test case workbooks
- scenario coverage document

### TestResults

- final execution workbooks
- metrics workbooks and CSV summaries
- evidence screenshots
- defect logs and GitHub issue support files
- runner output from Newman and Selenium xUnit

### TestScript-Data

- automation source code and run notes
- test accounts and reusable datasets

### Videos

- automation demonstration video
- video description and recording script

## Path Normalization Note

Some supporting documents were originally authored against the working workspace under `Report Test subject/`. For grading and submission, use these clean-package equivalents:

| Workspace path family | Clean package equivalent |
|---|---|
| `report/`, `slides/` | `MainReport/` |
| `test-cases/` | `TestCases/` |
| `test-data/` | `TestScript-Data/TestData/` |
| `automation/` | `TestScript-Data/Automation/` |
| `results/` | `TestResults/FinalResults/` or `TestResults/RunnerOutput/` |
| `metrics/` | `TestResults/Metrics/` |
| `defects/` | `TestResults/Defects/` |
| `evidence/` | `TestResults/Evidence/` |
| `video/` | `Videos/` |

## Known Remaining Gaps

- all designed UI and API cases are now executed in the synchronized baseline
- all four confirmed defects are now mirrored into live GitHub Issues
- basic cross-browser evidence now exists through an `Edge` smoke rerun of `TC-UI-AUTH-001`

