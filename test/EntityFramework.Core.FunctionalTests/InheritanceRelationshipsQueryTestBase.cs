// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.InheritanceRelationships;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class InheritanceRelationshipsQueryTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
    where TTestStore : TestStore
    where TFixture : InheritanceRelationshipsQueryFixtureBase<TTestStore>, new()
    {
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

        // doesn't work for now
        //[Fact]
        public virtual void Include_reference_with_inheritance2()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.DerivedReferenceOnBase);
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
        public virtual void Include_reference_with_inheritance_with_filter1()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.BaseReferenceOnBase).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        // doesn't work for now
        //[Fact]
        public virtual void Include_reference_with_inheritance_with_filter2()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.DerivedReferenceOnBase).Where(e => e.Name != "Bar");
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
            }
        }

        // doesn't work for now
        //[Fact]
        public virtual void Include_collection_with_inheritance2()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.DerivedCollectionOnBase);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
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
            }
        }

        // doesn't work for now
        //[Fact]
        public virtual void Include_collection_with_inheritance_with_filter2()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.DerivedCollectionOnBase).Where(e => e.Name != "Bar");
                var result = query.ToList();

                Assert.Equal(6, result.Count);
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

        // doesn't work for now
        //[Fact]
        public virtual void Include_reference_with_inheritance_on_derived3()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedReferenceOnBase);
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

        // doesn't work for now
        //[Fact]
        public virtual void Include_reference_with_inheritance_on_derived_with_filter3()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedReferenceOnBase).Where(e => e.Name != "Bar");
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

        // doesn't work for now
        //[Fact]
        public virtual void Include_collection_with_inheritance_on_derived3()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedCollectionOnBase);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        // doesn't work for now
        //[Fact]
        public virtual void Include_collection_with_inheritance_on_derived4()
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

        // doesn't work for now
        //[Fact]
        public virtual void Nested_include_with_inheritance_reference_reference2()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.DerivedReferenceOnBase.NestedReference);
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

        // doesn't work for now
        //[Fact]
        public virtual void Nested_include_with_inheritance_reference_reference4()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedReferenceOnBase.NestedReference);
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

        // doesn't work for now
        //[Fact]
        public virtual void Nested_include_with_inheritance_reference_collection2()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.DerivedReferenceOnBase.NestedCollection);
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

        // doesn't work for now
        //[Fact]
        public virtual void Nested_include_with_inheritance_reference_collection4()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedReferenceOnBase.NestedCollection);
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
            }
        }

        // doesn't work for now
        //[Fact]
        public virtual void Nested_include_with_inheritance_collection_reference2()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.DerivedCollectionOnBase).ThenInclude(e => e.NestedReference);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        // doesn't work for now
        //[Fact]
        public virtual void Nested_include_with_inheritance_collection_reference3()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedCollectionOnBase).ThenInclude(e => e.NestedReference);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        // doesn't work for now
        //[Fact]
        public virtual void Nested_include_with_inheritance_collection_reference4()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedCollectionOnBase).ThenInclude(e => e.NestedReference);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
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
            }
        }

        // doesn't work for now
        //[Fact]
        public virtual void Nested_include_with_inheritance_collection_collection2()
        {
            using (var context = CreateContext())
            {
                var query = context.BaseEntities.Include(e => e.DerivedCollectionOnBase).ThenInclude(e => e.NestedCollection);
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        // doesn't work for now
        //[Fact]
        public virtual void Nested_include_with_inheritance_collection_collection3()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedCollectionOnBase).ThenInclude(e => e.NestedCollection);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        // doesn't work for now
        //[Fact]
        public virtual void Nested_include_with_inheritance_collection_collection4()
        {
            using (var context = CreateContext())
            {
                var query = context.DerivedEntities.Include(e => e.DerivedCollectionOnBase).ThenInclude(e => e.NestedCollection);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
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

        protected InheritanceRelationshipsContext CreateContext() => Fixture.CreateContext(TestStore);

        protected InheritanceRelationshipsQueryTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        protected virtual void ClearLog()
        {
        }

        public void Dispose() => TestStore.Dispose();
    }
}
