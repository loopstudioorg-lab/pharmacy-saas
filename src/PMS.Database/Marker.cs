namespace PMS.Database;

/// <summary>
/// Anchor type used by Assembly.GetExecutingAssembly() callers to locate this resource assembly.
/// The real payload of this project is the embedded SQL scripts under Scripts/.
/// </summary>
public static class DatabaseAssemblyMarker
{
    public static string Name => typeof(DatabaseAssemblyMarker).Assembly.GetName().Name ?? "PMS.Database";
}
