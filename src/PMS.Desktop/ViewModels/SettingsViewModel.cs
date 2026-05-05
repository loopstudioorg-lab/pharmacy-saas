using CommunityToolkit.Mvvm.ComponentModel;

namespace PMS.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private string heading = "Settings";
    [ObservableProperty] private string description =
        "User management, license info, machine list, modules, audit log and backup live here. Phase 1 ships a placeholder; the full screens land in Phase 2-6.";
}

public partial class ComingNextViewModel : ObservableObject
{
    [ObservableProperty] private string heading = "Coming Next";
    [ObservableProperty] private string description =
        "This module is part of a future phase. Phase 1 ships the foundation only - dashboard, setup, login, license, machine binding and reusable UI. Medicines, sales, purchases, reports etc. are unlocked phase by phase.";
}
