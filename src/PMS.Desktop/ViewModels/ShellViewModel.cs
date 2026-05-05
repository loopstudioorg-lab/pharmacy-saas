using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PMS.Core.Abstractions;
using PMS.Core.Constants;
using PMS.Desktop.Services;

namespace PMS.Desktop.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IServiceProvider _provider;
    private readonly INavigationService _nav;
    private readonly IDialogService _dialogs;

    public ShellViewModel(
        IAuthService auth,
        IServiceProvider provider,
        INavigationService nav,
        IDialogService dialogs)
    {
        _auth = auth;
        _provider = provider;
        _nav = nav;
        _dialogs = dialogs;
    }

    [ObservableProperty] private string pharmacyName = "Pharmacy";
    [ObservableProperty] private string userFullName = "User";
    [ObservableProperty] private string userRole = "-";
    [ObservableProperty] private string licenseBadge = "Trial - 30 days";
    [ObservableProperty] private string connectivityState = "Local Only";
    [ObservableProperty] private string syncState = "Sync off";
    [ObservableProperty] private string nowText = DateTime.Now.ToString("ddd dd MMM, HH:mm");

    public async Task InitializeAsync()
    {
        UserFullName = _auth.CurrentUserFullName ?? "User";
        UserRole = _auth.CurrentRoleKey ?? "-";

        using var scope = _provider.CreateScope();
        var pharmacy = await scope.ServiceProvider.GetRequiredService<IPharmacyService>().GetActivePharmacyAsync();
        PharmacyName = pharmacy?.Name ?? "Pharmacy";

        var state = await scope.ServiceProvider.GetRequiredService<ILicenseService>().GetLicenseStateAsync();
        if (state != null)
        {
            LicenseBadge = state.IsInGrace
                ? $"{state.TierName} - GRACE"
                : $"{state.TierName} - {state.DaysRemaining} days left";
        }

        _nav.Navigate(NavigationTarget.Dashboard);
    }

    [RelayCommand] private void NavDashboard() => _nav.Navigate(NavigationTarget.Dashboard);
    [RelayCommand] private void NavSettings() => _nav.Navigate(NavigationTarget.Settings);
    [RelayCommand] private void NavComingNext() => _nav.Navigate(NavigationTarget.ComingNext);

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (!_dialogs.Confirm("Logout", "Are you sure you want to log out?")) return;
        if (_auth.CurrentUserId is int uid)
        {
            await _auth.LogoutAsync(uid);
        }
        _nav.GoToLogin();
    }

    [RelayCommand] private void Exit() => Application.Current.Shutdown();
}
