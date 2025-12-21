// FILE: OnlineSalesManagementSystem/Areas/Admin/Controllers/AdminsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using AppUser = OnlineSalesManagementSystem.Domain.Entities.ApplicationUser;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Admin + "." + PermissionConstants.Actions.Show)]
public class AdminsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Users
            .AsNoTracking()
            .Include(u => u.AdminGroup)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(q)) ||
                (u.UserName != null && u.UserName.Contains(q)) ||
                (u.FullName != null && u.FullName.Contains(q)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.IsActive)
            .ThenBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Admin + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadGroupsAsync();
        return View(new AdminCreateVm { IsActive = true });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Admin + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCreateVm vm)
    {
        await LoadGroupsAsync();

        vm.Email = (vm.Email ?? "").Trim().ToLowerInvariant();
        vm.FullName = (vm.FullName ?? "").Trim();

        if (!ModelState.IsValid)
            return View(vm);

        if (string.IsNullOrWhiteSpace(vm.Email))
        {
            ModelState.AddModelError(nameof(vm.Email), "Email is required.");
            return View(vm);
        }

        if (string.IsNullOrWhiteSpace(vm.Password))
        {
            ModelState.AddModelError(nameof(vm.Password), "Password is required.");
            return View(vm);
        }

        var existing = await _userManager.FindByEmailAsync(vm.Email);
        if (existing != null)
        {
            ModelState.AddModelError(nameof(vm.Email), "Email already exists.");
            return View(vm);
        }

        // Ensure role exists
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            EmailConfirmed = true,
            FullName = vm.FullName,
            IsActive = vm.IsActive,
            AdminGroupId = vm.AdminGroupId
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(vm);
        }

        await _userManager.AddToRoleAsync(user, "Admin");

        TempData["ToastSuccess"] = "Admin created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Admin + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        await LoadGroupsAsync();

        var user = await _db.Users.Include(u => u.AdminGroup).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        return View(new AdminEditVm
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FullName = user.FullName ?? "",
            IsActive = user.IsActive,
            AdminGroupId = user.AdminGroupId
        });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Admin + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminEditVm vm)
    {
        await LoadGroupsAsync();

        vm.FullName = (vm.FullName ?? "").Trim();

        if (!ModelState.IsValid)
            return View(vm);

        var user = await _userManager.FindByIdAsync(vm.Id);
        if (user == null) return NotFound();

        user.FullName = vm.FullName;
        user.IsActive = vm.IsActive;
        user.AdminGroupId = vm.AdminGroupId;

        var upd = await _userManager.UpdateAsync(user);
        if (!upd.Succeeded)
        {
            foreach (var e in upd.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(vm);
        }

        // Optional password reset
        if (!string.IsNullOrWhiteSpace(vm.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var pr = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

            if (!pr.Succeeded)
            {
                foreach (var e in pr.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(vm);
            }
        }

        TempData["ToastSuccess"] = "Admin updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Admin + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Don’t let an admin disable themselves accidentally
        var currentUserId = _userManager.GetUserId(User);
        if (string.Equals(currentUserId, id, StringComparison.Ordinal))
        {
            TempData["ToastError"] = "You cannot disable your own account.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        TempData["ToastSuccess"] = "Admin disabled.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadGroupsAsync()
    {
        ViewBag.Groups = await _db.AdminGroups.AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    // ===== ViewModels =====
    public sealed class AdminCreateVm
    {
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Password { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public int? AdminGroupId { get; set; }
    }

    public sealed class AdminEditVm
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public int? AdminGroupId { get; set; }
        public string? NewPassword { get; set; }
    }
}
