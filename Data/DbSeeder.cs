using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;

namespace OnlineSalesManagementSystem.Data
{
    public static class DbSeeder
    {
        /// <summary>
        /// Seed dữ liệu mẫu (idempotent).
        /// - resetDemoData = true: xóa dữ liệu nghiệp vụ (products/invoices/purchases/...) rồi seed lại.
        ///   Không xóa Identity users để tránh mất tài khoản login.
        /// </summary>
        public static async Task SeedAsync(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            bool resetDemoData = false)
        {
            if (resetDemoData)
            {
                await ResetBusinessDataAsync(db);
            }
            // =========================================================================
            // 1. SEED BASIC DATA (Units, Categories)
            // =========================================================================

            // Seed Units (Đơn vị tính)
            if (!await db.Units.AnyAsync())
            {
                var units = new List<Unit>
                {
                    new Unit { Name = "Cái", ShortName = "cái", IsActive = true },
                    new Unit { Name = "Hộp", ShortName = "hộp", IsActive = true },
                    new Unit { Name = "Bộ", ShortName = "bộ", IsActive = true },
                    new Unit { Name = "Chiếc", ShortName = "chiếc", IsActive = true },
                    new Unit { Name = "Kg", ShortName = "kg", IsActive = true },
                    new Unit { Name = "Thùng", ShortName = "thùng", IsActive = true },
                    new Unit { Name = "Chai", ShortName = "chai", IsActive = true }
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

            // Seed Brands (Thương hiệu)
            if (!await db.Brands.AnyAsync())
            {
                var brands = new List<Brand>
                {
                    new Brand { Name = "Apple", IsActive = true },
                    new Brand { Name = "Samsung", IsActive = true },
                    new Brand { Name = "Xiaomi", IsActive = true },
                    new Brand { Name = "OPPO", IsActive = true },
                    new Brand { Name = "Vivo", IsActive = true },
                    new Brand { Name = "Dell", IsActive = true },
                    new Brand { Name = "HP", IsActive = true },
                    new Brand { Name = "Asus", IsActive = true },
                    new Brand { Name = "Acer", IsActive = true },
                    new Brand { Name = "Lenovo", IsActive = true },
                    new Brand { Name = "Logitech", IsActive = true },
                    new Brand { Name = "DareU", IsActive = true },
                    new Brand { Name = "Anker", IsActive = true },
                    new Brand { Name = "Sony", IsActive = true },
                    new Brand { Name = "Philips", IsActive = true },
                };

                // chống trùng do seed chạy lại
                brands = brands
                    .GroupBy(x => x.Name.Trim().ToUpperInvariant())
                    .Select(g => g.First())
                    .ToList();

                db.Brands.AddRange(brands);
                await db.SaveChangesAsync();
            }

            // =========================================================================
            // 2. SEED ADMIN GROUPS (ROLES) & PERMISSIONS
            // =========================================================================

            // --- A. SUPER ADMIN ---
            var superGroup = await db.AdminGroups.FirstOrDefaultAsync(g => g.Name == "Super Admin");
            if (superGroup == null)
            {
                superGroup = new AdminGroup { Name = "Super Admin", Description = "Full System Access", IsActive = true };
                db.AdminGroups.Add(superGroup);
                await db.SaveChangesAsync();
            }
            else if (!superGroup.IsActive)
            {
                superGroup.IsActive = true;
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
                warehouseGroup = new AdminGroup { Name = "Warehouse Staff", Description = "Quản lý kho, nhập hàng, sản phẩm", IsActive = true };
                db.AdminGroups.Add(warehouseGroup);
                await db.SaveChangesAsync();
            }
            else if (!warehouseGroup.IsActive)
            {
                warehouseGroup.IsActive = true;
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
                salesGroup = new AdminGroup { Name = "Sales Staff", Description = "Nhân viên kinh doanh", IsActive = true };
                db.AdminGroups.Add(salesGroup);
                await db.SaveChangesAsync();
            }
            else if (!salesGroup.IsActive)
            {
                salesGroup.IsActive = true;
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

            // Settings (Cài đặt hệ thống)
            if (!await db.Settings.AnyAsync())
            {
                db.Settings.Add(new Setting
                {
                    CompanyName = "OSMS Store",
                    Currency = "VND",
                    LogoPath = null
                });
                await db.SaveChangesAsync();
            }

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
                var unitIds = await db.Units.Where(u => u.IsActive).Select(u => u.Id).ToListAsync();
                var brandIds = await db.Brands.Where(b => b.IsActive).Select(b => b.Id).ToListAsync();

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
                            BrandId = brandIds.Any() ? brandIds[random.Next(brandIds.Count)] : null,
                            ImagePath = null // Để null hoặc đường dẫn ảnh dummy
                        });
                    }
                    db.Products.AddRange(products);
                    await db.SaveChangesAsync();
                }
            }

