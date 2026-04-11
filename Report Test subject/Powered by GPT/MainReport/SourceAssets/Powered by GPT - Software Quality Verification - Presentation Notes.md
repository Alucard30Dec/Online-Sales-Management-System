# Presentation Notes

## Package Note

These notes are the package copy of the slide-preparation source. The canonical grading artifact is:

- `../Powered by GPT - Software Quality Verification - Presentation.pptx`

If any path below still uses the original workspace naming, resolve it through `../Submission-Index.md`.

## Objective

Use these notes while building the slide deck and during oral defense. The notes are optimized for short, direct delivery and for handling likely examiner questions.

## Delivery Strategy

- keep the deck under `10` main slides
- spend most time on:
  - strategy
  - real evidence
  - metrics
  - insights
- do not spend too long reading tables
- do not claim broad stability
- clearly separate:
  - what was designed
  - what was executed
  - what remains unresolved after execution

## Slide-By-Slide Speaking Points

### Slide 1. Title

- Introduce the project, team, and repository.
- State that the submission is based on the real source code of `Online Sales Management System`.

### Slide 2. Project And Test Scope

- Explain that the project has both UI and API surfaces.
- Emphasize that the scope was chosen based on real business-critical modules, not random screens.
- Mention that permissions, purchases, invoices, stock, and product import were treated as high-risk areas.

### Slide 3. Test Strategy

- Explain the mixed approach:
  - manual for business rules and visual validation
  - API checks for stable endpoint behavior
  - automation for high-value repeatable flows
- Mention that source-code analysis was used to design stronger edge cases.

### Slide 4. Coverage And Team Allocation

- State that the team produced `42` scenarios and `63` total test cases.
- Mention that each member had at least `10` UI test cases.
- If asked about non-overlap, explain module ownership split:
  - HVT focused more on auth, permissions, admin, invoices, reports
  - NTD focused on customers, suppliers, purchases
  - LQD focused on products, import, stock

### Slide 5. Automation Implementation

- Explain why Selenium + Newman were selected:
  - practical with current stack
  - bonus-friendly
  - maintainable
- Mention `Page Object Model` and reusable helpers as the main quality point.

### Slide 6. Real Execution Evidence

- This is one of the strongest slides.
- Show that UI and API both have real pass evidence.
- Explicitly say that the UI suite no longer has `Not Run`.
- Explain that all four confirmed defects are now mirrored into live GitHub Issues, and a focused retest on `2026-04-11` still reproduced all four defects. True post-fix retest evidence is still missing because no code fix has been verified yet.

### Slide 7. Current Metrics

- Explain that `63` cases exist and all `63` now have real execution evidence.
- Say clearly that the package is now execution-complete at the test-case level, but it is still not defect-free.
- This answer is stronger than pretending that high execution coverage means the system is already stable.

### Slide 8. Defect And Risk Analysis

- Explain that `4` confirmed defects now exist.
- Highlight that `BUG-20260406-001` remains the strongest defect because it has both UI proof and server-log proof plus a real GitHub issue.
- Mention that the execution gap is now closed, and even the retest status is clearer because the four live defects were re-run on `2026-04-11`. The remaining gap is specifically post-fix evidence, not missing execution.
- Also mention that the earlier authorization and import observations were closed by focused reruns, so they should not be presented as product bugs.

### Slide 9. Key Insights

- Summarize the main honest conclusion:
  - strong planning
  - strong traceability
  - real automation evidence exists
  - execution depth is now complete
  - but defect resolution is still incomplete
- This is the slide that shows analytical maturity.

### Slide 10. Next Steps And Submission Package

- Explain the immediate next actions:
  - fix and retest invoice creation through GitHub Issue `#1`
  - fix and retest purchase validation, import confirm, and invoice cancellation after fixes are available
  - convert the current re-failure retests into post-fix pass evidence once code changes are available
  - expand cross-browser proof beyond the current `Edge` smoke run
- End by pointing to the GitHub appendix link containing the deliverables.

### Slide 11. Q&A Backup

- Keep this hidden unless asked.

## Demo Order

If the examiner allows a short live demo or asks for concrete proof, use this order:

