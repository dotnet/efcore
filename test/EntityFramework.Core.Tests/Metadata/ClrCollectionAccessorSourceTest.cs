// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Moq;
using Xunit;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable UnusedMember.Local

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class ClrCollectionAccessorSourceTest
    {
        [Fact]
        public void Navigation_is_returned_if_it_implements_IClrCollectionAccessor()
        {
            var accessorMock = new Mock<IClrCollectionAccessor>();
            var navigationMock = accessorMock.As<INavigation>();

            var source = new ClrCollectionAccessorSource(new CollectionTypeFactory());

            Assert.Same(accessorMock.Object, source.GetAccessor(navigationMock.Object));
        }

        [Fact]
        public void Delegate_accessor_is_returned_for_ICollection_navigation()
        {
            AccessorTest("AsICollection", e => e.AsICollection);
        }

        [Fact]
        public void Delegate_accessor_is_returned_for_interface_navigation_derived_from_ICollection()
        {
            AccessorTest("AsIList", e => e.AsIList);
        }

        [Fact]
        public void Delegate_accessor_is_returned_for_concrete_generic_type_navigation()
        {
            AccessorTest("AsList", e => e.AsList);
        }

        [Fact]
        public void Delegate_accessor_is_returned_for_concrete_non_generic_type_navigation()
        {
            AccessorTest("AsMyCollection", e => e.AsMyCollection);
        }

        [Fact]
        public void Delegate_accessor_is_returned_when_no_setter()
        {
            AccessorTest("WithNoSetter", e => e.WithNoSetter);
        }

        [Fact]
        public void Delegate_accessor_is_returned_when_no_public_constructor()
        {
            AccessorTest("AsMyPrivateCollection", e => e.AsMyPrivateCollection);
        }

        [Fact]
        public void Delegate_accessor_is_returned_when_no_internal_constructor()
        {
            AccessorTest("AsMyInternalCollection", e => e.AsMyInternalCollection);
        }

        [Fact]
        public void Delegate_accessor_is_returned_when_no_parameterless_constructor()
        {
            AccessorTest("AsMyUnavailableCollection", e => e.AsMyUnavailableCollection);
        }

        [Fact]
        public void Delegate_accessor_handles_uninitialized_collections()
        {
            AccessorTest("AsICollection", e => e.AsICollection, initializeCollections: false);
        }

        [Fact]
        public void Delegate_accessor_handles_uninitialized_collections_for_interface_navigation_derived_from_ICollection()
        {
            AccessorTest("AsIList", e => e.AsIList, initializeCollections: false);
        }

        [Fact]
        public void Delegate_accessor_handles_uninitialized_collections_for_concrete_generic_type_navigation()
        {
            AccessorTest("AsList", e => e.AsList, initializeCollections: false);
        }

        [Fact]
        public void Delegate_accessor_handles_uninitialized_collections_for_concrete_non_generic_type_navigation()
        {
            AccessorTest("AsMyCollection", e => e.AsMyCollection, initializeCollections: false);
        }

        private static void AccessorTest(
            string navigationName, Func<MyEntity, IEnumerable<MyOtherEntity>> reader, bool initializeCollections = true)
        {
            var accessor = new ClrCollectionAccessorSource(new CollectionTypeFactory()).GetAccessor(CreateNavigation(navigationName));

            var entity = new MyEntity();
            var value = new MyOtherEntity();

            if (initializeCollections)
            {
                entity.InitializeCollections();
            }

            Assert.False(accessor.Contains(entity, value));
            accessor.Remove(entity, value);

            accessor.Add(entity, value);

            Assert.True(accessor.Contains(entity, value));
            Assert.Equal(1, reader(entity).Count());

            accessor.Remove(entity, value);

            Assert.False(accessor.Contains(entity, value));
            Assert.Equal(0, reader(entity).Count());
        }

        [Fact]
        public void Delegate_getter_is_cached_by_type_and_property_name()
        {
            var entityType = new Model().AddEntityType(typeof(MyEntity));
            var otherType = new Model().AddEntityType(typeof(MyOtherEntity));
            var foreignKey = otherType.GetOrAddForeignKey(otherType.GetOrAddProperty("MyEntityId", typeof(int), shadowProperty: true), entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true)));

            var navigation = entityType.AddNavigation("AsICollection", foreignKey, pointsToPrincipal: false);

            var source = new ClrCollectionAccessorSource(new CollectionTypeFactory());

            Assert.Same(source.GetAccessor(navigation), source.GetAccessor(navigation));
        }

        [Fact]
        public void Creating_accessor_for_navigation_without_getter_throws()
        {
            var navigation = CreateNavigation("WithNoGetter");

            Assert.Equal(
                Strings.NavigationNoGetter("WithNoGetter", typeof(MyEntity).FullName),
                Assert.Throws<NotSupportedException>(() => new ClrCollectionAccessorSource(new CollectionTypeFactory()).GetAccessor(navigation)).Message);
        }

        [Fact]
        public void Creating_accessor_for_enumerable_navigation_throws()
        {
            var navigation = CreateNavigation("AsIEnumerable");

            Assert.Equal(
                Strings.NavigationBadType(
                    "AsIEnumerable", typeof(MyEntity).FullName, typeof(IEnumerable<MyOtherEntity>).FullName, typeof(MyOtherEntity).FullName),
                Assert.Throws<NotSupportedException>(() => new ClrCollectionAccessorSource(new CollectionTypeFactory()).GetAccessor(navigation)).Message);
        }

        [Fact]
        public void Creating_accessor_for_array_navigation_throws()
        {
            var navigation = CreateNavigation("AsArray");

            Assert.Equal(
                Strings.NavigationArray("AsArray", typeof(MyEntity).FullName, typeof(MyOtherEntity[]).FullName),
                Assert.Throws<NotSupportedException>(() => new ClrCollectionAccessorSource(new CollectionTypeFactory()).GetAccessor(navigation)).Message);
        }

        [Fact]
        public void Initialization_for_navigation_without_setter_throws()
        {
            var accessor = new ClrCollectionAccessorSource(new CollectionTypeFactory()).GetAccessor(CreateNavigation("WithNoSetter"));

            Assert.Equal(
                Strings.NavigationNoSetter("WithNoSetter", typeof(MyEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => accessor.Add(new MyEntity(), new MyOtherEntity())).Message);
        }

        [Fact]
        public void Initialization_for_navigation_with_private_constructor_throws()
        {
            var accessor = new ClrCollectionAccessorSource(new CollectionTypeFactory()).GetAccessor(CreateNavigation("AsMyPrivateCollection"));

            Assert.Equal(
                Strings.NavigationCannotCreateType("AsMyPrivateCollection", typeof(MyEntity).FullName, typeof(MyPrivateCollection).FullName),
                Assert.Throws<InvalidOperationException>(() => accessor.Add(new MyEntity(), new MyOtherEntity())).Message);
        }

        [Fact]
        public void Initialization_for_navigation_with_internal_constructor_throws()
        {
            var accessor = new ClrCollectionAccessorSource(new CollectionTypeFactory()).GetAccessor(CreateNavigation("AsMyInternalCollection"));

            Assert.Equal(
                Strings.NavigationCannotCreateType("AsMyInternalCollection", typeof(MyEntity).FullName, typeof(MyInternalCollection).FullName),
                Assert.Throws<InvalidOperationException>(() => accessor.Add(new MyEntity(), new MyOtherEntity())).Message);
        }

        [Fact]
        public void Initialization_for_navigation_without_parameterless_constructor_throws()
        {
            var accessor = new ClrCollectionAccessorSource(new CollectionTypeFactory()).GetAccessor(CreateNavigation("AsMyUnavailableCollection"));

            Assert.Equal(
                Strings.NavigationCannotCreateType("AsMyUnavailableCollection", typeof(MyEntity).FullName, typeof(MyUnavailableCollection).FullName),
                Assert.Throws<InvalidOperationException>(() => accessor.Add(new MyEntity(), new MyOtherEntity())).Message);
        }

        private static Navigation CreateNavigation(string navigationName)
        {
            var entityType = new Model().AddEntityType(typeof(MyEntity));
            var otherType = new Model().AddEntityType(typeof(MyOtherEntity));
            var foreignKey = otherType.GetOrAddForeignKey(otherType.GetOrAddProperty("MyEntityId", typeof(int), shadowProperty: true), entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true)));

            return entityType.AddNavigation(navigationName, foreignKey, pointsToPrincipal: false);
        }

        private class MyEntity
        {
            private ICollection<MyOtherEntity> _asICollection;
            private IList<MyOtherEntity> _asIList;
            private List<MyOtherEntity> _asList;
            private MyCollection _myCollection;
            private ICollection<MyOtherEntity> _withNoSetter;
            private ICollection<MyOtherEntity> _withNoGetter;
            private IEnumerable<MyOtherEntity> _enumerable;
            private MyOtherEntity[] _array;
            private MyPrivateCollection _privateCollection;
            private MyInternalCollection _internalCollection;
            private MyUnavailableCollection _unavailableCollection;

            public void InitializeCollections()
            {
                _asICollection = new HashSet<MyOtherEntity>();
                _asIList = new List<MyOtherEntity>();
                _asList = new List<MyOtherEntity>();
                _myCollection = new MyCollection();
                _withNoSetter = new HashSet<MyOtherEntity>();
                _withNoGetter = new HashSet<MyOtherEntity>();
                _enumerable = new HashSet<MyOtherEntity>();
                _array = new MyOtherEntity[0];
                _privateCollection = MyPrivateCollection.Create();
                _internalCollection = new MyInternalCollection();
                _unavailableCollection = new MyUnavailableCollection(true);
            }

            internal ICollection<MyOtherEntity> AsICollection
            {
                get { return _asICollection; }
                set { _asICollection = value; }
            }

            internal IList<MyOtherEntity> AsIList
            {
                get { return _asIList; }
                set { _asIList = value; }
            }

            internal List<MyOtherEntity> AsList
            {
                get { return _asList; }
                set { _asList = value; }
            }

            internal MyCollection AsMyCollection
            {
                get { return _myCollection; }
                set { _myCollection = value; }
            }

            internal ICollection<MyOtherEntity> WithNoSetter
            {
                get { return _withNoSetter; }
            }

            internal ICollection<MyOtherEntity> WithNoGetter
            {
                set { _withNoGetter = value; }
            }

            internal IEnumerable<MyOtherEntity> AsIEnumerable
            {
                get { return _enumerable; }
                set { _enumerable = value; }
            }

            internal MyOtherEntity[] AsArray
            {
                get { return _array; }
                set { _array = value; }
            }

            internal MyPrivateCollection AsMyPrivateCollection
            {
                get { return _privateCollection; }
                set { _privateCollection = value; }
            }

            internal MyInternalCollection AsMyInternalCollection
            {
                get { return _internalCollection; }
                set { _internalCollection = value; }
            }

            internal MyUnavailableCollection AsMyUnavailableCollection
            {
                get { return _unavailableCollection; }
                set { _unavailableCollection = value; }
            }
        }

        private class MyOtherEntity
        {
        }

        private class MyCollection : List<MyOtherEntity>
        {
        }

        private class MyPrivateCollection : List<MyOtherEntity>
        {
            private MyPrivateCollection()
            {
            }

            public static MyPrivateCollection Create()
            {
                return new MyPrivateCollection();
            }
        }

        private class MyInternalCollection : List<MyOtherEntity>
        {
            // ReSharper disable once EmptyConstructor
            internal MyInternalCollection()
            {
            }
        }

        private class MyUnavailableCollection : List<MyOtherEntity>
        {
            public MyUnavailableCollection(bool _)
            {
            }
        }
    }
}
