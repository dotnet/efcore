// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#pragma warning disable EF9107 // ExecuteMerge is experimental

public class ExecuteMergeSqliteTest(ExecuteMergeSqliteFixture fixture)
    : ExecuteMergeRelationalTestBase<ExecuteMergeSqliteFixture>(fixture)
{
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Returning_on_table_without_returning_clause_throws(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<WidgetWithoutReturningContext>();
        await using var context = contextFactory.CreateDbContext();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => MergeReturning(async, context.Set<Widget>(), [new Widget { Id = 1, Name = "a" }], merge => { }, w => w.Id));

        Assert.Equal(SqliteStrings.ExecuteMergeReturningWithoutReturningClause("Widgets"), exception.InnerException!.Message);
    }

    protected class WidgetWithoutReturningContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Widget>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.ToTable("Widgets", tb => tb.UseSqlReturningClause(false));
                });
    }
}
