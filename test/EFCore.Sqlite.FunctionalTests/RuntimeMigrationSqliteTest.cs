// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

/// <summary>
///     Collection definition to ensure runtime migration tests run sequentially.
///     These tests share a database and must not run in parallel.
/// </summary>
[CollectionDefinition("RuntimeMigration", DisableParallelization = true)]
public class RuntimeMigrationCollection;

[Collection("RuntimeMigration")]
public class RuntimeMigrationSqliteTest : RuntimeMigrationTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override Assembly ProviderAssembly
        => typeof(SqliteDesignTimeServices).Assembly;

    protected override List<string> GetTableNames(DbConnection connection)
    {
        var tables = new List<string>();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name != '__EFMigrationsHistory'";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }
}
