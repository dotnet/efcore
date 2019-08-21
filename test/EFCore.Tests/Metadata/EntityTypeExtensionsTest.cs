// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class EntityTypeExtensionsTest
    {
        [ConditionalFact]
        public void Can_get_all_properties_and_navigations()
        {
            var entityType = CreateModel().AddEntityType(nameof(SelfRef));
            var pk = entityType.SetPrimaryKey(entityType.AddProperty(nameof(SelfRef.Id), typeof(int)));
            var fkProp = entityType.AddProperty(nameof(SelfRef.SelfRefId), typeof(int?));

            var fk = entityType.AddForeignKey(new[] { fkProp }, pk, entityType);
            fk.IsUnique = true;
            var dependentToPrincipal = fk.HasDependentToPrincipal(nameof(SelfRef.SelfRefPrincipal));
            var principalToDependent = fk.HasPrincipalToDependent(nameof(SelfRef.SelfRefDependent));

            Assert.Equal(
                new IPropertyBase[] { pk.Properties.Single(), fkProp, principalToDependent, dependentToPrincipal },
                entityType.GetPropertiesAndNavigations().ToArray());
        }

        [ConditionalFact]
        public void Can_get_referencing_foreign_keys()
        {
            var entityType = CreateModel().AddEntityType("Customer");
            var idProperty = entityType.AddProperty("id", typeof(int));
            var fkProperty = entityType.AddProperty("fk", typeof(int));
            var fk = entityType.AddForeignKey(fkProperty, entityType.SetPrimaryKey(idProperty), entityType);

            Assert.Same(fk, entityType.GetReferencingForeignKeys().Single());
        }

        [ConditionalFact]
        public void Can_get_root_type()
        {
            var model = CreateModel();
            var a = model.AddEntityType("A");
            var b = model.AddEntityType("B");
            var c = model.AddEntityType("C");
            b.BaseType = a;
            c.BaseType = b;

            Assert.Same(a, a.GetRootType());
            Assert.Same(a, b.GetRootType());
            Assert.Same(a, c.GetRootType());
        }

        [ConditionalFact]
        public void Can_get_derived_types()
        {
            var model = CreateModel();
            var a = model.AddEntityType("A");
            var b = model.AddEntityType("B");
            var c = model.AddEntityType("C");
            var d = model.AddEntityType("D");
            b.BaseType = a;
            c.BaseType = b;
            d.BaseType = a;

            Assert.Equal(new[] { b, d, c }, a.GetDerivedTypes().ToArray());
            Assert.Equal(new[] { c }, b.GetDerivedTypes().ToArray());
            Assert.Equal(new[] { b, d }, a.GetDirectlyDerivedTypes().ToArray());
        }

        [ConditionalFact]
        public void Can_determine_whether_IsAssignableFrom()
        {
            var model = CreateModel();
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

        [ConditionalFact]
        public void Can_get_proper_table_name_for_generic_entityType()
        {
            var entityType = CreateModel().AddEntityType(typeof(A<int>));

            Assert.Equal(
                "A<int>",
                entityType.DisplayName());
        }

        [ConditionalFact]
        public void Setting_discriminator_on_non_root_type_throws()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;
            var property = entityType.AddProperty("D", typeof(string));

            var derivedType = modelBuilder
                .Entity<SpecialCustomer>()
                .Metadata;
            derivedType.BaseType = entityType;

            Assert.Equal(
                CoreStrings.DiscriminatorPropertyMustBeOnRoot(nameof(SpecialCustomer)),
                Assert.Throws<InvalidOperationException>(() => derivedType.SetDiscriminatorProperty(property)).Message);
        }

        [ConditionalFact]
        public void Setting_discriminator_from_different_entity_type_throws()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            var otherType = modelBuilder
                .Entity<SpecialCustomer>()
                .Metadata;

            var property = entityType.AddProperty("D", typeof(string));

            Assert.Equal(
                CoreStrings.DiscriminatorPropertyNotFound("D", nameof(SpecialCustomer)),
                Assert.Throws<InvalidOperationException>(() => otherType.SetDiscriminatorProperty(property)).Message);
        }

        [ConditionalFact]
        public void Can_get_and_set_discriminator_value()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            var property = entityType.AddProperty("D", typeof(string));
            entityType.SetDiscriminatorProperty(property);

            Assert.Null(entityType.GetDiscriminatorValue());

            entityType.SetDiscriminatorValue("V");

            Assert.Equal("V", entityType.GetDiscriminatorValue());

            entityType.SetDiscriminatorValue(null);

            Assert.Null(entityType.GetDiscriminatorValue());
        }

        [ConditionalFact]
        public void Setting_discriminator_value_when_discriminator_not_set_throws()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal(
                CoreStrings.NoDiscriminatorForValue("Customer", "Customer"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.SetDiscriminatorValue("V")).Message);
        }

        [ConditionalFact]
        public void Setting_incompatible_discriminator_value_throws()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            var property = entityType.AddProperty("D", typeof(int));
            entityType.SetDiscriminatorProperty(property);

            Assert.Equal(
                CoreStrings.DiscriminatorValueIncompatible("V", "D", typeof(int)),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.SetDiscriminatorValue("V")).Message);

            entityType.SetDiscriminatorValue(null);
        }

        private static IMutableModel CreateModel() => new Model();

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

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Guid AlternateId { get; set; }
        }

        private class SpecialCustomer : Customer
        {
        }
    }
}
