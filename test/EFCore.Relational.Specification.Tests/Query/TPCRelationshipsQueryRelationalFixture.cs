// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPCRelationshipsQueryRelationalFixture : InheritanceRelationshipsQueryRelationalFixture
{
    protected override string StoreName
        => "TPCRelationships";

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w => w.Log(RelationalEventId.ForeignKeyTpcPrincipalWarning));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<BaseInheritanceRelationshipEntity>().UseTpcMappingStrategy()
            // Table-sharing is not supported in TPC mapping
            .OwnsMany(e => e.OwnedCollectionOnBase, e => e.ToTable("OwnedCollections"))
            .OwnsOne(e => e.OwnedReferenceOnBase, e => e.ToTable("OwnedReferences"));
        modelBuilder.Entity<BaseReferenceOnBase>().UseTpcMappingStrategy();
        modelBuilder.Entity<BaseCollectionOnBase>().UseTpcMappingStrategy();
        modelBuilder.Entity<BaseReferenceOnDerived>().UseTpcMappingStrategy();
        modelBuilder.Entity<BaseCollectionOnDerived>().UseTpcMappingStrategy();
        modelBuilder.Entity<NestedReferenceBase>().UseTpcMappingStrategy();
        modelBuilder.Entity<NestedCollectionBase>().UseTpcMappingStrategy();

        modelBuilder.Entity<DerivedInheritanceRelationshipEntity>().ToTable("DerivedEntities");

        modelBuilder.Entity<DerivedReferenceOnBase>().ToTable("DerivedReferencesOnBase");
        modelBuilder.Entity<DerivedCollectionOnBase>().ToTable("DerivedCollectionsOnBase");
        modelBuilder.Entity<DerivedReferenceOnDerived>().ToTable("DerivedReferencesOnDerived");
        modelBuilder.Entity<DerivedCollectionOnDerived>().ToTable("DerivedCollectionsOnDerived");

        modelBuilder.Entity<NestedReferenceDerived>().ToTable("NestedReferencesDerived");
        modelBuilder.Entity<NestedCollectionDerived>().ToTable("NestedCollectionsDerived");
    }
}
