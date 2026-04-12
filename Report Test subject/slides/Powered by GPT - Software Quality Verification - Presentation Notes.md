# Presentation Notes

## Canonical Binary

- `../Powered by GPT - Software Quality Verification - Presentation.pptx`

## Speaking Strategy

- keep the defense under `10` minutes
- show evidence before conclusions
- separate clearly:
  - what was designed
  - what was executed
  - what remains unresolved
- do not overclaim product stability while `4` confirmed defects remain open

## Slide Guidance

### Slide 1. Title

- Introduce the project as the final Software Testing submission for `Online Sales Management System`.
- State the execution baseline immediately:
  - `63` designed cases
  - `63` executed
  - `56` pass
  - `7` fail
  - `4` live GitHub issues

### Slide 2. Risk-Based Scope

- Explain that the scope covers both UI and API surfaces.
- Emphasize that risk was selected from real business flows: permissions, import, purchases, invoices, stock.

### Slide 3. Strategy And Evidence Model

- Say that the team used project-specific planning rather than a template-only approach.
- Mention black-box execution with white-box-informed edge cases.
- Highlight the evidence chain:
  - scenario -> testcase -> result -> evidence -> defect -> issue

### Slide 4. Coverage Snapshot

- State `42` scenarios and `63` cases.
- Point out `44` UI and `19` API cases.
- If asked about ownership split, use the three UI owner counts.

### Slide 5. Automation Implementation

- Explain why Selenium + Newman were practical for this stack.
- Mention `Page Object Model`, reusable helpers, and saved runner outputs.
- Explicitly mention that the demo video now shows Edge smoke, Chrome import preview, API run, and fail-case comparison.

### Slide 6. Real Execution Evidence

- This is the strongest proof slide.
- Point to three visible evidence types:
  - UI pass screenshot
  - import preview screenshot
  - Newman summary image
- Then state the fail evidence honestly:
  - purchase validation
  - import confirm
  - invoice create
  - invoice cancel

### Slide 7. Result Comparison

- Say this sentence almost verbatim:
  - `We do not stop at pass/fail labels. Each result is defended by expected result, actual runtime behavior, and the linked evidence file.`
- Use the two pass rows and two fail rows to show that the team can justify both kinds of status.
- If asked why the slide is important, say that it links the workbook status directly to runtime behavior instead of relying on summary numbers.

### Slide 8. Metrics Snapshot

- Explain that `100% execution` means every designed case has runtime evidence.
- Immediately add that `100% execution` does not mean defect-free product quality.
- This keeps the conclusion rigorous.

### Slide 9. Defect Management

- Say that `7` failed cases consolidate into `4` confirmed defects because several failures share the same root cause.
- Mention that all four defects now have live GitHub issues and rerun evidence from `2026-04-11`.
- State the severity split clearly: `3 High`, `1 Medium`.

### Slide 10. Final Assessment And Next Steps

- Split the conclusion into:
  - proven today
  - open after execution
- End with the next actions:
  - fix
  - rerun
  - attach post-fix evidence

### Slide 11. Q&A Backup

- Keep this hidden unless asked.

## Likely Questions

### Why do 7 failed cases become 4 defects?

- Because invoice-create has multiple failing variants under one root cause, while the remaining failures map to three additional distinct defects.

### How do you know a fail is a product defect and not an automation bug?

- Because the fail must be reproducible, must mismatch the expected result, and must still fail in focused reruns. The invoice-create defect additionally has server-log evidence.

### Why is 100% execution still not enough to claim stability?

- Because execution coverage measures how much was tested, not whether the product passed those tests.

### Which area is riskiest now?

- Invoices remain the riskiest because both create and cancel flows still have confirmed defects.

### What should be retested first after fixes?

- Invoice create first, then invoice cancel, then purchase validation, then import confirm.
