using Microsoft.EntityFrameworkCore;
using PMS.Core.Abstractions;
using PMS.Core.Constants;
using PMS.Core.Dtos;
using PMS.Core.Entities;
using PMS.Core.Enums;
using PMS.Data;

namespace PMS.Services;

/// <summary>
/// First-Time Setup orchestrator (blueprint Section 5).
/// Phase 1 uses MOCK activation: PHM-DEMO + 123456 only. Real cloud activation lands in Phase 6.
/// </summary>
public sealed class SetupService : ISetupService
{
    private readonly PharmacyDbContext _db;
    private readonly IPharmacyService _pharmacy;
    private readonly IBranchService _branches;
    private readonly IMachineService _machines;
    private readonly IUserService _users;
    private readonly ILicenseService _license;
    private readonly IModuleSettingsService _modules;
    private readonly IAuditService _audit;

    public SetupService(
        PharmacyDbContext db,
        IPharmacyService pharmacy,
        IBranchService branches,
        IMachineService machines,
        IUserService users,
        ILicenseService license,
        IModuleSettingsService modules,
        IAuditService audit)
    {
        _db = db;
        _pharmacy = pharmacy;
        _branches = branches;
        _machines = machines;
        _users = users;
        _license = license;
        _modules = modules;
        _audit = audit;
    }

    public async Task<bool> IsSetupCompletedAsync()
        => await _db.Pharmacies.AnyAsync(p => p.IsActive)
           && await _db.Users.AnyAsync(u => u.IsActive);

    public async Task<SetupResultDto> RunSetupAsync(SetupRequestDto req)
    {
        if (await IsSetupCompletedAsync())
        {
            return new SetupResultDto(false, "Setup is already completed.", null, null, null, null, null);
        }

        if (!string.Equals(req.PharmacyCode, AppConstants.DemoPharmacyCode, StringComparison.OrdinalIgnoreCase) ||
            req.SetupCode != AppConstants.DemoSetupCode)
        {
            return new SetupResultDto(false,
                $"Invalid Pharmacy Code or Setup Code. Phase 1 uses {AppConstants.DemoPharmacyCode} / {AppConstants.DemoSetupCode}.",
                null, null, null, null, null);
        }

        if (string.IsNullOrWhiteSpace(req.PharmacyName) ||
            string.IsNullOrWhiteSpace(req.OwnerUsername) ||
            string.IsNullOrEmpty(req.OwnerPassword))
        {
            return new SetupResultDto(false, "Pharmacy name, owner username and password are required.",
                null, null, null, null, null);
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var pharmacy = new Pharmacy
            {
                PharmacyCode = req.PharmacyCode.ToUpperInvariant(),
                Name = req.PharmacyName,
                OwnerName = req.OwnerName,
                Phone = req.Phone,
                Email = req.Email,
                Address = req.Address,
                City = req.City,
                Country = "Pakistan",
            };
            var pharmacyId = await _pharmacy.CreateAsync(pharmacy);

            var branchId = await _branches.EnsureMainBranchAsync(pharmacyId);

            var ownerRole = new Role
            {
                PharmacyId = pharmacyId,
                Key = AppConstants.OwnerRoleKey,
                Name = "Owner",
                IsSystemRole = true,
            };
            _db.Roles.Add(ownerRole);
            _db.Roles.Add(new Role
            {
                PharmacyId = pharmacyId,
                Key = AppConstants.AdminRoleKey,
                Name = "Admin",
                IsSystemRole = true,
            });
            _db.Roles.Add(new Role
            {
                PharmacyId = pharmacyId,
                Key = AppConstants.CashierRoleKey,
                Name = "Cashier",
                IsSystemRole = true,
            });
            _db.Roles.Add(new Role
            {
                PharmacyId = pharmacyId,
                Key = AppConstants.StockManagerRoleKey,
                Name = "Stock Manager",
                IsSystemRole = true,
            });
            await _db.SaveChangesAsync();

            var machineId = await _machines.RegisterCurrentMachineAsync(pharmacyId, branchId);

            var owner = new User
            {
                PharmacyId = pharmacyId,
                BranchId = branchId,
                Username = req.OwnerUsername.Trim(),
                FullName = string.IsNullOrWhiteSpace(req.OwnerName) ? req.OwnerUsername : req.OwnerName!,
                Email = req.Email,
                Phone = req.Phone,
                RoleId = ownerRole.Id,
            };
            var userId = await _users.CreateAsync(owner, req.OwnerPassword, null);

            await _modules.SeedDefaultsAsync(pharmacyId);
            var licenseId = await _license.CreateTrialAsync(pharmacyId, machineId, AppConstants.DefaultTrialDays);

            await _audit.LogAsync(
                AuditEventType.SetupCompleted,
                $"Pharmacy '{pharmacy.Name}' set up. Owner '{owner.Username}' created. {AppConstants.DefaultTrialDays}-day trial activated.",
                userId: userId,
                pharmacyId: pharmacyId,
                branchId: branchId,
                machineId: machineId);

            await tx.CommitAsync();

            return new SetupResultDto(
                true,
                "Setup completed. You can now log in.",
                pharmacyId,
                branchId,
                userId,
                machineId,
                DateTime.UtcNow.AddDays(AppConstants.DefaultTrialDays));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new SetupResultDto(false, $"Setup failed: {ex.Message}", null, null, null, null, null);
        }
    }
}
