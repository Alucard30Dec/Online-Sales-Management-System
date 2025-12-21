using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;
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

    // ========= INDEX =========
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        q ??= "";
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .OrderByDescending(i => i.Id)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var qq = q.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNo.ToLower().Contains(qq) ||
                (i.Customer != null && i.Customer.Name.ToLower().Contains(qq)));
        }

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    // ========= DETAILS / PRINT =========
    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Items).ThenInclude(it => it.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound();
        return View(invoice);
    }

    public async Task<IActionResult> Print(int id)
    {
        var invoice = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Items).ThenInclude(it => it.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound();
        return View(invoice);
    }

    // ========= CREATE =========
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
        if (vm.Items == null || vm.Items.Count == 0)
        {
            ModelState.AddModelError("", "Please add at least 1 item.");
        }
        else if (vm.Items.Any(x => x.ProductId == null))
        {
            ModelState.AddModelError("", "Please select product for all items.");
        }

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync();
            return View(vm);
        }

        using var tx = await _db.Database.BeginTransactionAsync();

        var customer = vm.CustomerId.HasValue
            ? await _db.Customers.FirstOrDefaultAsync(c => c.Id == vm.CustomerId.Value)
            : null;

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
                Quantity = qty,
                UnitPrice = price,
                LineTotal = lineTotal
            });
        }

        invoice.SubTotal = subTotal;
        invoice.GrandTotal = subTotal;

        // normalize payment + status
        if (invoice.GrandTotal <= 0)
        {
            invoice.PaidAmount = 0;
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.PaidAmount >= invoice.GrandTotal)
        {
            invoice.PaidAmount = invoice.GrandTotal;
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }
        else
        {
            invoice.Status = InvoiceStatus.Unpaid;
        }

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        // reduce stock + StockMovement (Out)
        foreach (var it in invoice.Items)
        {
            var product = await _db.Products.FirstAsync(p => p.Id == it.ProductId);
            if (product.StockOnHand < it.Quantity)
            {
                await tx.RollbackAsync();
                TempData["ToastError"] = $"Not enough stock for '{product.Name}'. Current stock: {product.StockOnHand}.";
                await LoadLookupsAsync();
                return View(vm);
            }

            product.StockOnHand -= it.Quantity;

            _db.StockMovements.Add(new StockMovement
            {
                ProductId = it.ProductId,
                MovementDate = DateTime.UtcNow,
                Type = StockMovementType.Out,
                Qty = it.Quantity,
                RefType = "Invoice",
                RefId = invoice.Id,
                Note = invoice.InvoiceNo
            });
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        TempData["ToastSuccess"] = "Invoice created successfully.";
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    // ========= RECORD PAYMENT =========
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(int id, decimal amount)
    {
        if (amount <= 0)
        {
            TempData["ToastError"] = "Amount must be greater than 0.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null) return NotFound();
        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            TempData["ToastError"] = "Cancelled invoice cannot be paid.";
            return RedirectToAction(nameof(Details), new { id });
        }

        invoice.PaidAmount += amount;

        if (invoice.GrandTotal <= 0)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.PaidAmount >= invoice.GrandTotal)
        {
            invoice.PaidAmount = invoice.GrandTotal;
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }
        else
        {
            invoice.Status = InvoiceStatus.Unpaid;
        }

        await _db.SaveChangesAsync();
        TempData["ToastSuccess"] = "Payment recorded.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ========= CANCEL =========
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Delete)]
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
            TempData["ToastInfo"] = "Invoice already cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        using var tx = await _db.Database.BeginTransactionAsync();

        // return stock + StockMovement (In)
        foreach (var it in invoice.Items)
        {
            var product = await _db.Products.FirstAsync(p => p.Id == it.ProductId);
            product.StockOnHand += it.Quantity;

            _db.StockMovements.Add(new StockMovement
            {
                ProductId = it.ProductId,
                MovementDate = DateTime.UtcNow,
                Type = StockMovementType.In,
                Qty = it.Quantity,
                RefType = "InvoiceCancel",
                RefId = invoice.Id,
                Note = invoice.InvoiceNo
            });
        }

        invoice.Status = InvoiceStatus.Cancelled;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        TempData["ToastSuccess"] = "Invoice cancelled and stock returned.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ========= HELPERS =========
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

    private Task<string> GenerateInvoiceNoAsync()
        => Task.FromResult($"INV-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}");

    // ===== ViewModels for Create =====
    public sealed class InvoiceCreateVm
    {
        public int? CustomerId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow.Date;

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
