// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
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
                Strings.ArgumentIsEmpty("value"),
                Assert.Throws<ArgumentException>(() => entityType.StorageName = "").Message);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(() => entityType.Key = null).ParamName);

            Assert.Equal(
                "property",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => entityType.AddProperty(null)).ParamName);

            Assert.Equal(
                "property",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => entityType.RemoveProperty(null)).ParamName);

            Assert.Equal(
                Strings.ArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => entityType.Property("")).Message);
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

            entityType.Key = new[] { property1, property2 };

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Key));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));

            entityType.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entityType.Key));
            Assert.True(new[] { property2 }.SequenceEqual(entityType.Properties));

            entityType.Key = new[] { property1 };

            Assert.True(new[] { property1 }.SequenceEqual(entityType.Key));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Setting_key_properties_should_update_existing_properties()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(new Property(Customer.IdProperty));

            var newIdProperty = new Property(Customer.IdProperty);

            var property2 = new Property(Customer.NameProperty);

            entityType.Key = new[] { newIdProperty, property2 };

            Assert.True(new[] { newIdProperty, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Can_clear_key()
        {
            var entityType = new EntityType(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entityType.Key = new[] { property1, property2 };

            Assert.Equal(2, entityType.Key.Count());

            entityType.Key = new Property[] { };

            Assert.Equal(0, entityType.Key.Count());
        }

        [Fact]
        public void Add_foreign_key()
        {
            var entityType = new EntityType(typeof(Order));

            var foreignKey = new ForeignKey(
                entityType, new[] { new PropertyPair(new Property(Customer.UniqueProperty), new Property(Order.CustomerUniqueProperty)) });

            Assert.Same(foreignKey, entityType.AddForeignKey(foreignKey));

            Assert.True(entityType.ForeignKeys.Contains(foreignKey));
        }

        [Fact]
        public void Adding_foreign_key_should_add_properties()
        {
            var entityType = new EntityType(typeof(Order));
            var idProperty = new Property(Order.CustomerIdProperty);

            entityType.AddForeignKey(new ForeignKey(
                entityType, new[] { new PropertyPair(new Property(Customer.IdProperty), new Property(Order.CustomerIdProperty)) }));

            Assert.True(entityType.Properties.Contains(idProperty));
        }

        [Fact]
        public void Setting_foreign_key_properties_should_update_existing_properties()
        {
            var entityType = new EntityType(typeof(Order));
            entityType.AddProperty(new Property(Order.CustomerIdProperty));

            var newIdProperty = new Property(Order.CustomerIdProperty);
            var property2 = new Property(Order.CustomerUniqueProperty);

            entityType.AddForeignKey(new ForeignKey(
                entityType, new[]
                    {
                        new PropertyPair(new Property(Customer.IdProperty), newIdProperty),
                        new PropertyPair(new Property(Customer.UniqueProperty), property2)
                    }));

            Assert.Equal(new[] { newIdProperty, property2 }, entityType.Properties.ToArray());
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
        public void Properties_is_IList_to_ensure_collecting_the_count_is_fast()
        {
            Assert.IsAssignableFrom<IList<Property>>(new EntityType(typeof(Customer)).Properties);
        }

        [Fact]
        public void Can_get_property_indexes()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(new Property(Customer.NameProperty));
            entityType.AddProperty(new Property(Customer.IdProperty));
            entityType.AddProperty(new Property(Customer.ManeProperty));

            Assert.Equal(0, entityType.PropertyIndex("Id"));
            Assert.Equal(1, entityType.PropertyIndex("Mane"));
            Assert.Equal(2, entityType.PropertyIndex("Name"));
        }

        [Fact]
        public void Indexes_are_rebuilt_when_more_properties_added()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(new Property(Customer.NameProperty));
            entityType.AddProperty(new Property(Customer.IdProperty));

            Assert.Equal(0, entityType.PropertyIndex("Id"));
            Assert.Equal(1, entityType.PropertyIndex("Name"));

            entityType.AddProperty(new Property(Customer.ManeProperty));

            Assert.Equal(0, entityType.PropertyIndex("Id"));
            Assert.Equal(1, entityType.PropertyIndex("Mane"));
            Assert.Equal(2, entityType.PropertyIndex("Name"));
        }
    }
}
