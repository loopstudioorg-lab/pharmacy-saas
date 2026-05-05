namespace PMS.Core.Constants;

/// <summary>
/// Application-wide constants. Phase 1 foundation values.
/// </summary>
public static class AppConstants
{
    public const string AppName = "Pharmacy Saas";
    public const string AppShortCode = "PMS";

    public const string DemoPharmacyCode = "PHM-DEMO";
    public const string DemoSetupCode = "123456";
    public const int DefaultTrialDays = 30;

    public const int DefaultMachineLimit = 1;
    public const int OfflineGracePeriodDays = 7;

    public const string DefaultExpressConnection =
        @"Server=.\SQLEXPRESS;Database=PharmacySaas;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";

    public const string DefaultLocalDbConnection =
        @"Server=(localdb)\MSSQLLocalDB;Database=PharmacySaas;Trusted_Connection=True;TrustServerCertificate=True;";

    public const int LicenseWarningDays7 = 7;
    public const int LicenseWarningDays3 = 3;
    public const int LicenseWarningDays1 = 1;

    public const int MaxFailedLoginAttempts = 5;

    public const string OwnerRoleKey = "Owner";
    public const string AdminRoleKey = "Admin";
    public const string CashierRoleKey = "Cashier";
    public const string StockManagerRoleKey = "StockManager";
}
