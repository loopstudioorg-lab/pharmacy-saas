using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PMS.Desktop.ViewModels;
using PMS.Desktop.Views;

namespace PMS.Desktop.Services;

/// <summary>
/// Coordinates main-window swaps after setup / login / logout, and routes sidebar
/// navigation to the right content view inside the shell.
/// </summary>
public interface INavigationService
{
    void GoToShell();
    void GoToLogin();
    event EventHandler<NavigationTarget>? Navigated;
    void Navigate(NavigationTarget target);
    NavigationTarget Current { get; }
}

public enum NavigationTarget
{
    Dashboard,
    Settings,
    ComingNext,
}

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;

    public NavigationService(IServiceProvider provider)
    {
        _provider = provider;
    }

    public NavigationTarget Current { get; private set; } = NavigationTarget.Dashboard;
    public event EventHandler<NavigationTarget>? Navigated;

    public void GoToShell()
    {
        var shell = _provider.GetRequiredService<ShellWindow>();
        var oldMain = Application.Current.MainWindow;
        Application.Current.MainWindow = shell;
        shell.Show();
        oldMain?.Close();
        Navigate(NavigationTarget.Dashboard);
    }

    public void GoToLogin()
    {
        var login = _provider.GetRequiredService<LoginWindow>();
        var oldMain = Application.Current.MainWindow;
        Application.Current.MainWindow = login;
        login.Show();
        oldMain?.Close();
    }

    public void Navigate(NavigationTarget target)
    {
        Current = target;
        Navigated?.Invoke(this, target);
    }
}
