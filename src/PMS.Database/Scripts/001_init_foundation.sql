-- =====================================================================
--  PMS.Database  -  001_init_foundation.sql
--  Phase 1 foundation tables. Idempotent.
--  Convention:
--      every table prefixed tbl_
--      every operational table includes RecordGuid + standard audit fields
--      every operational table includes BranchId where multi-branch matters
-- =====================================================================

IF OBJECT_ID(N'tbl_Pharmacies', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_Pharmacies (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        PharmacyGuid    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        PharmacyCode    NVARCHAR(40)  NOT NULL,
        Name            NVARCHAR(200) NOT NULL,
        OwnerName       NVARCHAR(150) NULL,
        Phone           NVARCHAR(40)  NULL,
        Email           NVARCHAR(150) NULL,
        Address         NVARCHAR(500) NULL,
        City            NVARCHAR(100) NULL,
        Country         NVARCHAR(100) NULL DEFAULT N'Pakistan',
        TaxNumber       NVARCHAR(50)  NULL,
        LogoPath        NVARCHAR(500) NULL,
        IsActive        BIT NOT NULL DEFAULT 1,
        SaveDate        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy         INT NULL,
        UpdatedDate     DATETIME2 NULL,
        UpdatedBy       INT NULL,
        IsSynced        BIT NOT NULL DEFAULT 0,
        CONSTRAINT UQ_Pharmacies_PharmacyCode UNIQUE (PharmacyCode)
    );
END
GO

IF OBJECT_ID(N'tbl_Branches', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_Branches (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        PharmacyId      INT NOT NULL,
        Code            NVARCHAR(40) NOT NULL DEFAULT N'MAIN',
        Name            NVARCHAR(200) NOT NULL DEFAULT N'Main Branch',
        Phone           NVARCHAR(40) NULL,
        Address         NVARCHAR(500) NULL,
        IsHeadOffice    BIT NOT NULL DEFAULT 1,
        IsActive        BIT NOT NULL DEFAULT 1,
        SaveDate        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy         INT NULL,
        UpdatedDate     DATETIME2 NULL,
        UpdatedBy       INT NULL,
        IsSynced        BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Branches_Pharmacy FOREIGN KEY (PharmacyId) REFERENCES tbl_Pharmacies(Id),
        CONSTRAINT UQ_Branches_PharmacyCode UNIQUE (PharmacyId, Code)
    );
END
GO

IF OBJECT_ID(N'tbl_Roles', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_Roles (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        PharmacyId      INT NOT NULL,
        [Key]           NVARCHAR(50)  NOT NULL,
        Name            NVARCHAR(150) NOT NULL,
        IsSystemRole    BIT NOT NULL DEFAULT 0,
        IsActive        BIT NOT NULL DEFAULT 1,
        SaveDate        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy         INT NULL,
        UpdatedDate     DATETIME2 NULL,
        UpdatedBy       INT NULL,
        IsSynced        BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Roles_Pharmacy FOREIGN KEY (PharmacyId) REFERENCES tbl_Pharmacies(Id),
        CONSTRAINT UQ_Roles_PharmacyKey UNIQUE (PharmacyId, [Key])
    );
END
GO

IF OBJECT_ID(N'tbl_Permissions', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_Permissions (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        ModuleKey       NVARCHAR(80)  NOT NULL,
        PermissionKey   NVARCHAR(80)  NOT NULL,
        DisplayName     NVARCHAR(150) NOT NULL,
        IsActive        BIT NOT NULL DEFAULT 1,
        SaveDate        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy         INT NULL,
        UpdatedDate     DATETIME2 NULL,
        UpdatedBy       INT NULL,
        IsSynced        BIT NOT NULL DEFAULT 0,
        CONSTRAINT UQ_Permissions_ModulePerm UNIQUE (ModuleKey, PermissionKey)
    );
END
GO

IF OBJECT_ID(N'tbl_RolePermissions', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_RolePermissions (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        RoleId          INT NOT NULL,
        PermissionId    INT NOT NULL,
        Allow           BIT NOT NULL DEFAULT 1,
        IsActive        BIT NOT NULL DEFAULT 1,
        SaveDate        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy         INT NULL,
        UpdatedDate     DATETIME2 NULL,
        UpdatedBy       INT NULL,
        IsSynced        BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES tbl_Roles(Id),
        CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES tbl_Permissions(Id),
        CONSTRAINT UQ_RolePermissions UNIQUE (RoleId, PermissionId)
    );
END
GO

IF OBJECT_ID(N'tbl_Users', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_Users (
        Id                          INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        PharmacyId                  INT NOT NULL,
        BranchId                    INT NULL,
        Username                    NVARCHAR(80)  NOT NULL,
        FullName                    NVARCHAR(200) NOT NULL,
        Email                       NVARCHAR(200) NULL,
        Phone                       NVARCHAR(40)  NULL,
        PasswordHash                NVARCHAR(400) NOT NULL,
        PasswordSalt                NVARCHAR(200) NOT NULL,
        RoleId                      INT NOT NULL,
        IsLocked                    BIT NOT NULL DEFAULT 0,
        FailedLoginAttempts         INT NOT NULL DEFAULT 0,
        LastLoginUtc                DATETIME2 NULL,
        LastPasswordChangeUtc       DATETIME2 NULL,
        MustChangePassword          BIT NOT NULL DEFAULT 0,
        IsActive                    BIT NOT NULL DEFAULT 1,
        SaveDate                    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy                     INT NULL,
        UpdatedDate                 DATETIME2 NULL,
        UpdatedBy                   INT NULL,
        IsSynced                    BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Users_Pharmacy FOREIGN KEY (PharmacyId) REFERENCES tbl_Pharmacies(Id),
        CONSTRAINT FK_Users_Branch   FOREIGN KEY (BranchId)   REFERENCES tbl_Branches(Id),
        CONSTRAINT FK_Users_Role     FOREIGN KEY (RoleId)     REFERENCES tbl_Roles(Id),
        CONSTRAINT UQ_Users_PharmacyUsername UNIQUE (PharmacyId, Username)
    );
END
GO

IF OBJECT_ID(N'tbl_PharmacyMachines', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_PharmacyMachines (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        PharmacyId          INT NOT NULL,
        BranchId            INT NULL,
        MachineCode         NVARCHAR(60)  NOT NULL,
        MachineName         NVARCHAR(150) NOT NULL,
        FingerprintHash     NVARCHAR(128) NOT NULL,
        OsVersion           NVARCHAR(150) NULL,
        AppVersion          NVARCHAR(50)  NULL,
        RegisteredAtUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        LastLoginUtc        DATETIME2 NULL,
        LastSyncUtc         DATETIME2 NULL,
        Status              INT NOT NULL DEFAULT 1,
        IsActive            BIT NOT NULL DEFAULT 1,
        SaveDate            DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy             INT NULL,
        UpdatedDate         DATETIME2 NULL,
        UpdatedBy           INT NULL,
        IsSynced            BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Machines_Pharmacy FOREIGN KEY (PharmacyId) REFERENCES tbl_Pharmacies(Id),
        CONSTRAINT FK_Machines_Branch   FOREIGN KEY (BranchId)   REFERENCES tbl_Branches(Id)
    );
    CREATE INDEX IX_Machines_Fingerprint ON tbl_PharmacyMachines(FingerprintHash);
END
GO

IF OBJECT_ID(N'tbl_LicenseInfo', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_LicenseInfo (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        PharmacyId          INT NOT NULL,
        MachineId           INT NOT NULL,
        Tier                INT NOT NULL DEFAULT 0,
        Status              INT NOT NULL DEFAULT 0,
        IssuedAtUtc         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ExpiryUtc           DATETIME2 NOT NULL,
        LastValidatedUtc    DATETIME2 NULL,
        MonotonicCounter    BIGINT NOT NULL DEFAULT 0,
        Signature           NVARCHAR(800) NULL,
        ModulesJson         NVARCHAR(MAX) NULL,
        IsActive            BIT NOT NULL DEFAULT 1,
        SaveDate            DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy             INT NULL,
        UpdatedDate         DATETIME2 NULL,
        UpdatedBy           INT NULL,
        IsSynced            BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_License_Pharmacy FOREIGN KEY (PharmacyId) REFERENCES tbl_Pharmacies(Id),
        CONSTRAINT FK_License_Machine  FOREIGN KEY (MachineId)  REFERENCES tbl_PharmacyMachines(Id)
    );
END
GO

IF OBJECT_ID(N'tbl_ModuleSettings', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_ModuleSettings (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        PharmacyId      INT NOT NULL,
        ModuleKey       NVARCHAR(80) NOT NULL,
        IsEnabled       BIT NOT NULL DEFAULT 0,
        ConfigJson      NVARCHAR(MAX) NULL,
        IsActive        BIT NOT NULL DEFAULT 1,
        SaveDate        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy         INT NULL,
        UpdatedDate     DATETIME2 NULL,
        UpdatedBy       INT NULL,
        IsSynced        BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_ModuleSettings_Pharmacy FOREIGN KEY (PharmacyId) REFERENCES tbl_Pharmacies(Id),
        CONSTRAINT UQ_ModuleSettings_PharmacyModule UNIQUE (PharmacyId, ModuleKey)
    );
END
GO

IF OBJECT_ID(N'tbl_FeatureFlags', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_FeatureFlags (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        PharmacyId      INT NULL,
        [Key]           NVARCHAR(80) NOT NULL,
        IsEnabled       BIT NOT NULL DEFAULT 0,
        Notes           NVARCHAR(500) NULL,
        IsActive        BIT NOT NULL DEFAULT 1,
        SaveDate        DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy         INT NULL,
        UpdatedDate     DATETIME2 NULL,
        UpdatedBy       INT NULL,
        IsSynced        BIT NOT NULL DEFAULT 0
    );
END
GO

IF OBJECT_ID(N'tbl_AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_AuditLogs (
        Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        PharmacyId      INT NULL,
        BranchId        INT NULL,
        UserId          INT NULL,
        MachineId       INT NULL,
        EventType       INT NOT NULL,
        EventCode       NVARCHAR(80)  NOT NULL,
        Description     NVARCHAR(800) NOT NULL,
        EntityName      NVARCHAR(120) NULL,
        EntityId        BIGINT NULL,
        OldValueJson    NVARCHAR(MAX) NULL,
        NewValueJson    NVARCHAR(MAX) NULL,
        IpAddress       NVARCHAR(60) NULL,
        EventAtUtc      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        IsSynced        BIT NOT NULL DEFAULT 0
    );
    CREATE INDEX IX_AuditLogs_EventAt ON tbl_AuditLogs(EventAtUtc DESC);
    CREATE INDEX IX_AuditLogs_Pharmacy ON tbl_AuditLogs(PharmacyId, EventAtUtc DESC);
END
GO

IF OBJECT_ID(N'tbl_SyncQueue', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_SyncQueue (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        EntityName          NVARCHAR(120) NOT NULL,
        EntityRecordGuid    UNIQUEIDENTIFIER NOT NULL,
        OperationType       NVARCHAR(20) NOT NULL DEFAULT N'Upsert',
        PayloadJson         NVARCHAR(MAX) NOT NULL,
        Status              INT NOT NULL DEFAULT 0,
        RetryCount          INT NOT NULL DEFAULT 0,
        LastError           NVARCHAR(MAX) NULL,
        QueuedAtUtc         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        LastAttemptUtc      DATETIME2 NULL,
        IsActive            BIT NOT NULL DEFAULT 1,
        SaveDate            DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        SavedBy             INT NULL,
        UpdatedDate         DATETIME2 NULL,
        UpdatedBy           INT NULL,
        IsSynced            BIT NOT NULL DEFAULT 0
    );
    CREATE INDEX IX_SyncQueue_Status ON tbl_SyncQueue(Status, QueuedAtUtc);
END
GO

IF OBJECT_ID(N'tbl_SyncLogs', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_SyncLogs (
        Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        StartedUtc      DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        FinishedUtc     DATETIME2 NULL,
        ItemsProcessed  INT NOT NULL DEFAULT 0,
        ItemsFailed     INT NOT NULL DEFAULT 0,
        ErrorSummary    NVARCHAR(MAX) NULL
    );
END
GO

IF OBJECT_ID(N'tbl_RecycleBin', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_RecycleBin (
        Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
        RecordGuid      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        EntityName      NVARCHAR(120) NOT NULL,
        EntityId        BIGINT NOT NULL,
        SnapshotJson    NVARCHAR(MAX) NOT NULL,
        DeletedBy       INT NOT NULL,
        DeletedAtUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        RestoredAtUtc   DATETIME2 NULL,
        RestoredBy      INT NULL
    );
END
GO
