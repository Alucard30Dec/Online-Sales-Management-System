using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OSMS.UITests.Pages;
using OSMS.UITests.Support;

namespace OSMS.UITests.Tests;

public sealed class AdditionalCrudCoverageTests : UiTestBase
{
    private static readonly By CustomerCreateSubmitButton = By.CssSelector("form[action='/Admin/Customers/Create'] button[type='submit']");
    private static readonly By SupplierCreateSubmitButton = By.CssSelector("form[action='/Admin/Suppliers/Create'] button[type='submit']");
    private static readonly By ProductCreateSubmitButton = By.CssSelector("form[action='/Admin/Products/Create'] button[type='submit']");

    [Fact]
    [Trait("CaseId", "TC-UI-CUS-001")]
    public void CreateCustomerSucceedsWithValidInformation()
    {
        ExecuteWithFailureCapture("TC-UI-CUS-001", () =>
        {
            LoginAsAdmin();

            var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var customerName = $"QA Customer Auto {suffix}";
            var customerEmail = $"qa.customer.{suffix}@example.com";

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Customers/Create"));
            Wait.UrlContains("/Admin/Customers/Create");

            SetValue(By.Name("Name"), customerName);
            SetValue(By.Name("Phone"), "0909001234");
            SetValue(By.Name("Email"), customerEmail);
            SetValue(By.Name("Address"), "Da Nang");
            ClickElement(CustomerCreateSubmitButton);

            Wait.Condition(
                driver => driver.Url.Contains("/Admin/Customers", StringComparison.OrdinalIgnoreCase)
                    && !driver.Url.Contains("/Admin/Customers/Create", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the customer create flow to redirect back to the customer list.");
            Wait.BodyContains(customerName);

            CaptureCheckpoint("TC-UI-CUS-001-created");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-SUP-001")]
    public void CreateSupplierSucceedsWithValidInformation()
    {
        ExecuteWithFailureCapture("TC-UI-SUP-001", () =>
        {
            LoginAsAdmin();

            var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var supplierName = $"QA Supplier Auto {suffix}";
            var supplierEmail = $"qa.supplier.{suffix}@example.com";

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Suppliers/Create"));
            Wait.UrlContains("/Admin/Suppliers/Create");

            SetValue(By.Name("Name"), supplierName);
            SetValue(By.Name("Phone"), "0911001234");
            SetValue(By.Name("Email"), supplierEmail);
            SetValue(By.Name("Address"), "Ho Chi Minh City");
            ClickElement(SupplierCreateSubmitButton);

            Wait.Condition(
                driver => driver.Url.Contains("/Admin/Suppliers", StringComparison.OrdinalIgnoreCase)
                    && !driver.Url.Contains("/Admin/Suppliers/Create", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the supplier create flow to redirect back to the supplier list.");
            Wait.BodyContains(supplierName);

            CaptureCheckpoint("TC-UI-SUP-001-created");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-GRP-001")]
    public void SuperAdminGroupEditIsBlocked()
    {
        ExecuteWithFailureCapture("TC-UI-GRP-001", () =>
        {
            LoginAsAdmin();

            Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/AdminGroups/Edit/1"));
            Wait.Condition(
                driver => driver.Url.Contains("/Admin/AdminGroups", StringComparison.OrdinalIgnoreCase)
                    && !driver.Url.Contains("/Admin/AdminGroups/Edit/1", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the protected Super Admin group edit route to redirect.");
            Wait.BodyContains("Super Admin");
            Wait.BodyContains("cannot edit");

            CaptureCheckpoint("TC-UI-GRP-001-protected");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PRD-001")]
    public void CreateProductSucceedsWithValidMandatoryFields()
    {
        ExecuteWithFailureCapture("TC-UI-PRD-001", () =>
        {
            LoginAsAdmin();

            var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var sku = $"QAAUTO{suffix}";
            var name = $"QA Product Auto {suffix}";

            OpenProductCreate();
            SetProductCreateValues(
                sku: sku,
                name: name,
                categoryText: "Phụ kiện",
                unitText: "Cái",
                brandText: "Acer",
                costPrice: "100000",
                salePrice: "120000",
                reorderLevel: "5");
            ClickElement(ProductCreateSubmitButton);

            Wait.Condition(
                driver => driver.Url.Contains("/Admin/Products", StringComparison.OrdinalIgnoreCase)
                    && !driver.Url.Contains("/Admin/Products/Create", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the product create flow to redirect back to the product list.");
            Wait.BodyContains(sku);

            CaptureCheckpoint("TC-UI-PRD-001-created");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PRD-002")]
    public void ProductCreateRejectsDuplicateSku()
    {
        ExecuteWithFailureCapture("TC-UI-PRD-002", () =>
        {
            LoginAsAdmin();

            OpenProductCreate();
            SetProductCreateValues(
                sku: "SP001",
                name: "Duplicate SKU Validation",
                categoryText: "Phụ kiện",
                unitText: "Cái",
                brandText: "Acer",
                costPrice: "100000",
                salePrice: "120000",
                reorderLevel: "5");
            ClickElement(ProductCreateSubmitButton);

            Wait.UrlContains("/Admin/Products/Create");
            Wait.BodyContains("SKU already exists.");

            CaptureCheckpoint("TC-UI-PRD-002-duplicate-sku");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PRD-004")]
    public void ProductCreateRejectsNegativePriceValues()
    {
        ExecuteWithFailureCapture("TC-UI-PRD-004", () =>
        {
            LoginAsAdmin();

            var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            OpenProductCreate();
            SetProductCreateValues(
                sku: $"QANEG{suffix}",
                name: $"Negative Price Product {suffix}",
                categoryText: "Phụ kiện",
                unitText: "Cái",
                brandText: "Acer",
                costPrice: "-1000",
                salePrice: "-2000",
                reorderLevel: "-1");
            ClickElement(ProductCreateSubmitButton);

            Wait.UrlContains("/Admin/Products/Create");
            Wait.BodyContains("Cost price cannot be negative.");
            Wait.BodyContains("Sale price cannot be negative.");
            Wait.BodyContains("Reorder level cannot be negative.");

            CaptureCheckpoint("TC-UI-PRD-004-negative-price");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-PRD-005")]
    public void ProductCreateRejectsUnsupportedImageType()
    {
        ExecuteWithFailureCapture("TC-UI-PRD-005", () =>
        {
            LoginAsAdmin();

            var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            OpenProductCreate();
            SetProductCreateValues(
                sku: $"QAIMG{suffix}",
                name: $"Invalid Image Product {suffix}",
                categoryText: "Phụ kiện",
                unitText: "Cái",
                brandText: "Acer",
                costPrice: "100000",
                salePrice: "120000",
                reorderLevel: "5");

            var imagePath = TestData.GetAbsolutePath("UI-DATA-IMG-001");
            Wait.Visible(By.Name("imageFile")).SendKeys(imagePath);
            ClickElement(ProductCreateSubmitButton);

            Wait.UrlContains("/Admin/Products/Create");
            Wait.BodyContains("Image type not supported.");

            CaptureCheckpoint("TC-UI-PRD-005-image-type");
        });
    }

    [Fact]
    [Trait("CaseId", "TC-UI-IMP-001")]
    public void ProductImportPreviewRejectsNonXlsxUpload()
    {
        ExecuteWithFailureCapture("TC-UI-IMP-001", () =>
        {
            LoginAsAdmin();

            var importPage = new ProductImportPage(Driver, Wait, Settings).Open();
            var csvPath = TestData.GetAbsolutePath("UI-DATA-IMP-002");

            var fileInput = Wait.Visible(By.CssSelector("form[action='/Admin/Products/ImportExcelPreview'] input[type='file'][name='file']"));
            fileInput.SendKeys(csvPath);
            ClickElement(By.CssSelector("form[action='/Admin/Products/ImportExcelPreview'] button[type='submit']"));

            Wait.UrlContains("/Admin/Products/ImportExcel");
            Wait.Condition(
                driver => driver.PageSource.Contains(".xlsx", StringComparison.OrdinalIgnoreCase)
                    || driver.FindElement(By.TagName("body")).Text.Contains("xlsx", StringComparison.OrdinalIgnoreCase),
                "Timed out waiting for the non-xlsx validation message to appear on the import page.");

            CaptureCheckpoint("TC-UI-IMP-001-non-xlsx");
        });
    }

    private void LoginAsAdmin()
    {
        var credential = TestData.GetCredential("UI-DATA-ACC-001");
        var loginPage = new LoginPage(Driver, Wait, Settings).Open();
        loginPage.Login(credential);

        Wait.Condition(
            driver => driver.Url.Contains("/Admin", StringComparison.OrdinalIgnoreCase)
                && !driver.Url.Contains("/Admin/Auth/Login", StringComparison.OrdinalIgnoreCase),
            "Timed out waiting for the admin account to finish the login redirect.");
    }

    private void OpenProductCreate()
    {
        Driver.Navigate().GoToUrl(Settings.ToAbsoluteUri("/Admin/Products/Create"));
        Wait.UrlContains("/Admin/Products/Create");
        Wait.Visible(By.Name("Name"));
    }

    private void SetProductCreateValues(
        string sku,
        string name,
        string categoryText,
        string unitText,
        string brandText,
        string costPrice,
        string salePrice,
        string reorderLevel)
    {
        SetValue(By.Name("Name"), name);
        SetValue(By.Name("SKU"), sku);
        SelectByContains(By.Name("CategoryId"), categoryText);
        SelectByContains(By.Name("UnitId"), unitText);
        SelectByContains(By.Name("BrandId"), brandText);
        SetValue(By.Name("CostPrice"), costPrice);
        SetValue(By.Name("SalePrice"), salePrice);
        SetValue(By.Name("ReorderLevel"), reorderLevel);
    }

    private void SetValue(By locator, string value)
    {
        var element = Wait.Visible(locator);
        element.Clear();
        element.SendKeys(value);
    }

    private void SelectByContains(By locator, string partialText)
    {
        var select = new SelectElement(Wait.Visible(locator));
        var option = select.Options.FirstOrDefault(o =>
            o.GetAttribute("value") != "0"
            && !string.IsNullOrWhiteSpace(o.GetAttribute("value"))
            && o.Text.Contains(partialText, StringComparison.OrdinalIgnoreCase));

        if (option == null)
        {
            throw new NoSuchElementException($"No option containing '{partialText}' was found for select '{locator}'.");
        }

        select.SelectByText(option.Text);
    }

    private void ClickElement(By locator)
    {
        var element = Wait.Visible(locator);
        if (Driver is IJavaScriptExecutor js)
        {
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
        }

        try
        {
            element.Click();
        }
        catch (ElementClickInterceptedException)
        {
            if (Driver is IJavaScriptExecutor clickJs)
            {
                clickJs.ExecuteScript("arguments[0].click();", element);
            }
            else
            {
                throw;
            }
        }
    }
}
