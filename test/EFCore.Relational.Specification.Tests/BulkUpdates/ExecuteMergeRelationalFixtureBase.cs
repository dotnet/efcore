// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ExecuteMergeModel;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class ExecuteMergeRelationalFixtureBase : ExecuteMergeFixtureBase, ITestSqlLoggerFactory
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Gadget>().Property(e => e.Tax).HasComputedColumnSql(@"""Price"" / 10");
        modelBuilder.Entity<Keyless>().ToTable("Keyless");
    }

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
