// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;

#nullable disable

public class InheritanceRelationshipsContext(DbContextOptions options) : PoolableDbContext(options)
{
    public static readonly string StoreName = "InheritanceRelationships";

    public DbSet<BaseCollectionOnBase> BaseCollectionsOnBase { get; set; }
    public DbSet<BaseCollectionOnDerived> BaseCollectionsOnDerived { get; set; }
    public DbSet<BaseInheritanceRelationshipEntity> BaseEntities { get; set; }
    public DbSet<BaseReferenceOnBase> BaseReferencesOnBase { get; set; }
    public DbSet<BaseReferenceOnDerived> BaseReferencesOnDerived { get; set; }
    public DbSet<CollectionOnBase> CollectionsOnBase { get; set; }
    public DbSet<CollectionOnDerived> CollectionsOnDerived { get; set; }
    public DbSet<NestedCollectionBase> NestedCollections { get; set; }
    public DbSet<NestedReferenceBase> NestedReferences { get; set; }
    public DbSet<PrincipalEntity> PrincipalEntities { get; set; }
    public DbSet<ReferencedEntity> ReferencedEntities { get; set; }
    public DbSet<ReferenceOnBase> ReferencesOnBase { get; set; }
    public DbSet<ReferenceOnDerived> ReferencesOnDerived { get; set; }

    public static Task SeedAsync(InheritanceRelationshipsContext context)
    {
        var baseCollectionsOnBase = InheritanceRelationshipsData.CreateBaseCollectionsOnBase();
        var baseCollectionsOnDerived = InheritanceRelationshipsData.CreateBaseCollectionsOnDerived();
        var baseEntities = InheritanceRelationshipsData.CreateBaseEntities();
        var baseReferencesOnBase = InheritanceRelationshipsData.CreateBaseReferencesOnBase();
        var baseReferencesOnDerived = InheritanceRelationshipsData.CreateBaseReferencesOnDerived();
        var collectionsOnBase = InheritanceRelationshipsData.CreateCollectionsOnBase();
        var collectionsOnDerived = InheritanceRelationshipsData.CreateCollectionsOnDerived();
        var nestedCollections = InheritanceRelationshipsData.CreateNestedCollections();
        var nestedReferences = InheritanceRelationshipsData.CreateNestedReferences();
        var principalEntities = InheritanceRelationshipsData.CreatePrincipalEntities();
        var referencedEntities = InheritanceRelationshipsData.CreateReferencedEntities();
        var referencesOnBase = InheritanceRelationshipsData.CreateReferencesOnBase();
        var referencesOnDerived = InheritanceRelationshipsData.CreateReferencesOnDerived();

        InheritanceRelationshipsData.WireUp(
            baseEntities,
            baseReferencesOnBase,
            baseReferencesOnDerived,
            referencesOnBase,
            referencesOnDerived,
            nestedReferences,
            baseCollectionsOnBase,
            baseCollectionsOnDerived,
            collectionsOnBase,
            collectionsOnDerived,
            nestedCollections);

        context.BaseCollectionsOnBase.AddRange(baseCollectionsOnBase);
        context.BaseCollectionsOnDerived.AddRange(baseCollectionsOnDerived);
        context.BaseEntities.AddRange(baseEntities);
        context.BaseReferencesOnBase.AddRange(baseReferencesOnBase);
        context.BaseReferencesOnDerived.AddRange(baseReferencesOnDerived);
        context.CollectionsOnBase.AddRange(collectionsOnBase);
        context.CollectionsOnDerived.AddRange(collectionsOnDerived);
        context.NestedCollections.AddRange(nestedCollections);
        context.NestedReferences.AddRange(nestedReferences);
        context.PrincipalEntities.AddRange(principalEntities);
        context.ReferencedEntities.AddRange(referencedEntities);
        context.ReferencesOnBase.AddRange(referencesOnBase);
        context.ReferencesOnDerived.AddRange(referencesOnDerived);

        return context.SaveChangesAsync();
    }
}
