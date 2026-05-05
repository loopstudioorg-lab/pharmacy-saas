using Microsoft.EntityFrameworkCore;
using PMS.Core.Abstractions;
using PMS.Core.Constants;
using PMS.Core.Dtos;
using PMS.Core.Enums;
using PMS.Data;

namespace PMS.Services;

/// <summary>
/// Authenticates users against the local database. Validates that the current machine
/// is registered to the user's pharmacy. Holds the in-process current-session state.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly PharmacyDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IMachineService _machines;
    private readonly ILicenseService _license;
    private readonly IAuditService _audit;
    private readonly IClock _clock;

    public AuthService(
        PharmacyDbContext db,
        IPasswordHasher hasher,
        IMachineService machines,
        ILicenseService license,
        IAuditService audit,
        IClock clock)
    {
        _db = db;
        _hasher = hasher;
        _machines = machines;
        _license = license;
        _audit = audit;
        _clock = clock;
    }

    public int? CurrentUserId { get; private set; }
    public int? CurrentPharmacyId { get; private set; }
    public int? CurrentBranchId { get; private set; }
    public int? CurrentMachineId { get; private set; }
    public string? CurrentRoleKey { get; private set; }
    public string? CurrentUserFullName { get; private set; }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return new LoginResultDto(false, "Username and password are required.", null, null, null, null, null, null);
        }

        var machine = await _machines.GetCurrentMachineAsync();
        if (machine == null || machine.Status != MachineStatus.Active)
        {
            return new LoginResultDto(false, "This machine is not registered. Please run setup.", null, null, null, null, null, null);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null)
        {
            await _audit.LogAsync(
                AuditEventType.LoginFailure,
                $"Login failed - unknown user '{request.Username}'.",
                machineId: machine.Id);
            return new LoginResultDto(false, "Invalid username or password.", null, null, null, null, null, null);
        }

        if (user.PharmacyId != machine.PharmacyId)
        {
            return new LoginResultDto(false, "User does not belong to this pharmacy.", null, null, null, null, null, null);
        }

        if (user.IsLocked)
        {
            return new LoginResultDto(false, "Account is locked. Contact your admin.", null, null, null, null, null, null);
        }

        if (!_hasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            user.FailedLoginAttempts += 1;
            if (user.FailedLoginAttempts >= AppConstants.MaxFailedLoginAttempts)
            {
                user.IsLocked = true;
            }
            await _db.SaveChangesAsync();
            await _audit.LogAsync(
                AuditEventType.LoginFailure,
                $"Login failed for '{request.Username}'.",
                userId: user.Id,
                pharmacyId: user.PharmacyId,
                machineId: machine.Id);
            return new LoginResultDto(false, "Invalid username or password.", null, null, null, null, null, null);
        }

        var lic = await _license.GetLicenseStateAsync();
        if (lic != null && lic.StatusName == nameof(LicenseStatus.Expired))
        {
            return new LoginResultDto(false, "Your license has expired. Please renew to continue.", null, null, null, null, null, null);
        }

        user.FailedLoginAttempts = 0;
        user.LastLoginUtc = _clock.UtcNow;
        await _db.SaveChangesAsync();
        await _machines.UpdateLastLoginAsync(machine.Id);

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == user.RoleId);
        CurrentUserId = user.Id;
        CurrentPharmacyId = user.PharmacyId;
        CurrentBranchId = user.BranchId ?? machine.BranchId;
        CurrentMachineId = machine.Id;
        CurrentRoleKey = role?.Key;
        CurrentUserFullName = user.FullName;

        await _audit.LogAsync(
            AuditEventType.LoginSuccess,
            $"User '{user.Username}' logged in.",
            userId: user.Id,
            pharmacyId: user.PharmacyId,
            branchId: CurrentBranchId,
            machineId: machine.Id);

        return new LoginResultDto(
            true,
            "Login successful.",
            user.Id,
            user.FullName,
            role?.Key,
            user.PharmacyId,
            CurrentBranchId,
            machine.Id);
    }

    public async Task LogoutAsync(int userId)
    {
        await _audit.LogAsync(
            AuditEventType.Logout,
            $"User logged out.",
            userId: userId,
            pharmacyId: CurrentPharmacyId,
            machineId: CurrentMachineId);

        CurrentUserId = null;
        CurrentPharmacyId = null;
        CurrentBranchId = null;
        CurrentMachineId = null;
        CurrentRoleKey = null;
        CurrentUserFullName = null;
    }
}
