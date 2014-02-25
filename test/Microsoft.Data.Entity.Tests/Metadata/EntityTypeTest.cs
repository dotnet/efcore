// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
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

            public int Id { get; set; }
            public string Name { get; set; }
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

            entityType.AddProperty(property1);
            entityType.AddProperty(property2);

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
            var entityType = new EntityType(typeof(Customer));
            var foreignKey = new ForeignKey(entityType, new[] { new Property(Customer.IdProperty) });

            entityType.AddForeignKey(foreignKey);

            Assert.True(entityType.ForeignKeys.Contains(foreignKey));
        }

        [Fact]
        public void Adding_foreign_key_should_add_properties()
        {
            var entityType = new EntityType(typeof(Customer));
            var idProperty = new Property(Customer.IdProperty);

            entityType.AddForeignKey(new ForeignKey(entityType, new[] { idProperty }));

            Assert.True(entityType.Properties.Contains(idProperty));
        }

        [Fact]
        public void Setting_foreign_key_properties_should_update_existing_properties()
        {
            var entityType = new EntityType(typeof(Customer));

            entityType.AddProperty(new Property(Customer.IdProperty));

            var newIdProperty = new Property(Customer.IdProperty);

            var property2 = new Property(Customer.NameProperty);

            entityType.AddForeignKey(new ForeignKey(entityType, new[] { newIdProperty, property2 }));

            Assert.True(new[] { newIdProperty, property2 }.SequenceEqual(entityType.Properties));
        }
    }
}
