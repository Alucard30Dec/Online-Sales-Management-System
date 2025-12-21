using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineSalesManagementSystem.Domain.Entities;
using AppUser = OnlineSalesManagementSystem.Domain.Entities.ApplicationUser;

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
        await db.Database.MigrateAsync();

        // Ensure default Setting exists
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

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed role(s)
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

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
            // Ensure linked to Super Admin group (safe default)
            if (adminUser.AdminGroupId != superAdmin.Id)
            {
                adminUser.AdminGroupId = superAdmin.Id;
                adminUser.IsActive = true;
                await userManager.UpdateAsync(adminUser);
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Optional: (Re)seed full permission matrix for Super Admin if missing wildcard
        var hasWildcard = await db.GroupPermissions.AnyAsync(p => p.AdminGroupId == superAdmin.Id && p.Module == "*" && p.Action == "*");
        if (!hasWildcard)
        {
            db.GroupPermissions.Add(new GroupPermission { AdminGroupId = superAdmin.Id, Module = "*", Action = "*" });
            await db.SaveChangesAsync();
        }
    }
}
