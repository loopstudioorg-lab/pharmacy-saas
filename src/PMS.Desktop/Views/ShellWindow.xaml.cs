using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using PMS.Desktop.Services;
using PMS.Desktop.ViewModels;

namespace PMS.Desktop.Views;

public partial class ShellWindow : Window
{
    private readonly ShellViewModel _vm;
    private readonly INavigationService _nav;
    private readonly IServiceProvider _provider;
    private readonly DispatcherTimer _clock;

    public ShellWindow(ShellViewModel vm, INavigationService nav, IServiceProvider provider)
    {
        InitializeComponent();
        DataContext = _vm = vm;
        _nav = nav;
        _provider = provider;
        _nav.Navigated += (_, target) => Dispatcher.BeginInvoke(() => SwapContent(target));
        Loaded += OnLoaded;

        _clock = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _clock.Tick += (_, _) => _vm.NowText = DateTime.Now.ToString("ddd dd MMM, HH:mm");
        _clock.Start();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _vm.InitializeAsync();
    }

    private void SwapContent(NavigationTarget target)
    {
        UserControl view = target switch
        {
            NavigationTarget.Dashboard  => BuildDashboard(),
            NavigationTarget.Settings   => BuildSettings(),
            NavigationTarget.ComingNext => BuildComingNext(),
            _ => BuildDashboard(),
        };
        ContentHost.Content = view;
    }

    private DashboardView BuildDashboard()
    {
        var vm = _provider.GetRequiredService<DashboardViewModel>();
        var view = new DashboardView { DataContext = vm };
        _ = LoadDashboardAsync(vm);
        return view;
    }

    private static async Task LoadDashboardAsync(DashboardViewModel vm)
    {
        try
        {
            await vm.LoadAsync();
        }
        catch
        {
        }
    }

    private SettingsView BuildSettings()
        => new() { DataContext = _provider.GetRequiredService<SettingsViewModel>() };

    private ComingNextView BuildComingNext()
        => new() { DataContext = _provider.GetRequiredService<ComingNextViewModel>() };
}
