// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public abstract class OwnedJsonRelationalFixtureBase : OwnedNavigationsFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "OwnedJsonJsonQueryTest";

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.OwnsOne(e => e.RequiredRelated, rrb => rrb.ToJson());
            b.OwnsOne(e => e.OptionalRelated, orb => orb.ToJson());
            b.OwnsMany(e => e.RelatedCollection, orb => orb.ToJson());
        });
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
