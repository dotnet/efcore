// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class EntityTypeTest
    {
        #region Fixture

        public class Customer
        {
            public static PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");
            public static PropertyInfo ManeProperty = typeof(Customer).GetProperty("Mane");
            public static PropertyInfo UniqueProperty = typeof(Customer).GetProperty("Unique");

            public int Id { get; set; }
            public Guid Unique { get; set; }
            public string Name { get; set; }
            public string Mane { get; set; }
        }

        public class Order
        {
            public static PropertyInfo IdProperty = typeof(Order).GetProperty("Id");
            public static PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");
            public static PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty("CustomerUnique");

            public int Id { get; set; }
            public int CustomerId { get; set; }
            public Guid CustomerUnique { get; set; }
        }

        #endregion

        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "type",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntityType((Type)null)).ParamName);

            Assert.Equal(
                "name",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntityType((string)null)).ParamName);

            var entityType = new EntityType(typeof(Random));

            Assert.Equal(
                "propertyInfo",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => entityType.AddProperty((PropertyInfo)null)).ParamName);

            Assert.Equal(
                "property",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => entityType.RemoveProperty(null)).ParamName);

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => entityType.TryGetProperty("")).Message);

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => entityType.GetProperty("")).Message);
        }

        [Fact]
        public void Storage_name_defaults_to_name()
        {
            var entityType = new EntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.StorageName);
        }

        [Fact]
        public void Storage_name_can_be_different_from_name()
        {
            var entityType = new EntityType(typeof(Customer)) { StorageName = "CustomerTable" };

            Assert.Equal("CustomerTable", entityType.StorageName);
        }

        [Fact]
        public void Can_create_entity_type()
        {
            var entityType = new EntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.Name);
            Assert.Same(typeof(Customer), entityType.Type);
        }

        [Fact]
        public void Can_add_and_remove_properties()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));

            entityType.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Property_back_pointer_is_fixed_up_as_property_is_added_and_removed()
        {
            var entityType1 = new EntityType(typeof(Customer));

            var property = entityType1.AddProperty(Customer.IdProperty);

            Assert.Same(entityType1, property.EntityType);

            entityType1.RemoveProperty(property);

            Assert.Empty(entityType1.Properties);
            Assert.Null(property.EntityType);
        }

        [Fact]
        public void Properties_are_ordered_by_name()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Can_set_and_reset_key()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            entityType.SetKey(property1, property2);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.GetKey().Properties));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));

            entityType.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entityType.GetKey().Properties));
            Assert.True(new[] { property2 }.SequenceEqual(entityType.Properties));

            property1 = entityType.AddProperty(Customer.IdProperty);

            entityType.SetKey(property1);

            Assert.True(new[] { property1 }.SequenceEqual(entityType.GetKey().Properties));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Setting_key_properties_should_update_existing_properties()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(Customer.IdProperty);

            var newIdProperty = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            entityType.SetKey(newIdProperty, property2);

            Assert.True(new[] { newIdProperty, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Can_clear_key()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            entityType.SetKey(property1, property2);

            Assert.Equal(2, entityType.GetKey().Properties.Count());

            entityType.SetKey(null);

            Assert.Null(entityType.TryGetKey());
        }

        [Fact]
        public void Add_foreign_key()
        {
            var entityType = new EntityType(typeof(Order));
            entityType.SetKey(entityType.AddProperty(Order.IdProperty));

            var foreignKey
                = entityType.AddForeignKey(
                    entityType.GetKey(),
                    new[] { entityType.AddProperty(Order.CustomerUniqueProperty) });

            Assert.True(entityType.ForeignKeys.Contains(foreignKey));
        }

        [Fact]
        public void Setting_foreign_key_properties_should_update_existing_properties()
        {
            var entityType = new EntityType(typeof(Order));
            entityType.SetKey(entityType.AddProperty(Order.CustomerIdProperty));

            var newIdProperty = entityType.AddProperty(Order.CustomerIdProperty);
            var property2 = entityType.AddProperty(Order.CustomerUniqueProperty);

            entityType.AddForeignKey(entityType.GetKey(), new[] { newIdProperty, property2 });

            Assert.Equal(new[] { newIdProperty, property2 }, entityType.Properties.ToArray());
        }

        [Fact]
        public void FK_back_pointer_is_fixed_up_as_FK_is_added()
        {
            var entityType = new EntityType(typeof(Customer));
            var property = entityType.AddProperty(Customer.IdProperty);
            entityType.SetKey(property);
            var foreignKey
                = entityType.AddForeignKey(entityType.GetKey(), property);

            Assert.Same(entityType, foreignKey.EntityType);
            Assert.Same(entityType, property.EntityType);

            entityType.RemoveForeignKey(foreignKey);

            // Currently property is not removed when FK is removed
            Assert.Empty(entityType.ForeignKeys);
            Assert.Same(property, entityType.Properties.Single());
            Assert.Same(entityType, foreignKey.EntityType); // TODO: Throw here?
            Assert.Same(entityType, property.EntityType);
        }

        [Fact]
        public void Can_add_navigations()
        {
            var entityType = new EntityType(typeof(Order));

            var navigation = new Navigation(new Mock<ForeignKey>().Object, "Milk");

            entityType.AddNavigation(navigation);

            Assert.Same(navigation, entityType.Navigations.Single());
            Assert.Same(entityType, navigation.EntityType);
        }

        [Fact]
        public void Navigation_back_pointer_is_fixed_up_as_navigation_is_added_and_removed()
        {
            var entityType1 = new EntityType(typeof(Customer));
            entityType1.SetKey(entityType1.AddProperty(Customer.IdProperty));
            var entityType2 = new EntityType(typeof(Customer));

            var navigation
                = new Navigation(
                    new ForeignKey(
                        entityType1.GetKey(),
                        new[] { entityType1.AddProperty(Customer.IdProperty) }), "Nav");

            entityType1.AddNavigation(navigation);

            Assert.Same(entityType1, navigation.EntityType);

            entityType2.AddNavigation(navigation);

            Assert.Same(entityType2, navigation.EntityType);
            Assert.Empty(entityType1.Navigations);

            entityType2.RemoveNavigation(navigation);

            Assert.Empty(entityType2.Navigations);
            Assert.Null(navigation.EntityType);
        }

        [Fact]
        public void Properties_is_IList_to_ensure_collecting_the_count_is_fast()
        {
            Assert.IsAssignableFrom<IList<Property>>(new EntityType(typeof(Customer)).Properties);
        }

        [Fact]
        public void Can_get_property_and_can_try_get_property()
        {
            var entityType = new EntityType(typeof(Customer));
            entityType.AddProperty(Customer.IdProperty);

            Assert.Equal("Id", entityType.TryGetProperty("Id").Name);
            Assert.Equal("Id", entityType.GetProperty("Id").Name);

            Assert.Null(entityType.TryGetProperty("Nose"));

            Assert.Equal(
                Strings.FormatPropertyNotFound("Nose", "Customer"),
                Assert.Throws<ModelItemNotFoundException>(() => entityType.GetProperty("Nose")).Message);
        }

        [Fact]
        public void Shadow_properties_have_CLR_flag_set_to_false()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(Customer.NameProperty);
            entityType.AddProperty("Id", typeof(int), shadowProperty: false);
            entityType.AddProperty("Mane", typeof(int), shadowProperty: true);

            Assert.True(entityType.GetProperty("Name").IsClrProperty);
            Assert.True(entityType.GetProperty("Id").IsClrProperty);
            Assert.False(entityType.GetProperty("Mane").IsClrProperty);
        }

        [Fact]
        public void Can_get_property_indexes()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(Customer.NameProperty);
            entityType.AddProperty("Id", typeof(int), shadowProperty: true);
            entityType.AddProperty("Mane", typeof(int), shadowProperty: true);

            Assert.Equal(0, entityType.GetProperty("Id").Index);
            Assert.Equal(1, entityType.GetProperty("Mane").Index);
            Assert.Equal(2, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Id").ShadowIndex);
            Assert.Equal(1, entityType.GetProperty("Mane").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Name").ShadowIndex);

            Assert.Equal(2, entityType.ShadowPropertyCount);
        }

        [Fact]
        public void Indexes_are_rebuilt_when_more_properties_added()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(Customer.NameProperty);
            entityType.AddProperty("Id", typeof(int), shadowProperty: true);

            Assert.Equal(0, entityType.GetProperty("Id").Index);
            Assert.Equal(1, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Id").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Name").ShadowIndex);

            Assert.Equal(1, entityType.ShadowPropertyCount);

            entityType.AddProperty("Game", typeof(int), shadowProperty: true);
            entityType.AddProperty("Mane", typeof(int), shadowProperty: true);

            Assert.Equal(0, entityType.GetProperty("Game").Index);
            Assert.Equal(1, entityType.GetProperty("Id").Index);
            Assert.Equal(2, entityType.GetProperty("Mane").Index);
            Assert.Equal(3, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Game").ShadowIndex);
            Assert.Equal(1, entityType.GetProperty("Id").ShadowIndex);
            Assert.Equal(2, entityType.GetProperty("Mane").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Name").ShadowIndex);

            Assert.Equal(3, entityType.ShadowPropertyCount);
        }
    }
}
