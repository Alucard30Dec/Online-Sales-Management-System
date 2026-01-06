using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using AppUser = OnlineSalesManagementSystem.Domain.Entities.ApplicationUser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Identity (cookie auth)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Auth/Login";
    options.LogoutPath = "/Admin/Auth/Logout";
    options.AccessDeniedPath = "/Admin/Auth/AccessDenied";

    options.Cookie.Name = "OSMS.Auth";
    options.SlidingExpiration = true;
});

// Authorization: permission-based policies (dynamic policy provider + handler will be added in Services layer)
builder.Services.AddAuthorization();

// These registrations are implemented in later files (Services/Security/*).
// Keep them here so the final solution compiles once those files exist.
builder.Services.AddSingleton<IAuthorizationPolicyProvider, OnlineSalesManagementSystem.Services.Security.PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, OnlineSalesManagementSystem.Services.Security.PermissionAuthorizationHandler>();
builder.Services.AddScoped<OnlineSalesManagementSystem.Services.Security.IPermissionService, OnlineSalesManagementSystem.Services.Security.PermissionService>();

// Stock + invoice totals services (implemented later)
builder.Services.AddScoped<OnlineSalesManagementSystem.Services.Inventory.IStockService, OnlineSalesManagementSystem.Services.Inventory.StockService>();
builder.Services.AddScoped<OnlineSalesManagementSystem.Services.Sales.IInvoiceTotalsService, OnlineSalesManagementSystem.Services.Sales.InvoiceTotalsService>();

var app = builder.Build();

// Auto-migrate + seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Tự động migrate database nếu chưa có
        db.Database.Migrate();

        // Gọi hàm seed
        await DbSeeder.SeedAsync(db, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi khi seed dữ liệu.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Areas first (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default (public site)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
