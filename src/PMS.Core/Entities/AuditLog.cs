using PMS.Core.Enums;

namespace PMS.Core.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public Guid RecordGuid { get; set; } = Guid.NewGuid();
    public int? PharmacyId { get; set; }
    public int? BranchId { get; set; }
    public int? UserId { get; set; }
    public int? MachineId { get; set; }
    public AuditEventType EventType { get; set; }
    public string EventCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public long? EntityId { get; set; }
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public string? IpAddress { get; set; }
    public DateTime EventAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; }
}

public class SyncQueueItem : AuditableEntity
{
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityRecordGuid { get; set; }
    public string OperationType { get; set; } = "Upsert";
    public string PayloadJson { get; set; } = string.Empty;
    public SyncStatus Status { get; set; } = SyncStatus.Pending;
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime QueuedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastAttemptUtc { get; set; }
}

public class SyncLog
{
    public long Id { get; set; }
    public Guid RecordGuid { get; set; } = Guid.NewGuid();
    public DateTime StartedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedUtc { get; set; }
    public int ItemsProcessed { get; set; }
    public int ItemsFailed { get; set; }
    public string? ErrorSummary { get; set; }
}

public class RecycleBinItem
{
    public long Id { get; set; }
    public Guid RecordGuid { get; set; } = Guid.NewGuid();
    public string EntityName { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string SnapshotJson { get; set; } = string.Empty;
    public int DeletedBy { get; set; }
    public DateTime DeletedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RestoredAtUtc { get; set; }
    public int? RestoredBy { get; set; }
}
