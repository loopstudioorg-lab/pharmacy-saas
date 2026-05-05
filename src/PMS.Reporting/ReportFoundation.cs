namespace PMS.Reporting;

/// <summary>
/// Phase 1 placeholder. Real report models, exporters, and PDF renderers (QuestPDF)
/// land in Phase 5 per the blueprint roadmap.
/// </summary>
public abstract record ReportRequestBase(DateTime FromUtc, DateTime ToUtc);

public abstract record ReportResultBase(string Title, DateTime GeneratedAtUtc);
