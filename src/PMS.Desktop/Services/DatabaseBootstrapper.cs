using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PMS.Data;

namespace PMS.Desktop.Services;

/// <summary>
/// Runs versioned SQL scripts on app start. Idempotent.
/// </summary>
public sealed class DatabaseBootstrapper
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseBootstrapper> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public DatabaseBootstrapper(IConfiguration config, ILoggerFactory loggerFactory)
    {
        _connectionString = config["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Database:ConnectionString is not configured.");
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<DatabaseBootstrapper>();
    }

    public async Task EnsureSchemaAsync()
    {
        var runner = new SqlScriptRunner(_connectionString, _loggerFactory.CreateLogger<SqlScriptRunner>());
        var ran = await runner.EnsureLatestAsync();
        _logger.LogInformation("Schema bootstrap complete. {Count} new script(s) applied.", ran);
    }

    public Task RunSeedScriptsAsync()
    {
        return Task.CompletedTask;
    }
}
