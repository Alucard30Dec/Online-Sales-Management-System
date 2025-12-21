using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.AdminGroups + "." + PermissionConstants.Actions.Show)]
public class AdminGroupsController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminGroupsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.AdminGroups.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(g => g.Name.Contains(q));
        }

        var items = await query.OrderBy(g => g.Name).ToListAsync();
        ViewBag.Query = q;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.AdminGroups + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new AdminGroup());
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.AdminGroups + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminGroup model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var exists = await _db.AdminGroups.AnyAsync(g => g.Name == model.Name);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Group name already exists.");
            return View(model);
        }

        _db.AdminGroups.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Admin group created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.AdminGroups + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _db.AdminGroups.FirstOrDefaultAsync(g => g.Id == id);
        if (entity == null) return NotFound();

        return View(entity);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.AdminGroups + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminGroup model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var entity = await _db.AdminGroups.FirstOrDefaultAsync(g => g.Id == model.Id);
        if (entity == null) return NotFound();

        var exists = await _db.AdminGroups.AnyAsync(g => g.Id != model.Id && g.Name == model.Name);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Group name already exists.");
            return View(model);
        }

        entity.Name = model.Name;
        entity.Description = model.Description;

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Admin group updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.AdminGroups + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.AdminGroups
            .Include(g => g.Permissions)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (entity == null) return NotFound();

        var usedByUsers = await _db.Users.AnyAsync(u => u.AdminGroupId == id);
        if (usedByUsers)
        {
            TempData["ToastError"] = "Cannot delete: this group is assigned to one or more admins.";
            return RedirectToAction(nameof(Index));
        }

        _db.GroupPermissions.RemoveRange(entity.Permissions);
        _db.AdminGroups.Remove(entity);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Admin group deleted.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.AdminGroups + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Permissions(int id)
    {
        var group = await _db.AdminGroups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
        if (group == null) return NotFound();

        var perms = await _db.GroupPermissions.AsNoTracking()
            .Where(p => p.AdminGroupId == id)
            .ToListAsync();

        var grantAll = perms.Any(p => p.Module == "*" && p.Action == "*");
        var set = new HashSet<string>(perms.Select(p => $"{p.Module}.{p.Action}"), StringComparer.OrdinalIgnoreCase);

        var modules = PermissionConstants.AllModules;
        var rows = modules.Select(m => new PermissionRowVm
        {
            Module = m,
            Show = grantAll || set.Contains($"{m}.{PermissionConstants.Actions.Show}"),
            Create = grantAll || set.Contains($"{m}.{PermissionConstants.Actions.Create}"),
            Edit = grantAll || set.Contains($"{m}.{PermissionConstants.Actions.Edit}"),
            Delete = grantAll || set.Contains($"{m}.{PermissionConstants.Actions.Delete}")
        }).ToList();

        return View(new GroupPermissionsVm
        {
            AdminGroupId = id,
            GroupName = group.Name,
            GrantAll = grantAll,
            Rows = rows
        });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.AdminGroups + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Permissions(GroupPermissionsVm vm)
    {
        var group = await _db.AdminGroups.FirstOrDefaultAsync(g => g.Id == vm.AdminGroupId);
        if (group == null) return NotFound();

        var modules = new HashSet<string>(PermissionConstants.AllModules, StringComparer.OrdinalIgnoreCase);

        vm.Rows ??= new();
        vm.Rows = vm.Rows.Where(r => modules.Contains(r.Module)).ToList();

        var old = await _db.GroupPermissions.Where(p => p.AdminGroupId == vm.AdminGroupId).ToListAsync();
        _db.GroupPermissions.RemoveRange(old);

        if (vm.GrantAll)
        {
            _db.GroupPermissions.Add(new GroupPermission
            {
                AdminGroupId = vm.AdminGroupId,
                Module = "*",
                Action = "*"
            });
        }
        else
        {
            foreach (var row in vm.Rows)
            {
                AddIf(row.Module, PermissionConstants.Actions.Show, row.Show);
                AddIf(row.Module, PermissionConstants.Actions.Create, row.Create);
                AddIf(row.Module, PermissionConstants.Actions.Edit, row.Edit);
                AddIf(row.Module, PermissionConstants.Actions.Delete, row.Delete);
            }
        }

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Permissions saved.";
        return RedirectToAction(nameof(Permissions), new { id = vm.AdminGroupId });

        void AddIf(string module, string action, bool enabled)
        {
            if (!enabled) return;

            _db.GroupPermissions.Add(new GroupPermission
            {
                AdminGroupId = vm.AdminGroupId,
                Module = module,
                Action = action
            });
        }
    }

    // ===== ViewModels =====
    public sealed class GroupPermissionsVm
    {
        public int AdminGroupId { get; set; }
        public string GroupName { get; set; } = "";
        public bool GrantAll { get; set; }
        public List<PermissionRowVm> Rows { get; set; } = new();
    }

    public sealed class PermissionRowVm
    {
        public string Module { get; set; } = "";
        public bool Show { get; set; }
        public bool Create { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
    }
}
