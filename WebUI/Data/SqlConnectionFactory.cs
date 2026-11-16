using Microsoft.Data.SqlClient;

namespace WebUI.Data;

public sealed class SqlConnectionFactory(string connectionString)
{
    public SqlConnection Create() => new(connectionString);
}

