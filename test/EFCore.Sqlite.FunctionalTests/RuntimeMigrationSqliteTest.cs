// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;

namespace Microsoft.EntityFrameworkCore;

public class RuntimeMigrationSqliteTest(RuntimeMigrationSqliteTest.RuntimeMigrationSqliteFixture fixture)
    : RuntimeMigrationTestBase<RuntimeMigrationSqliteTest.RuntimeMigrationSqliteFixture>(fixture)
{
    protected override Assembly ProviderAssembly
        => typeof(SqliteDesignTimeServices).Assembly;

    protected override List<string> GetTableNames(DbConnection connection)
    {
        var tables = new List<string>();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name != '__EFMigrationsHistory' AND name NOT LIKE 'sqlite_%'";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    public class RuntimeMigrationSqliteFixture : RuntimeMigrationFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
