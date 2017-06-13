// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class EntityTypeExtensionsTest
    {
        [Fact]
        public void Can_get_all_properties_and_navigations()
        {
            var entityType = new Model().AddEntityType(nameof(SelfRef));
            var pk = entityType.GetOrSetPrimaryKey(entityType.AddProperty(nameof(SelfRef.Id), typeof(int)));
            var fkProp = entityType.AddProperty(nameof(SelfRef.SelfRefId), typeof(int?));

            var fk = entityType.AddForeignKey(new[] { fkProp }, pk, entityType);
            fk.IsUnique = true;
            var dependentToPrincipal = fk.HasDependentToPrincipal(nameof(SelfRef.SelfRefPrincipal));
            var principalToDependent = fk.HasPrincipalToDependent(nameof(SelfRef.SelfRefDependent));

            Assert.Equal(
                new IPropertyBase[] { pk.Properties.Single(), fkProp, principalToDependent, dependentToPrincipal },
                entityType.GetPropertiesAndNavigations().ToArray());
        }

        [Fact]
        public void Can_get_referencing_foreign_keys()
        {
            var model = new Model();
            var entityType = model.AddEntityType("Customer");
            var idProperty = entityType.AddProperty("id", typeof(int));
            var fkProperty = entityType.AddProperty("fk", typeof(int));
            var fk = entityType.AddForeignKey(fkProperty, entityType.SetPrimaryKey(idProperty), entityType);

            Assert.Same(fk, entityType.GetReferencingForeignKeys().Single());
        }

        [Fact]
        public void Can_get_root_type()
        {
            var model = new Model();
            var a = model.AddEntityType("A");
            var b = model.AddEntityType("B");
            var c = model.AddEntityType("C");
            b.HasBaseType(a);
            c.HasBaseType(b);

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
            b.HasBaseType(a);
            c.HasBaseType(b);
            d.HasBaseType(a);

            Assert.Equal(new[] { b, d, c }, a.GetDerivedTypes().ToArray());
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
            b.HasBaseType(a);
            c.HasBaseType(b);
            d.HasBaseType(a);

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
            var entityType = new Model().AddEntityType(typeof(A<int>));

            Assert.Equal(
                "A<int>",
                entityType.DisplayName());
        }

        private class A<T>
        {
        }

        private class SelfRef
        {
            public static readonly PropertyInfo IdProperty = typeof(SelfRef).GetProperty("Id");
            public static readonly PropertyInfo SelfRefIdProperty = typeof(SelfRef).GetProperty("SelfRefId");

            public int Id { get; set; }
            public SelfRef SelfRefPrincipal { get; set; }
            public SelfRef SelfRefDependent { get; set; }
            public int? SelfRefId { get; set; }
        }
    }
}
