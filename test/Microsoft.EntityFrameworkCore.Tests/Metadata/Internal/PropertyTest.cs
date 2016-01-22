// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class PropertyTest
    {
        [Fact]
        public void Can_set_ClrType()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var property = entityType.AddProperty("Kake");

            Assert.Equal(typeof(string), property.ClrType);

            property.ClrType = typeof(int);
            Assert.Equal(typeof(int), property.ClrType);
        }

        [Fact]
        public void Setting_ClrType_throws_when_referenced()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var principalProperty = entityType.AddProperty("Kake");
            principalProperty.IsNullable = false;
            var key = entityType.AddKey(principalProperty);
            var dependentProperty = entityType.AddProperty("Alaska");
            entityType.AddForeignKey(dependentProperty, key, entityType);

            Assert.Equal(CoreStrings.PropertyClrTypeCannotBeChangedWhenReferenced("Kake", "{'Alaska'}", "object"),
                Assert.Throws<InvalidOperationException>(() =>
                    principalProperty.ClrType = typeof(int)).Message);
            Assert.Equal(typeof(string), ((IProperty)principalProperty).ClrType);
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

            Assert.Equal(CoreStrings.NullableKey(typeof(object).DisplayName(), stringProperty.Name),
                Assert.Throws<InvalidOperationException>(() =>
                    stringProperty.DeclaringEntityType.AddKey(stringProperty)).Message);
        }

        [Fact]
        public void Properties_with_non_nullable_types_cannot_be_made_nullable()
        {
            var entityType = new Model().AddEntityType(typeof(object));
            var intProperty = entityType.AddProperty("Name", typeof(int));

            Assert.Equal(
                CoreStrings.CannotBeNullable("Name", "object", "Int32"),
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
            property1.IsShadowProperty = false;
            Assert.Equal(typeof(int), property1.ClrType.UnwrapNullableType());
        }

        [Fact]
        public void IsShadowProperty_is_set_appropriately()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty(nameof(Entity.Name));

            Assert.True(property.IsShadowProperty);

            property.ClrType = typeof(string);
            property.IsShadowProperty = false;
            Assert.False(property.IsShadowProperty);

            property.IsShadowProperty = true;
            Assert.True(property.IsShadowProperty);
        }

        [Fact]
        public void IsShadowProperty_throws_if_shadow_entity_type()
        {
            var entityType = new Model().AddEntityType("Entity");
            var property = entityType.AddProperty(nameof(Entity.Name));

            property.ClrType = typeof(int);
            Assert.Equal(CoreStrings.ClrPropertyOnShadowEntity(nameof(Entity.Name), "Entity"),
                Assert.Throws<InvalidOperationException>(() => property.IsShadowProperty = false).Message);
        }

        [Fact]
        public void IsShadowProperty_throws_if_no_clr_property()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Random");

            property.ClrType = typeof(int);
            Assert.Equal(CoreStrings.NoClrProperty("Random", nameof(Entity)),
                Assert.Throws<InvalidOperationException>(() => property.IsShadowProperty = false).Message);
        }

        [Fact]
        public void IsShadowProperty_throws_if_clr_type_does_not_match()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty(nameof(Entity.Name));

            property.ClrType = typeof(int);
            Assert.Equal(CoreStrings.PropertyWrongClrType(nameof(Entity.Name), nameof(Entity)),
                Assert.Throws<InvalidOperationException>(() => property.IsShadowProperty = false).Message);
        }

        [Fact]
        public void Property_does_not_use_ValueGenerated_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Can_mark_property_as_using_ValueGenerated()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

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
            property.IsShadowProperty = false;

            Assert.False(property.IsConcurrencyToken);
        }

        [Fact]
        public void Can_mark_property_as_concurrency_token()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name");
            property.ClrType = typeof(string);
            property.IsShadowProperty = false;

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
            property.IsShadowProperty = false;

            Assert.False(property.IsStoreGeneratedAlways);

            property.IsStoreGeneratedAlways = true;
            Assert.True(property.IsStoreGeneratedAlways);

            property.IsStoreGeneratedAlways = false;
            Assert.False(property.IsStoreGeneratedAlways);
        }

        [Fact]
        public void Store_generated_concurrency_tokens_always_use_store_values_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            Assert.False(((IProperty)property).IsStoreGeneratedAlways);

            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            Assert.False(((IProperty)property).IsStoreGeneratedAlways);

            property.IsConcurrencyToken = true;
            Assert.True(((IProperty)property).IsStoreGeneratedAlways);

            property.ValueGenerated = ValueGenerated.OnAdd;
            Assert.False(((IProperty)property).IsStoreGeneratedAlways);

            property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            Assert.True(((IProperty)property).IsStoreGeneratedAlways);

            property.IsStoreGeneratedAlways = false;
            Assert.False(((IProperty)property).IsStoreGeneratedAlways);
        }

        [Fact]
        public void Property_is_read_write_by_default()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            Assert.False(property.IsReadOnlyAfterSave);
            Assert.False(property.IsReadOnlyBeforeSave);
        }

        [Fact]
        public void Property_can_be_marked_as_read_only_before_save()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;
            property.IsReadOnlyBeforeSave = true;

            Assert.True(property.IsReadOnlyBeforeSave);

            property.IsReadOnlyBeforeSave = false;
            Assert.False(property.IsReadOnlyBeforeSave);
        }

        [Fact]
        public void Property_can_be_marked_as_read_only_after_save()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;
            property.IsReadOnlyAfterSave = true;

            Assert.True(property.IsReadOnlyAfterSave);

            property.IsReadOnlyAfterSave = false;
            Assert.False(property.IsReadOnlyAfterSave);
        }

        [Fact]
        public void Property_can_be_marked_as_read_only_always()
        {
            var entityType = new Model().AddEntityType(typeof(Entity));
            var property = entityType.AddProperty("Name", typeof(string));
            property.IsShadowProperty = false;

            Assert.False(property.IsReadOnlyBeforeSave);
            Assert.False(property.IsReadOnlyAfterSave);

            property.IsReadOnlyBeforeSave = true;
            property.IsReadOnlyAfterSave = true;

            Assert.True(property.IsReadOnlyBeforeSave);
            Assert.True(property.IsReadOnlyAfterSave);
        }

        private class Entity
        {
            public string Name { get; set; }
            public int? Id { get; set; }
        }
    }
}