            // D0. Employees (Nhân viên)
            if (!await db.Employees.AnyAsync())
            {
                var employees = new List<Employee>
                {
                    new Employee { Name = "Nguyễn Văn An", Phone = "0903000001", Email = "an@company.local", Address = "TP.HCM", Position = "Sales", Salary = 9000000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-120) },
                    new Employee { Name = "Trần Thị Bình", Phone = "0903000002", Email = "binh@company.local", Address = "TP.HCM", Position = "Sales", Salary = 9500000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-110) },
                    new Employee { Name = "Lê Minh Châu", Phone = "0903000003", Email = "chau@company.local", Address = "Hà Nội", Position = "Warehouse", Salary = 10000000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-200) },
                    new Employee { Name = "Phạm Quốc Dũng", Phone = "0903000004", Email = "dung@company.local", Address = "Đà Nẵng", Position = "Warehouse", Salary = 10500000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-180) },
                    new Employee { Name = "Võ Thị Em", Phone = "0903000005", Email = "em@company.local", Address = "TP.HCM", Position = "Accountant", Salary = 12000000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-250) },
                    new Employee { Name = "Đặng Hữu Phước", Phone = "0903000006", Email = "phuoc@company.local", Address = "TP.HCM", Position = "Manager", Salary = 18000000, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-365) },
                };

                db.Employees.AddRange(employees);
                await db.SaveChangesAsync();
            }

            // D1. Attendance (Chấm công) - 30 ngày gần nhất
            if (!await db.Attendances.AnyAsync())
            {
                var employees = await db.Employees.Where(e => e.IsActive).Select(e => e.Id).ToListAsync();
                var random = new Random();
                var today = DateTime.UtcNow.Date;

                var attendances = new List<Attendance>();
                foreach (var empId in employees)
                {
                    for (int d = 0; d < 30; d++)
                    {
                        var date = today.AddDays(-d);
                        var roll = random.Next(0, 100);
                        var status = roll switch
                        {
                            < 80 => AttendanceStatus.Present,
                            < 88 => AttendanceStatus.Late,
                            < 95 => AttendanceStatus.Leave,
                            _ => AttendanceStatus.Absent
                        };

                        attendances.Add(new Attendance
                        {
                            EmployeeId = empId,
                            Date = date,
                            Status = status,
                            Note = status == AttendanceStatus.Late ? "Đi trễ" : null
                        });
                    }
                }

                db.Attendances.AddRange(attendances);
                await db.SaveChangesAsync();
            }

            // D2. Expenses (Chi phí) - để module Chi phí + báo cáo có dữ liệu
            if (!await db.Expenses.AnyAsync())
            {
                var random = new Random();
                var expenses = new List<Expense>();
                var now = DateTime.UtcNow;

                // Lương tháng gần nhất (tổng lương nhân viên)
                var totalSalary = await db.Employees.Where(e => e.IsActive).SumAsync(e => e.Salary);
                expenses.Add(new Expense
                {
                    Title = "Tổng lương nhân viên tháng này",
                    Amount = totalSalary,
                    ExpenseDate = now.AddDays(-3),
                    Note = "Chi phí vận hành: lương"
                });

                // Một số chi phí phát sinh
                var titles = new[]
                {
                    "Tiền điện",
                    "Tiền nước",
                    "Internet & dịch vụ",
                    "Thuê mặt bằng",
                    "Vận chuyển",
                    "Marketing",
                    "Văn phòng phẩm",
                    "Bảo trì thiết bị",
                    "Chi phí khác"
                };

                for (int i = 0; i < titles.Length; i++)
                {
                    expenses.Add(new Expense
                    {
                        Title = titles[i],
                        Amount = random.Next(2, 50) * 100000,
                        ExpenseDate = now.AddDays(-random.Next(1, 30)),
                        Note = "Seed demo",
                        IsActive = true
                    });
                }

                db.Expenses.AddRange(expenses);
                await db.SaveChangesAsync();
            }

            // E. Purchases (Lịch sử nhập kho - quan trọng cho Warehouse)
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

            // F. Invoices (Lịch sử bán hàng - Tạo doanh thu để Dashboard đẹp)
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

        private static async Task ResetBusinessDataAsync(ApplicationDbContext db)
        {
            // Xóa dữ liệu nghiệp vụ theo thứ tự phụ thuộc FK
            // (giữ nguyên Identity + nhóm quyền, tránh mất tài khoản login)

            // Stock
            db.StockMovements.RemoveRange(await db.StockMovements.ToListAsync());

            // Sales
            db.InvoiceItems.RemoveRange(await db.InvoiceItems.ToListAsync());
            db.Invoices.RemoveRange(await db.Invoices.ToListAsync());

            // Purchases
            db.PurchaseItems.RemoveRange(await db.PurchaseItems.ToListAsync());
            db.Purchases.RemoveRange(await db.Purchases.ToListAsync());

            // HR/Finance
            db.Attendances.RemoveRange(await db.Attendances.ToListAsync());
            db.Employees.RemoveRange(await db.Employees.ToListAsync());
            db.Expenses.RemoveRange(await db.Expenses.ToListAsync());

            // Masters
            db.Products.RemoveRange(await db.Products.ToListAsync());
            db.Brands.RemoveRange(await db.Brands.ToListAsync());
            db.Categories.RemoveRange(await db.Categories.ToListAsync());
            db.Units.RemoveRange(await db.Units.ToListAsync());
            db.Customers.RemoveRange(await db.Customers.ToListAsync());
            db.Suppliers.RemoveRange(await db.Suppliers.ToListAsync());
            db.Settings.RemoveRange(await db.Settings.ToListAsync());

            await db.SaveChangesAsync();
        }
    }
}
 