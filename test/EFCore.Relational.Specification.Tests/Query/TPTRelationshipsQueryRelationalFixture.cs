// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPTRelationshipsQueryRelationalFixture : InheritanceRelationshipsQueryRelationalFixture
{
    protected override string StoreName
        => "TPTRelationships";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<DerivedInheritanceRelationshipEntity>().ToTable("DerivedEntities");

        modelBuilder.Entity<DerivedReferenceOnBase>().ToTable("DerivedReferencesOnBase");
        modelBuilder.Entity<DerivedCollectionOnBase>().ToTable("DerivedCollectionsOnBase");
        modelBuilder.Entity<DerivedReferenceOnDerived>().ToTable("DerivedReferencesOnDerived");
        modelBuilder.Entity<DerivedCollectionOnDerived>().ToTable("DerivedCollectionsOnDerived");

        modelBuilder.Entity<NestedReferenceDerived>().ToTable("NestedReferencesDerived");
        modelBuilder.Entity<NestedCollectionDerived>().ToTable("NestedCollectionsDerived");
    }
}
