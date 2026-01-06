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
            // 1. Tạo các Role mặc định của Identity (nếu chưa có)
            string[] roles = { "Admin", "Staff" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Tạo Admin Group (Nhóm quyền trong hệ thống của bạn)
            if (!await db.AdminGroups.AnyAsync())
            {
                var superAdminGroup = new AdminGroup
                {
                    Name = "Super Admin",
                    Description = "Full quyền hệ thống",
                    Permissions = new List<GroupPermission>
                    {
                        // GrantAll = true (dựa trên logic code controller của bạn, thường là * *)
                        new GroupPermission { Module = "*", Action = "*" }
                    }
                };

                var salesGroup = new AdminGroup
                {
                    Name = "Sales Staff",
                    Description = "Nhân viên bán hàng",
                    Permissions = new List<GroupPermission>()
                };
                // Thêm quyền cụ thể cho Sales Staff (Ví dụ: Xem + Tạo đơn hàng, Khách hàng)
                var salesModules = new[] { PermissionConstants.Modules.Invoices, PermissionConstants.Modules.Customers, PermissionConstants.Modules.Products };
                foreach (var m in salesModules)
                {
                    salesGroup.Permissions.Add(new GroupPermission { Module = m, Action = PermissionConstants.Actions.Show });
                    salesGroup.Permissions.Add(new GroupPermission { Module = m, Action = PermissionConstants.Actions.Create });
                    salesGroup.Permissions.Add(new GroupPermission { Module = m, Action = PermissionConstants.Actions.Edit });
                }

                db.AdminGroups.AddRange(superAdminGroup, salesGroup);
                await db.SaveChangesAsync();
            }

            // 3. Tạo Tài khoản Admin & Nhân viên mẫu
            if (!await userManager.Users.AnyAsync())
            {
                var superAdminGroup = await db.AdminGroups.FirstAsync(g => g.Name == "Super Admin");
                var salesGroup = await db.AdminGroups.FirstAsync(g => g.Name == "Sales Staff");

                // Admin
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@osms.local",
                    Email = "admin@osms.local",
                    FullName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    AdminGroupId = superAdminGroup.Id
                };
                await userManager.CreateAsync(adminUser, "Admin@12345");
                await userManager.AddToRoleAsync(adminUser, "Admin");

                // Sales
                var salesUser = new ApplicationUser
                {
                    UserName = "sales@osms.local",
                    Email = "sales@osms.local",
                    FullName = "Nhân viên Bán hàng",
                    EmailConfirmed = true,
                    IsActive = true,
                    AdminGroupId = salesGroup.Id
                };
                await userManager.CreateAsync(salesUser, "Sales@12345");
                await userManager.AddToRoleAsync(salesUser, "Staff");
            }

            // 4. Cài đặt hệ thống (Settings)
            if (!await db.Settings.AnyAsync())
            {
                db.Settings.Add(new Setting
                {
                    CompanyName = "Cửa hàng Demo OSMS",
                    Currency = "VND",
                    LogoPath = null // Hoặc đường dẫn ảnh nếu có
                });
                await db.SaveChangesAsync();
            }

            // 5. Đơn vị tính (Units)
            if (!await db.Units.AnyAsync())
            {
                db.Units.AddRange(
                    new Unit { Name = "Cái", ShortName = "cái" },
                    new Unit { Name = "Hộp", ShortName = "hộp" },
                    new Unit { Name = "Bộ", ShortName = "bộ" },
                    new Unit { Name = "Kg", ShortName = "kg" }
                );
                await db.SaveChangesAsync();
            }

            // 6. Danh mục (Categories)
            if (!await db.Categories.AnyAsync())
            {
                db.Categories.AddRange(
                    new Category { Name = "Điện tử", Description = "Điện thoại, Laptop, Linh kiện", IsActive = true, IsTrending = true },
                    new Category { Name = "Thời trang", Description = "Quần áo, Giày dép", IsActive = true, IsTrending = false },
                    new Category { Name = "Gia dụng", Description = "Đồ dùng trong nhà", IsActive = true, IsTrending = true }
                );
                await db.SaveChangesAsync();
            }

            // 7. Nhà cung cấp (Suppliers)
            if (!await db.Suppliers.AnyAsync())
            {
                db.Suppliers.AddRange(
                    new Supplier { Name = "Công ty TNHH ABC", Phone = "0901234567", Email = "contact@abc.com", Address = "Hà Nội", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Supplier { Name = "NPP Quốc tế XYZ", Phone = "0987654321", Email = "sales@xyz.com", Address = "TP.HCM", IsActive = true, CreatedAt = DateTime.UtcNow }
                );
                await db.SaveChangesAsync();
            }

            // 8. Khách hàng (Customers)
            if (!await db.Customers.AnyAsync())
            {
                db.Customers.AddRange(
                    new Customer { Name = "Khách vãng lai", Phone = "", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Customer { Name = "Nguyễn Văn A", Phone = "0911222333", Email = "nguyenvana@gmail.com", Address = "Đà Nẵng", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Customer { Name = "Trần Thị B", Phone = "0944555666", Address = "Cần Thơ", IsActive = true, CreatedAt = DateTime.UtcNow }
                );
                await db.SaveChangesAsync();
            }

            // 9. Nhân viên (Employees)
            if (!await db.Employees.AnyAsync())
            {
                db.Employees.AddRange(
                    new Employee { Name = "Lê Văn C", Position = "Kho", Phone = "0909000111", Salary = 8000000, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Employee { Name = "Phạm Thị D", Position = "Kế toán", Phone = "0909000222", Salary = 12000000, IsActive = true, CreatedAt = DateTime.UtcNow }
                );
                await db.SaveChangesAsync();
            }

            // 10. Chi phí (Expenses)
            if (!await db.Expenses.AnyAsync())
            {
                db.Expenses.AddRange(
                    new Expense { Title = "Tiền điện tháng 12", Amount = 1500000, ExpenseDate = DateTime.UtcNow.AddDays(-10), Note = "Đã thanh toán" },
                    new Expense { Title = "Tiền nước tháng 12", Amount = 300000, ExpenseDate = DateTime.UtcNow.AddDays(-10) },
                    new Expense { Title = "Mua văn phòng phẩm", Amount = 500000, ExpenseDate = DateTime.UtcNow.AddDays(-2) }
                );
                await db.SaveChangesAsync();
            }

            // 11. Sản phẩm (Products)
            if (!await db.Products.AnyAsync())
            {
                var unitCai = await db.Units.FirstOrDefaultAsync(u => u.Name == "Cái");
                var catDienTu = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Điện tử");
                var catThoiTrang = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Thời trang");

                db.Products.AddRange(
                    new Product
                    {
                        SKU = "IP15PRO",
                        Name = "iPhone 15 Pro Max",
                        CategoryId = catDienTu?.Id,
                        UnitId = unitCai?.Id,
                        CostPrice = 28000000,
                        SalePrice = 32000000,
                        StockOnHand = 50,
                        ReorderLevel = 10,
                        IsActive = true,
                        IsTrending = true
                    },
                    new Product
                    {
                        SKU = "SS24U",
                        Name = "Samsung S24 Ultra",
                        CategoryId = catDienTu?.Id,
                        UnitId = unitCai?.Id,
                        CostPrice = 25000000,
                        SalePrice = 29000000,
                        StockOnHand = 40,
                        ReorderLevel = 5,
                        IsActive = true,
                        IsTrending = false
                    },
                    new Product
                    {
                        SKU = "TSHIRT01",
                        Name = "Áo thun Polo",
                        CategoryId = catThoiTrang?.Id,
                        UnitId = unitCai?.Id,
                        CostPrice = 150000,
                        SalePrice = 350000,
                        StockOnHand = 100,
                        ReorderLevel = 20,
                        IsActive = true,
                        IsTrending = true
                    }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}