using System.Text.Json;
using PMS.Core.Entities;
using PMS.Core.Enums;
using PMS.Data;

namespace PMS.Sync;

/// <summary>
/// Phase 1 foundation: lets services enqueue records into tbl_SyncQueue.
/// Background draining of the queue + cloud API client lands in Phase 6.
/// </summary>
public sealed class SyncQueueWriter
{
    private readonly PharmacyDbContext _db;

    public SyncQueueWriter(PharmacyDbContext db)
    {
        _db = db;
    }

    public async Task EnqueueAsync<T>(string entityName, Guid recordGuid, T payload, string operation = "Upsert")
        where T : class
    {
        _db.SyncQueue.Add(new SyncQueueItem
        {
            EntityName = entityName,
            EntityRecordGuid = recordGuid,
            OperationType = operation,
            PayloadJson = JsonSerializer.Serialize(payload),
            Status = SyncStatus.Pending,
        });
        await _db.SaveChangesAsync();
    }
}
