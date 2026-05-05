using System.Windows;
using System.Windows.Input;
using PMS.Desktop.ViewModels;

namespace PMS.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;

    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        DataContext = _vm = vm;
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        await _vm.LoginAsync(PasswordInput.Password);
    }

    private async void OnPasswordKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await _vm.LoginAsync(PasswordInput.Password);
        }
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        _vm.Exit();
    }
}
