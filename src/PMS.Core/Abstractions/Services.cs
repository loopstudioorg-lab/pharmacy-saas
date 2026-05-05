using PMS.Core.Dtos;
using PMS.Core.Entities;
using PMS.Core.Enums;

namespace PMS.Core.Abstractions;

public interface ISetupService
{
    Task<bool> IsSetupCompletedAsync();
    Task<SetupResultDto> RunSetupAsync(SetupRequestDto request);
}

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto request);
    Task LogoutAsync(int userId);
    int? CurrentUserId { get; }
    int? CurrentPharmacyId { get; }
    int? CurrentBranchId { get; }
    int? CurrentMachineId { get; }
    string? CurrentRoleKey { get; }
    string? CurrentUserFullName { get; }
}

public interface IUserService
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<int> CreateAsync(User user, string plainPassword, int? createdBy);
    Task ChangePasswordAsync(int userId, string newPassword, int changedBy);
    Task SetLockedAsync(int userId, bool locked, int updatedBy);
}

public interface IPharmacyService
{
    Task<Pharmacy?> GetActivePharmacyAsync();
    Task<int> CreateAsync(Pharmacy pharmacy);
}

public interface IBranchService
{
    Task<int> EnsureMainBranchAsync(int pharmacyId);
    Task<IReadOnlyList<Branch>> GetByPharmacyAsync(int pharmacyId);
}

public interface IMachineService
{
    Task<PharmacyMachine?> GetCurrentMachineAsync();
    Task<int> RegisterCurrentMachineAsync(int pharmacyId, int? branchId);
    Task<bool> IsCurrentMachineRegisteredAsync(int pharmacyId);
    Task UpdateLastLoginAsync(int machineId);
}

public interface ILicenseService
{
    Task<LicenseInfo?> GetLicenseAsync();
    Task<LicenseStateDto?> GetLicenseStateAsync();
    Task<int> CreateTrialAsync(int pharmacyId, int machineId, int trialDays);
    Task<bool> ValidateAsync();
}

public interface IModuleSettingsService
{
    Task<bool> IsEnabledAsync(int pharmacyId, string moduleKey);
    Task SetEnabledAsync(int pharmacyId, string moduleKey, bool enabled, int updatedBy);
    Task<IReadOnlyDictionary<string, bool>> GetAllAsync(int pharmacyId);
    Task SeedDefaultsAsync(int pharmacyId);
}

public interface IAuditService
{
    Task LogAsync(
        AuditEventType eventType,
        string description,
        int? userId = null,
        int? pharmacyId = null,
        int? branchId = null,
        int? machineId = null,
        string? entityName = null,
        long? entityId = null,
        object? oldValue = null,
        object? newValue = null);
}

public interface IDashboardService
{
    Task<DashboardKpiDto> GetTodayKpisAsync();
}
