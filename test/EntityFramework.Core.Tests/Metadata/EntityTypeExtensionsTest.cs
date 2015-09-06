// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Tests
{
    public class EntityTypeExtensionsTest
    {
        [Fact]
        public void Can_get_all_properties_and_navigations()
        {
            var typeMock = new Mock<IEntityType>();

            var property1 = Mock.Of<IProperty>();
            var property2 = Mock.Of<IProperty>();
            var navigation1 = Mock.Of<INavigation>();
            var navigation2 = Mock.Of<INavigation>();

            typeMock.Setup(m => m.GetProperties()).Returns(new List<IProperty> { property1, property2 });
            typeMock.Setup(m => m.GetNavigations()).Returns(new List<INavigation> { navigation1, navigation2 });

            Assert.Equal(
                new IPropertyBase[] { property1, property2, navigation1, navigation2 },
                typeMock.Object.GetPropertiesAndNavigations().ToArray());
        }

        [Fact]
        public void Can_get_referencing_foreign_keys()
        {
            var model = new Model();
            var entityType = model.AddEntityType("Customer");
            var idProperty = entityType.AddProperty("id", typeof(int));
            var fkProperty = entityType.AddProperty("fk", typeof(int));
            var fk = entityType.AddForeignKey(fkProperty, entityType.SetPrimaryKey(idProperty), entityType);

            Assert.Same(fk, entityType.FindReferencingForeignKeys().Single());
        }

        [Fact]
        public void Can_get_root_type()
        {
            var model = new Model();
            var a = model.AddEntityType("A");
            var b = model.AddEntityType("B");
            var c = model.AddEntityType("C");
            b.BaseType = a;
            c.BaseType = b;

            Assert.Same(a, a.RootType());
            Assert.Same(a, b.RootType());
            Assert.Same(a, c.RootType());
        }

        [Fact]
        public void Can_get_derived_types()
        {
            var model = new Model();
            var a = model.AddEntityType("A");
            var b = model.AddEntityType("B");
            var c = model.AddEntityType("C");
            var d = model.AddEntityType("D");
            b.BaseType = a;
            c.BaseType = b;
            d.BaseType = a;

            Assert.Equal(new[] { b, c, d }, a.GetDerivedTypes().ToArray());
            Assert.Equal(new[] { c }, b.GetDerivedTypes().ToArray());
            Assert.Equal(new[] { b, d }, a.GetDirectlyDerivedTypes().ToArray());
        }

        [Fact]
        public void Can_determine_whether_IsAssignableFrom()
        {
            var model = new Model();
            var a = model.AddEntityType("A");
            var b = model.AddEntityType("B");
            var c = model.AddEntityType("C");
            var d = model.AddEntityType("D");
            b.BaseType = a;
            c.BaseType = b;
            d.BaseType = a;

            Assert.True(a.IsAssignableFrom(a));
            Assert.True(a.IsAssignableFrom(b));
            Assert.True(a.IsAssignableFrom(c));
            Assert.False(b.IsAssignableFrom(a));
            Assert.False(c.IsAssignableFrom(a));
            Assert.False(b.IsAssignableFrom(d));
        }

        [Fact]
        public void Can_get_proper_table_name_for_generic_entityType()
        {
            var entityType = new EntityType(typeof(A<int>), new Model());

            Assert.Equal(
                "A<int>",
                ((IEntityType)entityType).DisplayName());

        }

        private class A<T>
        {

        }
    }
}
