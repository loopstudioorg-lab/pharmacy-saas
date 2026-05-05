namespace PMS.Core.Enums;

public enum LicenseStatus
{
    Trial = 0,
    Active = 1,
    Grace = 2,
    Expired = 3,
    Suspended = 4,
}

public enum LicenseTier
{
    Basic = 0,
    Standard = 1,
    Premium = 2,
}

public enum MachineStatus
{
    Pending = 0,
    Active = 1,
    Deactivated = 2,
    Replaced = 3,
}

public enum AuditEventType
{
    LoginSuccess = 1,
    LoginFailure = 2,
    Logout = 3,
    SetupCompleted = 10,
    PharmacyCreated = 11,
    UserCreated = 20,
    UserUpdated = 21,
    UserDisabled = 22,
    PasswordChanged = 23,
    RoleChanged = 24,
    MachineRegistered = 30,
    MachineReplaced = 31,
    MachineDeactivated = 32,
    LicenseActivated = 40,
    LicenseRenewed = 41,
    LicenseExpired = 42,
    ModuleEnabled = 50,
    ModuleDisabled = 51,
    BackupCreated = 60,
    BackupRestored = 61,
    DataMutation = 90,
    SecurityEvent = 99,
}

public enum SyncStatus
{
    Pending = 0,
    InFlight = 1,
    Synced = 2,
    Failed = 3,
    Conflict = 4,
}
