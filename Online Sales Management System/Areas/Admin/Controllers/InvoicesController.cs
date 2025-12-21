// FILE: OnlineSalesManagementSystem/Areas/Admin/Controllers/InvoicesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Show)]
public class InvoicesController : Controller
{
    private readonly ApplicationDbContext _db;

    public InvoicesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? status, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Invoices.AsNoTracking()
            .Include(i => i.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(i =>
                i.InvoiceNo.Contains(q) ||
                (i.Customer != null && i.Customer.Name.Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvoiceStatus>(status, true, out var st))
        {
            query = query.Where(i => i.Status == st);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Status = status;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadLookupsAsync();
        return View(new InvoiceCreateVm
        {
            InvoiceDate = DateTime.UtcNow.Date,
            Items = new List<InvoiceItemVm> { new() },
            PaidAmount = 0m
        });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceCreateVm vm)
    {
        await LoadLookupsAsync();

        vm.Items ??= new();
        vm.Items = vm.Items
            .Where(x => x.ProductId.HasValue && x.Qty > 0 && x.UnitPrice >= 0)
            .ToList();

        if (vm.InvoiceDate == default)
            ModelState.AddModelError(nameof(vm.InvoiceDate), "Invoice date is required.");

        if (vm.Items.Count == 0)
            ModelState.AddModelError(nameof(vm.Items), "Add at least one item.");

        if (vm.PaidAmount < 0)
            ModelState.AddModelError(nameof(vm.PaidAmount), "Paid amount cannot be negative.");

        if (!ModelState.IsValid)
            return View(vm);

        // Customer optional (walk-in)
        Customer? customer = null;
        if (vm.CustomerId.HasValue)
        {
            customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == vm.CustomerId.Value && c.IsActive);
            if (customer == null)
            {
                ModelState.AddModelError(nameof(vm.CustomerId), "Customer not found or inactive.");
                return View(vm);
            }
        }

        // Load products (tracked because we will update stock)
        var productIds = vm.Items.Select(x => x.ProductId!.Value).Distinct().ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        if (products.Count != productIds.Count)
        {
            ModelState.AddModelError(string.Empty, "One or more selected products are invalid.");
            return View(vm);
        }

        // Stock check
        foreach (var row in vm.Items)
        {
            var prod = products.First(p => p.Id == row.ProductId!.Value);
            if (prod.StockOnHand < row.Qty)
            {
                ModelState.AddModelError(string.Empty,
                    $"Not enough stock for {prod.Name} (On hand: {prod.StockOnHand}, required: {row.Qty}).");
            }
        }
        if (!ModelState.IsValid)
            return View(vm);

        var invoice = new Invoice
        {
            InvoiceNo = await GenerateInvoiceNoAsync(),
            CustomerId = customer?.Id,
            InvoiceDate = vm.InvoiceDate.Date,
            PaidAmount = vm.PaidAmount
        };

        decimal subTotal = 0m;
        foreach (var row in vm.Items)
        {
            var qty = row.Qty;
            var price = row.UnitPrice;
            var lineTotal = qty * price;
            subTotal += lineTotal;

            invoice.Items.Add(new InvoiceItem
            {
                ProductId = row.ProductId!.Value,
                Quantity = qty,      // hoặc Qty = qty
                UnitPrice = price
                // KHÔNG gán LineTotal
            });
        }


        invoice.SubTotal = subTotal;
        invoice.GrandTotal = subTotal;

        if (invoice.GrandTotal <= 0)
            invoice.Status = InvoiceStatus.Paid;
        else if (invoice.PaidAmount >= invoice.GrandTotal)
            invoice.Status = InvoiceStatus.Paid;
        else if (invoice.PaidAmount > 0)
            invoice.Status = InvoiceStatus.PartiallyPaid;
        else
            invoice.Status = InvoiceStatus.Unpaid;

        await using var tx = await _db.Database.BeginTransactionAsync(); // transaction chuẩn EF 

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(); // sau SaveChanges thì invoice.Id đã có 

        // Decrease stock + write stock movement (Out)
        var now = DateTime.UtcNow;
        foreach (var item in invoice.Items)
        {
            var prod = products.First(p => p.Id == item.ProductId);
            prod.StockOnHand -= item.Quantity;

            _db.StockMovements.Add(new StockMovement
            {
                ProductId = prod.Id,
                MovementDate = now,
                Type = StockMovementType.Out,
                Qty = item.Quantity,
                RefType = "Invoice",
                RefId = invoice.Id,
                Note = $"Sell {invoice.InvoiceNo}"
            });
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        TempData["ToastSuccess"] = $"Invoice created: {invoice.InvoiceNo}";
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _db.Invoices.AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Items).ThenInclude(ii => ii.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound();
        return View(invoice);
    }

    [HttpGet]
    public async Task<IActionResult> Print(int id)
    {
        var invoice = await _db.Invoices.AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Items).ThenInclude(ii => ii.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound();
        return View(invoice);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(int id, decimal amount)
    {
        if (amount <= 0)
        {
            TempData["ToastError"] = "Payment amount must be greater than 0.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null) return NotFound();

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            TempData["ToastError"] = "Cannot record payment for a cancelled invoice.";
            return RedirectToAction(nameof(Details), new { id });
        }

        invoice.PaidAmount += amount;

        if (invoice.GrandTotal <= 0)
            invoice.Status = InvoiceStatus.Paid;
        else if (invoice.PaidAmount >= invoice.GrandTotal)
        {
            invoice.PaidAmount = invoice.GrandTotal;
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.PaidAmount > 0)
            invoice.Status = InvoiceStatus.PartiallyPaid;
        else
            invoice.Status = InvoiceStatus.Unpaid;

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Payment recorded.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound();

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            TempData["ToastError"] = "Invoice is already cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var productIds = invoice.Items.Select(x => x.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var item in invoice.Items)
        {
            var prod = products.First(p => p.Id == item.ProductId);
            prod.StockOnHand += item.Qty;

            _db.StockMovements.Add(new StockMovement
            {
                ProductId = prod.Id,
                MovementDate = now,
                Type = StockMovementType.In,
                Qty = item.Qty,
                RefType = "InvoiceCancel",
                RefId = invoice.Id,
                Note = $"Cancel {invoice.InvoiceNo}"
            });
        }

        invoice.Status = InvoiceStatus.Cancelled;
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Invoice cancelled and stock restored.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadLookupsAsync()
    {
        ViewBag.Customers = await _db.Customers.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Products = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    private async Task<string> GenerateInvoiceNoAsync()
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var rand = Random.Shared.Next(1000, 9999);
            var no = $"INV-{stamp}-{rand}";

            var exists = await _db.Invoices.AnyAsync(p => p.InvoiceNo == no);
            if (!exists) return no;

            await Task.Delay(20);
        }

        return $"INV-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
    }

    // ===== ViewModels for Create =====
    public sealed class InvoiceCreateVm
    {
        public int? CustomerId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime InvoiceDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; } = 0m;

        public List<InvoiceItemVm> Items { get; set; } = new();
    }

    public sealed class InvoiceItemVm
    {
        [Required]
        public int? ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Qty { get; set; } = 1;

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; } = 0m;
    }
}
