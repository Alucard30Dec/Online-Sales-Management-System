using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

// ✅ Resolve ambiguous references if OSMS.PermissionPatch is also in the solution
using PermissionPolicyProvider = OnlineSalesManagementSystem.Services.Security.PermissionPolicyProvider;
using PermissionAuthorizationHandler = OnlineSalesManagementSystem.Services.Security.PermissionAuthorizationHandler;
using IPermissionService = OnlineSalesManagementSystem.Services.Security.IPermissionService;
using PermissionService = OnlineSalesManagementSystem.Services.Security.PermissionService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            // An toàn nếu sau này bạn tách DbContext sang project khác
            sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        });
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

// Authorization: permission-based policies
builder.Services.AddAuthorization();

// ✅ Use ONLY ONE policy provider (avoid conflicts/ambiguity)
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

builder.Services.AddScoped<OnlineSalesManagementSystem.Services.Inventory.IStockService, OnlineSalesManagementSystem.Services.Inventory.StockService>();
builder.Services.AddScoped<OnlineSalesManagementSystem.Services.Sales.IInvoiceTotalsService, OnlineSalesManagementSystem.Services.Sales.InvoiceTotalsService>();

var app = builder.Build();

// Auto-migrate + seed (FAIL FAST)
await using (var scope = app.Services.CreateAsyncScope())
{
    var sp = scope.ServiceProvider;
    var logger = sp.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

        logger.LogInformation("Applying EF Core migrations...");
        await db.Database.MigrateAsync();

        // Sanity check: nếu bảng Identity thiếu, nổ ngay tại đây cho dễ debug
        _ = await db.Users.AsNoTracking().Select(x => x.Id).Take(1).ToListAsync();

        logger.LogInformation("Seeding database...");
        await DbSeeder.SeedAsync(db, userManager, roleManager);

        logger.LogInformation("Database migration + seeding done.");
    }
    catch (Exception ex)
    {
        // QUAN TRỌNG: Đừng nuốt lỗi, vì app chạy tiếp sẽ gây lỗi lắt nhắt như AspNetUsers not found
        logger.LogCritical(ex, "Migration/Seeding FAILED. Application will stop.");
        throw;
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
