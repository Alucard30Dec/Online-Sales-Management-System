# Automation Video Capture Checklist

## Target output

- canonical package copy:
  - `Powered by GPT/Videos/OSMS-Automation-Demo.mp4`
- compatibility working copy:
  - `video/OSMS-Automation-Demo.mp4`

## Completion status

- Previous recording from `2026-04-06` is no longer accepted as final because the API section showed `npm error`.
- Current accepted recording was recreated on `2026-04-08`.
- Verified duration: `125 seconds`
- Verified file size: approximately `9.32 MB`
- Verified output:
  - `Powered by GPT/Videos/OSMS-Automation-Demo.mp4`
  - `video/OSMS-Automation-Demo.mp4`
- The accepted recording now shows:
  - one successful UI automation run
  - saved UI screenshot evidence
  - successful Newman execution
  - saved API result files
  - final results workbook visible during the demo

## Exact recording order

1. Show the repository root and mention the canonical submission package `Report Test subject/Powered by GPT`.
2. Open `Report Test subject/automation`.
3. Show the UI automation command:
   - `powershell -ExecutionPolicy Bypass -File "Report Test subject/automation/ui/run-ui-tests.ps1" -Filter FullyQualifiedName~AdminLoginSmokeSucceeds`
4. Show the browser launching and finishing one stable UI flow:
   - `TC-UI-AUTH-001`
5. Show where the newest generated screenshot was saved:
   - `evidence/ui/automation/*TC-UI-AUTH-001-success.png`
6. Show the API automation command:
   - `powershell -ExecutionPolicy Bypass -File "Report Test subject/automation/api/newman/run-api-tests.ps1"`
7. Show the Newman summary finishing successfully in the terminal.
8. Show the saved API result files:
   - `results/automation-api/newman-full-run.txt`
   - `results/automation-api/newman-results.xml`
9. Show the final results workbook in Excel:
   - `results/OSMS-Final-Test-Results.xlsx`
10. Keep the full video between `2` and `4` minutes.

## Recording rules

- Use the current local environment only.
- Do not splice in fake screens or non-project footage.
- If a run fails during recording, restart and keep only the clean successful capture.
- If audio is unnecessary, silent video is acceptable.
- The accepted final video must not show `npm error`, unhandled exceptions, or failed automation commands.
- Close or suppress intrusive popup windows before recording. The final video should not show TeamViewer or similar overlays.
- The last 20-30 seconds should keep `OSMS-Final-Test-Results.xlsx` visible, not idle on VS Code or a blank desktop.
