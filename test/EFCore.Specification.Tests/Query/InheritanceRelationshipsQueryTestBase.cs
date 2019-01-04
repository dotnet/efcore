// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationships;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceRelationshipsQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : InheritanceRelationshipsQueryFixtureBase, new()
    {
        protected InheritanceRelationshipsQueryTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [Fact]
        public virtual void Changes_in_derived_related_entities_are_detected()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var derivedEntity = context.BaseEntities.Include(e => e.BaseCollectionOnBase)
                    .Single(e => e.Name == "Derived1(4)") as DerivedInheritanceRelationshipEntity;

                Assert.NotNull(derivedEntity);

                var firstRelatedEntity = derivedEntity.BaseCollectionOnBase.Cast<DerivedCollectionOnBase>().First();

                var originalValue = firstRelatedEntity.DerivedProperty;
                Assert.NotEqual(0, originalValue);

                var entry = context.ChangeTracker.Entries<DerivedCollectionOnBase>()
                    .Single(e => e.Entity == firstRelatedEntity);

                Assert.IsType<DerivedCollectionOnBase>(entry.Entity);

                Assert.Equal(
                    "Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationships.DerivedCollectionOnBase",
                    entry.Metadata.Name);

                firstRelatedEntity.DerivedProperty = originalValue + 1;
                context.ChangeTracker.DetectChanges();

                Assert.Equal(EntityState.Modified, entry.State);
                Assert.Equal(originalValue, entry.Property(e => e.DerivedProperty).OriginalValue);
                Assert.Equal(originalValue + 1, entry.Property(e => e.DerivedProperty).CurrentValue);

                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        [Fact]
        public virtual void Entity_can_make_separate_relationships_with_base_type_and_derived_type_both()
        {
            using (var context = CreateContext())
            {
                var model = context.Model;
                var principalEntityType = model.FindEntityType(typeof(DerivedInheritanceRelationshipEntity));
                var dependentEntityType = model.FindEntityType(typeof(BaseReferenceOnDerived));
                var derivedDependentEntityType = model.FindEntityType(typeof(DerivedReferenceOnDerived));

                var fkOnBase = dependentEntityType.GetForeignKeys().Single();
                Assert.Equal(principalEntityType, fkOnBase.PrincipalEntityType);
                Assert.Equal(dependentEntityType, fkOnBase.DeclaringEntityType);
                Assert.Equal(nameof(BaseReferenceOnDerived.BaseParent), fkOnBase.DependentToPrincipal.Name);
                Assert.Equal(nameof(DerivedInheritanceRelationshipEntity.BaseReferenceOnDerived), fkOnBase.PrincipalToDependent.Name);

                var fkOnDerived = derivedDependentEntityType.GetDeclaredForeignKeys().Single();
                Assert.NotSame(fkOnBase, fkOnDerived);
                Assert.Equal(principalEntityType, fkOnDerived.PrincipalEntityType);
                Assert.Equal(derivedDependentEntityType, fkOnDerived.DeclaringEntityType);
                Assert.Null(fkOnDerived.DependentToPrincipal);
                Assert.Equal(nameof(DerivedInheritanceRelationshipEntity.DerivedReferenceOnDerived), fkOnDerived.PrincipalToDependent.Name);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance1()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.BaseReferenceOnBase);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseReferencesOnBase.Include(e => e.BaseParent);
                var result = query.ToList();

                Assert.Equal(8, result.Count);
            }
        }

        [Fact]
        public virtual void Include_self_refence_with_inheritence()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.DerivedSefReferenceOnBase);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Include_self_refence_with_inheritence_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.BaseSelfRerefenceOnDerived);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_with_filter1()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.BaseReferenceOnBase).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_with_filter_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseReferencesOnBase.Include(e => e.BaseParent).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(8, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_without_inheritance()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.ReferenceOnBase);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_without_inheritance_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.ReferencesOnBase.Include(e => e.Parent);
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_without_inheritance_with_filter()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.ReferenceOnBase).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_without_inheritance_with_filter_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.ReferencesOnBase.Include(e => e.Parent).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(4, result.Count);
            }
        }

        [Fact]
        public virtual void Include_collection_with_inheritance1()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.BaseCollectionOnBase);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.Equal(3, result.SelectMany(e => e.BaseCollectionOnBase.OfType<DerivedCollectionOnBase>()).Count(e => e.DerivedProperty != 0));
            }
        }

        [Fact]
        public virtual void Include_collection_with_inheritance_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseCollectionsOnBase.Include(e => e.BaseParent);
                var result = query.ToList();

                Assert.Equal(13, result.Count);
            }
        }

        [Fact]
        public virtual void Include_collection_with_inheritance_with_filter1()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.BaseCollectionOnBase).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.Equal(3, result.SelectMany(e => e.BaseCollectionOnBase.OfType<DerivedCollectionOnBase>()).Count(e => e.DerivedProperty != 0));
            }
        }

        [Fact]
        public virtual void Include_collection_with_inheritance_with_filter_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseCollectionsOnBase.Include(e => e.BaseParent).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(13, result.Count);
            }
        }

        [Fact]
        public virtual void Include_collection_without_inheritance()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.CollectionOnBase);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Include_collection_without_inheritance_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.CollectionsOnBase.Include(e => e.Parent);
                var result = query.ToList();

                Assert.Equal(9, result.Count);
            }
        }

        [Fact]
        public virtual void Include_collection_without_inheritance_with_filter()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.CollectionOnBase).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Include_collection_without_inheritance_with_filter_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.CollectionsOnBase.Include(e => e.Parent).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(9, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_on_derived1()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.BaseReferenceOnBase);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_on_derived2()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.BaseReferenceOnDerived);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_on_derived4()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedReferenceOnDerived);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_on_derived_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseReferencesOnDerived.Include(e => e.BaseParent);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_on_derived_with_filter1()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.BaseReferenceOnBase).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_on_derived_with_filter2()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.BaseReferenceOnDerived).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_on_derived_with_filter4()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedReferenceOnDerived).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_with_inheritance_on_derived_with_filter_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseReferencesOnDerived.Include(e => e.BaseParent).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_without_inheritance_on_derived1()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.ReferenceOnBase);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_without_inheritance_on_derived2()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.ReferenceOnDerived);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_reference_without_inheritance_on_derived_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.ReferencesOnDerived.Include(e => e.Parent);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_collection_with_inheritance_on_derived1()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.BaseCollectionOnBase);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal(2, result.SelectMany(e => e.BaseCollectionOnBase.OfType<DerivedCollectionOnBase>()).Count(e => e.DerivedProperty != 0));
            }
        }

        [Fact]
        public virtual void Include_collection_with_inheritance_on_derived2()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.BaseCollectionOnDerived);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_collection_with_inheritance_on_derived3()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedCollectionOnDerived);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Include_collection_with_inheritance_on_derived_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseCollectionsOnDerived.Include(e => e.BaseParent);
                var result = query.ToList();

                Assert.Equal(7, result.Count);
            }
        }

        [Fact]
        public virtual void Nested_include_with_inheritance_reference_reference1()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.BaseReferenceOnBase.NestedReference);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Nested_include_with_inheritance_reference_reference3()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.BaseReferenceOnBase.NestedReference);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Nested_include_with_inheritance_reference_reference_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.NestedReferences.Include(e => e.ParentReference.BaseParent);
                var result = query.ToList();

                Assert.Equal(9, result.Count);
            }
        }

        [Fact]
        public virtual void Nested_include_with_inheritance_reference_collection1()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.BaseReferenceOnBase.NestedCollection);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [Fact]
        public virtual void Nested_include_with_inheritance_reference_collection3()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.BaseReferenceOnBase.NestedCollection);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [Fact]
        public virtual void Nested_include_with_inheritance_reference_collection_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.NestedCollections.Include(e => e.ParentReference.BaseParent);
                var result = query.ToList();

                Assert.Equal(13, result.Count);
            }
        }

        [Fact]
        public virtual void Nested_include_with_inheritance_collection_reference1()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.BaseCollectionOnBase).ThenInclude(e => e.NestedReference);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.Equal(3, result.SelectMany(e => e.BaseCollectionOnBase.OfType<DerivedCollectionOnBase>()).Count(e => e.DerivedProperty != 0));
            }
        }

        [Fact]
        public virtual void Nested_include_with_inheritance_collection_reference_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.NestedReferences.Include(e => e.ParentCollection.BaseParent);
                var result = query.ToList();

                Assert.Equal(9, result.Count);
            }
        }

        [Fact]
        public virtual void Nested_include_with_inheritance_collection_collection1()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.BaseCollectionOnBase).ThenInclude(e => e.NestedCollection);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.Equal(3, result.SelectMany(e => e.BaseCollectionOnBase.OfType<DerivedCollectionOnBase>()).Count(e => e.DerivedProperty != 0));
            }
        }

        [Fact]
        public virtual void Nested_include_with_inheritance_collection_collection_reverse()
        {
            using (var context = CreateContext())
            {
                var query = context.NestedCollections.Include(e => e.ParentCollection.BaseParent);
                var result = query.ToList();

                Assert.Equal(13, result.Count);
            }
        }

        [Fact]
        public virtual void Nested_include_collection_reference_on_non_entity_base()
        {
            using (var context = CreateContext())
            {
                var query = context.ReferencedEntities.Include(e => e.Principals).ThenInclude(e => e.Reference);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        protected InheritanceRelationshipsContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }
    }
}
