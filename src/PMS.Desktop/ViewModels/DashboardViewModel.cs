using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PMS.Core.Abstractions;

namespace PMS.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IServiceProvider _provider;

    public DashboardViewModel(IServiceProvider provider)
    {
        _provider = provider;
    }

    [ObservableProperty] private decimal todaySales;
    [ObservableProperty] private decimal todayProfit;
    [ObservableProperty] private decimal stockValue;
    [ObservableProperty] private int lowStockCount;
    [ObservableProperty] private int nearExpiryCount;
    [ObservableProperty] private int licenseDaysLeft;
    [ObservableProperty] private string licenseTier = "-";
    [ObservableProperty] private string licenseStatus = "-";

    [RelayCommand]
    public async Task LoadAsync()
    {
        using var scope = _provider.CreateScope();
        var dash = scope.ServiceProvider.GetRequiredService<IDashboardService>();
        var lic = scope.ServiceProvider.GetRequiredService<ILicenseService>();

        var kpis = await dash.GetTodayKpisAsync();
        TodaySales = kpis.TodaySales;
        TodayProfit = kpis.TodayProfit;
        StockValue = kpis.StockValue;
        LowStockCount = kpis.LowStockCount;
        NearExpiryCount = kpis.NearExpiryCount;
        LicenseDaysLeft = kpis.LicenseDaysLeft;

        var state = await lic.GetLicenseStateAsync();
        LicenseTier = state?.TierName ?? "-";
        LicenseStatus = state?.StatusName ?? "-";
    }
}
