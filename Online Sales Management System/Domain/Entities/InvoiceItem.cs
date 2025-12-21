using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class InvoiceItem
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    // Alias cho các nơi đang dùng Qty (DbContext đã Ignore Qty):contentReference[oaicite:2]{index=2}
    [NotMapped]
    public int Qty
    {
        get => Quantity;
        set => Quantity = value;
    }
    public decimal LineTotal { get; set; }

}
