using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        // Only seed once (feel free to delete DB to re-seed)
        if (await db.Products.AnyAsync())
            return;

        // ===== Master data =====
        var categories = new[]
        {
            new Category { Name = "Auto Parts", Description = "Vehicle spare parts" },
            new Category { Name = "Engine Oil", Description = "Lubricants & oils" },
            new Category { Name = "Electronics", Description = "Accessories & gadgets" },
            new Category { Name = "Tools", Description = "Garage tools" }
        };
        db.Categories.AddRange(categories);

        var units = new[]
        {
            new Unit { Name = "Piece", ShortName = "pc" },
            new Unit { Name = "Bottle", ShortName = "btl" },
            new Unit { Name = "Box", ShortName = "box" }
        };
        db.Units.AddRange(units);

        var suppliers = new[]
        {
            new Supplier { Name = "AutoParts Viet Co.", Phone = "0900000001", Email = "supplier1@demo.local", Address = "HCMC" },
            new Supplier { Name = "Lubricants Pro Ltd.", Phone = "0900000002", Email = "supplier2@demo.local", Address = "Hanoi" }
        };
        db.Suppliers.AddRange(suppliers);

        var customers = new[]
        {
            new Customer { Name = "Walk-in Customer", Phone = "", Email = "", Address = "" },
            new Customer { Name = "Nguyen Van A", Phone = "0901234567", Email = "a@demo.local", Address = "District 1" },
            new Customer { Name = "Tran Thi B", Phone = "0902345678", Email = "b@demo.local", Address = "District 7" }
        };
        db.Customers.AddRange(customers);

        var employees = new[]
        {
            new Employee { Name = "Le Thanh Sales", Position = "Sales", Salary = 12000000, Phone = "0901111111", Email = "sales@demo.local", Address = "HCMC" },
            new Employee { Name = "Pham Kho", Position = "Warehouse", Salary = 11000000, Phone = "0902222222", Email = "warehouse@demo.local", Address = "HCMC" }
        };
        db.Employees.AddRange(employees);

        await db.SaveChangesAsync();

        var catAuto = categories.First(c => c.Name == "Auto Parts");
        var catOil = categories.First(c => c.Name == "Engine Oil");
        var catElec = categories.First(c => c.Name == "Electronics");
        var unitPc = units.First(u => u.Name == "Piece");
        var unitBtl = units.First(u => u.Name == "Bottle");
        var unitBox = units.First(u => u.Name == "Box");

        var products = new[]
        {
            new Product { SKU="BRK-001", Name="Brake Pads (Front)", CategoryId=catAuto.Id, UnitId=unitBox.Id, SalePrice=450000, CostPrice=320000, StockOnHand=20, ReorderLevel=5, IsActive=true },
            new Product { SKU="FLT-002", Name="Oil Filter", CategoryId=catAuto.Id, UnitId=unitPc.Id, SalePrice=120000, CostPrice=80000, StockOnHand=50, ReorderLevel=10, IsActive=true },
            new Product { SKU="OIL-5W30", Name="Engine Oil 5W-30", CategoryId=catOil.Id, UnitId=unitBtl.Id, SalePrice=210000, CostPrice=160000, StockOnHand=40, ReorderLevel=8, IsActive=true },
            new Product { SKU="BAT-12V", Name="Car Battery 12V", CategoryId=catAuto.Id, UnitId=unitPc.Id, SalePrice=1450000, CostPrice=1100000, StockOnHand=10, ReorderLevel=3, IsActive=true },
            new Product { SKU="CAM-USB", Name="USB Dash Cam", CategoryId=catElec.Id, UnitId=unitPc.Id, SalePrice=990000, CostPrice=750000, StockOnHand=8, ReorderLevel=2, IsActive=true }
        };
        db.Products.AddRange(products);

        await db.SaveChangesAsync();

        // ===== One received purchase (stock in) =====
        var sup1 = suppliers[0];
        var purchase = new Purchase
        {
            PurchaseNo = $"PUR-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
            SupplierId = sup1.Id,
            PurchaseDate = DateTime.UtcNow.Date.AddDays(-2),
            Status = PurchaseStatus.Received
        };

        purchase.Items.Add(new PurchaseItem
        {
            ProductId = products[0].Id,
            Qty = 10,
            UnitCost = products[0].CostPrice,
            LineTotal = 10 * products[0].CostPrice
        });
        purchase.Items.Add(new PurchaseItem
        {
            ProductId = products[2].Id,
            Qty = 12,
            UnitCost = products[2].CostPrice,
            LineTotal = 12 * products[2].CostPrice
        });

        purchase.SubTotal = purchase.Items.Sum(i => i.LineTotal);

        db.Purchases.Add(purchase);
        await db.SaveChangesAsync();

        foreach (var it in purchase.Items)
        {
            var prod = await db.Products.FirstAsync(p => p.Id == it.ProductId);
            prod.StockOnHand += it.Qty;

            db.StockMovements.Add(new StockMovement
            {
                ProductId = it.ProductId,
                MovementDate = DateTime.UtcNow.Date.AddDays(-2),
                Type = StockMovementType.In,
                Qty = it.Qty,
                RefType = "Purchase",
                RefId = purchase.Id,
                Note = purchase.PurchaseNo
            });
        }
        await db.SaveChangesAsync();

        // ===== One invoice (stock out) =====
        var inv = new Invoice
        {
            InvoiceNo = $"INV-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
            CustomerId = customers[1].Id,
            InvoiceDate = DateTime.UtcNow.Date.AddDays(-1),
            Status = InvoiceStatus.Paid
        };

        inv.Items.Add(new InvoiceItem
        {
            ProductId = products[1].Id,
            Quantity = 2,
            UnitPrice = products[1].SalePrice,
            LineTotal = 2 * products[1].SalePrice
        });
        inv.Items.Add(new InvoiceItem
        {
            ProductId = products[2].Id,
            Quantity = 1,
            UnitPrice = products[2].SalePrice,
            LineTotal = 1 * products[2].SalePrice
        });

        inv.SubTotal = inv.Items.Sum(i => i.LineTotal);
        inv.GrandTotal = inv.SubTotal;
        inv.PaidAmount = inv.GrandTotal;

        db.Invoices.Add(inv);
        await db.SaveChangesAsync();

        foreach (var it in inv.Items)
        {
            var prod = await db.Products.FirstAsync(p => p.Id == it.ProductId);
            if (prod.StockOnHand >= it.Quantity)
            {
                prod.StockOnHand -= it.Quantity;
            }

            db.StockMovements.Add(new StockMovement
            {
                ProductId = it.ProductId,
                MovementDate = DateTime.UtcNow.Date.AddDays(-1),
                Type = StockMovementType.Out,
                Qty = it.Quantity,
                RefType = "Invoice",
                RefId = inv.Id,
                Note = inv.InvoiceNo
            });
        }
        await db.SaveChangesAsync();

        // ===== One expense + attendance =====
        db.Expenses.Add(new Expense
        {
            Title = "Electricity bill",
            Amount = 850000,
            ExpenseDate = DateTime.UtcNow.Date.AddDays(-1),
            Note = "Demo expense record"
        });

        await db.SaveChangesAsync();
    }
}
