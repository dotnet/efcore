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

            public Customer(object[] values)
            {
                Id = (int)values[0];
            }

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
        public void CreateInstance_should_use_values_ctor()
        {
            var entityType = new EntityType(typeof(Customer));

            var instance = (Customer)entityType.CreateInstance(new object[] { 42 });

            Assert.Equal(42, instance.Id);
        }

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
                "property",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => entityType.AddProperty(null)).ParamName);

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

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            Assert.Same(property1, entityType.AddProperty(property1));
            Assert.Same(property2, entityType.AddProperty(property2));

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));

            entityType.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Property_back_pointer_is_fixed_up_as_property_is_added_and_removed()
        {
            var entityType1 = new EntityType(typeof(Customer));
            var entityType2 = new EntityType(typeof(Customer));

            var property = new Property(Customer.IdProperty);
            entityType1.AddProperty(property);

            Assert.Same(entityType1, property.EntityType);

            entityType2.AddProperty(property);

            Assert.Same(entityType2, property.EntityType);
            Assert.Empty(entityType1.Properties);

            entityType2.RemoveProperty(property);

            Assert.Empty(entityType2.Properties);
            Assert.Null(property.EntityType);
        }

        [Fact]
        public void Properties_are_ordered_by_name()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entityType.AddProperty(property2);
            entityType.AddProperty(property1);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Can_set_and_reset_key()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entityType.SetKey(new Key(new[] { property1, property2 }));

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.GetKey().Properties));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));

            entityType.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entityType.GetKey().Properties));
            Assert.True(new[] { property2 }.SequenceEqual(entityType.Properties));

            entityType.SetKey(new Key(new[] { property1 }));

            Assert.True(new[] { property1 }.SequenceEqual(entityType.GetKey().Properties));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Setting_key_properties_should_update_existing_properties()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(new Property(Customer.IdProperty));

            var newIdProperty = new Property(Customer.IdProperty);

            var property2 = new Property(Customer.NameProperty);

            entityType.SetKey(new Key(new[] { newIdProperty, property2 }));

            Assert.True(new[] { newIdProperty, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Can_clear_key()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entityType.SetKey(new Key(new[] { property1, property2 }));

            Assert.Equal(2, entityType.GetKey().Properties.Count());

            entityType.SetKey(null);

            Assert.Null(entityType.TryGetKey());
        }

        [Fact]
        public void Add_foreign_key()
        {
            var entityType = new EntityType(typeof(Order));

            var foreignKey = new ForeignKey(
                entityType, new[] { new Property(Order.CustomerUniqueProperty) });

            Assert.Same(foreignKey, entityType.AddForeignKey(foreignKey));

            Assert.True(entityType.ForeignKeys.Contains(foreignKey));
        }

        [Fact]
        public void Adding_foreign_key_should_add_properties()
        {
            var entityType = new EntityType(typeof(Order));
            var idProperty = new Property(Order.CustomerIdProperty);

            entityType.AddForeignKey(new ForeignKey(entityType, new[] { idProperty }));

            Assert.True(entityType.Properties.Contains(idProperty));
        }

        [Fact]
        public void Setting_foreign_key_properties_should_update_existing_properties()
        {
            var entityType = new EntityType(typeof(Order));
            entityType.AddProperty(new Property(Order.CustomerIdProperty));

            var newIdProperty = new Property(Order.CustomerIdProperty);
            var property2 = new Property(Order.CustomerUniqueProperty);

            entityType.AddForeignKey(
                new ForeignKey(
                    entityType, new[] { newIdProperty, property2 }));

            Assert.Equal(new[] { newIdProperty, property2 }, entityType.Properties.ToArray());
        }

        [Fact]
        public void FK_back_pointer_is_fixed_up_as_FK_is_added_and_removed()
        {
            var entityType1 = new EntityType(typeof(Customer));
            var entityType2 = new EntityType(typeof(Customer));

            var property = new Property(Customer.IdProperty);
            var foreignKey = new ForeignKey(entityType1, new[] { property });
            entityType1.AddForeignKey(foreignKey);

            Assert.Same(entityType1, foreignKey.DependentType);
            Assert.Same(entityType1, property.EntityType);

            entityType2.AddForeignKey(foreignKey);

            Assert.Same(entityType2, foreignKey.DependentType);
            Assert.Same(entityType2, property.EntityType);
            Assert.Empty(entityType1.ForeignKeys);
            Assert.Empty(entityType1.Properties);

            entityType2.RemoveForeignKey(foreignKey);

            // Currently property is not removed when FK is removed
            Assert.Empty(entityType2.ForeignKeys);
            Assert.Same(property, entityType2.Properties.Single());
            Assert.Null(foreignKey.DependentType);
            Assert.Same(entityType2, property.EntityType);
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
            var entityType2 = new EntityType(typeof(Customer));

            var navigation = new Navigation(
                new ForeignKey(entityType1, new[] { new Property(Customer.IdProperty) }), "Nav");

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
            entityType.AddProperty(new Property(Customer.IdProperty));

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

            entityType.AddProperty(new Property(Customer.NameProperty));
            entityType.AddProperty(new Property("Id", typeof(int), hasClrProperty: true));
            entityType.AddProperty(new Property("Mane", typeof(int), hasClrProperty: false));

            Assert.True(entityType.GetProperty("Name").HasClrProperty);
            Assert.True(entityType.GetProperty("Id").HasClrProperty);
            Assert.False(entityType.GetProperty("Mane").HasClrProperty);
        }

        [Fact]
        public void Can_get_property_indexes()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(new Property(Customer.NameProperty));
            entityType.AddProperty(new Property("Id", typeof(int), hasClrProperty: false));
            entityType.AddProperty(new Property("Mane", typeof(int), hasClrProperty: false));

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

            entityType.AddProperty(new Property(Customer.NameProperty));
            entityType.AddProperty(new Property("Id", typeof(int), hasClrProperty: false));

            Assert.Equal(0, entityType.GetProperty("Id").Index);
            Assert.Equal(1, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Id").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Name").ShadowIndex);

            Assert.Equal(1, entityType.ShadowPropertyCount);

            entityType.AddProperty(new Property("Game", typeof(int), hasClrProperty: false));
            entityType.AddProperty(new Property("Mane", typeof(int), hasClrProperty: false));

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
