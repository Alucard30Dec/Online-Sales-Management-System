using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace OSMS.UITests.Support;

public sealed class WaitHelper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public WaitHelper(IWebDriver driver, TimeSpan timeout)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, timeout);
    }

    public IWebElement Visible(By locator)
    {
        return _wait.Until(driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Displayed ? element : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        }) ?? throw new WebDriverTimeoutException($"Timed out waiting for element '{locator}' to become visible.");
    }

    public IWebElement Clickable(By locator)
    {
        return _wait.Until(driver =>
        {
            try
            {
                var element = driver.FindElement(locator);
                return element.Displayed && element.Enabled ? element : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        }) ?? throw new WebDriverTimeoutException($"Timed out waiting for element '{locator}' to become clickable.");
    }

    public IReadOnlyCollection<IWebElement> VisibleAll(By locator)
    {
        return _wait.Until(driver =>
        {
            try
            {
                var elements = driver.FindElements(locator);
                return elements.Count > 0 && elements.All(x => x.Displayed) ? elements : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        }) ?? throw new WebDriverTimeoutException($"Timed out waiting for elements '{locator}' to become visible.");
    }

    public void UrlContains(string fragment)
    {
        _wait.Until(driver => driver.Url.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    public void BodyContains(string text)
    {
        _wait.Until(driver =>
        {
            try
            {
                var bodyText = driver.FindElement(By.TagName("body")).Text;
                return bodyText.Contains(text, StringComparison.OrdinalIgnoreCase);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });
    }

    public void Condition(Func<IWebDriver, bool> condition, string failureMessage)
    {
        var success = _wait.Until(condition);
        if (!success)
        {
            throw new WebDriverTimeoutException(failureMessage);
        }
    }
}
