using System.Windows;

namespace PMS.Desktop.Services;

public interface IDialogService
{
    void ShowSuccess(string title, string message);
    void ShowError(string title, string message);
    void ShowWarning(string title, string message);
    bool Confirm(string title, string message);
}

public sealed class DialogService : IDialogService
{
    public void ShowSuccess(string title, string message)
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowError(string title, string message)
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public void ShowWarning(string title, string message)
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public bool Confirm(string title, string message)
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
