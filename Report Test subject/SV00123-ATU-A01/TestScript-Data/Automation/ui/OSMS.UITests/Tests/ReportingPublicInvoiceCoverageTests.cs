using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenQA.Selenium;
using OSMS.UITests.Support;

namespace OSMS.UITests.Tests;

public sealed class ReportingPublicInvoiceCoverageTests : UiTestBase
{
    [Fact]
    [Trait("CaseId", "TC-UI-REP-002")]
    public async Task ReportsExportGeneratesWorkbookForSelectedDateRange()
    {
        await ExecuteWithFailureCaptureAsync("TC-UI-REP-002", async () =>
        {
            LoginAsAdmin();
            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Reports?from=2026-03-01&to=2026-04-05"));
            Wait.UrlContains("/Admin/Reports");
            Wait.BodyContains("Báo Cáo và Thống Kê");
            CaptureCheckpoint("TC-UI-REP-002-export-source");

            var downloadPath = await DownloadAuthenticatedFileAsync(
                "/Admin/Reports/ExportExcel?from=2026-03-01&to=2026-04-05",
                "tc-ui-rep-002-export.xlsx");

            var sheets = ReadWorkbookSheetNames(downloadPath);
            Assert.Contains("Tong Quan", sheets);
            Assert.Contains("Chi Tiet Doanh Thu", sheets);
            Assert.Contains("Chi Tiet Nhap Hang", sheets);
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-STK-002")]
    public void StockMovementHistorySupportsFilteringByProductAndDate()
    {
        ExecuteWithFailureCapture("TC-UI-STK-002", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Stock/Movements?productId=30082&from=2026-03-01&to=2026-04-10"));
            Wait.UrlContains("/Admin/Stock/Movements");
            Wait.BodyContains("Sản phẩm Test 10");

            var rows = Driver.FindElements(By.CssSelector("tbody tr"));
            Assert.NotEmpty(rows);
            Assert.All(rows, row => Assert.Contains("Sản phẩm Test 10", row.Text, StringComparison.OrdinalIgnoreCase));
            CaptureCheckpoint("TC-UI-STK-002-filtered-movements");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-STK-003")]
    public async Task StockMovementExportDownloadsWorkbookForCurrentFilterScope()
    {
        await ExecuteWithFailureCaptureAsync("TC-UI-STK-003", async () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Stock/Movements?productId=30082&from=2026-03-01&to=2026-04-10"));
            Wait.UrlContains("/Admin/Stock/Movements");
            CaptureCheckpoint("TC-UI-STK-003-export-source");

            var downloadPath = await DownloadAuthenticatedFileAsync(
                "/Admin/Stock/ExportMovementsExcel?productId=30082&from=2026-03-01&to=2026-04-10",
                "tc-ui-stk-003-export.xlsx");

            var sheets = ReadWorkbookSheetNames(downloadPath);
            Assert.Contains("XuatKho", sheets);
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PUB-001")]
    public void PublicCatalogSupportsSearchFiltersAndDetailsNavigation()
    {
        ExecuteWithFailureCapture("TC-UI-PUB-001", () =>
        {
            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Product?search=SP01"));
            Wait.UrlContains("/Product");
            Wait.BodyContains("Sản phẩm");
            Wait.Visible(By.CssSelector(".single-product .title a"));

            var firstDetailsLink = Wait.Visible(By.CssSelector(".single-product .title a"));
            var firstProductName = firstDetailsLink.Text.Trim();
            firstDetailsLink.Click();

            Wait.UrlContains("/Product/Details/");
            Wait.BodyContains(firstProductName);

            ClickElement(By.CssSelector(".product-meta a[href*='category=']"));
            Wait.UrlContains("/Product");
            Wait.Condition(
                driver => driver.Url.Contains("category=", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the public catalog category filter to update the URL.");

            Driver.Navigate().Back();
            Wait.UrlContains("/Product/Details/");

            ClickElement(By.CssSelector(".product-meta a[href*='brand=']"));
            Wait.UrlContains("/Product");
            Wait.Condition(
                driver => driver.Url.Contains("brand=", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the public catalog brand filter to update the URL.");

            CaptureCheckpoint("TC-UI-PUB-001-search-filter-details");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PUB-002")]
    public void PublicCatalogSupportsPriceFilterAndSorting()
    {
        ExecuteWithFailureCapture("TC-UI-PUB-002", () =>
        {
            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Product?min=200&max=500&sort=price_asc"));
            Wait.UrlContains("/Product");
            Wait.Visible(By.CssSelector(".single-product .price"));

            var prices = Driver.FindElements(By.CssSelector(".single-product .price"))
                .Select(element => ParseMoney(element.Text))
                .ToList();

            Assert.NotEmpty(prices);
            Assert.All(prices, price => Assert.InRange(price, 200_000m, 500_000m));
            Assert.Equal(prices.OrderBy(x => x).ToList(), prices);
            CaptureCheckpoint("TC-UI-PUB-002-price-sort");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-INV-002")]
    public void InvoiceCreateRejectsMissingProductSelection()
    {
        ExecuteWithFailureCapture("TC-UI-INV-002", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Invoices/Create"));
            Wait.UrlContains("/Admin/Invoices/Create");

            SubmitForm(By.CssSelector("form[action='/Admin/Invoices/Create']"));

            Wait.UrlContains("/Admin/Invoices/Create");
            var alertText = Wait.Visible(By.CssSelector(".alert.alert-danger")).Text;
            Assert.Contains("Please select product for all items.", alertText, StringComparison.OrdinalIgnoreCase);
            CaptureCheckpoint("TC-UI-INV-002-missing-product");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-INV-003")]
    public void InvoiceCreateShowsWrongFailureWhenQuantityExceedsStock()
    {
        ExecuteWithFailureCapture("TC-UI-INV-003", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Invoices/Create"));
            Wait.UrlContains("/Admin/Invoices/Create");

            SelectByContains(By.CssSelector("#itemsTable tbody tr:first-child select.product-select"), "SP001");
            SetValue(By.CssSelector("#itemsTable tbody tr:first-child input.qty-input"), "1");
            ClickElement(By.CssSelector("form[action='/Admin/Invoices/Create'] button[type='submit']"));

            Wait.UrlContains("/Admin/Invoices/Create");
            Wait.BodyContains("Failed to create invoice.");
            throw new Xunit.Sdk.XunitException("Expected an insufficient-stock failure message, but the system returned the generic invoice creation failure caused by the known create defect.");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-INV-004")]
    public void RecordingPaymentUpdatesInvoiceFromPartialToPaid()
    {
        ExecuteWithFailureCapture("TC-UI-INV-004", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Invoices?pageSize=100"));
            Wait.UrlContains("/Admin/Invoices");

            var targetRow = Wait.Visible(By.XPath("//tr[td[contains(normalize-space(),'Unpaid')]][1]"));
            var invoiceNo = targetRow.FindElements(By.TagName("td"))[1].Text.Trim();
            var grandTotal = ParseMoney(targetRow.FindElements(By.TagName("td"))[4].Text);
            var paidAmount = ParseMoney(targetRow.FindElements(By.TagName("td"))[5].Text);
            var due = grandTotal - paidAmount;
            var partialAmount = Math.Max(1000m, Math.Floor((due / 2m) / 1000m) * 1000m);
            if (partialAmount >= due)
            {
                partialAmount = Math.Max(1000m, due - 1000m);
            }

            targetRow.FindElement(By.CssSelector("a[href*='/Admin/Invoices/Details/']")).Click();
            Wait.UrlContains("/Admin/Invoices/Details/");
            Wait.Visible(By.Name("amount")).SendKeys(partialAmount.ToString(CultureInfo.InvariantCulture));
            ClickElement(By.CssSelector("form[action*='/Admin/Invoices/RecordPayment/'] button[type='submit']"));

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri($"/Admin/Invoices?q={invoiceNo}&pageSize=10"));
            Wait.UrlContains("/Admin/Invoices");
            Wait.BodyContains(invoiceNo);
            Wait.BodyContains("PartiallyPaid");

            ClickElement(By.CssSelector($"a[href*='/Admin/Invoices/Details/']"));
            Wait.UrlContains("/Admin/Invoices/Details/");
            var remainingDue = due - partialAmount;
            Wait.Visible(By.Name("amount")).SendKeys(remainingDue.ToString(CultureInfo.InvariantCulture));
            ClickElement(By.CssSelector("form[action*='/Admin/Invoices/RecordPayment/'] button[type='submit']"));

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri($"/Admin/Invoices?q={invoiceNo}&pageSize=10"));
            Wait.UrlContains("/Admin/Invoices");
            Wait.BodyContains(invoiceNo);
            Wait.BodyContains("Paid");
            CaptureCheckpoint("TC-UI-INV-004-paid");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-INV-005")]
    public void CancellingInvoiceMarksItCancelledAndCreatesStockReturnMovement()
    {
        ExecuteWithFailureCapture("TC-UI-INV-005", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Invoices?pageSize=100"));
            Wait.UrlContains("/Admin/Invoices");

            var targetRow = Wait.Visible(By.XPath("//tr[td[contains(normalize-space(),'PartiallyPaid') or contains(normalize-space(),'Unpaid')]][1]"));
            var invoiceNo = targetRow.FindElements(By.TagName("td"))[1].Text.Trim();

            targetRow.FindElement(By.CssSelector("a[href*='/Admin/Invoices/Details/']")).Click();
            Wait.UrlContains("/Admin/Invoices/Details/");

            ClickElement(By.CssSelector("form[action*='/Admin/Invoices/Cancel/'] button[type='submit']"));
            AcceptAlertIfPresent();

            Wait.BodyContains("Đã hủy");
            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Stock/Movements?from=2026-03-01&to=2026-04-10"));
            Wait.UrlContains("/Admin/Stock/Movements");
            Wait.BodyContains(invoiceNo);
            Wait.Condition(
                driver =>
                {
                    var body = driver.FindElement(By.TagName("body")).Text;
                    return body.Contains("InvoiceCancel", StringComparison.OrdinalIgnoreCase)
                        || body.Contains("Invoice", StringComparison.OrdinalIgnoreCase);
                },
                "Timed out waiting for the stock movement history to show the invoice cancellation reference.");
            CaptureCheckpoint("TC-UI-INV-005-cancelled");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-INV-006")]
    public void InvoiceCreateDoesNotReachServerSidePriceVerificationBecauseCreateFlowFails()
    {
        ExecuteWithFailureCapture("TC-UI-INV-006", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Invoices/Create"));
            Wait.UrlContains("/Admin/Invoices/Create");

            SelectByContains(By.CssSelector("#itemsTable tbody tr:first-child select.product-select"), "SP010");
            SetValue(By.CssSelector("#itemsTable tbody tr:first-child input.qty-input"), "1");

            if (Driver is not IJavaScriptExecutor js)
            {
                throw new InvalidOperationException("The active web driver does not support JavaScript field tampering.");
            }

            js.ExecuteScript(
                """
                const hidden = document.querySelector('#itemsTable tbody tr:first-child input.unitprice-hidden');
                const visible = document.querySelector('#itemsTable tbody tr:first-child input.unitprice-input');
                if (hidden) hidden.value = '1';
                if (visible) visible.value = '1';
                """);

            ClickElement(By.CssSelector("form[action='/Admin/Invoices/Create'] button[type='submit']"));

            Wait.UrlContains("/Admin/Invoices/Create");
            Wait.BodyContains("Failed to create invoice.");
            throw new Xunit.Sdk.XunitException("Expected the server-side sale price to override the tampered client-posted UnitPrice, but the invoice create flow failed before the price check because of the known transaction defect.");
        });
    }

    private async Task ExecuteWithFailureCaptureAsync(string name, Func<Task> action)
    {
        try
        {
            await action();
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

    private async Task<string> DownloadAuthenticatedFileAsync(string relativeUrl, string fileName)
    {
        var outputPath = RepositoryPathHelper.ResolveFromRepository($"Report Test subject/SV00123-ATU-A01/TestResults/RunnerOutput/UI/{fileName}");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        using var client = CreateAuthenticatedHttpClient();
        using var response = await client.GetAsync(relativeUrl);
        response.EnsureSuccessStatusCode();

        await using var output = File.Create(outputPath);
        await response.Content.CopyToAsync(output);
        return outputPath;
    }

    private HttpClient CreateAuthenticatedHttpClient()
    {
        var cookieContainer = new CookieContainer();
        var baseUri = new Uri(Settings.BaseUrl + "/");

        foreach (var seleniumCookie in Driver.Manage().Cookies.AllCookies)
        {
            var cookie = new System.Net.Cookie(
                seleniumCookie.Name,
                seleniumCookie.Value,
                string.IsNullOrWhiteSpace(seleniumCookie.Path) ? "/" : seleniumCookie.Path,
                baseUri.Host);

            cookieContainer.Add(baseUri, cookie);
        }

        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = baseUri
        };
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        return client;
    }

    private static IReadOnlyCollection<string> ReadWorkbookSheetNames(string absoluteWorkbookPath)
    {
        using var zip = ZipFile.OpenRead(absoluteWorkbookPath);
        var workbookEntry = zip.GetEntry("xl/workbook.xml")
            ?? throw new InvalidOperationException("The exported workbook does not contain xl/workbook.xml.");

        using var stream = workbookEntry.Open();
        var document = XDocument.Load(stream);
        return document.Descendants()
            .Where(element => element.Name.LocalName == "sheet")
            .Select(element => element.Attribute("name")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToList();
    }

    private static decimal ParseMoney(string text)
    {
        var digits = Regex.Matches(text ?? string.Empty, @"\d+")
            .Select(match => match.Value)
            .ToArray();

        if (digits.Length == 0)
        {
            return 0m;
        }

        return decimal.Parse(string.Concat(digits), CultureInfo.InvariantCulture);
    }
}
