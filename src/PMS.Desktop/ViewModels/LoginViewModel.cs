using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using PMS.Core.Abstractions;
using PMS.Core.Dtos;
using PMS.Desktop.Services;

namespace PMS.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly IDialogService _dialogs;
    private readonly INavigationService _nav;

    public LoginViewModel(IAuthService auth, IDialogService dialogs, INavigationService nav)
    {
        _auth = auth;
        _dialogs = dialogs;
        _nav = nav;
    }

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string errorMessage = string.Empty;

    public async Task LoginAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Please enter your username.";
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            ErrorMessage = "Please enter your password.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var result = await _auth.LoginAsync(new LoginRequestDto(Username.Trim(), password));
            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return;
            }

            _nav.GoToShell();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Exit() => Application.Current.Shutdown();
}
