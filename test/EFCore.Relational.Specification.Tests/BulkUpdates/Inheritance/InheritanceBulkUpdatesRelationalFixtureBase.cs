// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

public abstract class InheritanceBulkUpdatesRelationalFixtureBase : InheritanceBulkUpdatesFixtureBase, ITestSqlLoggerFactory
{
    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // In relational, complex collections are only supported as JSON and must be explicitly configured as such
        modelBuilder.Entity<Drink>().ComplexCollection(n => n.ComplexTypeCollection, n => n.ToJson());
    }

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(c => c.Log(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning))
            .EnableDetailedErrors();

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Query.Name;
}
