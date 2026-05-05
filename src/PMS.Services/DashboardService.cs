using PMS.Core.Abstractions;
using PMS.Core.Dtos;

namespace PMS.Services;

/// <summary>
/// Dashboard KPIs. Phase 1 returns placeholder zero-state values plus the live license days
/// remaining. Real numbers light up in Phase 2 (medicines/stock) and Phase 4 (sales).
/// </summary>
public sealed class DashboardService : IDashboardService
{
    private readonly ILicenseService _license;

    public DashboardService(ILicenseService license)
    {
        _license = license;
    }

    public async Task<DashboardKpiDto> GetTodayKpisAsync()
    {
        var lic = await _license.GetLicenseStateAsync();
        var daysLeft = lic?.DaysRemaining ?? 0;

        return new DashboardKpiDto(
            TodaySales: 0m,
            TodayProfit: 0m,
            StockValue: 0m,
            LowStockCount: 0,
            NearExpiryCount: 0,
            LicenseDaysLeft: daysLeft);
    }
}
