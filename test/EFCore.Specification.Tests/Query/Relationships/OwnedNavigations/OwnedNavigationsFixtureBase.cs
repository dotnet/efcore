// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public abstract class OwnedNavigationsFixtureBase : RelationshipsQueryFixtureBase
{
    protected override string StoreName => "OwnedNavigationsQueryTest";

    protected override Task SeedAsync(PoolableDbContext context)
    {
        var rootEntities = RelationshipsData.CreateRootEntities();
        context.Set<RootEntity>().AddRange(rootEntities);

        return context.SaveChangesAsync();
    }

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

    // Derived fixtures may need to ignore some owned navigations that are mapped in this fixture.
    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(b =>
            b.Default(WarningBehavior.Ignore).Log(CoreEventId.MappedNavigationIgnoredWarning));
}
