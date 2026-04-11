using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OSMS.UITests.Pages;

namespace OSMS.UITests.Support;

public abstract class UiTestBase : IDisposable
{
    protected UiTestBase()
    {
        Settings = AutomationSettings.Load();
        TestData = UiTestDataCatalog.Load(Settings);
        Driver = WebDriverFactory.Create(Settings);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        Wait = new WaitHelper(Driver, TimeSpan.FromSeconds(Settings.DefaultTimeoutSeconds));
        Screenshots = new ScreenshotHelper(Settings);
    }

    protected AutomationSettings Settings { get; }

    protected UiTestDataCatalog TestData { get; }

    protected IWebDriver Driver { get; }

    protected WaitHelper Wait { get; }

    protected ScreenshotHelper Screenshots { get; }

    protected string CaptureCheckpoint(string name)
    {
        return Screenshots.Capture(Driver, name);
    }

    protected void PauseForDemo()
    {
        if (Settings.DemoPauseSeconds > 0)
        {
            Thread.Sleep(TimeSpan.FromSeconds(Settings.DemoPauseSeconds));
        }
    }

    protected void ExecuteWithFailureCapture(string name, Action action)
    {
        try
        {
            action();
        }
        catch
        {
            try
            {
                CaptureCheckpoint($"{name}-failure");
            }
            catch
            {
                // Best-effort capture only.
            }

            throw;
        }
    }

    protected void LoginAs(LoginCredential credential)
    {
        var loginPage = new LoginPage(Driver, Wait, Settings).Open();
        loginPage.Login(credential);

        Wait.Condition(
            driver => driver.Url.Contains("/Admin", StringComparison.OrdinalIgnoreCase)
                && !driver.Url.Contains("/Admin/Auth/Login", StringComparison.OrdinalIgnoreCase),
            "Timed out waiting for the authenticated user to reach the admin area.");
    }

    protected void LoginAsAdmin()
    {
        LoginAs(TestData.GetCredential("UI-DATA-ACC-001"));
    }

    protected void SetValue(By locator, string value)
    {
        var element = Wait.Visible(locator);
        element.Clear();
        element.SendKeys(value);
    }

    protected void SelectByContains(By locator, string partialText)
    {
        var select = new SelectElement(Wait.Visible(locator));
        var option = select.Options.FirstOrDefault(o =>
            !string.IsNullOrWhiteSpace(o.GetAttribute("value"))
            && o.Text.Contains(partialText, StringComparison.OrdinalIgnoreCase));

        if (option == null)
        {
            throw new NoSuchElementException($"No option containing '{partialText}' was found for select '{locator}'.");
        }

        select.SelectByText(option.Text);
    }

    protected void ClickElement(By locator)
    {
        var element = Wait.Visible(locator);

        if (Driver is IJavaScriptExecutor scrollJs)
        {
            scrollJs.ExecuteScript("arguments[0].scrollIntoView({ block: 'center' });", element);
        }

        try
        {
            Wait.Clickable(locator).Click();
        }
        catch (ElementClickInterceptedException)
        {
            if (Driver is IJavaScriptExecutor clickJs)
            {
                clickJs.ExecuteScript("arguments[0].click();", element);
                return;
            }

            throw;
        }
    }

    protected void SubmitForm(By formLocator)
    {
        var form = Wait.Visible(formLocator);
        if (Driver is not IJavaScriptExecutor js)
        {
            throw new InvalidOperationException("The active web driver does not support JavaScript form submission.");
        }

        js.ExecuteScript("arguments[0].submit();", form);
    }

    protected string ReadBodyText()
    {
        return Wait.Visible(By.TagName("body")).Text;
    }

    protected void AcceptAlertIfPresent(TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(2));
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                Driver.SwitchTo().Alert().Accept();
                return;
            }
            catch (NoAlertPresentException)
            {
                Thread.Sleep(100);
            }
        }
    }

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
    }
}
