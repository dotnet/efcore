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

    protected override void CleanDatabase(RuntimeMigrationDbContext context)
    {
        context.Database.EnsureCreated();
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = """
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' DROP CONSTRAINT ' + QUOTENAME(f.name) + ';'
                FROM sys.foreign_keys f
                INNER JOIN sys.tables t ON f.parent_object_id = t.object_id
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;
                EXEC sp_executesql @sql;
                """;
            command.ExecuteNonQuery();

            var tables = GetTableNames(connection);
            foreach (var table in tables)
            {
                using var dropCommand = connection.CreateCommand();
                dropCommand.CommandText = $"DROP TABLE IF EXISTS [{table}]";
                dropCommand.ExecuteNonQuery();
            }

            using var dropHistoryCommand = connection.CreateCommand();
            dropHistoryCommand.CommandText = "DROP TABLE IF EXISTS [__EFMigrationsHistory]";
            dropHistoryCommand.ExecuteNonQuery();
        }
        finally
        {
            if (!wasOpen)
            {
                connection.Close();
            }
        }
    }

    public class RuntimeMigrationSqlServerFixture : RuntimeMigrationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
