// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedTableSplitting;

/// <summary>
///     Base fixture for tests exercising table splitting via owned entities, where the entity and its owned entity are mapped to the same
///     table, and the complex type's properties are mapped to columns in that table. Collections are mapped to separate tables.
/// </summary>
public abstract class OwnedTableSplittingRelationalFixtureBase : OwnedNavigationsFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName => "OwnedTableSplittingQueryTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.OwnsOne(e => e.RequiredRelated, rrb =>
            {
                rrb.Property(x => x.Id).ValueGeneratedNever();

                rrb.OwnsOne(r => r.RequiredNested, rnb => rnb.Property(x => x.Id).ValueGeneratedNever());
                rrb.Navigation(x => x.RequiredNested).IsRequired(true);

                rrb.OwnsOne(r => r.OptionalNested, rnb => rnb.Property(x => x.Id).ValueGeneratedNever());
                rrb.Navigation(x => x.RequiredNested).IsRequired(false);

                rrb.OwnsMany(r => r.NestedCollection, rcb =>
                {
                    rcb.Property(x => x.Id).ValueGeneratedNever();
                    rcb.ToTable("RequiredRelated_NestedCollection");
                });
            });
            b.Navigation(x => x.RequiredRelated).IsRequired(true);

            b.OwnsOne(e => e.OptionalRelated, orb =>
            {
                orb.Property(x => x.Id).ValueGeneratedNever();

                orb.OwnsOne(r => r.RequiredNested, rnb => rnb.Property(x => x.Id).ValueGeneratedNever());
                orb.Navigation(x => x.RequiredNested).IsRequired(true);

                orb.OwnsOne(r => r.OptionalNested, rnb => rnb.Property(x => x.Id).ValueGeneratedNever());
                orb.Navigation(x => x.RequiredNested).IsRequired(false);

                orb.OwnsMany(r => r.NestedCollection, rcb =>
                {
                    rcb.Property(x => x.Id).ValueGeneratedNever();
                    rcb.ToTable("OptionalRelated_NestedCollection");
                });
            });
            b.Navigation(x => x.OptionalRelated).IsRequired(false);

            b.OwnsMany(e => e.RelatedCollection, rcb =>
            {
                rcb.Property(x => x.Id).ValueGeneratedNever();
                rcb.ToTable("RelatedCollection");

                rcb.OwnsOne(r => r.RequiredNested, rnb => rnb.Property(x => x.Id).ValueGeneratedNever());
                rcb.Navigation(x => x.RequiredNested).IsRequired(true);

                rcb.OwnsOne(r => r.OptionalNested, rnb => rnb.Property(x => x.Id).ValueGeneratedNever());
                rcb.Navigation(x => x.RequiredNested).IsRequired(false);

                rcb.OwnsMany(r => r.NestedCollection, rcb =>
                {
                    rcb.Property(x => x.Id).ValueGeneratedNever();
                    rcb.ToTable("RelatedCollection_NestedCollection");
                });
            });
        });
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
