namespace PMS.Core.Entities;

/// <summary>
/// Branches table is in the schema from day 1 (multi-branch decision). UI surfaces it from Phase 6+.
/// Phase 1 auto-creates a single "Main" branch for every pharmacy.
/// </summary>
public class Branch : AuditableEntity
{
    public int PharmacyId { get; set; }
    public string Code { get; set; } = "MAIN";
    public string Name { get; set; } = "Main Branch";
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsHeadOffice { get; set; } = true;
}
