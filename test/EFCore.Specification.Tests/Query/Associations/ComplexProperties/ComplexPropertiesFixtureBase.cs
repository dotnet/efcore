// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public abstract class ComplexPropertiesFixtureBase : AssociationsQueryFixtureBase
{
    protected override string StoreName
        => "ComplexRelationshipsQueryTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.ComplexProperty(e => e.RequiredRelated, rrb
                => rrb.ComplexProperty(r => r.OptionalNested).IsRequired(false));

            b.ComplexProperty(e => e.OptionalRelated, orb =>
            {
                orb.IsRequired(false);
                orb.ComplexProperty(r => r.OptionalNested).IsRequired(false);
            });

            b.ComplexCollection(e => e.RelatedCollection, rcb
                => rcb.ComplexProperty(r => r.OptionalNested).IsRequired(false));
        });

        // Value types are only supported with complex types, so we add them to the model here.
        modelBuilder.Entity<ValueRootEntity>(b =>
        {
            b.Property(x => x.Id).ValueGeneratedNever();

            // Note that all collections below are reference type collections,
            // as we don't yet support complex collections of value types, #31411
            b.ComplexProperty(e => e.RequiredRelated);

            b.ComplexProperty(e => e.OptionalRelated, orb =>
            {
                orb.IsRequired(false);
                orb.ComplexProperty(r => r.OptionalNested).IsRequired(false);
            });

            b.ComplexCollection(e => e.RelatedCollection, rcb
                => rcb.ComplexProperty(r => r.OptionalNested).IsRequired(false));
        });
    }

    protected override async Task SeedAsync(PoolableDbContext context)
    {
        await base.SeedAsync(context);

        context.Set<ValueRootEntity>().AddRange(Data.ValueRootEntities);

        await context.SaveChangesAsync();
    }

    // Derived fixtures ignore some complex properties that are mapped in this one
    // (e.g. complex table splitting does not support collections)
    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(b =>
            b.Default(WarningBehavior.Ignore).Log(CoreEventId.MappedComplexPropertyIgnoredWarning));
}
