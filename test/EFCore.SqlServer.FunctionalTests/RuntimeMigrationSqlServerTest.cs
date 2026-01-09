// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class RuntimeMigrationSqlServerTest(RuntimeMigrationSqlServerTest.RuntimeMigrationSqlServerFixture fixture)
    : RuntimeMigrationTestBase<RuntimeMigrationSqlServerTest.RuntimeMigrationSqlServerFixture>(fixture)
{
    protected override Assembly ProviderAssembly
        => typeof(SqlServerDesignTimeServices).Assembly;

    protected override List<string> GetTableNames(DbConnection connection)
    {
        var tables = new List<string>();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME != '__EFMigrationsHistory'";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    public class RuntimeMigrationSqlServerFixture : RuntimeMigrationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override async Task CleanAsync(DbContext context)
        {
            await context.Database.EnsureCreatedAsync();
            var connection = context.Database.GetDbConnection();
            await context.Database.OpenConnectionAsync();

            try
            {
                // Drop foreign key constraints first
                using var dropFkCommand = connection.CreateCommand();
                dropFkCommand.CommandText = """
                    DECLARE @sql NVARCHAR(MAX) = N'';
                    SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' DROP CONSTRAINT ' + QUOTENAME(f.name) + ';'
                    FROM sys.foreign_keys f
                    INNER JOIN sys.tables t ON f.parent_object_id = t.object_id
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;
                    EXEC sp_executesql @sql;
                    """;
                await dropFkCommand.ExecuteNonQueryAsync();

                var tables = await GetTableNamesAsync(connection);
                foreach (var table in tables)
                {
                    using var dropCommand = connection.CreateCommand();
                    dropCommand.CommandText = $"DROP TABLE IF EXISTS [{table}]";
                    await dropCommand.ExecuteNonQueryAsync();
                }

                using var dropHistoryCommand = connection.CreateCommand();
                dropHistoryCommand.CommandText = "DROP TABLE IF EXISTS [__EFMigrationsHistory]";
                await dropHistoryCommand.ExecuteNonQueryAsync();
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }

        protected override async Task<List<string>> GetTableNamesAsync(DbConnection connection)
        {
            var tables = new List<string>();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME != '__EFMigrationsHistory'";
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
            return tables;
        }
    }
}
