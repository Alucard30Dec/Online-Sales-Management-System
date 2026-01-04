using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Show)]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db)
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
            .OrderByDescending(p => p.IsTrending) // Ưu tiên Trending lên đầu
            .ThenBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadLookupsAsync();
        return View(new Product
        {
            IsActive = true,
            StockOnHand = 0,
            ReorderLevel = 5,
            IsTrending = false // Mặc định
        });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model)
    {
        await LoadLookupsAsync();

        model.SKU = (model.SKU ?? "").Trim();
        model.Name = (model.Name ?? "").Trim();
        model.ImagePath = string.IsNullOrWhiteSpace(model.ImagePath) ? null : model.ImagePath.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var skuExists = await _db.Products.AnyAsync(p => p.SKU == model.SKU);
        if (skuExists)
        {
            ModelState.AddModelError(nameof(model.SKU), "SKU already exists.");
            return View(model);
        }

        model.IsActive = true;
        if (model.StockOnHand < 0) model.StockOnHand = 0;
        if (model.ReorderLevel < 0) model.ReorderLevel = 0;
        // IsTrending tự bind từ form

        _db.Products.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Product created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        await LoadLookupsAsync();

        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (entity == null) return NotFound();

        return View(entity);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product model)
    {
        await LoadLookupsAsync();

        model.SKU = (model.SKU ?? "").Trim();
        model.Name = (model.Name ?? "").Trim();
        model.ImagePath = string.IsNullOrWhiteSpace(model.ImagePath) ? null : model.ImagePath.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == model.Id && p.IsActive);
        if (entity == null) return NotFound();

        var skuExists = await _db.Products.AnyAsync(p => p.Id != model.Id && p.SKU == model.SKU);
        if (skuExists)
        {
            ModelState.AddModelError(nameof(model.SKU), "SKU already exists.");
            return View(model);
        }

        entity.SKU = model.SKU;
        entity.Name = model.Name;
        entity.CategoryId = model.CategoryId;
        entity.UnitId = model.UnitId;
        entity.CostPrice = model.CostPrice;
        entity.SalePrice = model.SalePrice;
        entity.ReorderLevel = model.ReorderLevel < 0 ? 0 : model.ReorderLevel;
        entity.ImagePath = model.ImagePath;
        entity.IsTrending = model.IsTrending; // <--- Cập nhật Trending

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }

    // --- MỚI THÊM: Toggle Trending API ---
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    public async Task<IActionResult> ToggleTrending(int id)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return Json(new { success = false, message = "Not found" });

        entity.IsTrending = !entity.IsTrending;
        await _db.SaveChangesAsync();

        return Json(new { success = true, isTrending = entity.IsTrending });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return NotFound();

        entity.IsActive = false;
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Product deleted (disabled).";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookupsAsync()
    {
        ViewBag.Categories = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Units = await _db.Units.AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();
    }
}