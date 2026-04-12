using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Interactions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

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

        if (!settings.Headless)
        {
            try
            {
                if (settings.FullScreen)
                {
                    driver.Manage().Window.Maximize();
                    Thread.Sleep(500);
                    TryBringBrowserWindowToFront(settings);
                    driver.Manage().Window.FullScreen();
                    Thread.Sleep(500);
                    TryEnterKeyboardFullScreen(driver);
                    Thread.Sleep(500);
                    TryBringBrowserWindowToFront(settings);
                }
                else
                {
                    driver.Manage().Window.Maximize();
                    TryBringBrowserWindowToFront(settings);
                }
            }
            catch (WebDriverException)
            {
                driver.Manage().Window.Maximize();
            }
        }

        return driver;
    }

    private static void TryEnterKeyboardFullScreen(IWebDriver driver)
    {
        try
        {
            new Actions(driver).SendKeys(Keys.F11).Perform();
        }
        catch (WebDriverException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static void TryBringBrowserWindowToFront(AutomationSettings settings)
    {
        var processName = settings.Browser switch
        {
            "edge" => "msedge",
            _ => "chrome"
        };

        for (var attempt = 0; attempt < 10; attempt++)
        {
            Process? browserProcess = null;

            try
            {
                browserProcess = Process.GetProcessesByName(processName)
                    .Where(process => process.MainWindowHandle != IntPtr.Zero)
                    .OrderByDescending(process => process.StartTime)
                    .FirstOrDefault();
            }
            catch (InvalidOperationException)
            {
            }

            var windowHandle = browserProcess?.MainWindowHandle ?? IntPtr.Zero;

            if (windowHandle != IntPtr.Zero)
            {
                NativeWindowFocus.ShowWindow(windowHandle, 9);
                Thread.Sleep(200);
                NativeWindowFocus.SetForegroundWindow(windowHandle);
                return;
            }

            Thread.Sleep(300);
        }
    }

    private static class NativeWindowFocus
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }

    private static IWebDriver CreateChromeDriver(AutomationSettings settings)
    {
        var options = new ChromeOptions();
        options.AcceptInsecureCertificates = true;
        options.AddExcludedArgument("enable-automation");
        options.AddArgument("--window-size=1600,1000");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-blink-features=AutomationControlled");

        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
        }
        else
        {
            options.AddArgument("--start-maximized");
            options.AddArgument("--start-fullscreen");
        }

        return new ChromeDriver(options);
    }

    private static IWebDriver CreateEdgeDriver(AutomationSettings settings)
    {
        var options = new EdgeOptions();
        options.AcceptInsecureCertificates = true;
        options.AddArgument("--window-size=1600,1000");

        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
        }
        else
        {
            options.AddArgument("start-maximized");
            options.AddArgument("start-fullscreen");
        }

        return new EdgeDriver(options);
    }
}
