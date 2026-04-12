using OpenQA.Selenium;
using OSMS.UITests.Pages;
using OSMS.UITests.Support;

namespace OSMS.UITests.Tests;

public sealed class PurchaseProductImportCoverageTests : UiTestBase
{
    [Fact]
    [Trait("CaseId", "TC-UI-PUR-002")]
    public void PurchaseCreateRejectsMissingSupplier()
    {
        ExecuteWithFailureCapture("TC-UI-PUR-002", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Purchases/Create"));
            Wait.UrlContains("/Admin/Purchases/Create");

            SelectByContains(By.CssSelector("#itemsTable tbody tr:first-child select"), "SP010");
            SetValue(By.CssSelector("#itemsTable tbody tr:first-child input[name$='.Qty']"), "5");
            SetValue(By.CssSelector("#itemsTable tbody tr:first-child input[name$='.UnitCost']"), "1716000");

            SubmitForm(By.CssSelector("form[action='/Admin/Purchases/Create']"));

            Wait.UrlContains("/Admin/Purchases/Create");
            var alertText = Wait.Visible(By.CssSelector(".alert.alert-danger")).Text;
            Assert.Contains("Supplier is required.", alertText, StringComparison.OrdinalIgnoreCase);
            CaptureCheckpoint("TC-UI-PUR-002-missing-supplier");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PUR-003")]
    public void PurchaseCreateRejectsMissingValidItems()
    {
        ExecuteWithFailureCapture("TC-UI-PUR-003", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Purchases/Create"));
            Wait.UrlContains("/Admin/Purchases/Create");

            SelectByContains(By.Id("supplierSelect"), "Nhà cung cấp 1");
            SubmitForm(By.CssSelector("form[action='/Admin/Purchases/Create']"));

            Wait.UrlContains("/Admin/Purchases/Create");
            var alertText = Wait.Visible(By.CssSelector(".alert.alert-danger")).Text;
            Assert.Contains("Add at least one item.", alertText, StringComparison.OrdinalIgnoreCase);
            CaptureCheckpoint("TC-UI-PUR-003-missing-items");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PUR-004")]
    public void ReceiveDraftPurchaseUpdatesStatusAndStock()
    {
        ExecuteWithFailureCapture("TC-UI-PUR-004", () =>
        {
            LoginAsAdmin();

            var stockBefore = ReadStockValue("SP010");
            var createPage = new PurchaseCreatePage(Driver, Wait, Settings).Open();
            createPage.FillDraftPurchase("Nhà cung cấp 1", "SP010", 2, 1716000m);

            var detailsPage = createPage.Submit();
            detailsPage.WaitUntilLoaded();
            Wait.BodyContains("Bản nháp");

            ClickElement(By.CssSelector("form[action*='/Admin/Purchases/Receive/'] button[type='submit']"));
            AcceptAlertIfPresent();
            Wait.BodyContains("Đã nhập kho");

            var stockAfter = ReadStockValue("SP010");
            Assert.True(stockAfter >= stockBefore + 2, $"Expected stock after receiving the purchase to increase by at least 2. Before={stockBefore}, After={stockAfter}.");
            CaptureCheckpoint("TC-UI-PUR-004-received");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PUR-005")]
    public void CancelReceivedPurchaseIsBlocked()
    {
        ExecuteWithFailureCapture("TC-UI-PUR-005", () =>
        {
            LoginAsAdmin();

            var createPage = new PurchaseCreatePage(Driver, Wait, Settings).Open();
            createPage.FillDraftPurchase("Nhà cung cấp 1", "SP010", 1, 1716000m);

            var detailsPage = createPage.Submit();
            detailsPage.WaitUntilLoaded();

            ClickElement(By.CssSelector("form[action*='/Admin/Purchases/Receive/'] button[type='submit']"));
            AcceptAlertIfPresent();
            Wait.BodyContains("Đã nhập kho");

            var currentUrl = Driver.Url;
            var purchaseId = currentUrl.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
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
                Settings.ToAbsoluteUri($"/Admin/Purchases/Cancel/{purchaseId}").ToString(),
                requestVerificationToken);

            Wait.UrlContains($"/Admin/Purchases/Details/{purchaseId}");
            Wait.BodyContains("Cannot cancel a Received purchase");
            Wait.BodyContains("Đã nhập kho");
            CaptureCheckpoint("TC-UI-PUR-005-cancel-received-blocked");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PUR-006")]
    public void CancelDraftPurchaseSucceedsWithoutStockUpdate()
    {
        ExecuteWithFailureCapture("TC-UI-PUR-006", () =>
        {
            LoginAsAdmin();

            var stockBefore = ReadStockValue("SP010");
            var createPage = new PurchaseCreatePage(Driver, Wait, Settings).Open();
            createPage.FillDraftPurchase("Nhà cung cấp 1", "SP010", 1, 1716000m);

            var detailsPage = createPage.Submit();
            detailsPage.WaitUntilLoaded();
            Wait.BodyContains("Bản nháp");

            ClickElement(By.CssSelector("form[action*='/Admin/Purchases/Cancel/'] button[type='submit']"));
            AcceptAlertIfPresent();
            Wait.BodyContains("Đã hủy");

            var stockAfter = ReadStockValue("SP010");
            Assert.Equal(stockBefore, stockAfter);
            CaptureCheckpoint("TC-UI-PUR-006-cancel-draft");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PRD-003")]
    public void ProductCreateRejectsMissingName()
    {
        ExecuteWithFailureCapture("TC-UI-PRD-003", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Products/Create"));
            Wait.UrlContains("/Admin/Products/Create");

            SetValue(By.Name("SKU"), $"QABLANK{DateTime.UtcNow:yyyyMMddHHmmss}");
            SelectByContains(By.Name("CategoryId"), "Phụ kiện");
            SelectByContains(By.Name("UnitId"), "Cái");
            SelectByContains(By.Name("BrandId"), "Acer");
            SetValue(By.Name("CostPrice"), "100000");
            SetValue(By.Name("SalePrice"), "120000");
            SetValue(By.Name("ReorderLevel"), "5");

            SubmitForm(By.CssSelector("form[action='/Admin/Products/Create']"));

            Wait.UrlContains("/Admin/Products/Create");
            Wait.BodyContains("The Name field is required.");
            CaptureCheckpoint("TC-UI-PRD-003-missing-name");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PRD-006")]
    public void ProductTrendingTogglePersistsAfterRefresh()
    {
        ExecuteWithFailureCapture("TC-UI-PRD-006", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Products?q=SP012&pageSize=10"));
            Wait.UrlContains("/Admin/Products");
            Wait.BodyContains("SP012");

            var trendingToggle = Wait.Visible(By.CssSelector("tbody tr .form-check-input[type='checkbox']"));
            var initialValue = trendingToggle.Selected;
            trendingToggle.Click();

            Wait.Condition(
                driver =>
                {
                    var checkbox = driver.FindElement(By.CssSelector("tbody tr .form-check-input[type='checkbox']"));
                    return checkbox.Selected != initialValue;
                },
                "Timed out waiting for the trending toggle to change state.");

            Driver.Navigate().Refresh();
            Wait.BodyContains("SP012");

            var refreshedValue = Wait.Visible(By.CssSelector("tbody tr .form-check-input[type='checkbox']")).Selected;
            Assert.Equal(!initialValue, refreshedValue);
            CaptureCheckpoint("TC-UI-PRD-006-trending-toggled");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-IMP-003")]
    public void ProductImportConfirmImportsValidPreviewRows()
    {
        ExecuteWithFailureCapture("TC-UI-IMP-003", () =>
        {
            LoginAsAdmin();

            var workbookPath = TestData.GetAbsolutePath("UI-DATA-IMP-001");
            var previewPage = new ProductImportPage(Driver, Wait, Settings).Open().UploadAndPreview(workbookPath);
            previewPage.WaitUntilLoaded();

            ClickElement(By.CssSelector("form[action='/Admin/Products/ImportExcelConfirm'] button[type='submit']"));
            Wait.Condition(
                driver => driver.Url.Contains("/Admin/Products", StringComparison.OrdinalIgnoreCase)
                    || driver.Url.Contains("/Admin/Products/ImportExcel", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the product import confirm flow to finish.");

            if (Driver.Url.Contains("/Admin/Products/ImportExcel", StringComparison.OrdinalIgnoreCase))
            {
                Wait.BodyContains("Import thất bại");
                throw new Xunit.Sdk.XunitException("Expected the valid preview rows to import successfully, but the system returned to the import page with a failure message.");
            }

            Wait.BodyContains("QAIMP001");
            CaptureCheckpoint("TC-UI-IMP-003-imported");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-IMP-004")]
    public void ProductImportConfirmRejectsMissingPreviewData()
    {
        ExecuteWithFailureCapture("TC-UI-IMP-004", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Products/ImportExcel"));
            Wait.UrlContains("/Admin/Products/ImportExcel");
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
                const cacheKeyInput = document.createElement('input');
                cacheKeyInput.type = 'hidden';
                cacheKeyInput.name = 'cacheKey';
                cacheKeyInput.value = 'expired-preview-key';
                form.appendChild(cacheKeyInput);
                document.body.appendChild(form);
                form.submit();
                """,
                Settings.ToAbsoluteUri("/Admin/Products/ImportExcelConfirm").ToString(),
                requestVerificationToken);

            Wait.UrlContains("/Admin/Products/ImportExcel");
            Wait.Condition(
                driver =>
                {
                    var body = driver.FindElement(By.TagName("body")).Text;
                    return body.Contains("preview", StringComparison.OrdinalIgnoreCase)
                        || body.Contains("upload", StringComparison.OrdinalIgnoreCase)
                        || body.Contains("hết hạn", StringComparison.OrdinalIgnoreCase);
                },
                "Timed out waiting for the expired-preview validation message on the import page.");
            CaptureCheckpoint("TC-UI-IMP-004-expired-preview");
        });
    }

    private int ReadStockValue(string sku)
    {
        Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri($"/Admin/Stock?q={sku}&pageSize=10"));
        Wait.UrlContains("/Admin/Stock");
        Wait.BodyContains(sku);

        var stockCellText = Wait.Visible(By.XPath($"//tr[.//td[contains(normalize-space(),'{sku}')]]/td[6]")).Text;
        var digits = new string(stockCellText.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? 0 : int.Parse(digits);
    }
}
