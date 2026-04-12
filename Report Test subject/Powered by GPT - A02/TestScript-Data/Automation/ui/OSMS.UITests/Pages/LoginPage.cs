using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Pages;

public sealed class LoginPage : PageBase
{
    private static readonly By EmailInput = By.Name("Email");
    private static readonly By PasswordInput = By.Id("passwordInput");
    private static readonly By SubmitButton = By.CssSelector("button[type='submit']");

    public LoginPage(IWebDriver driver, WaitHelper wait, AutomationSettings settings)
        : base(driver, wait, settings)
    {
    }

    public LoginPage Open()
    {
        OpenRelativeUrl("/Admin/Auth/Login");
        Wait.UrlContains("/Admin/Auth/Login");
        Wait.Visible(EmailInput);
        return this;
    }

    public void Login(LoginCredential credential)
    {
        SetInputValue(EmailInput, credential.Username);
        SetInputValue(PasswordInput, credential.Password);
        Wait.Condition(
            driver => driver.FindElements(SubmitButton).Count > 0,
            "Timed out waiting for the login submit button to appear in the DOM.");

        var submitButton = Driver.FindElement(SubmitButton);

        if (Driver is IJavaScriptExecutor js)
        {
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", submitButton);
        }

        try
        {
            submitButton.Click();
        }
        catch (ElementClickInterceptedException)
        {
            if (Driver is IJavaScriptExecutor clickJs)
            {
                clickJs.ExecuteScript("arguments[0].click();", submitButton);
            }
            else
            {
                throw;
            }
        }
    }

    public string GetPageText() => ReadBodyText();
}
