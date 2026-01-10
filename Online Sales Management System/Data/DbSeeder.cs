using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;

namespace OnlineSalesManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // =========================================================================
            // 1. SEED BASIC DATA (Units, Categories)
            // =========================================================================

            // Seed Units (Đơn vị tính)
            if (!await db.Units.AnyAsync())
            {
                var units = new List<Unit>
                {
                    new Unit { Name = "Cái", ShortName = "cái" },
                    new Unit { Name = "Hộp", ShortName = "hộp" },
                    new Unit { Name = "Bộ", ShortName = "bộ" },
                    new Unit { Name = "Chiếc", ShortName = "chiếc" },
                    new Unit { Name = "Kg", ShortName = "kg" },
                    new Unit { Name = "Thùng", ShortName = "thùng" },
                    new Unit { Name = "Chai", ShortName = "chai" }
                };
                db.Units.AddRange(units);
                await db.SaveChangesAsync();
            }

            // Seed Categories (Danh mục)
            if (!await db.Categories.AnyAsync())
            {
                var cats = new List<Category>
                {
                    new Category { Name = "Điện thoại", Description = "Smartphone các loại", IsActive = true, IsTrending = true },
                    new Category { Name = "Laptop", Description = "Máy tính xách tay & Macbook", IsActive = true, IsTrending = true },
                    new Category { Name = "Phụ kiện", Description = "Tai nghe, sạc, cáp, ốp lưng", IsActive = true, IsTrending = false },
                    new Category { Name = "Đồ gia dụng", Description = "Nồi cơm, quạt, máy lọc không khí", IsActive = true, IsTrending = false },
                    new Category { Name = "Thời trang", Description = "Quần áo, giày dép", IsActive = true, IsTrending = true }
                };
                db.Categories.AddRange(cats);
                await db.SaveChangesAsync();
            }

            // =========================================================================
            // 2. SEED ADMIN GROUPS (ROLES) & PERMISSIONS
            // =========================================================================

            // --- A. SUPER ADMIN ---
            var superGroup = await db.AdminGroups.FirstOrDefaultAsync(g => g.Name == "Super Admin");
            if (superGroup == null)
            {
                superGroup = new AdminGroup { Name = "Super Admin", Description = "Full System Access" };
                db.AdminGroups.Add(superGroup);
                await db.SaveChangesAsync();
            }

            // Cấp quyền Wildcard (*.*) cho Super Admin nếu chưa có
            if (!await db.GroupPermissions.AnyAsync(p => p.AdminGroupId == superGroup.Id && p.Module == PermissionConstants.Wildcard))
            {
                db.GroupPermissions.Add(new GroupPermission
                {
                    AdminGroupId = superGroup.Id,
                    Module = PermissionConstants.Wildcard,
                    Action = PermissionConstants.Wildcard
                });
            }

            // --- B. WAREHOUSE STAFF (YÊU CẦU CỦA BẠN) ---
            var warehouseGroup = await db.AdminGroups.FirstOrDefaultAsync(g => g.Name == "Warehouse Staff");
            if (warehouseGroup == null)
            {
                warehouseGroup = new AdminGroup { Name = "Warehouse Staff", Description = "Quản lý kho, nhập hàng, sản phẩm" };
                db.AdminGroups.Add(warehouseGroup);
                await db.SaveChangesAsync();
            }

            // Xóa quyền cũ của Warehouse (nếu có) để seed lại cho chuẩn
            var whExistingPerms = await db.GroupPermissions.Where(p => p.AdminGroupId == warehouseGroup.Id).ToListAsync();
            db.GroupPermissions.RemoveRange(whExistingPerms);

            // Danh sách các Module mà Kho được phép truy cập
            var whModules = new[]
            {
                PermissionConstants.Modules.Products,
                PermissionConstants.Modules.Stock,
                PermissionConstants.Modules.Purchases,
                PermissionConstants.Modules.Suppliers,
                PermissionConstants.Modules.Units,
                PermissionConstants.Modules.Categories
            };

            foreach (var mod in whModules)
            {
                // Cấp các quyền cơ bản: Xem, Thêm, Sửa, Xuất excel
                db.GroupPermissions.Add(new GroupPermission { AdminGroupId = warehouseGroup.Id, Module = mod, Action = PermissionConstants.Actions.Show });
                db.GroupPermissions.Add(new GroupPermission { AdminGroupId = warehouseGroup.Id, Module = mod, Action = PermissionConstants.Actions.Create });
                db.GroupPermissions.Add(new GroupPermission { AdminGroupId = warehouseGroup.Id, Module = mod, Action = PermissionConstants.Actions.Edit });
                db.GroupPermissions.Add(new GroupPermission { AdminGroupId = warehouseGroup.Id, Module = mod, Action = PermissionConstants.Actions.Export });
            }
            // Cho phép xem Dashboard để login không bị lỗi, nhưng không cho thao tác gì khác
            db.GroupPermissions.Add(new GroupPermission { AdminGroupId = warehouseGroup.Id, Module = PermissionConstants.Modules.Dashboard, Action = PermissionConstants.Actions.Show });


            // --- C. SALES STAFF ---
            var salesGroup = await db.AdminGroups.FirstOrDefaultAsync(g => g.Name == "Sales Staff");
            if (salesGroup == null)
            {
                salesGroup = new AdminGroup { Name = "Sales Staff", Description = "Nhân viên kinh doanh" };
                db.AdminGroups.Add(salesGroup);
                await db.SaveChangesAsync();
            }
            // Seed quyền Sales nếu cần (Logic tương tự Warehouse nhưng đổi Module thành Customers, Invoices...)

            await db.SaveChangesAsync();

            // =========================================================================
            // 3. SEED USERS
            // =========================================================================
            async Task CreateUser(string email, string pass, string name, int? groupId)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = name,
                        IsActive = true,
                        EmailConfirmed = true,
                        AdminGroupId = groupId
                    };
                    await userManager.CreateAsync(user, pass);
                }
            }

            await CreateUser("admin@osms.local", "Admin@12345", "Super Administrator", superGroup.Id);
            await CreateUser("warehouse@osms.local", "Warehouse@12345", "Trưởng Kho", warehouseGroup.Id);
            await CreateUser("sales@osms.local", "Sales@12345", "Nhân viên Sales", salesGroup.Id);


            // =========================================================================
            // 4. SEED BUSINESS DATA (RICH DATA FOR TESTING)
            // =========================================================================

            // A. Suppliers (Nhà cung cấp)
            if (!await db.Suppliers.AnyAsync())
            {
                var suppliers = new List<Supplier>();
                for (int i = 1; i <= 5; i++)
                {
                    suppliers.Add(new Supplier
                    {
                        Name = $"Nhà cung cấp {i}",
                        Phone = $"098877766{i}",
                        Email = $"supplier{i}@partner.com",
                        Address = $"KCN Số {i}, Hà Nội",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                db.Suppliers.AddRange(suppliers);
                await db.SaveChangesAsync();
            }

            // B. Customers (Khách hàng)
            if (!await db.Customers.AnyAsync())
            {
                var customers = new List<Customer>();
                for (int i = 1; i <= 10; i++)
                {
                    customers.Add(new Customer
                    {
                        Name = $"Khách hàng {i}",
                        Phone = $"090512345{i}",
                        Email = $"customer{i}@gmail.com",
                        Address = $"Số {i} đường ABC, TP.HCM",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 100))
                    });
                }
                db.Customers.AddRange(customers);
                await db.SaveChangesAsync();
            }

            // C. Products (Sản phẩm - tạo nhiều để test phân trang)
            if (!await db.Products.AnyAsync())
            {
                var catIds = await db.Categories.Select(c => c.Id).ToListAsync();
                var unitIds = await db.Units.Select(u => u.Id).ToListAsync();

                if (catIds.Any() && unitIds.Any())
                {
                    var products = new List<Product>();
                    var random = new Random();

                    for (int i = 1; i <= 30; i++)
                    {
                        var cost = random.Next(10, 500) * 10000; // 100k -> 5M
                        var sale = cost + (cost * random.Next(10, 40) / 100); // Lãi 10-40%

                        products.Add(new Product
                        {
                            SKU = $"SP{i:000}",
                            Name = $"Sản phẩm Test {i}",
                            Description = $"Mô tả chi tiết cho sản phẩm {i}. Hàng chất lượng cao.",
                            CostPrice = cost,
                            SalePrice = sale,
                            StockOnHand = 0, // Sẽ được cập nhật khi seed Purchase
                            ReorderLevel = 10,
                            IsActive = true,
                            IsTrending = i % 5 == 0,
                            CategoryId = catIds[random.Next(catIds.Count)],
                            UnitId = unitIds[random.Next(unitIds.Count)],
                            ImagePath = null // Để null hoặc đường dẫn ảnh dummy
                        });
                    }
                    db.Products.AddRange(products);
                    await db.SaveChangesAsync();
                }
            }

            // D. Purchases (Lịch sử nhập kho - quan trọng cho Warehouse)
            if (!await db.Purchases.AnyAsync())
            {
                var products = await db.Products.ToListAsync();
                var suppliers = await db.Suppliers.ToListAsync();
                var random = new Random();

                // Tạo 20 đơn nhập hàng trong quá khứ
                for (int i = 0; i < 20; i++)
                {
                    var date = DateTime.UtcNow.AddDays(-random.Next(1, 60)); // Trong vòng 60 ngày
                    var supplier = suppliers[random.Next(suppliers.Count)];

                    var purchase = new Purchase
                    {
                        PurchaseNo = $"PO-{date:yyyyMMdd}-{random.Next(1000, 9999)}",
                        SupplierId = supplier.Id,
                        PurchaseDate = date,
                        Status = PurchaseStatus.Received, // Đã nhập kho
                        Items = new List<PurchaseItem>()
                    };

                    decimal subTotal = 0;
                    // Mỗi đơn nhập 3-5 loại sản phẩm
                    for (int j = 0; j < random.Next(3, 6); j++)
                    {
                        var prod = products[random.Next(products.Count)];
                        var qty = random.Next(10, 100);
                        var cost = prod.CostPrice;

                        purchase.Items.Add(new PurchaseItem
                        {
                            ProductId = prod.Id,
                            Qty = qty,
                            UnitCost = cost,
                            LineTotal = qty * cost
                        });

                        subTotal += qty * cost;

                        // CẬP NHẬT TỒN KHO THẬT
                        prod.StockOnHand += qty;

                        // TẠO STOCK MOVEMENT (Lịch sử kho)
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = prod.Id,
                            MovementDate = date,
                            Type = StockMovementType.In,
                            Qty = qty,
                            RefType = "Purchase",
                            Note = $"Nhập hàng theo đơn {purchase.PurchaseNo}"
                        });
                    }
                    purchase.SubTotal = subTotal;
                    purchase.GrandTotal = subTotal;

                    db.Purchases.Add(purchase);
                }
                await db.SaveChangesAsync();
            }

            // E. Invoices (Lịch sử bán hàng - Tạo doanh thu để Dashboard đẹp)
            if (!await db.Invoices.AnyAsync())
            {
                var products = await db.Products.Where(p => p.StockOnHand > 0).ToListAsync();
                var customers = await db.Customers.ToListAsync();
                var random = new Random();

                // Tạo 50 đơn hàng bán ra
                for (int i = 0; i < 50; i++)
                {
                    var date = DateTime.UtcNow.AddDays(-random.Next(0, 30));
                    var cust = customers[random.Next(customers.Count)];

                    var invoice = new Invoice
                    {
                        InvoiceNo = $"INV-{date:yyyyMMdd}-{random.Next(1000, 9999)}",
                        CustomerId = cust.Id,
                        InvoiceDate = date,
                        Status = InvoiceStatus.Paid,
                        Items = new List<InvoiceItem>()
                    };

                    decimal subTotal = 0;
                    // Khách mua 1-3 món
                    for (int j = 0; j < random.Next(1, 4); j++)
                    {
                        if (!products.Any()) break;
                        var prod = products[random.Next(products.Count)];

                        if (prod.StockOnHand <= 0) continue; // Hết hàng thì bỏ qua

                        var qty = random.Next(1, 5);
                        if (qty > prod.StockOnHand) qty = prod.StockOnHand; // Không bán quá tồn kho

                        var price = prod.SalePrice;

                        invoice.Items.Add(new InvoiceItem
                        {
                            ProductId = prod.Id,
                            Quantity = qty,
                            UnitPrice = price,
                            LineTotal = qty * price
                        });

                        subTotal += qty * price;

                        // TRỪ TỒN KHO
                        prod.StockOnHand -= qty;

                        // GHI LỊCH SỬ KHO
                        db.StockMovements.Add(new StockMovement
                        {
                            ProductId = prod.Id,
                            MovementDate = date,
                            Type = StockMovementType.Out,
                            Qty = qty,
                            RefType = "Invoice",
                            Note = $"Xuất bán đơn {invoice.InvoiceNo}"
                        });
                    }

                    if (invoice.Items.Count > 0)
                    {
                        invoice.SubTotal = subTotal;
                        invoice.GrandTotal = subTotal;
                        invoice.PaidAmount = subTotal;
                        db.Invoices.Add(invoice);
                    }
                }
                await db.SaveChangesAsync();
            }
        }
    }
}