// FILE: OnlineSalesManagementSystem/Areas/Admin/Controllers/SettingsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Settings + "." + PermissionConstants.Actions.Show)]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public SettingsController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var setting = await _db.Settings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new Setting
            {
                CompanyName = "Online Sales Management System",
                Currency = "VND",
                LogoPath = null
            };
            _db.Settings.Add(setting);
            await _db.SaveChangesAsync();
        }

        return View(setting);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Settings + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Setting model, IFormFile? logoFile)
    {
        if (!ModelState.IsValid)
            return View(model);

        var setting = await _db.Settings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new Setting();
            _db.Settings.Add(setting);
        }

        setting.CompanyName = model.CompanyName.Trim();
        setting.Currency = string.IsNullOrWhiteSpace(model.Currency) ? "VND" : model.Currency.Trim().ToUpperInvariant();

        if (logoFile != null && logoFile.Length > 0)
        {
            var ext = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp" };

            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError(string.Empty, "Logo must be .png/.jpg/.jpeg/.webp");
                return View(setting);
            }

            var uploads = Path.Combine(_env.WebRootPath, "uploads", "logos");
            Directory.CreateDirectory(uploads);

            var fileName = $"logo_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            var filePath = Path.Combine(uploads, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await logoFile.CopyToAsync(fs);
            }

            setting.LogoPath = $"/uploads/logos/{fileName}";
        }

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Settings saved.";
        return RedirectToAction(nameof(Index));
    }
}
