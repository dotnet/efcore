// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTRelationshipsQueryRelationalFixture : InheritanceRelationshipsQueryRelationalFixture
    {
        protected override string StoreName { get; } = "TPTRelationships";

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
}
