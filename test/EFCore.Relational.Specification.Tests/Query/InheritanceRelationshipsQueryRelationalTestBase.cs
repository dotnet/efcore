// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationships;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceRelationshipsQueryRelationalTestBase<TFixture> : InheritanceRelationshipsQueryTestBase<TFixture>
        where TFixture : InheritanceRelationshipsQueryRelationalFixture, new()
    {
        public InheritanceRelationshipsQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public virtual void Include_collection_with_inheritance_split()
        {
            using var context = CreateContext();
            var query = context.BaseEntities.Include(e => e.BaseCollectionOnBase);
            var result = query.ToList();

            Assert.Equal(6, result.Count);
            Assert.Equal(
                3, result.SelectMany(e => e.BaseCollectionOnBase.OfType<DerivedCollectionOnBase>()).Count(e => e.DerivedProperty != 0));
        }

        [ConditionalFact]
        public virtual void Include_collection_with_inheritance_reverse_split()
        {
            using var context = CreateContext();
            var query = context.BaseCollectionsOnBase.Include(e => e.BaseParent);
            var result = query.ToList();

            Assert.Equal(13, result.Count);
        }

        [ConditionalFact]
        public virtual void Include_collection_with_inheritance_with_filter_split()
        {
            using var context = CreateContext();
            var query = context.BaseEntities.Include(e => e.BaseCollectionOnBase).Where(e => e.Name != "Bar");
            var result = query.ToList();

            Assert.Equal(6, result.Count);
            Assert.Equal(
                3, result.SelectMany(e => e.BaseCollectionOnBase.OfType<DerivedCollectionOnBase>()).Count(e => e.DerivedProperty != 0));
        }

        [ConditionalFact]
        public virtual void Include_collection_with_inheritance_with_filter_reverse_split()
        {
            using var context = CreateContext();
            var query = context.BaseCollectionsOnBase.Include(e => e.BaseParent).Where(e => e.Name != "Bar");
            var result = query.ToList();

            Assert.Equal(13, result.Count);
        }

        [ConditionalFact]
        public virtual void Include_collection_without_inheritance_split()
        {
            using var context = CreateContext();
            var query = context.BaseEntities.Include(e => e.CollectionOnBase);
            var result = query.ToList();

            Assert.Equal(6, result.Count);
        }

        [ConditionalFact]
        public virtual void Include_collection_without_inheritance_reverse_split()
        {
            using var context = CreateContext();
            var query = context.CollectionsOnBase.Include(e => e.Parent);
            var result = query.ToList();

            Assert.Equal(9, result.Count);
        }

        [ConditionalFact]
        public virtual void Include_collection_without_inheritance_with_filter_split()
        {
            using var context = CreateContext();
            var query = context.BaseEntities.Include(e => e.CollectionOnBase).Where(e => e.Name != "Bar");
            var result = query.ToList();

            Assert.Equal(6, result.Count);
        }

        [ConditionalFact]
        public virtual void Include_collection_without_inheritance_with_filter_reverse_split()
        {
            using var context = CreateContext();
            var query = context.CollectionsOnBase.Include(e => e.Parent).Where(e => e.Name != "Bar");
            var result = query.ToList();

            Assert.Equal(9, result.Count);
        }

        [ConditionalFact]
        public virtual void Include_collection_with_inheritance_on_derived1_split()
        {
            using var context = CreateContext();
            var query = context.DerivedEntities.Include(e => e.BaseCollectionOnBase);
            var result = query.ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(
                2, result.SelectMany(e => e.BaseCollectionOnBase.OfType<DerivedCollectionOnBase>()).Count(e => e.DerivedProperty != 0));
        }

        [ConditionalFact]
        public virtual void Include_collection_with_inheritance_on_derived2_split()
        {
            using var context = CreateContext();
            var query = context.DerivedEntities.Include(e => e.BaseCollectionOnDerived);
            var result = query.ToList();

            Assert.Equal(3, result.Count);
        }

        [ConditionalFact]
        public virtual void Include_collection_with_inheritance_on_derived3_split()
        {
            using var context = CreateContext();
            var query = context.DerivedEntities.Include(e => e.DerivedCollectionOnDerived);
            var result = query.ToList();

            Assert.Equal(3, result.Count);
        }

        [ConditionalFact]
        public virtual void Include_collection_with_inheritance_on_derived_reverse_split()
        {
            using var context = CreateContext();
            var query = context.BaseCollectionsOnDerived.Include(e => e.BaseParent);
            var result = query.ToList();

            Assert.Equal(7, result.Count);
        }

        [ConditionalFact]
        public virtual void Nested_include_with_inheritance_reference_collection_split()
        {
            using var context = CreateContext();
            var query = context.BaseEntities.Include(e => e.BaseReferenceOnBase.NestedCollection);
            var result = query.ToList();

            Assert.Equal(6, result.Count);
        }

        [ConditionalFact]
        public virtual void Nested_include_with_inheritance_reference_collection_on_base_split()
        {
            using var context = CreateContext();
            var query = context.DerivedEntities.Include(e => e.BaseReferenceOnBase.NestedCollection);
            var result = query.ToList();

            Assert.Equal(3, result.Count);
        }

        [ConditionalFact]
        public virtual void Nested_include_with_inheritance_reference_collection_reverse_split()
        {
            using var context = CreateContext();
            var query = context.NestedCollections.Include(e => e.ParentReference.BaseParent);
            var result = query.ToList();

            Assert.Equal(13, result.Count);
        }

        [ConditionalFact]
        public virtual void Nested_include_with_inheritance_collection_reference_split()
        {
            using var context = CreateContext();
            var query = context.BaseEntities.Include(e => e.BaseCollectionOnBase).ThenInclude(e => e.NestedReference);
            var result = query.ToList();

            Assert.Equal(6, result.Count);
            Assert.Equal(
                3, result.SelectMany(e => e.BaseCollectionOnBase.OfType<DerivedCollectionOnBase>()).Count(e => e.DerivedProperty != 0));
        }

        [ConditionalFact]
        public virtual void Nested_include_with_inheritance_collection_reference_reverse_split()
        {
            using var context = CreateContext();
            var query = context.NestedReferences.Include(e => e.ParentCollection.BaseParent);
            var result = query.ToList();

            Assert.Equal(9, result.Count);
        }

        [ConditionalFact]
        public virtual void Nested_include_with_inheritance_collection_collection_split()
        {
            using var context = CreateContext();
            var query = context.BaseEntities.Include(e => e.BaseCollectionOnBase).ThenInclude(e => e.NestedCollection);
            var result = query.ToList();

            Assert.Equal(6, result.Count);
            Assert.Equal(
                3, result.SelectMany(e => e.BaseCollectionOnBase.OfType<DerivedCollectionOnBase>()).Count(e => e.DerivedProperty != 0));
        }

        [ConditionalFact]
        public virtual void Nested_include_with_inheritance_collection_collection_reverse_split()
        {
            using var context = CreateContext();
            var query = context.NestedCollections.Include(e => e.ParentCollection.BaseParent);
            var result = query.ToList();

            Assert.Equal(13, result.Count);
        }

        [ConditionalFact]
        public virtual void Nested_include_collection_reference_on_non_entity_base_split()
        {
            using var context = CreateContext();
            var query = context.ReferencedEntities.Include(e => e.Principals).ThenInclude(e => e.Reference);
            var result = query.ToList();

            Assert.Equal(2, result.Count);
        }
    }
}
