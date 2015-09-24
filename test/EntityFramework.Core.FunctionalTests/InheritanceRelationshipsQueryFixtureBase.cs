// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests.TestModels.InheritanceRelationships;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class InheritanceRelationshipsQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract InheritanceRelationshipsContext CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>().BaseType<BaseInheritanceRelationshipEntity>();
            modelBuilder.Entity<BaseInheritanceRelationshipEntity>().HasKey(e => e.Id);

            modelBuilder.Entity<NestedReferenceDerived>().BaseType<NestedReferenceBase>();
            modelBuilder.Entity<NestedCollectionDerived>().BaseType<NestedCollectionBase>();
            modelBuilder.Entity<DerivedReferenceOnBase>().BaseType<BaseReferenceOnBase>();
            modelBuilder.Entity<DerivedCollectionOnBase>().BaseType<BaseCollectionOnBase>();
            modelBuilder.Entity<DerivedReferenceOnDerived>().BaseType<BaseReferenceOnDerived>();
            modelBuilder.Entity<DerivedCollectionOnDerived>().BaseType<BaseCollectionOnDerived>();
            modelBuilder.Entity<BaseReferenceOnBase>().HasKey(e => e.Id);
            modelBuilder.Entity<BaseReferenceOnDerived>().HasKey(e => e.Id);
            modelBuilder.Entity<BaseCollectionOnBase>().HasKey(e => e.Id);
            modelBuilder.Entity<BaseCollectionOnDerived>().HasKey(e => e.Id);
            modelBuilder.Entity<NestedReferenceBase>().HasKey(e => e.Id);
            modelBuilder.Entity<NestedCollectionBase>().HasKey(e => e.Id);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .HasOne(e => e.BaseReferenceOnBase)
                .WithOne(e => e.BaseParent)
                .ForeignKey<BaseReferenceOnBase>(e => e.BaseParentId)
                .Required(false);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .HasOne(e => e.ReferenceOnBase)
                .WithOne(e => e.Parent)
                .ForeignKey<ReferenceOnBase>(e => e.ParentId)
                .Required(false);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .HasMany(e => e.BaseCollectionOnBase)
                .WithOne(e => e.BaseParent)
                .ForeignKey(e => e.BaseParentId)
                .Required(false);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .HasMany(e => e.CollectionOnBase)
                .WithOne(e => e.Parent)
                .ForeignKey(e => e.ParentId)
                .Required(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .HasOne(e => e.BaseReferenceOnDerived)
                .WithOne(e => e.BaseParent)
                .ForeignKey<BaseReferenceOnDerived>(e => e.BaseParentId)
                .Required(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .HasOne(e => e.ReferenceOnDerived)
                .WithOne(e => e.Parent)
                .ForeignKey<ReferenceOnDerived>(e => e.ParentId)
                .Required(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .HasMany(e => e.BaseCollectionOnDerived)
                .WithOne(e => e.BaseParent)
                .ForeignKey(e => e.ParentId)
                .Required(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .HasMany(e => e.CollectionOnDerived)
                .WithOne(e => e.Parent)
                .ForeignKey(e => e.ParentId)
                .Required(false);

            modelBuilder.Entity<BaseReferenceOnBase>()
                .HasOne(e => e.NestedReference)
                .WithOne(e => e.ParentReference)
                .ForeignKey<NestedReferenceBase>(e => e.ParentReferenceId)
                .Required(false);

            modelBuilder.Entity<BaseReferenceOnBase>()
                .HasMany(e => e.NestedCollection)
                .WithOne(e => e.ParentReference)
                .ForeignKey(e => e.ParentReferenceId)
                .Required(false);

            modelBuilder.Entity<BaseCollectionOnBase>()
                .HasOne(e => e.NestedReference)
                .WithOne(e => e.ParentCollection)
                .ForeignKey<NestedReferenceBase>(e => e.ParentCollectionId)
                .Required(false);

            modelBuilder.Entity<BaseCollectionOnBase>()
                .HasMany(e => e.NestedCollection)
                .WithOne(e => e.ParentCollection)
                .ForeignKey(e => e.ParentCollectionId)
                .Required(false);
        }
    }
}
