// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public abstract class OwnedNavigationsFixtureBase : RelationshipsQueryFixtureBase
{
    protected override string StoreName => "OwnedNavigationsQueryTest";

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

                rrb.OwnsMany(r => r.NestedCollection, rcb => rcb.Property(x => x.Id).ValueGeneratedNever());
            });
            b.Navigation(x => x.RequiredRelated).IsRequired(true);

            b.OwnsOne(e => e.OptionalRelated, orb =>
            {
                orb.Property(x => x.Id).ValueGeneratedNever();

                orb.OwnsOne(r => r.RequiredNested, rnb => rnb.Property(x => x.Id).ValueGeneratedNever());
                orb.Navigation(x => x.RequiredNested).IsRequired(true);

                orb.OwnsOne(r => r.OptionalNested, rnb => rnb.Property(x => x.Id).ValueGeneratedNever());
                orb.Navigation(x => x.RequiredNested).IsRequired(false);

                orb.Ignore(r => r.NestedCollection);

                orb.OwnsMany(r => r.NestedCollection, rcb => rcb.Property(x => x.Id).ValueGeneratedNever());
            });
            b.Navigation(x => x.OptionalRelated).IsRequired(false);

            b.OwnsMany(e => e.RelatedCollection, rcb =>
            {
                rcb.Property(x => x.Id).ValueGeneratedNever();

                rcb.OwnsOne(r => r.RequiredNested, rnb => rnb.Property(x => x.Id).ValueGeneratedNever());
                rcb.Navigation(x => x.RequiredNested).IsRequired(true);

                rcb.OwnsOne(r => r.OptionalNested, rnb => rnb.Property(x => x.Id).ValueGeneratedNever());
                rcb.Navigation(x => x.RequiredNested).IsRequired(false);

                rcb.OwnsMany(r => r.NestedCollection, rcb => rcb.Property(x => x.Id).ValueGeneratedNever());
            });
        });
    }

    protected override RelationshipsData CreateData()
    {
        var data = base.CreateData();

        // Owned mapping does not support the same instance being referenced multiple times;
        // go over the referential identity entity and clone.
        var rootEntity = data.RootEntities.Single(e => e.Name.EndsWith("With_referential_identity"));
        rootEntity.RequiredRelated.OptionalNested = rootEntity.RequiredRelated.RequiredNested.DeepClone();
        rootEntity.OptionalRelated = rootEntity.RequiredRelated.DeepClone();
        rootEntity.RelatedCollection = rootEntity.RelatedCollection.Select(r => r.DeepClone()).ToList();

        return data;
    }

    // Derived fixtures may need to ignore some owned navigations that are mapped in this fixture.
    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(b =>
            b.Default(WarningBehavior.Ignore).Log(CoreEventId.MappedNavigationIgnoredWarning));
}
