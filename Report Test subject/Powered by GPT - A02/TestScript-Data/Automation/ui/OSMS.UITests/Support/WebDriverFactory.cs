using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace OSMS.UITests.Support;

public static class WebDriverFactory
{
    public static IWebDriver Create(AutomationSettings settings)
    {
        var driver = settings.Browser switch
        {
            "edge" => CreateEdgeDriver(settings),
            _ => CreateChromeDriver(settings)
        };

        ApplyWindowMode(driver, settings);
        return driver;
    }

    private static IWebDriver CreateChromeDriver(AutomationSettings settings)
    {
        var options = new ChromeOptions();
        options.AcceptInsecureCertificates = true;
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument(settings.Fullscreen ? "--start-fullscreen" : "--start-maximized");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);

        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
        }

        return new ChromeDriver(options);
    }

    private static IWebDriver CreateEdgeDriver(AutomationSettings settings)
    {
        var options = new EdgeOptions();
        options.AcceptInsecureCertificates = true;
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument(settings.Fullscreen ? "--start-fullscreen" : "--start-maximized");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);

        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
        }

        return new EdgeDriver(options);
    }

    private static void ApplyWindowMode(IWebDriver driver, AutomationSettings settings)
    {
        try
        {
            driver.Manage().Window.Maximize();

            if (settings.Fullscreen)
            {
                driver.Manage().Window.FullScreen();
            }
        }
        catch (WebDriverException)
        {
            // Best-effort only. Browser launch should not fail solely because a window state command is not supported.
        }
    }
}
