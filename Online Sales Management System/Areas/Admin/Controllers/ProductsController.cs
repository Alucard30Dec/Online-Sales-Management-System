using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;

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

    // ====== DETAILS: thông tin sản phẩm + lịch sử hóa đơn có sản phẩm này ======
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();

        var orders = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .Where(i => i.Items.Any(it => it.ProductId == id))
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new ProductOrderRow
            {
                InvoiceId = i.Id,
                InvoiceNo = i.InvoiceNo,
                CustomerId = i.CustomerId,
                CustomerName = i.Customer != null ? i.Customer.Name : "Khách lẻ",
                InvoiceDate = i.InvoiceDate,
                Qty = i.Items.Where(it => it.ProductId == id).Sum(it => it.Quantity),
                UnitPrice = i.Items.Where(it => it.ProductId == id).Select(it => it.UnitPrice).FirstOrDefault(),
                LineTotal = i.Items.Where(it => it.ProductId == id).Sum(it => it.LineTotal),
                Status = i.Status
            })
            .ToListAsync();

        var vm = new ProductDetailsVm
        {
            Product = product,
            Orders = orders,
            TotalSoldQty = orders.Sum(x => x.Qty),
            TotalRevenue = orders.Sum(x => x.LineTotal)
        };

        return View(vm);
    }

    // ==========================================================
    // CÁC HÀM EDIT ĐÃ ĐƯỢC BỔ SUNG ĐỂ SỬA LỖI 404
    // ==========================================================

    // 1. GET: Hiển thị form chỉnh sửa
    [HttpGet]
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Edit)]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        // Lấy danh sách danh mục & đơn vị tính để hiển thị dropdown
        ViewBag.Categories = await _db.Categories.ToListAsync();
        ViewBag.Units = await _db.Units.ToListAsync();

        return View(product);
    }

    // 2. POST: Xử lý lưu thay đổi
    // Lưu ý: Route("Edit/{id?}") giúp chấp nhận cả URL có ID (Edit/10) và không ID (Edit)
    [HttpPost("Edit/{id?}")]
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Edit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product model)
    {
        // Tìm sản phẩm cũ trong database
        var existing = await _db.Products.FindAsync(model.Id);
        if (existing == null) return NotFound();

        // Cập nhật thông tin mới
        existing.SKU = model.SKU;
        existing.Name = model.Name;
        existing.CategoryId = model.CategoryId;
        existing.UnitId = model.UnitId;
        existing.Description = model.Description;
        existing.SalePrice = model.SalePrice;
        existing.CostPrice = model.CostPrice;
        existing.ReorderLevel = model.ReorderLevel;
        existing.ImagePath = model.ImagePath;
        existing.IsTrending = model.IsTrending;
        existing.IsActive = model.IsActive;

        // Lưu xuống database
        _db.Products.Update(existing);
        await _db.SaveChangesAsync();

        // Quay về trang danh sách
        return RedirectToAction(nameof(Index));
    }

    // ==========================================================

    // ====== TOGGLE TRENDING ======
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

    // ====== VIEWMODELS ======
    public sealed class ProductDetailsVm
    {
        public Product Product { get; set; } = default!;
        public List<ProductOrderRow> Orders { get; set; } = new();
        public int TotalSoldQty { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public sealed class ProductOrderRow
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = "";
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime InvoiceDate { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public InvoiceStatus Status { get; set; }
    }
}