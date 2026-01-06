// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

/// <summary>
///     Collection definition to ensure runtime migration tests run sequentially.
///     These tests share a database and must not run in parallel.
/// </summary>
[CollectionDefinition("RuntimeMigrationSqlServer", DisableParallelization = true)]
public class RuntimeMigrationSqlServerCollection;

[Collection("RuntimeMigrationSqlServer")]
public class RuntimeMigrationSqlServerTest : RuntimeMigrationTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

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
        // SQL Server requires dropping foreign key constraints before dropping tables
        context.Database.EnsureCreated();
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }

        // First, drop all foreign key constraints
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
                FROM sys.foreign_keys;
                EXEC sp_executesql @sql;";
            command.ExecuteNonQuery();
        }

        // Then drop all tables
        var tables = GetTableNames(connection);
        foreach (var table in tables)
        {
            using var dropCommand = connection.CreateCommand();
            dropCommand.CommandText = $"DROP TABLE IF EXISTS [{table}]";
            dropCommand.ExecuteNonQuery();
        }

        // Drop migrations history table
        using var dropHistoryCommand = connection.CreateCommand();
        dropHistoryCommand.CommandText = "DROP TABLE IF EXISTS [__EFMigrationsHistory]";
        dropHistoryCommand.ExecuteNonQuery();
    }
}
