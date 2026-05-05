using Microsoft.EntityFrameworkCore;
using PMS.Core.Abstractions;
using PMS.Core.Constants;
using PMS.Data;

namespace PMS.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly PharmacyDbContext _db;
    private readonly IModuleSettingsService _modules;

    public PermissionService(PharmacyDbContext db, IModuleSettingsService modules)
    {
        _db = db;
        _modules = modules;
    }

    public async Task<bool> CanAccessAsync(int userId, string moduleKey, string permissionKey)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        if (user == null || user.IsLocked) return false;

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId && r.IsActive);
        if (role == null) return false;

        if (string.Equals(role.Key, AppConstants.OwnerRoleKey, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var perm = await _db.Permissions
            .FirstOrDefaultAsync(p => p.ModuleKey == moduleKey && p.PermissionKey == permissionKey && p.IsActive);
        if (perm == null) return false;

        var allowed = await _db.RolePermissions
            .AnyAsync(rp =>
                rp.RoleId == role.Id &&
                rp.PermissionId == perm.Id &&
                rp.Allow &&
                rp.IsActive);
        return allowed;
    }

    public Task<bool> IsModuleEnabledAsync(int pharmacyId, string moduleKey)
        => _modules.IsEnabledAsync(pharmacyId, moduleKey);
}
