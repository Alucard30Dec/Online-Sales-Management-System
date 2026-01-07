using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminGroupsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminGroupsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [PermissionAuthorize(PermissionConstants.Modules.AdminGroups, PermissionConstants.Actions.Show)]
        public async Task<IActionResult> Index()
        {
            var groups = await _db.AdminGroups
                .AsNoTracking()
                .OrderBy(g => g.Id)
                .ToListAsync();

            return View(groups);
        }

        [PermissionAuthorize(PermissionConstants.Modules.AdminGroups, PermissionConstants.Actions.Create)]
        public IActionResult Create()
        {
            return View(new AdminGroup());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(PermissionConstants.Modules.AdminGroups, PermissionConstants.Actions.Create)]
        public async Task<IActionResult> Create(AdminGroup model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.AdminGroups.Add(model);
            await _db.SaveChangesAsync();

            TempData["ToastSuccess"] = "Admin group created.";
            return RedirectToAction(nameof(Index));
        }

        [PermissionAuthorize(PermissionConstants.Modules.AdminGroups, PermissionConstants.Actions.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _db.AdminGroups.FindAsync(id);
            if (entity == null) return NotFound();

            // Block editing Super Admin group
            if (SuperAdminProtection.IsSuperAdminGroupName(entity.Name))
            {
                TempData["ToastError"] = "You cannot edit the Super Admin group.";
                return RedirectToAction(nameof(Index));
            }

            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(PermissionConstants.Modules.AdminGroups, PermissionConstants.Actions.Edit)]
        public async Task<IActionResult> Edit(AdminGroup model)
        {
            if (!ModelState.IsValid) return View(model);

            var entity = await _db.AdminGroups.FindAsync(model.Id);
            if (entity == null) return NotFound();

            // Block editing Super Admin group
            if (SuperAdminProtection.IsSuperAdminGroupName(entity.Name))
            {
                TempData["ToastError"] = "You cannot edit the Super Admin group.";
                return RedirectToAction(nameof(Index));
            }

            entity.Name = model.Name;
            entity.Description = model.Description;

            await _db.SaveChangesAsync();

            TempData["ToastSuccess"] = "Admin group updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(PermissionConstants.Modules.AdminGroups, PermissionConstants.Actions.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _db.AdminGroups
                .Include(g => g.Permissions)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return NotFound();

            // Block deleting Super Admin group
            if (SuperAdminProtection.IsSuperAdminGroupName(group.Name))
            {
                TempData["ToastError"] = "You cannot delete the Super Admin group.";
                return RedirectToAction(nameof(Index));
            }

            _db.GroupPermissions.RemoveRange(group.Permissions);
            _db.AdminGroups.Remove(group);

            await _db.SaveChangesAsync();

            TempData["ToastSuccess"] = "Admin group deleted.";
            return RedirectToAction(nameof(Index));
        }

        [PermissionAuthorize(PermissionConstants.Modules.AdminGroups, PermissionConstants.Actions.Edit)]
        public async Task<IActionResult> Permissions(int id)
        {
            var group = await _db.AdminGroups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();

            var existing = await _db.GroupPermissions
                .AsNoTracking()
                .Where(p => p.AdminGroupId == id)
                .ToListAsync();

            var set = new HashSet<string>(existing.Select(p => $"{p.Module}:{p.Action}"));

            var vm = new GroupPermissionsVm
            {
                GroupId = id,
                GroupName = group.Name ?? string.Empty,
                GrantAll = set.Contains("*:*"),
                Rows = PermissionConstants.AllModules.Select(m => new PermissionRowVm
                {
                    Module = m,
                    Show = set.Contains($"{m}:{PermissionConstants.Actions.Show}"),
                    Create = set.Contains($"{m}:{PermissionConstants.Actions.Create}"),
                    Edit = set.Contains($"{m}:{PermissionConstants.Actions.Edit}"),
                    Delete = set.Contains($"{m}:{PermissionConstants.Actions.Delete}"),
                    Approve = set.Contains($"{m}:{PermissionConstants.Actions.Approve}"),
                    Export = set.Contains($"{m}:{PermissionConstants.Actions.Export}"),
                    Manage = set.Contains($"{m}:{PermissionConstants.Actions.Manage}")
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(PermissionConstants.Modules.AdminGroups, PermissionConstants.Actions.Edit)]
        public async Task<IActionResult> Permissions(GroupPermissionsVm vm)
        {
            // Always re-check server side
            var group = await _db.AdminGroups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == vm.GroupId);
            if (group == null) return NotFound();

            // A) Super Admin group: allow view but reject saving (read-only)
            if (SuperAdminProtection.IsSuperAdminGroupName(group.Name))
            {
                TempData["ToastError"] = "Super Admin group permissions are read-only.";
                return RedirectToAction(nameof(Permissions), new { id = vm.GroupId });
            }

            var existing = await _db.GroupPermissions.Where(p => p.AdminGroupId == vm.GroupId).ToListAsync();
            _db.GroupPermissions.RemoveRange(existing);

            var newPerms = new List<GroupPermission>();

            if (vm.GrantAll)
            {
                newPerms.Add(new GroupPermission
                {
                    AdminGroupId = vm.GroupId,
                    Module = PermissionConstants.Wildcard,
                    Action = PermissionConstants.Wildcard
                });
            }
            else
            {
                vm.Rows ??= new List<PermissionRowVm>();

                foreach (var row in vm.Rows.Where(r => !string.IsNullOrWhiteSpace(r.Module)))
                {
                    void AddIf(bool ok, string action)
                    {
                        if (!ok) return;
                        newPerms.Add(new GroupPermission
                        {
                            AdminGroupId = vm.GroupId,
                            Module = row.Module,
                            Action = action
                        });
                    }

                    AddIf(row.Show, PermissionConstants.Actions.Show);
                    AddIf(row.Create, PermissionConstants.Actions.Create);
                    AddIf(row.Edit, PermissionConstants.Actions.Edit);
                    AddIf(row.Delete, PermissionConstants.Actions.Delete);
                    AddIf(row.Approve, PermissionConstants.Actions.Approve);
                    AddIf(row.Export, PermissionConstants.Actions.Export);
                    AddIf(row.Manage, PermissionConstants.Actions.Manage);
                }
            }

            if (newPerms.Count > 0)
                _db.GroupPermissions.AddRange(newPerms);

            await _db.SaveChangesAsync();

            TempData["ToastSuccess"] = "Permissions updated.";
            return RedirectToAction(nameof(Index));
        }

        public class GroupPermissionsVm
        {
            public int GroupId { get; set; }
            public string GroupName { get; set; } = string.Empty;
            public bool GrantAll { get; set; }
            public List<PermissionRowVm> Rows { get; set; } = new();
        }

        public class PermissionRowVm
        {
            public string Module { get; set; } = string.Empty;

            public bool Show { get; set; }
            public bool Create { get; set; }
            public bool Edit { get; set; }
            public bool Delete { get; set; }

            public bool Approve { get; set; }
            public bool Export { get; set; }
            public bool Manage { get; set; }
        }
    }
}
