using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Online_Sales_Management_System.Data;

var builder = WebApplication.CreateBuilder(args);

// KẾT NỐI DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// IDENTITY
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ĐANG DÙNG IDENTITY NÊN PHẢI CÓ:
app.UseAuthentication();
app.UseAuthorization();

// Static assets (.NET 9 style)
app.MapStaticAssets();

// Route cho Area Admin (AdminLTE)
app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();

// Route mặc định cho customer site (Home/Index)
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();

// Razor Pages (Identity UI)
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
