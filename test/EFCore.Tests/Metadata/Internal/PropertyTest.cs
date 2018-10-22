// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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
        [Fact]
        public void Use_of_custom_IProperty_throws()
        {
            var property = new FakeProperty();

            Assert.Equal(
                CoreStrings.CustomMetadata(nameof(Use_of_custom_IProperty_throws), nameof(IProperty), nameof(FakeProperty)),
                Assert.Throws<NotSupportedException>(() => property.AsProperty()).Message);
        }

        [Fact]
        public void Use_of_custom_IPropertyBase_throws()
        {
            var property = new FakeProperty();

            Assert.Equal(
                CoreStrings.CustomMetadata(nameof(Use_of_custom_IPropertyBase_throws), nameof(IPropertyBase), nameof(FakeProperty)),
                Assert.Throws<NotSupportedException>(() => property.AsPropertyBase()).Message);
        }

        private class FakeProperty : IProperty
        {
            public object this[string name] => throw new NotImplementedException();
            public IAnnotation FindAnnotation(string name) => throw new NotImplementedException();
            public IEnumerable<IAnnotation> GetAnnotations() => throw new NotImplementedException();
            public string Name { get; }
            public ITypeBase DeclaringType { get; }
            public Type ClrType { get; }
            public bool IsShadowProperty { get; }
            public bool IsIndexedProperty { get; }
            public IEntityType DeclaringEntityType { get; }
            public bool IsNullable { get; }
            public PropertySaveBehavior BeforeSaveBehavior { get; }
            public PropertySaveBehavior AfterSaveBehavior { get; }
            public bool IsReadOnlyBeforeSave { get; }
            public bool IsReadOnlyAfterSave { get; }
            public bool IsStoreGeneratedAlways { get; }
            public ValueGenerated ValueGenerated { get; }
            public bool IsConcurrencyToken { get; }
            public PropertyInfo PropertyInfo { get; }
            public FieldInfo FieldInfo { get; }
        }

        [Fact]
        public void Can_set_ClrType()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake", typeof(string));

            Assert.Equal(typeof(string), property.ClrType);
        }

        [Fact]
        public void Default_nullability_of_property_is_based_on_nullability_of_CLR_type()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("stringName", typeof(string));
            var nullableIntProperty = entityType.AddProperty("nullableIntName", typeof(int?));
            var intProperty = entityType.AddProperty("intName", typeof(int));

            Assert.True(stringProperty.IsNullable);
            Assert.True(nullableIntProperty.IsNullable);
            Assert.False(intProperty.IsNullable);
        }

        [Fact]
        public void Property_nullability_can_be_mutated()
        {
            var entityType = new Model().AddEntityType(typeof(object));
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

        [Fact]
        public void Adding_a_nullable_property_to_a_key_throws()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("Name", typeof(string));

            stringProperty.IsNullable = true;
            Assert.True(stringProperty.IsNullable);

            Assert.Equal(
                CoreStrings.NullableKey(typeof(object).DisplayName(), stringProperty.Name),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        stringProperty.DeclaringEntityType.AddKey(stringProperty)).Message);
        }

        [Fact]
        public void Properties_with_non_nullable_types_cannot_be_made_nullable()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var intProperty = entityType.AddProperty("Name", typeof(int));

            Assert.Equal(
                CoreStrings.CannotBeNullable("Name", "object", "int"),
                Assert.Throws<InvalidOperationException>(() => intProperty.IsNullable = true).Message);
        }

        [Fact]
        public void Properties_which_are_part_of_primary_key_cannot_be_made_nullable()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("Name", typeof(string));
            stringProperty.IsNullable = false;
            stringProperty.DeclaringEntityType.SetPrimaryKey(stringProperty);

            Assert.Equal(
                CoreStrings.CannotBeNullablePK("Name", "object"),
                Assert.Throws<InvalidOperationException>(() => stringProperty.IsNullable = true).Message);
        }

        [Fact]
        public void UnderlyingType_returns_correct_underlying_type()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property1 = entityType.AddProperty("Id", typeof(int?));
            Assert.Equal(typeof(int), property1.ClrType.UnwrapNullableType());
        }

        [Fact]
        public void IsShadowProperty_is_set()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty(nameof(Entity.Name), typeof(string));

            Assert.False(property.IsShadowProperty);
        }

        [Fact]
        public void Property_does_not_use_ValueGenerated_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Can_mark_property_as_using_ValueGenerated()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);

            property.ValueGenerated = ValueGenerated.Never;
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Property_is_not_concurrency_token_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

            Assert.False(property.IsConcurrencyToken);
        }

        [Fact]
        public void Can_mark_property_as_concurrency_token()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

            property.IsConcurrencyToken = true;
            Assert.True(property.IsConcurrencyToken);

            property.IsConcurrencyToken = false;
            Assert.False(property.IsConcurrencyToken);
        }

        [Fact]
        public void Can_mark_property_to_always_use_store_generated_values()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

#pragma warning disable 618
            Assert.False(property.IsStoreGeneratedAlways);

            property.IsStoreGeneratedAlways = true;
            Assert.True(property.IsStoreGeneratedAlways);

            property.IsStoreGeneratedAlways = false;
            Assert.False(property.IsStoreGeneratedAlways);
#pragma warning restore 618
        }

        [Fact]
        public void Store_generated_concurrency_tokens_always_use_store_values_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

#pragma warning disable 618
            Assert.False(((IProperty)property).IsStoreGeneratedAlways);

            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            Assert.True(((IProperty)property).IsStoreGeneratedAlways);

            property.IsConcurrencyToken = true;
            Assert.True(((IProperty)property).IsStoreGeneratedAlways);

            property.ValueGenerated = ValueGenerated.OnAdd;
            Assert.False(((IProperty)property).IsStoreGeneratedAlways);

            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            Assert.True(((IProperty)property).IsStoreGeneratedAlways);

            property.IsStoreGeneratedAlways = false;
            Assert.False(((IProperty)property).IsStoreGeneratedAlways);
#pragma warning restore 618
        }

        [Fact]
        public void Property_is_read_write_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

#pragma warning disable 618
            Assert.False(property.IsReadOnlyAfterSave);
            Assert.False(property.IsReadOnlyBeforeSave);
#pragma warning restore 618
        }

        [Fact]
        public void Property_can_be_marked_as_read_only_before_save()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));
#pragma warning disable 618
            property.IsReadOnlyBeforeSave = true;

            Assert.True(property.IsReadOnlyBeforeSave);
#pragma warning restore 618
        }

        [Fact]
        public void Property_can_be_marked_as_read_only_after_save()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));
#pragma warning disable 618
            property.IsReadOnlyAfterSave = true;

            Assert.True(property.IsReadOnlyAfterSave);
#pragma warning restore 618
        }

        [Fact]
        public void Property_can_be_marked_as_read_only_always()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));

#pragma warning disable 618
            Assert.False(property.IsReadOnlyBeforeSave);
            Assert.False(property.IsReadOnlyAfterSave);

            property.IsReadOnlyBeforeSave = true;
            property.IsReadOnlyAfterSave = true;

            Assert.True(property.IsReadOnlyBeforeSave);
            Assert.True(property.IsReadOnlyAfterSave);
#pragma warning restore 618
        }

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
