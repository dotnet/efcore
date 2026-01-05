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
}
