using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Tests;

public sealed class AdminAndCustomerCoverageTests : UiTestBase
{
    [Fact]
    [Trait("CaseId", "TC-UI-ADM-001")]
    public void CreateAdminSucceedsWithValidGroupAssignment()
    {
        ExecuteWithFailureCapture("TC-UI-ADM-001", () =>
        {
            LoginAsAdmin();

            var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var email = $"qa.admin.auto.{suffix}@osms.local";

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Admins/Create"));
            Wait.UrlContains("/Admin/Admins/Create");

            SetValue(By.Name("Email"), email);
            SetValue(By.Name("FullName"), $"QA Admin Auto {suffix}");
            SetValue(By.Name("Password"), "QaAdmin@12345");
            SelectByContains(By.Name("AdminGroupId"), "Warehouse Staff");
            ClickElement(By.CssSelector("form[action='/Admin/Admins/Create'] button[type='submit']"));

            Wait.UrlContains("/Admin/Admins");
            Wait.BodyContains(email);
            CaptureCheckpoint("TC-UI-ADM-001-created");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-ADM-002")]
    public void CurrentAdminCannotDeactivateOwnAccount()
    {
        ExecuteWithFailureCapture("TC-UI-ADM-002", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Admins"));
            Wait.UrlContains("/Admin/Admins");
            Wait.BodyContains("You");

            var selfRow = Wait.Visible(By.XPath("//tr[.//span[contains(normalize-space(),'You')]]"));
            var editLink = selfRow.FindElement(By.CssSelector("a[href*='/Admin/Admins/Edit/']")).GetAttribute("href");
            var adminId = editLink.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
            var requestVerificationToken = Driver.FindElement(By.CssSelector("input[name='__RequestVerificationToken']")).GetAttribute("value");

            if (Driver is not IJavaScriptExecutor js)
            {
                throw new InvalidOperationException("The active web driver does not support JavaScript form submission.");
            }

            js.ExecuteScript(
                """
                const actionUrl = arguments[0];
                const token = arguments[1];
                const form = document.createElement('form');
                form.method = 'post';
                form.action = actionUrl;
                const tokenInput = document.createElement('input');
                tokenInput.type = 'hidden';
                tokenInput.name = '__RequestVerificationToken';
                tokenInput.value = token;
                form.appendChild(tokenInput);
                document.body.appendChild(form);
                form.submit();
                """,
                Settings.ToAbsoluteUri($"/Admin/Admins/Disable/{adminId}").ToString(),
                requestVerificationToken);

            Wait.UrlContains("/Admin/Admins");
            Wait.BodyContains("cannot deactivate your own account");
            Wait.BodyContains("Active");
            CaptureCheckpoint("TC-UI-ADM-002-self-deactivate-blocked");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-AUTH-005")]
    public void InactiveAdminAccountCannotLogIn()
    {
        ExecuteWithFailureCapture("TC-UI-AUTH-005", () =>
        {
            LoginAsAdmin();

            var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var email = $"qa.inactive.auto.{suffix}@osms.local";

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Admins/Create"));
            Wait.UrlContains("/Admin/Admins/Create");

            SetValue(By.Name("Email"), email);
            SetValue(By.Name("FullName"), $"QA Inactive Admin {suffix}");
            SetValue(By.Name("Password"), "QaInactive@12345");
            SelectByContains(By.Name("AdminGroupId"), "Sales Staff");

            var activeCheckbox = Wait.Visible(By.Id("isActive"));
            if (activeCheckbox.Selected)
            {
                activeCheckbox.Click();
            }

            ClickElement(By.CssSelector("form[action='/Admin/Admins/Create'] button[type='submit']"));
            Wait.UrlContains("/Admin/Admins");
            Wait.BodyContains(email);

            Driver.Manage().Cookies.DeleteAllCookies();
            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Auth/Login"));
            Wait.UrlContains("/Admin/Auth/Login");

            SetValue(By.Name("Email"), email);
            SetValue(By.Name("Password"), "QaInactive@12345");
            ClickElement(By.CssSelector("form[action='/Admin/Auth/Login'] button[type='submit']"));

            Wait.UrlContains("/Admin/Auth/Login");
            Wait.BodyContains("inactive");
            CaptureCheckpoint("TC-UI-AUTH-005-inactive-account");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-CUS-002")]
    public void CustomerCreateRejectsMissingName()
    {
        ExecuteWithFailureCapture("TC-UI-CUS-002", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Customers/Create"));
            Wait.UrlContains("/Admin/Customers/Create");

            SetValue(By.Name("Phone"), "0909000002");
            SetValue(By.Name("Email"), "qa.missing.name@example.com");

            SubmitForm(By.CssSelector("form[action='/Admin/Customers/Create']"));

            Wait.UrlContains("/Admin/Customers/Create");
            Wait.Condition(
                driver =>
                {
                    var body = driver.FindElement(By.TagName("body")).Text;
                    return body.Contains("required", StringComparison.OrdinalIgnoreCase)
                        || body.Contains("Name", StringComparison.OrdinalIgnoreCase);
                },
                "Timed out waiting for the missing-name validation message on the customer create page.");

            CaptureCheckpoint("TC-UI-CUS-002-missing-name");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-CUS-003")]
    public void CustomerCreateRejectsInvalidEmailFormat()
    {
        ExecuteWithFailureCapture("TC-UI-CUS-003", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Customers/Create"));
            Wait.UrlContains("/Admin/Customers/Create");

            SetValue(By.Name("Name"), "QA Customer Invalid Email");
            SetValue(By.Name("Email"), "invalid-email-format");

            SubmitForm(By.CssSelector("form[action='/Admin/Customers/Create']"));

            Wait.UrlContains("/Admin/Customers/Create");
            Wait.Condition(
                driver =>
                {
                    var body = driver.FindElement(By.TagName("body")).Text;
                    return body.Contains("valid", StringComparison.OrdinalIgnoreCase)
                        && body.Contains("email", StringComparison.OrdinalIgnoreCase);
                },
                "Timed out waiting for the invalid-email validation message on the customer create page.");

            CaptureCheckpoint("TC-UI-CUS-003-invalid-email");
        });
    }
}
