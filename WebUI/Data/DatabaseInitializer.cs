using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;

namespace WebUI.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(string connectionString, string scriptsDirectory)
    {
        var settings = new SqlConnectionStringBuilder(connectionString);

        if (settings.InitialCatalog != "EnergyPulse")
        {
            throw new InvalidOperationException("Kurulum yalnızca EnergyPulse veritabanını oluşturur.");
        }

        settings.InitialCatalog = "master";

        await using (var master = new SqlConnection(settings.ConnectionString))
        {
            await master.ExecuteAsync(
                "IF DB_ID(N'EnergyPulse') IS NULL CREATE DATABASE [EnergyPulse]",
                commandTimeout: 120);
        }

        await using var db = new SqlConnection(connectionString);
        await db.OpenAsync();
        await ApplySchemaAsync(db, Path.Combine(scriptsDirectory, "01-schema.sql"));

        Console.WriteLine("Sentetik veri üretimi kontrol ediliyor...");
        await ExecuteScriptAsync(db, Path.Combine(scriptsDirectory, "02-seed.sql"));

        var count = await db.ExecuteScalarAsync<long>("SELECT COUNT_BIG(*) FROM dbo.Consumption");
        Console.WriteLine($"Kayıt sayısı: {count:N0}");
    }

    private static async Task ApplySchemaAsync(SqlConnection db, string scriptPath)
    {
        await using var transaction = (SqlTransaction)await db.BeginTransactionAsync();

        await db.ExecuteAsync(
            """
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock
                @Resource = N'EnergyPulseSchema',
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;

            IF @result < 0
                THROW 50002, 'Schema migration lock unavailable.', 1;

            IF OBJECT_ID('dbo.SchemaMigrations', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.SchemaMigrations
                (
                    Name nvarchar(200) NOT NULL PRIMARY KEY,
                    AppliedAtUtc datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
                );
            END;
            """,
            transaction: transaction);

        var migrationName = Path.GetFileName(scriptPath);
        var applied = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM dbo.SchemaMigrations WHERE Name = @migrationName",
            new
            {
                migrationName
            },
            transaction);

        if (applied == 0)
        {
            Console.WriteLine($"Migration uygulanıyor: {migrationName}");
            await ExecuteScriptAsync(db, scriptPath, transaction);
            await db.ExecuteAsync(
                "INSERT dbo.SchemaMigrations(Name) VALUES (@migrationName)",
                new
                {
                    migrationName
                },
                transaction);
        }
        else
        {
            Console.WriteLine($"Migration daha önce uygulanmış: {migrationName}");
        }

        await transaction.CommitAsync();
    }

    private static async Task ExecuteScriptAsync(
        SqlConnection db,
        string path,
        SqlTransaction? transaction = null)
    {
        var sql = await File.ReadAllTextAsync(path);
        var batches = Regex.Split(
            sql,
            @"^\s*GO\s*$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        foreach (var batch in batches)
        {
            if (!string.IsNullOrWhiteSpace(batch))
            {
                await db.ExecuteAsync(batch, transaction: transaction, commandTimeout: 600);
            }
        }
    }
}

