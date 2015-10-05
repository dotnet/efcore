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
            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>().HasBaseType<BaseInheritanceRelationshipEntity>();
            modelBuilder.Entity<BaseInheritanceRelationshipEntity>().HasKey(e => e.Id);

            modelBuilder.Entity<NestedReferenceDerived>().HasBaseType<NestedReferenceBase>();
            modelBuilder.Entity<NestedCollectionDerived>().HasBaseType<NestedCollectionBase>();
            modelBuilder.Entity<DerivedReferenceOnBase>().HasBaseType<BaseReferenceOnBase>();
            modelBuilder.Entity<DerivedCollectionOnBase>().HasBaseType<BaseCollectionOnBase>();
            modelBuilder.Entity<DerivedReferenceOnDerived>().HasBaseType<BaseReferenceOnDerived>();
            modelBuilder.Entity<DerivedCollectionOnDerived>().HasBaseType<BaseCollectionOnDerived>();
            modelBuilder.Entity<BaseReferenceOnBase>().HasKey(e => e.Id);
            modelBuilder.Entity<BaseReferenceOnDerived>().HasKey(e => e.Id);
            modelBuilder.Entity<BaseCollectionOnBase>().HasKey(e => e.Id);
            modelBuilder.Entity<BaseCollectionOnDerived>().HasKey(e => e.Id);
            modelBuilder.Entity<NestedReferenceBase>().HasKey(e => e.Id);
            modelBuilder.Entity<NestedCollectionBase>().HasKey(e => e.Id);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .HasOne(e => e.BaseReferenceOnBase)
                .WithOne(e => e.BaseParent)
                .HasForeignKey<BaseReferenceOnBase>(e => e.BaseParentId)
                .IsRequired(false);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .HasOne(e => e.ReferenceOnBase)
                .WithOne(e => e.Parent)
                .HasForeignKey<ReferenceOnBase>(e => e.ParentId)
                .IsRequired(false);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .HasMany(e => e.BaseCollectionOnBase)
                .WithOne(e => e.BaseParent)
                .HasForeignKey(e => e.BaseParentId)
                .IsRequired(false);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .HasMany(e => e.CollectionOnBase)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => e.ParentId)
                .IsRequired(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .HasOne(e => e.BaseReferenceOnDerived)
                .WithOne(e => e.BaseParent)
                .HasForeignKey<BaseReferenceOnDerived>(e => e.BaseParentId)
                .IsRequired(false);

            //TODO: See issue #3289
            modelBuilder.Entity<DerivedReferenceOnDerived>().Property(typeof(int?), "DerivedInheritanceRelationshipEntityId");

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .HasOne(e => e.DerivedReferenceOnDerived)
                .WithOne()
                .HasForeignKey(typeof(DerivedReferenceOnDerived), "DerivedInheritanceRelationshipEntityId")
                .IsRequired(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .HasOne(e => e.ReferenceOnDerived)
                .WithOne(e => e.Parent)
                .HasForeignKey<ReferenceOnDerived>(e => e.ParentId)
                .IsRequired(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .HasMany(e => e.BaseCollectionOnDerived)
                .WithOne(e => e.BaseParent)
                .HasForeignKey(e => e.ParentId)
                .IsRequired(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .HasMany(e => e.CollectionOnDerived)
                .WithOne(e => e.Parent)
                .HasForeignKey(e => e.ParentId)
                .IsRequired(false);

            modelBuilder.Entity<BaseReferenceOnBase>()
                .HasOne(e => e.NestedReference)
                .WithOne(e => e.ParentReference)
                .HasForeignKey<NestedReferenceBase>(e => e.ParentReferenceId)
                .IsRequired(false);

            modelBuilder.Entity<BaseReferenceOnBase>()
                .HasMany(e => e.NestedCollection)
                .WithOne(e => e.ParentReference)
                .HasForeignKey(e => e.ParentReferenceId)
                .IsRequired(false);

            modelBuilder.Entity<BaseCollectionOnBase>()
                .HasOne(e => e.NestedReference)
                .WithOne(e => e.ParentCollection)
                .HasForeignKey<NestedReferenceBase>(e => e.ParentCollectionId)
                .IsRequired(false);

            modelBuilder.Entity<BaseCollectionOnBase>()
                .HasMany(e => e.NestedCollection)
                .WithOne(e => e.ParentCollection)
                .HasForeignKey(e => e.ParentCollectionId)
                .IsRequired(false);
        }
    }
}
