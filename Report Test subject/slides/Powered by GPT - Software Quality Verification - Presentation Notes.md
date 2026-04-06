# Presentation Notes

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
  - what is still pending

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

- State that the team produced `42` scenarios and `59` total test cases.
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
- Explicitly say that blocked results were not converted into fake defects.
- This makes the submission more credible.

### Slide 7. Current Metrics

- Explain that `59` cases exist but only `4` have real execution evidence so far.
- Say clearly that the current package proves a verified baseline, not full regression stability.
- This answer is stronger than pretending the whole project is already fully verified.

### Slide 8. Defect And Risk Analysis

- Explain that `0` confirmed defects does not mean the system is defect-free.
- It means the current failed evidence is not yet strong enough to justify defect logging.
- Mention that the biggest unresolved business risks are still purchases, invoices, stock, and product import.

### Slide 9. Key Insights

- Summarize the main honest conclusion:
  - strong planning
  - strong traceability
  - real automation evidence exists
  - but execution depth is still limited
- This is the slide that shows analytical maturity.

### Slide 10. Next Steps And Submission Package

- Explain the immediate next actions:
  - retest observations manually
  - run more critical flows
  - finish video and final evidence
- End by pointing to the GitHub appendix link containing the deliverables.

### Slide 11. Q&A Backup

- Keep this hidden unless asked.

## Demo Order

If the examiner allows a short live demo or asks for concrete proof, use this order:

1. show the GitHub folder structure in `Report Test subject`
2. show `test-cases/ui/OSMS-UI-Test-Cases.xlsx`
3. show `automation/ui/OSMS.UITests`
4. show login success screenshot
5. show Newman health smoke output
6. show `results/OSMS-Final-Results.xlsx`
7. show `metrics/OSMS-Test-Metrics.xlsx`

This order starts from structure, then evidence, then summary, which is easier to defend than starting from theory.

## Evidence To Show On Screen

### Strongest evidence already available

- `evidence/ui/automation/20260405_115322_TC-UI-AUTH-001-success.png`
- `results/automation-api/newman-health-smoke.txt`
- `results/automation-ui/ui-tests.trx`
- `results/OSMS-Final-Results.xlsx`
- `metrics/OSMS-Test-Metrics.xlsx`

### Do not overuse as bug proof

- `evidence/ui/automation/20260405_115343_TC-UI-AUTH-003-failure.png`
- `evidence/ui/automation/20260405_115405_TC-UI-IMP-002-failure.png`

These two files are useful to explain automation instability, but they are not strong enough to present as confirmed application defects.

## Likely Q&A And Recommended Answers

### Why do you have 0 confirmed defects?

Because the current failed automation evidence is not yet enough to prove a reproducible application bug. We kept those records as observations instead of forcing them into false defect reports.

### Why is execution progress only 6.78%?

Because the current package prioritizes correctness and traceability over fake completion. We executed the flows that could produce credible evidence first, then measured the real current baseline.

### Why did you choose Selenium and Newman?

The project stack is `.NET 8`, so Selenium with `.NET + xUnit` is practical and maintainable. The exposed API surface is small and stable, so Postman/Newman gives fast repeatable coverage with low setup cost.

### Which module is riskiest right now?

The highest current business risk is in modules that affect money and stock: purchases, invoices, stock, and product import, because those areas are still largely unexecuted.

### How do you distinguish automation failure from product defect?

A product defect must have a reproducible expected-versus-actual mismatch. If the runner times out or the script expectation is unstable, we classify it as an automation script issue or observation until manual confirmation is completed.

### What is the strongest part of your submission?

The strongest part is the traceable structure: source-based audit, real test cases, real automation assets, real evidence files, and metrics that do not overstate the result.

## Slide Design Guidance

- use short titles, not paragraphs
- keep each slide to `3-5` bullets
- prioritize screenshots and numbers over text walls
- use one color for `Pass`, one for `Blocked`, one for `Pending`
- if time is short, Slides `3`, `6`, `7`, `9`, and `10` are the highest-value core
