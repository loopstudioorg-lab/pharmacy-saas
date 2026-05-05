namespace PMS.Core.Entities;

/// <summary>
/// Base for every domain entity. Mirrors the standard audit fields in blueprint Section 17.
/// RecordGuid is set by the DB default (NEWID) but can be overridden client-side for sync.
/// </summary>
public abstract class AuditableEntity
{
    public int Id { get; set; }
    public Guid RecordGuid { get; set; } = Guid.NewGuid();
    public bool IsActive { get; set; } = true;
    public DateTime SaveDate { get; set; } = DateTime.UtcNow;
    public int? SavedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsSynced { get; set; }
}
