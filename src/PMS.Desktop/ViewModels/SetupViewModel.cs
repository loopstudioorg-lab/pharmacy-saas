using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PMS.Core.Abstractions;
using PMS.Core.Constants;
using PMS.Core.Dtos;
using PMS.Desktop.Services;

namespace PMS.Desktop.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    private readonly IServiceProvider _provider;
    private readonly IDialogService _dialogs;
    private readonly INavigationService _nav;

    public SetupViewModel(IServiceProvider provider, IDialogService dialogs, INavigationService nav)
    {
        _provider = provider;
        _dialogs = dialogs;
        _nav = nav;

        PharmacyCode = AppConstants.DemoPharmacyCode;
        SetupCode = AppConstants.DemoSetupCode;
    }

    [ObservableProperty] private string pharmacyCode = string.Empty;
    [ObservableProperty] private string setupCode = string.Empty;
    [ObservableProperty] private string pharmacyName = string.Empty;
    [ObservableProperty] private string ownerName = string.Empty;
    [ObservableProperty] private string ownerUsername = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private string city = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = string.Empty;

    /// <summary>
    /// Called by code-behind so we can read PasswordBox.Password (which cannot bind for security).
    /// </summary>
    public async Task CompleteSetupAsync(string password, string confirm)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            _dialogs.ShowError("Setup", "Password must be at least 6 characters.");
            return;
        }
        if (password != confirm)
        {
            _dialogs.ShowError("Setup", "Passwords do not match.");
            return;
        }
        if (string.IsNullOrWhiteSpace(PharmacyName) || string.IsNullOrWhiteSpace(OwnerUsername))
        {
            _dialogs.ShowError("Setup", "Pharmacy Name and Owner Username are required.");
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Setting up your pharmacy...";

            using var scope = _provider.CreateScope();
            var setup = scope.ServiceProvider.GetRequiredService<ISetupService>();

            var req = new SetupRequestDto(
                PharmacyCode.Trim(),
                SetupCode.Trim(),
                PharmacyName.Trim(),
                OwnerName.Trim(),
                OwnerUsername.Trim(),
                password,
                Phone.Trim(),
                Email.Trim(),
                Address.Trim(),
                City.Trim());

            var result = await setup.RunSetupAsync(req);
            if (!result.Success)
            {
                StatusMessage = result.Message;
                _dialogs.ShowError("Setup", result.Message);
                return;
            }

            _dialogs.ShowSuccess(
                "Setup Complete",
                $"Pharmacy created.\n\nA {AppConstants.DefaultTrialDays}-day trial has been activated.\nYou can now log in with your owner account.");
            _nav.GoToLogin();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Exit() => Application.Current.Shutdown();
}
