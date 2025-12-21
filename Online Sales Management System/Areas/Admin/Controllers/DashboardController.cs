using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Services.Security;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Dashboard + "." + PermissionConstants.Actions.Show)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.AdminsCount = await _db.Users.CountAsync(u => u.IsActive);
        ViewBag.ProductsCount = await _db.Products.CountAsync(p => p.IsActive);
        ViewBag.CustomersCount = await _db.Customers.CountAsync(c => c.IsActive);
        ViewBag.SuppliersCount = await _db.Suppliers.CountAsync(s => s.IsActive);
        ViewBag.EmployeesCount = await _db.Employees.CountAsync(e => e.IsActive);

        ViewBag.PurchasesCount = await _db.Purchases.CountAsync();
        ViewBag.InvoicesCount = await _db.Invoices.CountAsync();
        ViewBag.ExpensesCount = await _db.Expenses.CountAsync();

        ViewBag.LowStockCount = await _db.Products.CountAsync(p => p.IsActive && p.StockOnHand <= p.ReorderLevel);

        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var todaySales = await _db.Invoices
            .Where(i => i.InvoiceDate >= today && i.InvoiceDate < today.AddDays(1) && i.Status != Domain.Entities.InvoiceStatus.Cancelled)
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0m;

        var monthSales = await _db.Invoices
            .Where(i => i.InvoiceDate >= monthStart && i.InvoiceDate < monthStart.AddMonths(1) && i.Status != Domain.Entities.InvoiceStatus.Cancelled)
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0m;

        ViewBag.TodaySales = todaySales;
        ViewBag.MonthSales = monthSales;

        return View();
    }
}
