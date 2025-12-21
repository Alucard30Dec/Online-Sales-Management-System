using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Data;

public static class DbInitializer
{
    private static readonly string[] Modules =
    [
        "Dashboard","Admin","Suppliers","Customers","Employees","Categories","Units","Products",
        "Purchases","Invoices","Expenses","Attendance","Reports","Stock","Settings"
    ];

    private static readonly string[] Actions = ["Show", "Create", "Edit", "Delete"];

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();

        // 1) Apply migrations (DB schema)
        await db.Database.MigrateAsync();

        // 2) Ensure default Setting exists (sau khi DB đã có bảng Settings)
        await EnsureDefaultSettingAsync(db);

        // 3) Seed roles/groups/users (Admin)
        await EnsureSecuritySeedAsync(services, db);

        // 4) Seed sample data (idempotent)
        await SeedSampleDataAsync(db);
    }

    private static async Task EnsureDefaultSettingAsync(ApplicationDbContext db)
    {
        if (!await db.Settings.AnyAsync())
        {
            db.Settings.Add(new Setting
            {
                CompanyName = "Online Sales Management System",
                Currency = "VND",
                LogoPath = null
            });

            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureSecuritySeedAsync(IServiceProvider services, ApplicationDbContext db)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed role(s)
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        // Seed groups
        var superAdmin = await db.AdminGroups.FirstOrDefaultAsync(x => x.Name == "Super Admin");
        if (superAdmin == null)
        {
            superAdmin = new AdminGroup { Name = "Super Admin", Description = "All permissions." };
            db.AdminGroups.Add(superAdmin);
            await db.SaveChangesAsync();

            // wildcard permission
            db.GroupPermissions.Add(new GroupPermission
            {
                AdminGroupId = superAdmin.Id,
                Module = "*",
                Action = "*"
            });
            await db.SaveChangesAsync();
        }

        var salesStaff = await db.AdminGroups.FirstOrDefaultAsync(x => x.Name == "Sales Staff");
        if (salesStaff == null)
        {
            salesStaff = new AdminGroup { Name = "Sales Staff", Description = "Limited sales permissions." };
            db.AdminGroups.Add(salesStaff);
            await db.SaveChangesAsync();

            var perms = new[]
            {
                "Dashboard.Show",
                "Customers.Show",
                "Invoices.Show",
                "Invoices.Create",
                "Invoices.Edit",
                "Reports.Show"
            };

            foreach (var p in perms)
            {
                var parts = p.Split('.', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) continue;

                db.GroupPermissions.Add(new GroupPermission
                {
                    AdminGroupId = salesStaff.Id,
                    Module = parts[0],
                    Action = parts[1]
                });
            }

            await db.SaveChangesAsync();
        }

        // Seed default admin user
        var adminEmail = "admin@osms.local";
        var adminUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Admin",
                IsActive = true,
                AdminGroupId = superAdmin.Id
            };

            var createRes = await userManager.CreateAsync(adminUser, "Admin@12345");
            if (!createRes.Succeeded)
            {
                var errors = string.Join("; ", createRes.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Failed to create default admin. {errors}");
            }

            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            if (adminUser.AdminGroupId != superAdmin.Id)
            {
                adminUser.AdminGroupId = superAdmin.Id;
                adminUser.IsActive = true;
                await userManager.UpdateAsync(adminUser);
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Ensure wildcard permission exists
        var hasWildcard = await db.GroupPermissions.AnyAsync(p => p.AdminGroupId == superAdmin.Id && p.Module == "*" && p.Action == "*");
        if (!hasWildcard)
        {
            db.GroupPermissions.Add(new GroupPermission { AdminGroupId = superAdmin.Id, Module = "*", Action = "*" });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedSampleDataAsync(ApplicationDbContext db)
    {
        // Nếu đã có data rồi thì không seed nữa
        if (await db.Products.AnyAsync())
            return;

        // ===== Master data =====
        var catOil = new Category { Name = "Dầu nhớt", Description = "Các loại dầu động cơ", IsActive = true };
        var catBrake = new Category { Name = "Phanh", Description = "Má phanh, đĩa phanh", IsActive = true };
        var catFilter = new Category { Name = "Lọc", Description = "Lọc gió, lọc dầu, lọc xăng", IsActive = true };
        db.Categories.AddRange(catOil, catBrake, catFilter);

        var uBottle = new Unit { Name = "Chai", ShortName = "chai" };
        var uPiece = new Unit { Name = "Cái", ShortName = "cái" };
        var uSet = new Unit { Name = "Bộ", ShortName = "bộ" };
        db.Units.AddRange(uBottle, uPiece, uSet);

        var sup1 = new Supplier
        {
            Name = "Saigon Auto Parts",
            Phone = "0900000001",
            Email = "sales@saigonparts.local",
            Address = "TP.HCM"
        };
        var sup2 = new Supplier
        {
            Name = "VietLub Distributor",
            Phone = "0900000002",
            Email = "contact@vietlub.local",
            Address = "TP.HCM"
        };
        db.Suppliers.AddRange(sup1, sup2);

        var cust1 = new Customer { Name = "Nguyễn Văn A", Phone = "0911111111", Email = "a@test.local", Address = "Q1, TP.HCM", IsActive = true };
        var cust2 = new Customer { Name = "Trần Thị B", Phone = "0922222222", Email = "b@test.local", Address = "Q7, TP.HCM", IsActive = true };
        db.Customers.AddRange(cust1, cust2);

        var emp1 = new Employee { Name = "Lê Minh K", Phone = "0933333333", Email = "k@osms.local", Address = "TP.HCM", IsActive = true };
        var emp2 = new Employee { Name = "Phạm Hải N", Phone = "0944444444", Email = "n@osms.local", Address = "TP.HCM", IsActive = true };
        db.Employees.AddRange(emp1, emp2);

        await db.SaveChangesAsync();

        // ===== Products (stock starts 0, will be increased by Purchase seed) =====
        var p1 = new Product
        {
            Name = "Castrol GTX 10W-40 4L",
            SKU = "OIL-001",
            CategoryId = catOil.Id,
            UnitId = uBottle.Id,
            CostPrice = 210000m,
            SalePrice = 270000m,
            StockOnHand = 0,
            IsActive = true
        };

        var p2 = new Product
        {
            Name = "Lọc dầu Toyota",
            SKU = "FIL-001",
            CategoryId = catFilter.Id,
            UnitId = uPiece.Id,
            CostPrice = 45000m,
            SalePrice = 65000m,
            StockOnHand = 0,
            IsActive = true
        };

        var p3 = new Product
        {
            Name = "Bộ má phanh trước",
            SKU = "BRK-001",
            CategoryId = catBrake.Id,
            UnitId = uSet.Id,
            CostPrice = 320000m,
            SalePrice = 420000m,
            StockOnHand = 0,
            IsActive = true
        };

        db.Products.AddRange(p1, p2, p3);
        await db.SaveChangesAsync();

        // ===== Purchase (Received) to increase stock =====
        var purchase = new Purchase
        {
            PurchaseNo = "PUR-0001",
            SupplierId = sup1.Id,
            PurchaseDate = DateTime.UtcNow.Date.AddDays(-7),
            Status = PurchaseStatus.Received
        };

        purchase.Items.Add(new PurchaseItem { ProductId = p1.Id, Qty = 20, UnitCost = p1.CostPrice, LineTotal = 20 * p1.CostPrice });
        purchase.Items.Add(new PurchaseItem { ProductId = p2.Id, Qty = 50, UnitCost = p2.CostPrice, LineTotal = 50 * p2.CostPrice });
        purchase.Items.Add(new PurchaseItem { ProductId = p3.Id, Qty = 10, UnitCost = p3.CostPrice, LineTotal = 10 * p3.CostPrice });

        purchase.SubTotal = purchase.Items.Sum(x => x.LineTotal);
        purchase.GrandTotal = purchase.SubTotal;

        db.Purchases.Add(purchase);
        await db.SaveChangesAsync();

        // Apply stock movements for purchase
        var now = DateTime.UtcNow;
        foreach (var it in purchase.Items)
        {
            var prod = await db.Products.FirstAsync(x => x.Id == it.ProductId);
            prod.StockOnHand += it.Qty;

            db.StockMovements.Add(new StockMovement
            {
                ProductId = it.ProductId,
                MovementDate = now,
                Type = StockMovementType.In,
                Qty = it.Qty,
                RefType = "Purchase",
                RefId = purchase.Id,
                Note = $"Seed receive {purchase.PurchaseNo}"
            });
        }
        await db.SaveChangesAsync();

        // ===== Invoice to decrease stock =====
        var invoice = new Invoice
        {
            InvoiceNo = "INV-0001",
            CustomerId = cust1.Id,
            InvoiceDate = DateTime.UtcNow.Date.AddDays(-2),
            PaidAmount = 0m,
            Status = InvoiceStatus.Unpaid
        };

        invoice.Items.Add(new InvoiceItem { ProductId = p1.Id, Qty = 2, UnitPrice = p1.SalePrice, LineTotal = 2 * p1.SalePrice });
        invoice.Items.Add(new InvoiceItem { ProductId = p2.Id, Qty = 1, UnitPrice = p2.SalePrice, LineTotal = 1 * p2.SalePrice });

        invoice.SubTotal = invoice.Items.Sum(x => x.LineTotal);
        invoice.GrandTotal = invoice.SubTotal;

        // giả lập đã thanh toán một phần
        invoice.PaidAmount = invoice.GrandTotal - 50000m;
        invoice.Status = invoice.PaidAmount <= 0 ? InvoiceStatus.Unpaid
                     : invoice.PaidAmount >= invoice.GrandTotal ? InvoiceStatus.Paid
                     : InvoiceStatus.PartiallyPaid;

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();

        // Apply stock movements for invoice
        foreach (var it in invoice.Items)
        {
            var prod = await db.Products.FirstAsync(x => x.Id == it.ProductId);
            prod.StockOnHand -= it.Qty;

            db.StockMovements.Add(new StockMovement
            {
                ProductId = it.ProductId,
                MovementDate = now,
                Type = StockMovementType.Out,
                Qty = it.Qty,
                RefType = "Invoice",
                RefId = invoice.Id,
                Note = $"Seed sell {invoice.InvoiceNo}"
            });
        }

        // ===== Expenses sample =====
        db.Expenses.AddRange(
            new Expense { Title = "Tiền điện", Amount = 850000m, ExpenseDate = DateTime.UtcNow.Date.AddDays(-5), Note = "Tháng này" },
            new Expense { Title = "Văn phòng phẩm", Amount = 250000m, ExpenseDate = DateTime.UtcNow.Date.AddDays(-3), Note = "Bút, giấy" }
        );

        await db.SaveChangesAsync();
    }
}
