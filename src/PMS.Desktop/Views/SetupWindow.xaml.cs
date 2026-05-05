using System.Windows;
using PMS.Desktop.ViewModels;

namespace PMS.Desktop.Views;

public partial class SetupWindow : Window
{
    private readonly SetupViewModel _vm;

    public SetupWindow(SetupViewModel vm)
    {
        InitializeComponent();
        DataContext = _vm = vm;
    }

    private async void OnCompleteClick(object sender, RoutedEventArgs e)
    {
        await _vm.CompleteSetupAsync(PasswordInput.Password, ConfirmInput.Password);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        _vm.Exit();
    }
}
