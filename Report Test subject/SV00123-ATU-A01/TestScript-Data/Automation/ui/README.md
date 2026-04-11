# UI Automation

Framework: `.NET 8 + xUnit + Selenium WebDriver`

Primary browser: `Chrome` via Selenium Manager

## Current package scope

The clean submission package now contains executed UI evidence for all `44` UI test cases. Automation coverage is split across focused test classes so that evidence can be rerun and mapped back to specific case IDs.

### Main test classes

- `AuthenticationTests`
- `PurchaseFlowTests`
- `InvoiceFlowTests`
- `ProductImportTests`
- `AdminAndCustomerCoverageTests`
- `PurchaseProductImportCoverageTests`
- `ReportingPublicInvoiceCoverageTests`
- `ExtendedCoverageTests`
- `AdditionalCrudCoverageTests`

## Canonical structure

```text
TestScript-Data/
  Automation/
    ui/
      run-ui-tests.ps1
      OSMS.UITests/
        appsettings.json
        Pages/
        Support/
        Tests/
```

## Run

### Default run

```powershell
pwsh "Report Test subject/SV00123-ATU-A01/TestScript-Data/Automation/ui/run-ui-tests.ps1"
```

### Headless run

```powershell
pwsh "Report Test subject/SV00123-ATU-A01/TestScript-Data/Automation/ui/run-ui-tests.ps1" -Headless
```

### Full-screen demo run

```powershell
pwsh "Report Test subject/SV00123-ATU-A01/TestScript-Data/Automation/ui/run-ui-tests.ps1" -Fullscreen
```

### Filter one focused test

```powershell
pwsh "Report Test subject/SV00123-ATU-A01/TestScript-Data/Automation/ui/run-ui-tests.ps1" -Filter "FullyQualifiedName~AdminLoginSmokeSucceeds"
```

## Canonical evidence outputs

- TRX files: `SV00123-ATU-A01/TestResults/RunnerOutput/UI/`
- screenshots: `SV00123-ATU-A01/TestResults/Evidence/UI/automation/`

## Preconditions

- the OSMS application is running locally at `http://127.0.0.1:5068`
- seeded demo accounts are available
- Chrome is installed
- package test data remains under `SV00123-ATU-A01/TestScript-Data/TestData/`
