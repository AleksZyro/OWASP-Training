using Microsoft.EntityFrameworkCore;

namespace OwaspForge.Web.Data;

public static class ForgeDatabaseInitializer
{
    public const string InitialMigrationId = "20260906120000_InitialProgress";
    public const string ScoringMigrationId = "20260907100000_Scoring";

    public static async Task InitializeAsync(ForgeDbContext database, CancellationToken cancellationToken = default)
    {
        if (await HasLegacyProgressTableAsync(database, cancellationToken))
        {
            await database.Database.ExecuteSqlRawAsync(
                "CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (MigrationId TEXT NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY, ProductVersion TEXT NOT NULL);",
                cancellationToken);
            await database.Database.ExecuteSqlRawAsync(
                $"INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('{InitialMigrationId}', '8.0.30');",
                cancellationToken);
            if (await HasColumnAsync(database, "Attempts", cancellationToken))
            {
                await database.Database.ExecuteSqlRawAsync(
                    $"INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('{ScoringMigrationId}', '8.0.30');",
                    cancellationToken);
            }
        }

        await database.Database.MigrateAsync(cancellationToken);
    }

    private static async Task<bool> HasColumnAsync(ForgeDbContext database, string columnName, CancellationToken cancellationToken)
    {
        var connection = database.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info('Progress');";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (StringComparer.Ordinal.Equals(reader.GetString(1), columnName)) return true;
            }

            return false;
        }
        finally { await connection.CloseAsync(); }
    }

    private static async Task<bool> HasLegacyProgressTableAsync(ForgeDbContext database, CancellationToken cancellationToken)
    {
        var connection = database.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'Progress' LIMIT 1;";
            return await command.ExecuteScalarAsync(cancellationToken) is not null;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
