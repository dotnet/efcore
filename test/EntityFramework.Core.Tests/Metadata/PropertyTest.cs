// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class PropertyTest
    {
        [Fact]
        public void Can_set_ClrType()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake");

            Assert.Null(property.ClrType);
            Assert.Equal(typeof(string), ((IProperty)property).ClrType);

            property.ClrType = typeof(int);
            Assert.Equal(typeof(int), property.ClrType);
        }

        [Fact]
        public void Setting_ClrType_throws_when_referenced()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var principalProperty = entityType.AddProperty("Kake");
            var key = entityType.AddKey(principalProperty);
            var dependentProperty = entityType.AddProperty("Alaska");
            entityType.AddForeignKey(dependentProperty, key, entityType);

            Assert.Equal(Strings.PropertyClrTypeCannotBeChangedWhenReferenced("Kake", "{'Alaska'}", "object"),
                Assert.Throws<InvalidOperationException>(() =>
                    principalProperty.ClrType = typeof(int)).Message);
            Assert.Equal(typeof(string), ((IProperty)principalProperty).ClrType);
        }

        [Fact]
        public void Default_nullability_of_property_is_based_on_nullability_of_CLR_type_and_property_being_part_of_primary_key()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("stringName", typeof(string));
            var nullableIntProperty = entityType.AddProperty("nullableIntName", typeof(int?));
            var intProperty = entityType.AddProperty("intName", typeof(int));

            Assert.Null(stringProperty.IsNullable);
            Assert.True(((IProperty)stringProperty).IsNullable);
            Assert.Null(stringProperty.IsNullable);
            Assert.True(((IProperty)nullableIntProperty).IsNullable);
            Assert.Null(intProperty.IsNullable);
            Assert.False(((IProperty)intProperty).IsNullable);

            entityType.SetPrimaryKey(stringProperty);

            Assert.Null(stringProperty.IsNullable);
            Assert.False(((IProperty)stringProperty).IsNullable);
        }

        [Fact]
        public void Property_nullability_can_be_mutated()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("Name", typeof(string));
            var intProperty = entityType.AddProperty("Id", typeof(int));

            stringProperty.IsNullable = false;
            Assert.False(stringProperty.IsNullable.Value);
            Assert.Null(intProperty.IsNullable);

            stringProperty.IsNullable = true;
            intProperty.IsNullable = false;
            Assert.True(stringProperty.IsNullable.Value);
            Assert.False(intProperty.IsNullable.Value);

            stringProperty.IsNullable = null;
            intProperty.IsNullable = null;
            Assert.Null(stringProperty.IsNullable);
            Assert.Null(intProperty.IsNullable);
        }

        [Fact]
        public void Property_nullability_is_changed_if_property_made_part_of_primary_key()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("Name", typeof(string));

            stringProperty.IsNullable = true;
            Assert.True(stringProperty.IsNullable.Value);

            stringProperty.DeclaringEntityType.SetPrimaryKey(stringProperty);

            Assert.Null(stringProperty.IsNullable);
            Assert.False(((IProperty)stringProperty).IsNullable);
        }

        [Fact]
        public void Properties_with_non_nullable_types_cannot_be_made_nullable()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var intProperty = entityType.AddProperty("Name", typeof(int));
            intProperty.IsShadowProperty = false;

            Assert.Equal(
                Strings.CannotBeNullable("Name", "object", "Int32"),
                Assert.Throws<InvalidOperationException>(() => intProperty.IsNullable = true).Message);
        }

        [Fact]
        public void Properties_which_are_part_of_primary_key_cannot_be_made_nullable()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var stringProperty = entityType.AddProperty("Name", typeof(string));
            stringProperty.DeclaringEntityType.SetPrimaryKey(stringProperty);

            Assert.Equal(
                Strings.CannotBeNullablePK("Name", "object"),
                Assert.Throws<InvalidOperationException>(() => stringProperty.IsNullable = true).Message);
        }

        [Fact]
        public void UnderlyingType_returns_correct_underlying_type()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property1 = entityType.AddProperty("Name", typeof(int?));
            property1.IsShadowProperty = false;
            Assert.Equal(typeof(int), property1.ClrType.UnwrapNullableType());
            var property = entityType.AddProperty("Name2", typeof(int));
            property.IsShadowProperty = false;
            Assert.Equal(typeof(int), property.ClrType.UnwrapNullableType());
        }

        [Fact]
        public void IsShadowProperty_is_set_appropriately()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake");

            Assert.Null(property.IsShadowProperty);
            Assert.True(((IProperty)property).IsShadowProperty);

            property.ClrType = typeof(int);
            property.IsShadowProperty = false;
            Assert.False(property.IsShadowProperty);

            property.IsShadowProperty = true;
            Assert.True(property.IsShadowProperty);
        }

        [Fact]
        public void Property_does_not_use_ValueGenerated_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            Assert.Null(property.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, ((IProperty)property).ValueGenerated);
        }

        [Fact]
        public void Can_mark_property_as_using_ValueGenerated()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated.Value);

            property.ValueGenerated = ValueGenerated.Never;
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated.Value);

            property.ValueGenerated = null;
            Assert.Null(property.ValueGenerated);
        }

        [Fact]
        public void Property_is_not_concurrency_token_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            Assert.Null(property.IsConcurrencyToken);
            Assert.False(((IProperty)property).IsConcurrencyToken);
        }

        [Fact]
        public void Can_mark_property_as_concurrency_token()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name");
            property.ClrType = typeof(string);
            property.IsShadowProperty = false;

            property.IsConcurrencyToken = true;
            Assert.True(property.IsConcurrencyToken.Value);

            property.IsConcurrencyToken = false;
            Assert.False(property.IsConcurrencyToken.Value);

            property.IsConcurrencyToken = null;
            Assert.Null(property.IsConcurrencyToken);
        }

        [Fact]
        public void Can_mark_property_to_always_use_store_generated_values()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            Assert.Null(property.StoreGeneratedAlways);
            Assert.False(((IProperty)property).StoreGeneratedAlways);

            property.StoreGeneratedAlways = true;
            Assert.True(property.StoreGeneratedAlways.Value);
            Assert.True(((IProperty)property).StoreGeneratedAlways);

            property.StoreGeneratedAlways = false;
            Assert.False(property.StoreGeneratedAlways.Value);
            Assert.False(((IProperty)property).StoreGeneratedAlways);

            property.StoreGeneratedAlways = null;
            Assert.Null(property.StoreGeneratedAlways);
            Assert.False(((IProperty)property).StoreGeneratedAlways);
        }

        [Fact]
        public void Store_generated_concurrency_tokens_always_use_store_values_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            Assert.False(((IProperty)property).StoreGeneratedAlways);

            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            Assert.False(((IProperty)property).StoreGeneratedAlways);

            property.IsConcurrencyToken = true;
            Assert.True(((IProperty)property).StoreGeneratedAlways);

            property.ValueGenerated = ValueGenerated.OnAdd;
            Assert.False(((IProperty)property).StoreGeneratedAlways);

            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            Assert.True(((IProperty)property).StoreGeneratedAlways);

            property.StoreGeneratedAlways = false;
            Assert.False(((IProperty)property).StoreGeneratedAlways);
        }

        [Fact]
        public void Property_is_read_write_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            Assert.Null(property.IsReadOnlyAfterSave);
            Assert.False(((IProperty)property).IsReadOnlyAfterSave);
            Assert.Null(property.IsReadOnlyBeforeSave);
            Assert.False(((IProperty)property).IsReadOnlyBeforeSave);
        }

        [Fact]
        public void Property_can_be_marked_as_read_only_before_save()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;
            property.IsReadOnlyBeforeSave = true;

            Assert.True(property.IsReadOnlyBeforeSave.Value);

            property.IsReadOnlyBeforeSave = false;
            Assert.False(property.IsReadOnlyBeforeSave.Value);

            property.IsReadOnlyBeforeSave = null;
            Assert.Null(property.IsReadOnlyBeforeSave);
        }

        [Fact]
        public void Property_can_be_marked_as_read_only_after_save()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;
            property.IsReadOnlyAfterSave = true;

            Assert.True(property.IsReadOnlyAfterSave.Value);

            property.IsReadOnlyAfterSave = false;
            Assert.False(property.IsReadOnlyAfterSave.Value);

            property.IsReadOnlyAfterSave = null;
            Assert.Null(property.IsReadOnlyAfterSave);
        }

        [Fact]
        public void Property_can_be_marked_as_read_only_always()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;
            property.IsReadOnlyBeforeSave = true;
            property.IsReadOnlyAfterSave = true;

            Assert.True(property.IsReadOnlyBeforeSave.Value);
            Assert.True(property.IsReadOnlyAfterSave.Value);
        }

        [Fact]
        public void Can_get_and_set_property_index_for_normal_property()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake", typeof(int));
            property.IsShadowProperty = false;

            Assert.Equal(0, property.Index);
            Assert.Equal(-1, property.GetShadowIndex());

            property.Index = 1;

            Assert.Equal(1, property.Index);
            Assert.Equal(-1, property.GetShadowIndex());

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.Index = -1).ParamName);

            Assert.Equal(
                "index",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.SetShadowIndex(-1)).ParamName);

            Assert.Equal(
                "index",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.SetShadowIndex(1)).ParamName);
        }

        [Fact]
        public void Can_get_and_set_property_and_shadow_index_for_shadow_property()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake", typeof(int));
            property.IsShadowProperty = true;

            Assert.Equal(0, property.Index);
            Assert.Equal(0, property.GetShadowIndex());

            property.Index = 1;
            property.SetShadowIndex(2);

            Assert.Equal(1, property.Index);
            Assert.Equal(2, property.GetShadowIndex());

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.Index = -1).ParamName);

            Assert.Equal(
                "index",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.SetShadowIndex(-1)).ParamName);
        }

        [Fact]
        public void Nullable_property_has_null_sentinel_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            Assert.Null(property.SentinelValue);
            Assert.Null(((IProperty)property).SentinelValue);
        }

        [Fact]
        public void Non_nullable_property_has_CLR_default_sentinel_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(int));
            property.IsShadowProperty = false;

            Assert.Null(property.SentinelValue);
            Assert.Equal(0, ((IProperty)property).SentinelValue);
        }

        [Fact]
        public void Can_set_sentinel_for_nullable_property()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;
            property.SentinelValue = "Void";

            Assert.Equal("Void", property.SentinelValue);
            Assert.Equal("Void", ((IProperty)property).SentinelValue);
        }

        [Fact]
        public void Can_set_sentinel_for_non_nullable_property()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(int));
            property.IsShadowProperty = false;
            property.SentinelValue = -1;

            Assert.Equal(-1, property.SentinelValue);
            Assert.Equal(-1, ((IProperty)property).SentinelValue);
        }

        [Fact]
        public void IsSentinelValue_on_nullable_propertyalways_returns_true_for_null_or_sentinel()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            property.SentinelValue = "Void";

            Assert.True(property.IsSentinelValue(null));
            Assert.True(property.IsSentinelValue("Void"));
            Assert.False(property.IsSentinelValue("Ether"));
        }

        [Fact]
        public void IsSentinelValue_on_non_nullable_propertyalways_returns_true_for_null_or_sentinel()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Name", typeof(int));
            property.IsShadowProperty = false;
            property.SentinelValue = -1;

            Assert.True(property.IsSentinelValue(null));
            Assert.True(property.IsSentinelValue(-1));
            Assert.False(property.IsSentinelValue(0));
        }

        private class Entity
        {
            public string Name { get; set; }
        }
    }
}
