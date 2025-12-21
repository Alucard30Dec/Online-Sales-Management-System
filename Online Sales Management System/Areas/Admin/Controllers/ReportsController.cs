using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Reports + "." + PermissionConstants.Actions.Show)]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var f = (from ?? DateTime.UtcNow.Date.AddDays(-30)).Date;
        var t = (to ?? DateTime.UtcNow.Date).Date;

        if (t < f)
        {
            var tmp = f; f = t; t = tmp;
        }

        // Sales = invoices not cancelled
        var invoicesQuery = _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Where(i => i.InvoiceDate >= f && i.InvoiceDate <= t && i.Status != InvoiceStatus.Cancelled);

        // Purchases = purchases received (or all non-cancelled)
        var purchasesQuery = _db.Purchases
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => p.PurchaseDate >= f && p.PurchaseDate <= t && p.Status != PurchaseStatus.Cancelled);

        var invoices = await invoicesQuery
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.Id)
            .Take(200)
            .ToListAsync();

        var purchases = await purchasesQuery
            .OrderByDescending(p => p.PurchaseDate)
            .ThenByDescending(p => p.Id)
            .Take(200)
            .ToListAsync();

        var salesTotal = await invoicesQuery.SumAsync(x => (decimal?)x.GrandTotal) ?? 0m;
        var purchaseTotal = await purchasesQuery.SumAsync(x => (decimal?)x.GrandTotal) ?? 0m;

        // Rough profit: Sales - Purchases (basic)
        var profit = salesTotal - purchaseTotal;

        var vm = new ReportsIndexVm
        {
            From = f,
            To = t,
            SalesTotal = salesTotal,
            PurchaseTotal = purchaseTotal,
            Profit = profit,
            Invoices = invoices,
            Purchases = purchases
        };

        return View(vm);
    }

    public sealed class ReportsIndexVm
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public decimal SalesTotal { get; set; }
        public decimal PurchaseTotal { get; set; }
        public decimal Profit { get; set; }

        public List<Invoice> Invoices { get; set; } = new();
        public List<Purchase> Purchases { get; set; } = new();
    }
}
