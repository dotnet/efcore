// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexProperties;

public abstract class ComplexPropertiesFixtureBase : RelationshipsQueryFixtureBase
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
    }

    // Derived fixtures ignore some complex properties that are mapped in this one
    // (e.g. complex table splitting does not support collections)
    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(b =>
            b.Default(WarningBehavior.Ignore).Log(CoreEventId.MappedComplexPropertyIgnoredWarning));
}
