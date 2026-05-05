using Microsoft.EntityFrameworkCore;
using PMS.Core.Abstractions;
using PMS.Core.Constants;
using PMS.Core.Entities;
using PMS.Data;

namespace PMS.Services;

public sealed class ModuleSettingsService : IModuleSettingsService
{
    private readonly PharmacyDbContext _db;
    private readonly IClock _clock;

    public ModuleSettingsService(PharmacyDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<bool> IsEnabledAsync(int pharmacyId, string moduleKey)
    {
        var setting = await _db.ModuleSettings
            .FirstOrDefaultAsync(m => m.PharmacyId == pharmacyId && m.ModuleKey == moduleKey);
        return setting?.IsEnabled ?? false;
    }

    public async Task SetEnabledAsync(int pharmacyId, string moduleKey, bool enabled, int updatedBy)
    {
        var setting = await _db.ModuleSettings
            .FirstOrDefaultAsync(m => m.PharmacyId == pharmacyId && m.ModuleKey == moduleKey);
        if (setting == null)
        {
            _db.ModuleSettings.Add(new ModuleSetting
            {
                PharmacyId = pharmacyId,
                ModuleKey = moduleKey,
                IsEnabled = enabled,
                SavedBy = updatedBy,
                SaveDate = _clock.UtcNow,
            });
        }
        else
        {
            setting.IsEnabled = enabled;
            setting.UpdatedBy = updatedBy;
            setting.UpdatedDate = _clock.UtcNow;
        }
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyDictionary<string, bool>> GetAllAsync(int pharmacyId)
    {
        var rows = await _db.ModuleSettings
            .Where(m => m.PharmacyId == pharmacyId)
            .ToListAsync();
        return rows.ToDictionary(m => m.ModuleKey, m => m.IsEnabled);
    }

    public async Task SeedDefaultsAsync(int pharmacyId)
    {
        var defaults = new Dictionary<string, bool>
        {
            [ModuleKeys.Dashboard] = true,
            [ModuleKeys.SalesPos] = true,
            [ModuleKeys.Medicines] = true,
            [ModuleKeys.Purchase] = true,
            [ModuleKeys.Stock] = true,
            [ModuleKeys.Accounts] = true,
            [ModuleKeys.Reports] = true,
            [ModuleKeys.Backup] = true,

            [ModuleKeys.Sync] = false,
            [ModuleKeys.EnableSmartInvoiceImport] = false,
            [ModuleKeys.EnableCloudAIInvoiceReading] = false,
            [ModuleKeys.EnableAutoCreateMedicineFromImport] = false,
            [ModuleKeys.EnableMobileReporting] = false,
            [ModuleKeys.EnableWhatsAppReports] = false,
            [ModuleKeys.EnableMultiBranch] = false,
            [ModuleKeys.EnableColdChainLog] = false,
            [ModuleKeys.EnableDrugRecall] = false,
            [ModuleKeys.EnableShiftManagement] = false,
            [ModuleKeys.EnableBarcodeLabelPrint] = false,
            [ModuleKeys.EnablePrescriptionModule] = false,
            [ModuleKeys.EnableDrugInteractions] = false,
            [ModuleKeys.EnableRefillReminders] = false,
            [ModuleKeys.EnableInsurancePanels] = false,
            [ModuleKeys.EnableHomeDelivery] = false,
            [ModuleKeys.EnableLoyaltyPoints] = false,
            [ModuleKeys.EnableForecasting] = false,
            [ModuleKeys.EnableAnomalyDetection] = false,
            [ModuleKeys.EnableTaxEngine] = false,
        };

        var existing = await _db.ModuleSettings
            .Where(m => m.PharmacyId == pharmacyId)
            .Select(m => m.ModuleKey)
            .ToListAsync();
        var existingSet = new HashSet<string>(existing);

        foreach (var (key, enabled) in defaults)
        {
            if (existingSet.Contains(key)) continue;
            _db.ModuleSettings.Add(new ModuleSetting
            {
                PharmacyId = pharmacyId,
                ModuleKey = key,
                IsEnabled = enabled,
                SaveDate = _clock.UtcNow,
            });
        }
        await _db.SaveChangesAsync();
    }
}
