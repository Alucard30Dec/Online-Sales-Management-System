using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Services.Inventory;

public sealed class StockService : IStockService
{
    private readonly ApplicationDbContext _db;

    public StockService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task IncreaseStockAsync(int productId, int qty, string refType, int refId, string? note = null, CancellationToken ct = default)
        => ApplyDeltaAsync(productId, Math.Abs(qty), StockMovementType.In, refType, refId, note, ct);

    public Task DecreaseStockAsync(int productId, int qty, string refType, int refId, string? note = null, CancellationToken ct = default)
        => ApplyDeltaAsync(productId, -Math.Abs(qty), StockMovementType.Out, refType, refId, note, ct);

    public Task AdjustStockAsync(int productId, int deltaQty, string refType, int refId, string? note = null, CancellationToken ct = default)
        => ApplyDeltaAsync(productId, deltaQty, StockMovementType.Adjust, refType, refId, note, ct);

    private async Task ApplyDeltaAsync(
        int productId,
        int deltaQty,
        StockMovementType movementType,
        string refType,
        int refId,
        string? note,
        CancellationToken ct)
    {
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
        if (deltaQty == 0) return;
        if (string.IsNullOrWhiteSpace(refType)) refType = "System";

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product == null) throw new InvalidOperationException($"Product not found: #{productId}");

        var newStock = product.StockOnHand + deltaQty;
        if (newStock < 0)
            throw new InvalidOperationException($"Insufficient stock for Product #{productId}. Current={product.StockOnHand}, Delta={deltaQty}");

        product.StockOnHand = newStock;

        _db.StockMovements.Add(new StockMovement
        {
            ProductId = productId,
            MovementDate = DateTime.UtcNow,
            Type = movementType,
            Qty = Math.Abs(deltaQty),
            RefType = refType,
            RefId = refId,
            Note = note
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
