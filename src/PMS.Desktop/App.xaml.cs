using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PMS.Core.Abstractions;
using PMS.Data;
using PMS.Desktop.Services;
using PMS.Desktop.ViewModels;
using PMS.Desktop.Views;
using PMS.Security;
using PMS.Services;
using Serilog;

namespace PMS.Desktop;

public partial class App : Application
{
    public static IHost Host { get; private set; } = null!;

    public static T Resolve<T>() where T : notnull => Host.Services.GetRequiredService<T>();

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            Host = BuildHost();
            await InitializeAsync();
            ShowFirstWindow();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "The application failed to start.\n\n" + ex.Message,
                "Pharmacy Saas - Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static IHost BuildHost()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();

        var basePath = AppContext.BaseDirectory;
        builder.Configuration
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var dataFolder = Environment.ExpandEnvironmentVariables(
            builder.Configuration["Storage:DataFolder"] ?? "%LOCALAPPDATA%\\PharmacySaas");
        Directory.CreateDirectory(dataFolder);

        var logPath = Path.Combine(dataFolder, "logs", "pms-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
            .CreateLogger();
        builder.Services.AddSerilog();

        var connectionString = builder.Configuration["Database:ConnectionString"]!;

        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
        builder.Services.AddSingleton<IClock, SystemClock>();
        builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        builder.Services.AddSingleton<IDeviceFingerprintService, DeviceFingerprintService>();

        var aesKey = SecretsBootstrapper.LoadOrCreateInstallSecret(dataFolder);
        var hmacKey = SecretsBootstrapper.LoadOrCreateHmacSecret(dataFolder);
        builder.Services.AddSingleton<IEncryptionService>(_ => new AesEncryptionService(aesKey));
        builder.Services.AddSingleton<ILicenseSigner>(_ => new HmacLicenseSigner(hmacKey));

        var licensePath = Environment.ExpandEnvironmentVariables(
            builder.Configuration["Storage:LicenseFile"] ?? Path.Combine(dataFolder, "license.dat"));

        builder.Services.AddDbContext<PharmacyDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddScoped<IAuditService, AuditService>();
        builder.Services.AddScoped<IPharmacyService, PharmacyService>();
        builder.Services.AddScoped<IBranchService, BranchService>();
        builder.Services.AddScoped<IMachineService, MachineService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IModuleSettingsService, ModuleSettingsService>();
        builder.Services.AddScoped<IPermissionService, PermissionService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<ILicenseService>(sp => new LicenseService(
            sp.GetRequiredService<PharmacyDbContext>(),
            sp.GetRequiredService<ILicenseSigner>(),
            sp.GetRequiredService<IEncryptionService>(),
            sp.GetRequiredService<IClock>(),
            licensePath));
        builder.Services.AddScoped<ISetupService, SetupService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();

        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<DatabaseBootstrapper>();

        builder.Services.AddTransient<SetupViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ShellViewModel>();
        builder.Services.AddTransient<ComingNextViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        builder.Services.AddTransient<SetupWindow>();
        builder.Services.AddTransient<LoginWindow>();
        builder.Services.AddTransient<ShellWindow>();

        return builder.Build();
    }

    private static async Task InitializeAsync()
    {
        var bootstrapper = Host.Services.GetRequiredService<DatabaseBootstrapper>();
        await bootstrapper.EnsureSchemaAsync();
        await bootstrapper.RunSeedScriptsAsync();
    }

    private static void ShowFirstWindow()
    {
        using var scope = Host.Services.CreateScope();
        var setup = scope.ServiceProvider.GetRequiredService<ISetupService>();
        var setupCompleted = setup.IsSetupCompletedAsync().GetAwaiter().GetResult();

        Window first = setupCompleted
            ? Host.Services.GetRequiredService<LoginWindow>()
            : Host.Services.GetRequiredService<SetupWindow>();

        Current.MainWindow = first;
        first.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        Host?.Dispose();
        base.OnExit(e);
    }
}
