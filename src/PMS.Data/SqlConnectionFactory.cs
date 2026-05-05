using System.Data;
using Microsoft.Data.SqlClient;
using PMS.Core.Abstractions;

namespace PMS.Data;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    public SqlConnectionFactory(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public IDbConnection CreateOpenConnection()
    {
        var conn = new SqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }
}
