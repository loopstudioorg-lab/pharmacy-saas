using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace PMS.Data;

/// <summary>
/// Applies versioned SQL scripts (001_*.sql, 002_*.sql ...) shipped as embedded resources by PMS.Database.
/// Tracks applied scripts in tbl_SchemaVersion. Idempotent - safe to call on every startup.
/// </summary>
public sealed class SqlScriptRunner
{
    private readonly string _connectionString;
    private readonly ILogger<SqlScriptRunner>? _logger;

    public SqlScriptRunner(string connectionString, ILogger<SqlScriptRunner>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<int> EnsureLatestAsync()
    {
        await EnsureDatabaseExistsAsync();

        var assembly = LoadDatabaseAssembly();
        var scripts = assembly
            .GetManifestResourceNames()
            .Where(n => n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (scripts.Count == 0)
        {
            _logger?.LogWarning("No SQL scripts found in PMS.Database assembly.");
            return 0;
        }

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await conn.ExecuteAsync(@"
IF OBJECT_ID(N'tbl_SchemaVersion', N'U') IS NULL
BEGIN
    CREATE TABLE tbl_SchemaVersion (
        Id            INT IDENTITY(1,1) PRIMARY KEY,
        ScriptName    NVARCHAR(200) NOT NULL UNIQUE,
        AppliedAtUtc  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Checksum      NVARCHAR(100) NULL
    );
END");

        var applied = (await conn.QueryAsync<string>(
            "SELECT ScriptName FROM tbl_SchemaVersion")).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var ranCount = 0;
        foreach (var resourceName in scripts)
        {
            var fileName = resourceName[(resourceName.IndexOf("Scripts.", StringComparison.Ordinal) + "Scripts.".Length)..];
            if (applied.Contains(fileName))
            {
                continue;
            }

            await using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync();

            _logger?.LogInformation("Applying schema script {Script}", fileName);

            foreach (var batch in SplitOnGo(sql))
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;
                await conn.ExecuteAsync(batch);
            }

            var checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)));
            await conn.ExecuteAsync(
                "INSERT INTO tbl_SchemaVersion (ScriptName, Checksum) VALUES (@ScriptName, @Checksum)",
                new { ScriptName = fileName, Checksum = checksum });

            ranCount++;
        }

        return ranCount;
    }

    private async Task EnsureDatabaseExistsAsync()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var dbName = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(dbName)) return;

        builder.InitialCatalog = "master";
        await using var conn = new SqlConnection(builder.ConnectionString);
        await conn.OpenAsync();

        var exists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sys.databases WHERE name = @name",
            new { name = dbName });

        if (exists == 0)
        {
            _logger?.LogInformation("Creating database {Database}", dbName);
            await conn.ExecuteAsync($"CREATE DATABASE [{dbName.Replace("]", "]]")}]");
        }
    }

    private static Assembly LoadDatabaseAssembly()
    {
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "PMS.Database");
        if (loaded != null) return loaded;

        return Assembly.Load("PMS.Database");
    }

    private static IEnumerable<string> SplitOnGo(string sql)
    {
        var lines = sql.Split('\n');
        var current = new StringBuilder();
        foreach (var raw in lines)
        {
            var trimmed = raw.Trim();
            if (string.Equals(trimmed, "GO", StringComparison.OrdinalIgnoreCase))
            {
                yield return current.ToString();
                current.Clear();
            }
            else
            {
                current.AppendLine(raw);
            }
        }

        var tail = current.ToString();
        if (!string.IsNullOrWhiteSpace(tail))
        {
            yield return tail;
        }
    }
}
