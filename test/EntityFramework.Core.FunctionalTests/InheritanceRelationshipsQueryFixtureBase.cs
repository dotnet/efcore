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
            modelBuilder.Entity<BaseInheritanceRelationshipEntity>().Key(e => e.Id);

            modelBuilder.Entity<NestedReferenceDerived>().BaseType<NestedReferenceBase>();
            modelBuilder.Entity<NestedCollectionDerived>().BaseType<NestedCollectionBase>();
            modelBuilder.Entity<DerivedReferenceOnBase>().BaseType<BaseReferenceOnBase>();
            modelBuilder.Entity<DerivedCollectionOnBase>().BaseType<BaseCollectionOnBase>();
            modelBuilder.Entity<DerivedReferenceOnDerived>().BaseType<BaseReferenceOnDerived>();
            modelBuilder.Entity<DerivedCollectionOnDerived>().BaseType<BaseCollectionOnDerived>();
            modelBuilder.Entity<BaseReferenceOnBase>().Key(e => e.Id);
            modelBuilder.Entity<BaseReferenceOnDerived>().Key(e => e.Id);
            modelBuilder.Entity<BaseCollectionOnBase>().Key(e => e.Id);
            modelBuilder.Entity<BaseCollectionOnDerived>().Key(e => e.Id);
            modelBuilder.Entity<NestedReferenceBase>().Key(e => e.Id);
            modelBuilder.Entity<NestedCollectionBase>().Key(e => e.Id);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .Reference(e => e.BaseReferenceOnBase)
                .InverseReference(e => e.BaseParent)
                .ForeignKey<BaseReferenceOnBase>(e => e.BaseParentId)
                .Required(false);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .Reference(e => e.ReferenceOnBase)
                .InverseReference(e => e.Parent)
                .ForeignKey<ReferenceOnBase>(e => e.ParentId)
                .Required(false);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .Collection(e => e.BaseCollectionOnBase)
                .InverseReference(e => e.BaseParent)
                .ForeignKey(e => e.BaseParentId)
                .Required(false);

            modelBuilder.Entity<BaseInheritanceRelationshipEntity>()
                .Collection(e => e.CollectionOnBase)
                .InverseReference(e => e.Parent)
                .ForeignKey(e => e.ParentId)
                .Required(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .Reference(e => e.BaseReferenceOnDerived)
                .InverseReference(e => e.BaseParent)
                .ForeignKey<BaseReferenceOnDerived>(e => e.BaseParentId)
                .Required(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .Reference(e => e.ReferenceOnDerived)
                .InverseReference(e => e.Parent)
                .ForeignKey<ReferenceOnDerived>(e => e.ParentId)
                .Required(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .Collection(e => e.BaseCollectionOnDerived)
                .InverseReference(e => e.BaseParent)
                .ForeignKey(e => e.ParentId)
                .Required(false);

            modelBuilder.Entity<DerivedInheritanceRelationshipEntity>()
                .Collection(e => e.CollectionOnDerived)
                .InverseReference(e => e.Parent)
                .ForeignKey(e => e.ParentId)
                .Required(false);

            modelBuilder.Entity<BaseReferenceOnBase>()
                .Reference(e => e.NestedReference)
                .InverseReference(e => e.ParentReference)
                .ForeignKey<NestedReferenceBase>(e => e.ParentReferenceId)
                .Required(false);

            modelBuilder.Entity<BaseReferenceOnBase>()
                .Collection(e => e.NestedCollection)
                .InverseReference(e => e.ParentReference)
                .ForeignKey(e => e.ParentReferenceId)
                .Required(false);

            modelBuilder.Entity<BaseCollectionOnBase>()
                .Reference(e => e.NestedReference)
                .InverseReference(e => e.ParentCollection)
                .ForeignKey<NestedReferenceBase>(e => e.ParentCollectionId)
                .Required(false);

            modelBuilder.Entity<BaseCollectionOnBase>()
                .Collection(e => e.NestedCollection)
                .InverseReference(e => e.ParentCollection)
                .ForeignKey(e => e.ParentCollectionId)
                .Required(false);
        }
    }
}
