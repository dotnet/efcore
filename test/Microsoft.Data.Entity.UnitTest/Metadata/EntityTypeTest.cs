// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.ChangeTracking;
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
        public void MembersCheckArguments()
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
        public void StorageNameDefaultsToName()
        {
            var entity = new EntityType(typeof(Customer));

            Assert.Equal("Customer", entity.StorageName);
        }

        [Fact]
        public void StorageNameCanBeDifferentFromName()
        {
            var entity = new EntityType(typeof(Customer)) { StorageName = "CustomerTable" };

            Assert.Equal("CustomerTable", entity.StorageName);
        }

        [Fact]
        public void CanCreateEntityType()
        {
            var entity = new EntityType(typeof(Customer));

            Assert.Equal("Customer", entity.Name);
            Assert.Same(typeof(Customer), entity.Type);
        }

        [Fact]
        public void CanAddAndRemoveProperties()
        {
            var entity = new EntityType(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entity.AddProperty(property1);
            entity.AddProperty(property2);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entity.Properties));

            entity.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entity.Properties));
        }

        [Fact]
        public void PropertiesAreOrderedByName()
        {
            var entity = new EntityType(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entity.AddProperty(property2);
            entity.AddProperty(property1);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entity.Properties));
        }

        [Fact]
        public void CanSetAndResetKey()
        {
            var entity = new EntityType(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entity.Key = new[] { property1, property2 };

            Assert.True(new[] { property1, property2 }.SequenceEqual(entity.Key));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entity.Properties));

            entity.RemoveProperty(property1);

            Assert.True(new[] { property2 }.SequenceEqual(entity.Key));
            Assert.True(new[] { property2 }.SequenceEqual(entity.Properties));

            entity.Key = new[] { property1 };

            Assert.True(new[] { property1 }.SequenceEqual(entity.Key));
            Assert.True(new[] { property1, property2 }.SequenceEqual(entity.Properties));
        }

        [Fact]
        public void SettingKeyPropertiesShouldUpdateExistingProperties()
        {
            var entity = new EntityType(typeof(Customer));

            entity.AddProperty(new Property(Customer.IdProperty));

            var newIdProperty = new Property(Customer.IdProperty);

            var property2 = new Property(Customer.NameProperty);

            entity.Key = new[] { newIdProperty, property2 };

            Assert.True(new[] { newIdProperty, property2 }.SequenceEqual(entity.Properties));
        }

        [Fact]
        public void CanClearKey()
        {
            var entity = new EntityType(typeof(Customer));

            var property1 = new Property(Customer.IdProperty);
            var property2 = new Property(Customer.NameProperty);

            entity.Key = new[] { property1, property2 };

            Assert.Equal(2, entity.Key.Count());

            entity.Key = new Property[] { };

            Assert.Equal(0, entity.Key.Count());
        }

        [Fact]
        public void Can_create_EntityKey_fopr_given_entity_instance()
        {
            var entityType = new EntityType(typeof(Customer)) { Key = new[] { new Property(Customer.IdProperty) } };

            var key = entityType.CreateKey(new Customer { Id = 77 });

            Assert.IsType<SimpleEntityKey<Customer, int>>(key);
            Assert.Equal(77, key.Value);
        }
    }
}
