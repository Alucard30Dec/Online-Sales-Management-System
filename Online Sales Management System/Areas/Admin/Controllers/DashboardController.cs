using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;

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

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Lightweight KPIs (views will be added later)
        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        ViewBag.ProductsCount = await _db.Products.CountAsync(p => p.IsActive);
        ViewBag.CustomersCount = await _db.Customers.CountAsync(c => c.IsActive);
        ViewBag.SuppliersCount = await _db.Suppliers.CountAsync(s => s.IsActive);

        ViewBag.TodaySales = await _db.Invoices
            .Where(i => i.Status != Domain.Entities.InvoiceStatus.Cancelled && i.InvoiceDate >= today && i.InvoiceDate < today.AddDays(1))
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0m;

        ViewBag.MonthSales = await _db.Invoices
            .Where(i => i.Status != Domain.Entities.InvoiceStatus.Cancelled && i.InvoiceDate >= firstDayOfMonth && i.InvoiceDate < firstDayOfMonth.AddMonths(1))
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0m;

        return View();
    }
}
