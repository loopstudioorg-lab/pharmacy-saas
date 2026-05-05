namespace PMS.Core.Entities;

public class Role : AuditableEntity
{
    public int PharmacyId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
}

public class Permission : AuditableEntity
{
    public string ModuleKey { get; set; } = string.Empty;
    public string PermissionKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class RolePermission : AuditableEntity
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public bool Allow { get; set; } = true;
}
