namespace PMS.Core.Dtos;

public sealed record SetupRequestDto(
    string PharmacyCode,
    string SetupCode,
    string PharmacyName,
    string OwnerName,
    string OwnerUsername,
    string OwnerPassword,
    string? Phone,
    string? Email,
    string? Address,
    string? City);

public sealed record SetupResultDto(
    bool Success,
    string Message,
    int? PharmacyId,
    int? BranchId,
    int? OwnerUserId,
    int? MachineId,
    DateTime? TrialExpiryUtc);

public sealed record LoginRequestDto(string Username, string Password);

public sealed record LoginResultDto(
    bool Success,
    string Message,
    int? UserId,
    string? FullName,
    string? RoleKey,
    int? PharmacyId,
    int? BranchId,
    int? MachineId);

public sealed record LicenseStateDto(
    string TierName,
    string StatusName,
    DateTime ExpiryUtc,
    int DaysRemaining,
    bool IsInGrace);

public sealed record DashboardKpiDto(
    decimal TodaySales,
    decimal TodayProfit,
    decimal StockValue,
    int LowStockCount,
    int NearExpiryCount,
    int LicenseDaysLeft);
