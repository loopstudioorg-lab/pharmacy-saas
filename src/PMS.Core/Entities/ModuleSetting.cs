namespace PMS.Core.Entities;

public class ModuleSetting : AuditableEntity
{
    public int PharmacyId { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? ConfigJson { get; set; }
}

public class FeatureFlag : AuditableEntity
{
    public int? PharmacyId { get; set; }
    public string Key { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? Notes { get; set; }
}

public class SchemaVersion
{
    public int Id { get; set; }
    public string ScriptName { get; set; } = string.Empty;
    public DateTime AppliedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Checksum { get; set; }
}
