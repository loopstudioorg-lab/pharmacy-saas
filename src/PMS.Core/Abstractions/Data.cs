using System.Data;

namespace PMS.Core.Abstractions;

/// <summary>
/// Hybrid data layer entry point. EF Core does CRUD/migrations via the DbContext;
/// Dapper hot-path queries open a connection through this factory.
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateOpenConnection();
    string ConnectionString { get; }
}

public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
