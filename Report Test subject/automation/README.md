# Automation Workspace

This folder contains the planned automation deliverables for the OSMS coursework submission.

## Chosen stacks

- UI automation: `.NET 8 + xUnit + Selenium WebDriver`
- API automation: `Postman + Newman`

## Why this stack

- The main application is ASP.NET Core, so `.NET` keeps the UI automation close to the project stack.
- `dotnet`, `node`, and `npm` are available locally.
- `mvn` is not available, so Java Selenium would create avoidable setup work.
- `Google Chrome` is installed, making Chrome the safest primary automation browser.

## Planned structure

```text
automation/
  ui/
    README.md
    OSMS.UITests/
      Pages/
      Support/
      TestData/
      Tests/
  api/
    README.md
    postman/
      collections/
      environments/
    newman/
```

Phase 7 defines the structure and scope. Phase 8 will add the real scripts, project files, run commands, and evidence hooks.
