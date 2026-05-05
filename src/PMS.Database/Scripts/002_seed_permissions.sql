-- =====================================================================
--  002_seed_permissions.sql
--  Seeds the catalog of permissions used by IPermissionService.
--  Roles are seeded per-pharmacy at setup time, not here.
-- =====================================================================

;WITH src(ModuleKey, PermissionKey, DisplayName) AS (
    SELECT * FROM (VALUES
        (N'Dashboard',  N'View',   N'View Dashboard'),
        (N'SalesPos',   N'View',   N'View POS'),
        (N'SalesPos',   N'Create', N'Create Sale'),
        (N'SalesPos',   N'Approve',N'Approve Discount / Return'),
        (N'Medicines',  N'View',   N'View Medicines'),
        (N'Medicines',  N'Create', N'Create Medicine'),
        (N'Medicines',  N'Edit',   N'Edit Medicine'),
        (N'Medicines',  N'Delete', N'Delete Medicine'),
        (N'Purchase',   N'View',   N'View Purchases'),
        (N'Purchase',   N'Create', N'Create Purchase'),
        (N'Stock',      N'View',   N'View Stock'),
        (N'Stock',      N'Edit',   N'Adjust Stock'),
        (N'Reports',    N'View',   N'View Reports'),
        (N'Reports',    N'Export', N'Export Reports'),
        (N'Accounts',   N'View',   N'View Accounts'),
        (N'Accounts',   N'Edit',   N'Edit Accounts'),
        (N'Backup',     N'Create', N'Create Backup'),
        (N'Backup',     N'Approve',N'Restore Backup'),
        (N'Sync',       N'View',   N'View Sync Status'),
        (N'Settings',   N'View',   N'View Settings'),
        (N'Settings',   N'Edit',   N'Edit Settings'),
        (N'Settings',   N'ManageUsers',    N'Manage Users'),
        (N'Settings',   N'ManageLicense',  N'Manage License'),
        (N'Settings',   N'ManageMachines', N'Manage Machines'),
        (N'Settings',   N'ViewAuditLog',   N'View Audit Log')
    ) AS v(ModuleKey, PermissionKey, DisplayName)
)
MERGE tbl_Permissions AS tgt
USING src
   ON tgt.ModuleKey = src.ModuleKey AND tgt.PermissionKey = src.PermissionKey
WHEN NOT MATCHED THEN
    INSERT (ModuleKey, PermissionKey, DisplayName)
    VALUES (src.ModuleKey, src.PermissionKey, src.DisplayName);
GO
