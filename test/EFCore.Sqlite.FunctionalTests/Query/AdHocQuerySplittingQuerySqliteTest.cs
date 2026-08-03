// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocQuerySplittingQuerySqliteTest(NonSharedFixture fixture) : AdHocQuerySplittingQueryTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    private static readonly FieldInfo _querySplittingBehaviorFieldInfo =
        typeof(RelationalOptionsExtension).GetField("_querySplittingBehavior", BindingFlags.NonPublic | BindingFlags.Instance);

    protected override DbContextOptionsBuilder SetQuerySplittingBehavior(
        DbContextOptionsBuilder optionsBuilder,
        QuerySplittingBehavior splittingBehavior)
    {
        new SqliteDbContextOptionsBuilder(optionsBuilder).UseQuerySplittingBehavior(splittingBehavior);

        return optionsBuilder;
    }

    protected override DbContextOptionsBuilder ClearQuerySplittingBehavior(DbContextOptionsBuilder optionsBuilder)
    {
        var extension = optionsBuilder.Options.FindExtension<SqliteOptionsExtension>();
        if (extension == null)
        {
            extension = new SqliteOptionsExtension();
        }
        else
        {
            _querySplittingBehaviorFieldInfo.SetValue(extension, null);
        }

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    // SQLite uses a per-connection snapshot within a transaction, so a concurrent modification made on a separate connection
    // is not visible to the split query's child statements (which run on the same connection). The concurrent-modification
    // scenario from #33826 therefore cannot be reproduced on SQLite; it is covered by the SQL Server tests.
    public override Task Split_include_collection_throws_for_orphan_child_rows_after_concurrent_insert(bool async)
        => Task.CompletedTask;

    public override Task Split_include_collection_not_dropped_when_other_parent_made_childless_concurrently(bool async)
        => Task.CompletedTask;
}
