using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required]
    public int UnitId { get; set; }
    public Unit? Unit { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CostPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SalePrice { get; set; }

    public int StockOnHand { get; set; } = 0;

    public int ReorderLevel { get; set; } = 5;

    public string? ImagePath { get; set; }

    public bool IsActive { get; set; } = true;
}