using System.Text.Json;
using PMS.Core.Abstractions;
using PMS.Core.Entities;
using PMS.Core.Enums;
using PMS.Data;

namespace PMS.Services;

public sealed class AuditService : IAuditService
{
    private readonly PharmacyDbContext _db;
    private readonly IClock _clock;

    public AuditService(PharmacyDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task LogAsync(
        AuditEventType eventType,
        string description,
        int? userId = null,
        int? pharmacyId = null,
        int? branchId = null,
        int? machineId = null,
        string? entityName = null,
        long? entityId = null,
        object? oldValue = null,
        object? newValue = null)
    {
        var log = new AuditLog
        {
            EventType = eventType,
            EventCode = eventType.ToString(),
            Description = description,
            UserId = userId,
            PharmacyId = pharmacyId,
            BranchId = branchId,
            MachineId = machineId,
            EntityName = entityName,
            EntityId = entityId,
            OldValueJson = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValueJson = newValue is null ? null : JsonSerializer.Serialize(newValue),
            EventAtUtc = _clock.UtcNow,
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
