// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

/// <summary>
///     Base fixture for tests exercising complex properties on owned entity types: the associates are owned entity types,
///     and the nested associates are complex types.
/// </summary>
public abstract class OwnedComplexPropertiesFixtureBase : AssociationsQueryFixtureBase
{
    protected override string StoreName
        => "OwnedComplexPropertiesQueryTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.OwnsOne(e => e.RequiredAssociate, ConfigureAssociate);
            b.Navigation(x => x.RequiredAssociate).IsRequired();

            b.OwnsOne(e => e.OptionalAssociate, ConfigureAssociate);
            b.Navigation(x => x.OptionalAssociate).IsRequired(false);

            b.OwnsMany(e => e.AssociateCollection, ConfigureAssociate);
        });
    }

    protected virtual void ConfigureAssociate(OwnedNavigationBuilder<RootEntity, AssociateType> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.ComplexProperty(r => r.RequiredNestedAssociate);
        builder.ComplexProperty(r => r.OptionalNestedAssociate).IsRequired(false);
        builder.ComplexCollection(r => r.NestedCollection);
    }

    protected override AssociationsData CreateData()
    {
        var data = base.CreateData();

        // Owned mapping does not support the same instance being referenced multiple times;
        // go over the referential identity entity and clone.
        var rootEntity = data.RootEntities.Single(e => e.Name.EndsWith("With_referential_identity"));
        rootEntity.OptionalAssociate = rootEntity.RequiredAssociate.DeepClone();
        rootEntity.AssociateCollection = rootEntity.AssociateCollection.Select(r => r.DeepClone()).ToList();

        return data;
    }

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder)
            .ConfigureWarnings(b => b
                // Cosmos (and possibly others) don't support navigations, so we remove RootReferencingType from the model
                .Ignore(CoreEventId.MappedEntityTypeIgnoredWarning));
}
