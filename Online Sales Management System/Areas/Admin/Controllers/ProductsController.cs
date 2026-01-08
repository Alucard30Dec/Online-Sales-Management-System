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
    private readonly IWebHostEnvironment _env;

    public ProductsController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
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
            .OrderByDescending(p => p.IsTrending)
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
            IsTrending = false
        });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
    {
        await LoadLookupsAsync();

        // ===== Normalize =====
        model.SKU = (model.SKU ?? "").Trim();
        model.Name = (model.Name ?? "").Trim();
        model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

        // Nếu bạn muốn cho nhập link ảnh thủ công thì giữ ImagePath,
        // còn nếu ưu tiên upload file thì ImagePath sẽ set lại khi upload.
        model.ImagePath = string.IsNullOrWhiteSpace(model.ImagePath) ? null : model.ImagePath.Trim();

        if (model.StockOnHand < 0) model.StockOnHand = 0;
        if (model.ReorderLevel < 0) model.ReorderLevel = 0;
        if (model.CostPrice < 0) model.CostPrice = 0;
        if (model.SalePrice < 0) model.SalePrice = 0;

        // ===== Validate dropdown (int default = 0 vẫn "hợp lệ" theo ModelState) =====
        if (model.CategoryId <= 0)
            ModelState.AddModelError(nameof(model.CategoryId), "Vui lòng chọn danh mục.");

        if (model.UnitId <= 0)
            ModelState.AddModelError(nameof(model.UnitId), "Vui lòng chọn đơn vị.");

        // SKU thường nên bắt buộc (vì bạn đang check trùng).
        if (string.IsNullOrWhiteSpace(model.SKU))
            ModelState.AddModelError(nameof(model.SKU), "Vui lòng nhập SKU.");

        if (!ModelState.IsValid)
            return View(model);

        // ===== Check tồn tại Category/Unit =====
        var catOk = await _db.Categories.AsNoTracking().AnyAsync(c => c.Id == model.CategoryId && c.IsActive);
        if (!catOk)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Danh mục không hợp lệ hoặc đã bị vô hiệu hoá.");
            return View(model);
        }

        var unitOk = await _db.Units.AsNoTracking().AnyAsync(u => u.Id == model.UnitId);
        if (!unitOk)
        {
            ModelState.AddModelError(nameof(model.UnitId), "Đơn vị không hợp lệ.");
            return View(model);
        }

        // ===== Check SKU trùng =====
        var skuExists = await _db.Products.AnyAsync(p => p.SKU == model.SKU);
        if (skuExists)
        {
            ModelState.AddModelError(nameof(model.SKU), "SKU already exists.");
            return View(model);
        }

        // ===== Upload ảnh (nếu có) =====
        if (imageFile != null && imageFile.Length > 0)
        {
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp" };

            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError(string.Empty, "Ảnh sản phẩm phải là .png/.jpg/.jpeg/.webp");
                return View(model);
            }

            var uploads = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploads);

            var fileName = $"prd_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploads, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fs);
            }

            model.ImagePath = $"/uploads/products/{fileName}";
        }

        // ===== Business defaults =====
        // Nếu bạn muốn luôn active khi tạo mới, giữ dòng này.
        // Nếu muốn theo checkbox form, bỏ dòng này.
        model.IsActive = true;

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
        entity.IsTrending = model.IsTrending;

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }

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
        // Create.cshtml đang dùng ViewBag.Categories/Units
        // => ta sẽ trả về LIST để view tự loop (an toàn, không phụ thuộc SelectListItem).
        ViewBag.Categories = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Units = await _db.Units.AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();
    }
}
