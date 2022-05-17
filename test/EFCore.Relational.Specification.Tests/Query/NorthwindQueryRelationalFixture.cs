// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindQueryRelationalFixture<TModelCustomizer> : NorthwindQueryFixtureBase<TModelCustomizer>, IBulkUpdatesFixtureBase
    where TModelCustomizer : IModelCustomizer, new()
{
    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
                c => c
                    .Log(RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning))
            .EnableDetailedErrors();

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Query.Name;

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override Type ContextType
        => typeof(NorthwindRelationalContext);
}
