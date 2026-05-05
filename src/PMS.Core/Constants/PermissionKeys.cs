namespace PMS.Core.Constants;

/// <summary>
/// Permission keys used by IPermissionService.CanAccess(module, permission).
/// </summary>
public static class PermissionKeys
{
    public const string View = "View";
    public const string Create = "Create";
    public const string Edit = "Edit";
    public const string Delete = "Delete";
    public const string Export = "Export";
    public const string Approve = "Approve";

    public const string ManageUsers = "ManageUsers";
    public const string ManageLicense = "ManageLicense";
    public const string ManageMachines = "ManageMachines";
    public const string ManageSettings = "ManageSettings";
    public const string RestoreBackup = "RestoreBackup";
    public const string ViewAuditLog = "ViewAuditLog";
}
