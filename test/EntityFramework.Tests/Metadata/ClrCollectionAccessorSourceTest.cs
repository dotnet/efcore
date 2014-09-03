// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class ClrCollectionAccessorSourceTest
    {
        [Fact]
        public void Navigation_is_returned_if_it_implements_IClrCollectionAccessor()
        {
            var accessorMock = new Mock<IClrCollectionAccessor>();
            var navigationMock = accessorMock.As<INavigation>();

            var source = new ClrCollectionAccessorSource();

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

        private static void AccessorTest(string navigationName, Func<MyEntity, IEnumerable<MyOtherEntity>> reader)
        {
            var entityType = new EntityType(typeof(MyEntity));
            var otherType = new EntityType(typeof(MyOtherEntity));
            var foreignKey = otherType.GetOrAddForeignKey(
                entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true)),
                otherType.GetOrAddProperty("MyEntityId", typeof(int), shadowProperty: true));

            var navigation = entityType.AddNavigation(new Navigation(foreignKey, navigationName, pointsToPrincipal: false));

            var accessor = new ClrCollectionAccessorSource().GetAccessor(navigation);

            var entity = new MyEntity();
            var value = new MyOtherEntity();

            Assert.False(accessor.Contains(entity, value));
            Assert.DoesNotThrow(() => accessor.Remove(entity, value));

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
            var entityType = new EntityType(typeof(MyEntity));
            var otherType = new EntityType(typeof(MyOtherEntity));
            var foreignKey = otherType.GetOrAddForeignKey(
                entityType.GetOrSetPrimaryKey(entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true)),
                otherType.GetOrAddProperty("MyEntityId", typeof(int), shadowProperty: true));

            var navigation = entityType.AddNavigation(new Navigation(foreignKey, "AsICollection", pointsToPrincipal: false));

            var source = new ClrCollectionAccessorSource();

            Assert.Same(source.GetAccessor(navigation), source.GetAccessor(navigation));
        }

        #region Fixture

        private class MyEntity
        {
            private readonly ICollection<MyOtherEntity> _asICollection = new HashSet<MyOtherEntity>();
            private readonly IList<MyOtherEntity> _asIList = new List<MyOtherEntity>();
            private readonly List<MyOtherEntity> _asList = new List<MyOtherEntity>();
            private readonly MyCollection _myCollection = new MyCollection();

            internal ICollection<MyOtherEntity> AsICollection
            {
                get { return _asICollection; }
            }

            internal IList<MyOtherEntity> AsIList
            {
                get { return _asIList; }
            }

            internal List<MyOtherEntity> AsList
            {
                get { return _asList; }
            }

            internal List<MyOtherEntity> AsMyCollection
            {
                get { return _myCollection; }
            }
        }

        private class MyOtherEntity
        {
        }

        private class MyCollection : List<MyOtherEntity>
        {
        }

        #endregion
    }
}
