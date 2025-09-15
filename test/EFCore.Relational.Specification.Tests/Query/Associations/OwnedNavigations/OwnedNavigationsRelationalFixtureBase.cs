// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

/// <summary>
///     Base fixture for tests exercising owned entities mapped to separate tables.
/// </summary>
public abstract class OwnedNavigationsRelationalFixtureBase : OwnedTableSplittingRelationalFixtureBase, ITestSqlLoggerFactory
{
    public override bool AreCollectionsOrdered
        => false;

    protected override string StoreName
        => "OwnedNavigationsQueryTest";

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // Note that this fixture extends OwnedTableSplittingRelationalFixtureBase; it only overrides to
        // map the non-collection owned navigations to separate tables (disabling the default table splitting behavior).
        modelBuilder.Entity<RootEntity>(b =>
        {
            b.OwnsOne(
                e => e.RequiredRelated, rrb =>
                {
                    rrb.ToTable("RequiredRelated");

                    rrb.OwnsOne(r => r.RequiredNested, rnb => rnb.ToTable("RequiredRelated_RequiredNested"));
                    rrb.OwnsOne(r => r.OptionalNested, rnb => rnb.ToTable("RequiredRelated_OptionalNested"));
                });

            b.OwnsOne(
                e => e.OptionalRelated, rrb =>
                {
                    rrb.ToTable("OptionalRelated");

                    rrb.OwnsOne(r => r.RequiredNested, rnb => rnb.ToTable("OptionalRelated_RequiredNested"));
                    rrb.OwnsOne(r => r.OptionalNested, rnb => rnb.ToTable("OptionalRelated_OptionalNested"));
                });

            b.OwnsMany(
                e => e.RelatedCollection, rcb =>
                {
                    rcb.OwnsOne(r => r.RequiredNested, rnb => rnb.ToTable("RelatedCollection_RequiredNested"));
                    rcb.OwnsOne(r => r.OptionalNested, rnb => rnb.ToTable("RelatedCollection_OptionalNested"));
                });
        });
    }

    public void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}
