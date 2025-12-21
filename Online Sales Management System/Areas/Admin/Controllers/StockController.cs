// FILE: OnlineSalesManagementSystem/Areas/Admin/Controllers/StockController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Stock + "." + PermissionConstants.Actions.Show)]
public class StockController : Controller
{
    private readonly ApplicationDbContext _db;

    public StockController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.SKU.Contains(q) || p.Name.Contains(q));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Low(string? q)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Where(p => p.IsActive && p.StockOnHand <= p.ReorderLevel)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.SKU.Contains(q) || p.Name.Contains(q));
        }

        var items = await query.OrderBy(p => p.Name).ToListAsync();
        ViewBag.Query = q;

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Movements(int? productId, DateTime? from, DateTime? to)
    {
        var f = from?.Date;
        var t = to?.Date;

        var query = _db.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .AsQueryable();

        if (productId.HasValue && productId.Value > 0)
            query = query.Where(m => m.ProductId == productId.Value);

        if (f.HasValue)
            query = query.Where(m => m.MovementDate >= f.Value);

        if (t.HasValue)
            query = query.Where(m => m.MovementDate <= t.Value.AddDays(1).AddTicks(-1));

        var items = await query
            .OrderByDescending(m => m.MovementDate)
            .ThenByDescending(m => m.Id)
            .Take(1000)
            .ToListAsync();

        ViewBag.ProductId = productId;
        ViewBag.From = f?.ToString("yyyy-MM-dd");
        ViewBag.To = t?.ToString("yyyy-MM-dd");

        ViewBag.Products = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(items);
    }
}
