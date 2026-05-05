namespace PMS.Core.Constants;

/// <summary>
/// Module flag keys stored in tbl_ModuleSettings. Used by IPermissionService and license gating.
/// Mirrors the module catalog in blueprint Section 9 / Appendix B.
/// </summary>
public static class ModuleKeys
{
    public const string Dashboard = "Dashboard";
    public const string SalesPos = "SalesPos";
    public const string Medicines = "Medicines";
    public const string Purchase = "Purchase";
    public const string Stock = "Stock";
    public const string Accounts = "Accounts";
    public const string Reports = "Reports";
    public const string Backup = "Backup";
    public const string Sync = "Sync";

    public const string EnableSmartInvoiceImport = "EnableSmartInvoiceImport";
    public const string EnableCloudAIInvoiceReading = "EnableCloudAIInvoiceReading";
    public const string EnableAutoCreateMedicineFromImport = "EnableAutoCreateMedicineFromImport";
    public const string EnableMobileReporting = "EnableMobileReporting";
    public const string EnableWhatsAppReports = "EnableWhatsAppReports";
    public const string EnableMultiBranch = "EnableMultiBranch";
    public const string EnableColdChainLog = "EnableColdChainLog";
    public const string EnableDrugRecall = "EnableDrugRecall";
    public const string EnableShiftManagement = "EnableShiftManagement";
    public const string EnableBarcodeLabelPrint = "EnableBarcodeLabelPrint";
    public const string EnablePrescriptionModule = "EnablePrescriptionModule";
    public const string EnableDrugInteractions = "EnableDrugInteractions";
    public const string EnableRefillReminders = "EnableRefillReminders";
    public const string EnableInsurancePanels = "EnableInsurancePanels";
    public const string EnableHomeDelivery = "EnableHomeDelivery";
    public const string EnableLoyaltyPoints = "EnableLoyaltyPoints";
    public const string EnableForecasting = "EnableForecasting";
    public const string EnableAnomalyDetection = "EnableAnomalyDetection";
    public const string EnableTaxEngine = "EnableTaxEngine";
}
