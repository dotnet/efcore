// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class StoreValueGenerationSqliteFixture : StoreValueGenerationFixtureBase
{
    private string? _cleanDataSql;

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override void CleanData()
    {
        using var context = CreateContext();
        context.Database.ExecuteSqlRaw(_cleanDataSql ??= GenerateCleanDataSql());
    }

    private string GenerateCleanDataSql()
    {
        var context = CreateContext();
        var builder = new StringBuilder();

        foreach (var table in context.Model.GetEntityTypes().SelectMany(e => e.GetTableMappings().Select(m => m.Table.Name)))
        {
            builder.AppendLine($"DELETE FROM {table};");
            builder.AppendLine($"DELETE FROM sqlite_sequence WHERE name='{table}';");
        }

        return builder.ToString();
    }
}
