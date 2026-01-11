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
    }
}