1. show the GitHub folder structure in `Report Test subject`
2. open the clean package folder `Powered by GPT`
3. show `TestCases/UI/OSMS-UI-Test-Cases.xlsx`
4. show `TestScript-Data/Automation/ui/OSMS.UITests`
5. show login success screenshot
6. show Newman full-collection output
7. show `TestResults/FinalResults/OSMS-Final-Test-Results.xlsx`
8. show `TestResults/Metrics/OSMS-Test-Metrics.xlsx`

This order starts from structure, then evidence, then summary, which is easier to defend than starting from theory.

## Evidence To Show On Screen

### Strongest evidence already available

- `Powered by GPT/TestResults/Evidence/UI/automation/20260406_053930_TC-UI-AUTH-001-success.png`
- `Powered by GPT/TestResults/Evidence/UI/automation/20260406_054245_TC-UI-AUTH-003-access-denied.png`
- `Powered by GPT/TestResults/Evidence/UI/automation/20260406_054115_TC-UI-IMP-002-preview.png`
- `Powered by GPT/TestResults/Evidence/UI/automation/20260406_054004_TC-UI-PUR-001-draft-created.png`
- `Powered by GPT/TestResults/Evidence/API/newman-full-run.txt`
- `Powered by GPT/TestResults/RunnerOutput/UI/auth-permission-rerun.trx`
- `Powered by GPT/TestResults/RunnerOutput/UI/import-preview-rerun.trx`
- `Powered by GPT/TestResults/RunnerOutput/UI/purchase-rerun.trx`
- `Powered by GPT/TestResults/FinalResults/OSMS-Final-Test-Results.xlsx`
- `Powered by GPT/TestResults/Metrics/OSMS-Test-Metrics.xlsx`

### Strongest defect proof

- `Powered by GPT/TestResults/Evidence/UI/automation/20260406_053902_TC-UI-INV-001-failure.png`
- `Powered by GPT/TestResults/Evidence/Defects/BUG-20260406-001-invoice-create-log.txt`

These two files should be presented together because the screenshot shows the user-facing failure and the log proves the root cause.

## Likely Q&A And Recommended Answers

### Why do you now have 4 confirmed defects?

Because the expanded reruns converted the previously unexecuted UI cases into real evidence. Seven failing cases were observed, but they map to four distinct confirmed defects after consolidating the three invoice-create variations under the same root-cause defect.

### Why is execution progress now 100%?

Because every designed UI and API test case now has runtime evidence. We did not remove `Not Run` by editing cells manually; we removed it by running the remaining UI coverage batches and synchronizing the evidence files, results workbook, and metrics pack.

### Why did you choose Selenium and Newman?

The project stack is `.NET 8`, so Selenium with `.NET + xUnit` is practical and maintainable. The exposed API surface is small and stable, so Postman/Newman gives fast repeatable coverage with low setup cost.

### Which module is riskiest right now?

The highest current business risk is still invoices, because invoice creation and invoice cancellation both have confirmed failures, and the create defect also blocks the insufficient-stock and tampered-price scenarios.

### How do you distinguish automation failure from product defect?

A product defect must have a reproducible expected-versus-actual mismatch. If the runner times out or the script expectation is unstable, we keep it as an observation. The current package has four confirmed defects because the reruns now show stable mismatches with screenshot and runner-output evidence, and the invoice-create defect is additionally backed by a server-side exception trace.

### What is the strongest part of your submission?

The strongest part is the traceable structure: source-based audit, real test cases, real automation assets, real evidence files, and metrics that do not overstate the result.

### Why were the earlier authorization and import failures not logged as bugs?

Because focused reruns showed that the authorization case was a valid redirect-based denial and the import case was an automation locator issue. The team only promoted the invoice case into a confirmed defect after the UI failure and server log matched.

## Slide Design Guidance

- use short titles, not paragraphs
- keep each slide to `3-5` bullets
- prioritize screenshots and numbers over text walls
- use one color for `Pass`, one for `Fail`, one for `Open Defects`
- if time is short, Slides `3`, `6`, `7`, `9`, and `10` are the highest-value core

