// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public abstract class NavigationsRelationalFixtureBase : NavigationsFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "NavigationsQueryTest";

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.Navigation(e => e.RequiredRelated).AutoInclude();
            b.Navigation(e => e.OptionalRelated).AutoInclude();
            b.Navigation(e => e.RelatedCollection).AutoInclude();
        });

        modelBuilder.Entity<RelatedType>(b =>
        {
            b.Navigation(e => e.RequiredNested).AutoInclude();
            b.Navigation(e => e.OptionalNested).AutoInclude();
            b.Navigation(e => e.NestedCollection).AutoInclude();
        });
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
