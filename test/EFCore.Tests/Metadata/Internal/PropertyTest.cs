// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class PropertyTest
    {
        [ConditionalFact]
        public void Throws_when_model_is_readonly()
        {
            var model = CreateModel();

            var entityType = model.AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake", typeof(string));

            model.FinalizeModel();

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => entityType.AddProperty("Kuke", typeof(string))).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.IsNullable = false).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.IsConcurrencyToken = false).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.ValueGenerated = ValueGenerated.OnAddOrUpdate).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetAfterSaveBehavior(PropertySaveBehavior.Throw)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetField(null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetIsUnicode(null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetMaxLength(null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetPrecision(null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetScale(null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetPropertyAccessMode(null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetProviderClrType(null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetValueComparer((ValueComparer)null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetValueComparer((Type)null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetValueConverter((ValueConverter)null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetValueConverter((Type)null)).Message);

            Assert.Equal(
                CoreStrings.ModelReadOnly,
                Assert.Throws<InvalidOperationException>(() => property.SetValueGeneratorFactory((Type)null)).Message);
        }

        [ConditionalFact]
        public void Can_set_ClrType()
        {
            var entityType = CreateModel().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake", typeof(string));

            Assert.Equal(typeof(string), property.ClrType);
        }

        [ConditionalFact]
        public void Default_nullability_of_property_is_based_on_nullability_of_CLR_type()
        {
            var entityType = CreateModel().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("stringName", typeof(string));
            var nullableIntProperty = entityType.AddProperty("nullableIntName", typeof(int?));
            var intProperty = entityType.AddProperty("intName", typeof(int));

            Assert.True(stringProperty.IsNullable);
            Assert.True(nullableIntProperty.IsNullable);
            Assert.False(intProperty.IsNullable);
        }

        [ConditionalFact]
        public void Property_nullability_can_be_mutated()
        {
            var entityType = CreateModel().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("Name", typeof(string));
            var intProperty = entityType.AddProperty("Id", typeof(int));

            stringProperty.IsNullable = false;
            Assert.False(stringProperty.IsNullable);
            Assert.False(intProperty.IsNullable);

            stringProperty.IsNullable = true;
            intProperty.IsNullable = false;
            Assert.True(stringProperty.IsNullable);
            Assert.False(intProperty.IsNullable);
        }

        [ConditionalFact]
        public void Adding_a_nullable_property_to_a_key_throws()
        {
            var entityType = CreateModel().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("Name", typeof(string));

            stringProperty.IsNullable = true;
            Assert.True(stringProperty.IsNullable);

            Assert.Equal(
                CoreStrings.NullableKey(typeof(object).DisplayName(), stringProperty.Name),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        stringProperty.DeclaringEntityType.AddKey(stringProperty)).Message);
        }

        [ConditionalFact]
        public void Properties_with_non_nullable_types_cannot_be_made_nullable()
        {
            var entityType = CreateModel().AddEntityType(typeof(object));
            var intProperty = entityType.AddProperty("Name", typeof(int));

            Assert.Equal(
                CoreStrings.CannotBeNullable("Name", "object", "int"),
                Assert.Throws<InvalidOperationException>(() => intProperty.IsNullable = true).Message);
        }

        [ConditionalFact]
        public void Properties_which_are_part_of_primary_key_cannot_be_made_nullable()
        {
            var entityType = CreateModel().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("Name", typeof(string));
            stringProperty.IsNullable = false;
            stringProperty.DeclaringEntityType.SetPrimaryKey(stringProperty);

            Assert.Equal(
                CoreStrings.CannotBeNullablePK("Name", "object"),
                Assert.Throws<InvalidOperationException>(() => stringProperty.IsNullable = true).Message);
        }

        [ConditionalFact]
        public void UnderlyingType_returns_correct_underlying_type()
        {
            var entityType = CreateModel().AddEntityType(typeof(Entity));
            var property1 = entityType.AddProperty("Id", typeof(int?));
            Assert.Equal(typeof(int), property1.ClrType.UnwrapNullableType());
        }

        [ConditionalFact]
        public void IsShadowProperty_is_set()
        {
            var entityType = CreateModel().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty(nameof(Entity.Name), typeof(string));

            Assert.False(property.IsShadowProperty());
        }

        [ConditionalFact]
        public void Property_does_not_use_ValueGenerated_by_default()
        {
            var entityType = CreateModel().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Can_mark_property_as_using_ValueGenerated()
        {
            var entityType = CreateModel().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);

            property.ValueGenerated = ValueGenerated.Never;
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Property_is_not_concurrency_token_by_default()
        {
            var entityType = CreateModel().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

            Assert.False(property.IsConcurrencyToken);
        }

        [ConditionalFact]
        public void Can_mark_property_as_concurrency_token()
        {
            var entityType = CreateModel().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

            property.IsConcurrencyToken = true;
            Assert.True(property.IsConcurrencyToken);

            property.IsConcurrencyToken = false;
            Assert.False(property.IsConcurrencyToken);
        }

        [ConditionalFact]
        public void Throws_when_ValueGeneratorFactory_is_invalid()
        {
            var model = CreateModel();

            var entityType = model.AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake", typeof(string));

            Assert.Equal(
                CoreStrings.BadValueGeneratorType(nameof(NonDerivedValueGeneratorFactory), nameof(ValueGeneratorFactory)),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueGeneratorFactory(typeof(NonDerivedValueGeneratorFactory))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueGenerator(nameof(AbstractValueGeneratorFactory), "SetValueGeneratorFactory"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueGeneratorFactory(typeof(AbstractValueGeneratorFactory))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueGenerator(nameof(StaticValueGeneratorFactory), "SetValueGeneratorFactory"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueGeneratorFactory(typeof(StaticValueGeneratorFactory))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueGenerator(nameof(PrivateValueGeneratorFactory), "SetValueGeneratorFactory"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueGeneratorFactory(typeof(PrivateValueGeneratorFactory))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueGenerator(nameof(NonParameterlessValueGeneratorFactory), "SetValueGeneratorFactory"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueGeneratorFactory(typeof(NonParameterlessValueGeneratorFactory))).Message);
        }

        private class NonDerivedValueGeneratorFactory
        {
            public ValueGenerator Create(IProperty property, IEntityType entityType)
                => null;
        }

        private abstract class AbstractValueGeneratorFactory : ValueGeneratorFactory
        {
            public override ValueGenerator Create(IProperty property, IEntityType entityType)
                => null;
        }

        private class StaticValueGeneratorFactory : ValueGeneratorFactory
        {
            static StaticValueGeneratorFactory()
            {
            }

            private StaticValueGeneratorFactory()
            {
            }

            public override ValueGenerator Create(IProperty property, IEntityType entityType)
                => null;
        }

        private class PrivateValueGeneratorFactory : ValueGeneratorFactory
        {
            private PrivateValueGeneratorFactory()
            {
            }

            public override ValueGenerator Create(IProperty property, IEntityType entityType)
                => null;
        }

        private class NonParameterlessValueGeneratorFactory : ValueGeneratorFactory
        {
            public NonParameterlessValueGeneratorFactory(object _)
            {
            }

            public override ValueGenerator Create(IProperty property, IEntityType entityType)
                => null;
        }

        [ConditionalFact]
        public void Throws_when_ValueConverter_type_is_invalid()
        {
            var model = CreateModel();

            var entityType = model.AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake", typeof(string));

            Assert.Equal(
                CoreStrings.BadValueConverterType(nameof(NonDerivedValueConverter), nameof(ValueConverter)),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueConverter(typeof(NonDerivedValueConverter))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueConverter(nameof(AbstractValueConverter), "HasConversion"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueConverter(typeof(AbstractValueConverter))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueConverter(nameof(StaticValueConverter), "HasConversion"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueConverter(typeof(StaticValueConverter))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueConverter(nameof(PrivateValueConverter), "HasConversion"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueConverter(typeof(PrivateValueConverter))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueConverter(nameof(NonParameterlessValueConverter), "HasConversion"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueConverter(typeof(NonParameterlessValueConverter))).Message);
        }

        private class NonDerivedValueConverter
        {
        }

        private abstract class AbstractValueConverter : StringToBoolConverter
        {
        }

        private class StaticValueConverter : StringToBoolConverter
        {
            static StaticValueConverter()
            {
            }

            private StaticValueConverter()
            {
            }
        }

        private class PrivateValueConverter : StringToBoolConverter
        {
            private PrivateValueConverter()
            {
            }
        }

        private class NonParameterlessValueConverter : StringToBoolConverter
        {
            public NonParameterlessValueConverter(ConverterMappingHints mappingHints = null)
                : base(mappingHints)
            {
            }
        }

        [ConditionalFact]
        public void Throws_when_ValueComparer_type_is_invalid()
        {
            var model = CreateModel();

            var entityType = model.AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake", typeof(string));

            Assert.Equal(
                CoreStrings.BadValueComparerType(nameof(NonDerivedValueComparer), nameof(ValueComparer)),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueComparer(typeof(NonDerivedValueComparer))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueComparer(nameof(AbstractValueComparer), "HasConversion"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueComparer(typeof(AbstractValueComparer))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueComparer(nameof(StaticValueComparer), "HasConversion"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueComparer(typeof(StaticValueComparer))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueComparer(nameof(PrivateValueComparer), "HasConversion"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueComparer(typeof(PrivateValueComparer))).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueComparer(nameof(NonParameterlessValueComparer), "HasConversion"),
                Assert.Throws<InvalidOperationException>(() =>
                    property.SetValueComparer(typeof(NonParameterlessValueComparer))).Message);
        }

        private class NonDerivedValueComparer
        {
        }

        private abstract class AbstractValueComparer : ValueComparer<string>
        {
            public AbstractValueComparer()
                : base(false)
            {
            }
        }

        private class StaticValueComparer : ValueComparer<string>
        {
            static StaticValueComparer()
            {
            }

            private StaticValueComparer()
                : base(false)
            {
            }
        }

        private class PrivateValueComparer : ValueComparer<string>
        {
            private PrivateValueComparer()
                : base(false)
            {
            }
        }

        private class NonParameterlessValueComparer : ValueComparer<string>
        {
            public NonParameterlessValueComparer(bool favorStructuralComparison)
                : base(favorStructuralComparison)
            {
            }
        }

        private static IMutableModel CreateModel()
            => new Model();

        private class Entity
        {
            public string Name { get; set; }
            public int? Id { get; set; }
        }

        private class BaseType
        {
            public int Id { get; set; }
        }

        private class Customer : BaseType
        {
            public int AlternateId { get; set; }
            public Guid Unique { get; set; }
            public string Name { get; set; }
            public string Mane { get; set; }
            public ICollection<Order> Orders { get; set; }

            public IEnumerable<Order> EnumerableOrders { get; set; }
            public Order NotCollectionOrders { get; set; }
        }

        private class Order : BaseType
        {
            public int CustomerId { get; set; }
            public Guid CustomerUnique { get; set; }
            public Customer Customer { get; set; }

            public Order OrderCustomer { get; set; }
        }
    }
}
