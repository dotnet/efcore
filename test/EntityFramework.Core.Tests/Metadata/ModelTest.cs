// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class ModelTest
    {
        [Fact]
        public void Can_add_and_remove_entity_by_type()
        {
            var model = new Model();
            Assert.Null(model.FindEntityType(typeof(Customer)));
            Assert.Null(model.RemoveEntityType(new EntityType(typeof(Customer), model)));

            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Equal(typeof(Customer), entityType.ClrType);
            Assert.NotNull(model.FindEntityType(typeof(Customer)));
            Assert.Same(model, entityType.Model);

            Assert.Same(entityType, model.GetOrAddEntityType(typeof(Customer)));

            Assert.Equal(new[] { entityType }, model.EntityTypes.ToArray());

            Assert.Same(entityType, model.RemoveEntityType(entityType));
            Assert.Null(model.RemoveEntityType(entityType));
            Assert.Null(model.FindEntityType(typeof(Customer)));
        }

        [Fact]
        public void Can_add_and_remove_entity_by_name()
        {
            var model = new Model();
            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
            Assert.Null(model.RemoveEntityType(new EntityType(typeof(Customer).FullName, model)));

            var entityType = model.AddEntityType(typeof(Customer).FullName);

            Assert.Null(entityType.ClrType);
            Assert.Equal(typeof(Customer).FullName, entityType.Name);
            Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));
            Assert.Same(model, entityType.Model);

            Assert.Same(entityType, model.GetOrAddEntityType(typeof(Customer).FullName));

            Assert.Equal(new[] { entityType }, model.EntityTypes.ToArray());

            Assert.Same(entityType, model.RemoveEntityType(entityType));
            Assert.Null(model.RemoveEntityType(entityType));
            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
        }

        [Fact]
        public void Cannot_remove_entity_type_when_referenced_by_foreign_key()
        {
            var model = new Model();
            var customerType = model.GetOrAddEntityType(typeof(Customer));
            var idProperty = customerType.GetOrAddProperty(Customer.IdProperty);
            var customerKey = customerType.GetOrAddKey(idProperty);
            var orderType = model.GetOrAddEntityType(typeof(Order));
            var customerFk = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            orderType.AddForeignKey(customerFk, customerKey);

            Assert.Equal(
                Strings.EntityTypeInUse(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(customerType)).Message);
        }

        [Fact]
        public void Adding_duplicate_entity_by_type_throws()
        {
            var model = new Model();
            Assert.Null(model.RemoveEntityType(new EntityType(typeof(Customer).FullName, model)));

            model.AddEntityType(typeof(Customer));

            Assert.Equal(
                Strings.DuplicateEntityType(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Customer))).Message);
        }

        [Fact]
        public void Adding_duplicate_entity_by_name_throws()
        {
            var model = new Model();
            Assert.Null(model.RemoveEntityType(new EntityType(typeof(Customer), model)));

            model.AddEntityType(typeof(Customer));

            Assert.Equal(
                Strings.DuplicateEntityType(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Customer).FullName)).Message);
        }

        [Fact]
        public void Can_get_entity_by_type()
        {
            var model = new Model();
            var entityType = model.GetOrAddEntityType(typeof(Customer));

            Assert.Same(entityType, model.GetEntityType(typeof(Customer)));
            Assert.Same(entityType, model.FindEntityType(typeof(Customer)));
            Assert.Null(model.FindEntityType(typeof(string)));

            Assert.Equal(
                Strings.EntityTypeNotFound("String"),
                Assert.Throws<ModelItemNotFoundException>(() => model.GetEntityType(typeof(string))).Message);
        }

        [Fact]
        public void Can_get_entity_by_name()
        {
            var model = new Model();
            var entityType = model.GetOrAddEntityType(typeof(Customer).FullName);

            Assert.Same(entityType, model.GetEntityType(typeof(Customer).FullName));
            Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));
            Assert.Null(model.FindEntityType(typeof(string)));

            Assert.Equal(
                Strings.EntityTypeNotFound("String"),
                Assert.Throws<ModelItemNotFoundException>(() => model.GetEntityType("String")).Message);
        }

        [Fact]
        public void Entities_are_ordered_by_name()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Order));
            var entityType2 = model.AddEntityType(typeof(Customer));

            Assert.True(new[] { entityType2, entityType1 }.SequenceEqual(model.EntityTypes));
        }

        [Fact]
        public void Can_get_referencing_foreign_keys()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var keyProperty = new Property("Id", typeof(int), entityType1);
            var fkProperty = new Property("CustomerId", typeof(int?), entityType2);
            var foreignKey = entityType2.GetOrAddForeignKey(fkProperty, new Key(new[] { keyProperty }));

            var referencingForeignKeys = model.GetReferencingForeignKeys(entityType1);

            Assert.Same(foreignKey, referencingForeignKeys.Single());
            Assert.Same(foreignKey, entityType1.GetReferencingForeignKeys().Single());
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");

            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Order
        {
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");

            public int Id { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
