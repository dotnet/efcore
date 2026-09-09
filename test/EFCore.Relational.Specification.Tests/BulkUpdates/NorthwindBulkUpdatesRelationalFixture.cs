// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class NorthwindBulkUpdatesRelationalFixture<TModelCustomizer> : NorthwindBulkUpdatesFixture<TModelCustomizer>,
    ITestSqlLoggerFactory
    where TModelCustomizer : ITestModelCustomizer, new()
{
    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
                c => c.Log(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning))
            .EnableDetailedErrors();

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Query.Name;

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
