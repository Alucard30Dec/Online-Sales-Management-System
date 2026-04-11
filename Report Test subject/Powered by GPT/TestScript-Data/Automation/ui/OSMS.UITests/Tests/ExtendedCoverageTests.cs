using OpenQA.Selenium;
using OSMS.UITests.Pages;
using OSMS.UITests.Support;

namespace OSMS.UITests.Tests;

public sealed class ExtendedCoverageTests : UiTestBase
{
    [Fact]
    [Trait("CaseId", "TC-UI-AUTH-002")]
    public void AdminLoginRejectsInvalidPassword()
    {
        ExecuteWithFailureCapture("TC-UI-AUTH-002", () =>
        {
            var loginPage = new LoginPage(Driver, Wait, Settings).Open();
            loginPage.Login(new LoginCredential("admin@osms.local", "WrongAdmin@12345"));

            Wait.UrlContains("/Admin/Auth/Login");
            Wait.BodyContains("Đăng nhập thất bại");

            Assert.Contains("/Admin/Auth/Login", Driver.Url, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Đăng nhập thất bại", Driver.FindElement(By.TagName("body")).Text, StringComparison.OrdinalIgnoreCase);
            PauseForDemo();
            CaptureCheckpoint("TC-UI-AUTH-002-invalid-password");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-AUTH-004")]
    public void WarehouseUserIsDeniedAccessToInvoices()
    {
        ExecuteWithFailureCapture("TC-UI-AUTH-004", () =>
        {
            var credential = TestData.GetCredential("UI-DATA-ACC-003");
            var loginPage = new LoginPage(Driver, Wait, Settings).Open();

            loginPage.Login(credential);
            Wait.Condition(
                driver => driver.Url.Contains("/Admin", StringComparison.OrdinalIgnoreCase)
                    && !driver.Url.Contains("/Admin/Auth/Login", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the warehouse account to finish the login redirect.");

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Invoices"));
            Wait.Condition(
                driver => !driver.Url.Contains("/Admin/Invoices", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the warehouse user to be redirected away from the protected Invoices route.");
            Wait.BodyContains("Dashboard");

            Assert.Contains("/Admin", Driver.Url, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("/Admin/Invoices", Driver.Url, StringComparison.OrdinalIgnoreCase);
            PauseForDemo();
            CaptureCheckpoint("TC-UI-AUTH-004-access-denied");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-AUTH-006")]
    public void FullAdminCanOpenCoreManagementModules()
    {
        ExecuteWithFailureCapture("TC-UI-AUTH-006", () =>
        {
            var credential = TestData.GetCredential("UI-DATA-ACC-001");
            var loginPage = new LoginPage(Driver, Wait, Settings).Open();

            loginPage.Login(credential);
            Wait.Condition(
                driver => driver.Url.Contains("/Admin", StringComparison.OrdinalIgnoreCase)
                    && !driver.Url.Contains("/Admin/Auth/Login", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the admin account to finish the login redirect.");

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Products"));
            Wait.Condition(
                driver => driver.Url.Contains("/Admin/Products", StringComparison.OrdinalIgnoreCase)
                    || driver.FindElements(By.CssSelector("a[href='/Admin/Products/Create']")).Count > 0,
                "Timed out waiting for the Products module to open for the admin account.");

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Purchases"));
            Wait.Condition(
                driver => driver.Url.Contains("/Admin/Purchases", StringComparison.OrdinalIgnoreCase)
                    || driver.FindElements(By.CssSelector("a[href='/Admin/Purchases/Create']")).Count > 0,
                "Timed out waiting for the Purchases module to open for the admin account.");

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Invoices"));
            Wait.Condition(
                driver => driver.Url.Contains("/Admin/Invoices", StringComparison.OrdinalIgnoreCase)
                    || driver.FindElements(By.CssSelector("a[href='/Admin/Invoices/Create']")).Count > 0,
                "Timed out waiting for the Invoices module to open for the admin account.");

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Reports"));
            Wait.Condition(
                driver => driver.Url.Contains("/Admin/Reports", StringComparison.OrdinalIgnoreCase)
                    || driver.FindElements(By.Name("from")).Count > 0,
                "Timed out waiting for the Reports module to open for the admin account.");
            Wait.Visible(By.Name("from"));
            Wait.Visible(By.Name("to"));

            Assert.DoesNotContain("Access Denied", Driver.FindElement(By.TagName("body")).Text, StringComparison.OrdinalIgnoreCase);
            PauseForDemo();
            CaptureCheckpoint("TC-UI-AUTH-006-core-modules");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-REP-001")]
    public void ReportsPageNormalizesReversedDateRange()
    {
        ExecuteWithFailureCapture("TC-UI-REP-001", () =>
        {
            var credential = TestData.GetCredential("UI-DATA-ACC-001");
            var loginPage = new LoginPage(Driver, Wait, Settings).Open();

            loginPage.Login(credential);

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Reports?from=2026-04-05&to=2026-03-01"));
            Wait.Condition(
                driver => driver.Url.Contains("/Admin/Reports", StringComparison.OrdinalIgnoreCase)
                    || driver.FindElements(By.Id("dateFrom")).Count > 0,
                "Timed out waiting for the reports page to open after requesting a reversed date range.");

            var fromInput = Wait.Visible(By.Id("dateFrom"));
            var toInput = Wait.Visible(By.Id("dateTo"));

            Assert.Equal("2026-03-01", fromInput.GetAttribute("value"));
            Assert.Equal("2026-04-05", toInput.GetAttribute("value"));
            PauseForDemo();
            CaptureCheckpoint("TC-UI-REP-001-reversed-range");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-STK-001")]
    public void LowStockScreenListsSeededLowStockProducts()
    {
        ExecuteWithFailureCapture("TC-UI-STK-001", () =>
        {
            var credential = TestData.GetCredential("UI-DATA-ACC-001");
            var loginPage = new LoginPage(Driver, Wait, Settings).Open();

            loginPage.Login(credential);
            Wait.Condition(
                driver => driver.Url.Contains("/Admin", StringComparison.OrdinalIgnoreCase)
                    && !driver.Url.Contains("/Admin/Auth/Login", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the admin account to finish the login redirect.");

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Stock/Low"));
            Wait.Condition(
                driver => driver.Url.Contains("/Admin/Stock/Low", StringComparison.OrdinalIgnoreCase)
                    || driver.FindElements(By.CssSelector("table")).Count > 0,
                "Timed out waiting for the low-stock page to open.");
            Wait.Condition(
                driver => driver.FindElements(By.CssSelector("table")).Count > 0,
                "Timed out waiting for the low-stock table to render.");

            var body = Driver.FindElement(By.TagName("body")).Text;
            Assert.Contains("SP001", body, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("SP002", body, StringComparison.OrdinalIgnoreCase);
            PauseForDemo();
            CaptureCheckpoint("TC-UI-STK-001-low-stock");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PUB-003")]
    public void PublicProductDetailsShowsSelectedProductInformation()
    {
        ExecuteWithFailureCapture("TC-UI-PUB-003", () =>
        {
            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Product/Details/30082"));
            Wait.Condition(
                driver => driver.Url.Contains("/Product/Details/30082", StringComparison.OrdinalIgnoreCase)
                    || driver.FindElements(By.CssSelector("h2.title")).Count > 0,
                "Timed out waiting for the public product details page to open.");
            Wait.Condition(
                driver => driver.FindElements(By.CssSelector("a[href='/Product?category=30013']")).Count > 0,
                "Timed out waiting for the expected category link to appear on the public product details page.");
            Wait.Condition(
                driver => driver.FindElements(By.CssSelector("a[href='/Product?brand=30039']")).Count > 0,
                "Timed out waiting for the expected brand link to appear on the public product details page.");

            var title = Driver.FindElement(By.CssSelector("h2.title")).Text;
            Assert.False(string.IsNullOrWhiteSpace(title));
            PauseForDemo();
            CaptureCheckpoint("TC-UI-PUB-003-details");
        });
    }
}
