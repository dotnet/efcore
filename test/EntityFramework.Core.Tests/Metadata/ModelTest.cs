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
            Assert.Null(model.RemoveEntityType(typeof(Customer)));

            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Equal(typeof(Customer), entityType.ClrType);
            Assert.NotNull(model.FindEntityType(typeof(Customer)));
            Assert.Same(model, entityType.Model);

            Assert.Same(entityType, model.GetOrAddEntityType(typeof(Customer)));

            Assert.Equal(new[] { entityType }, model.GetEntityTypes().ToArray());

            Assert.Same(entityType, model.RemoveEntityType(entityType.ClrType));
            Assert.Null(model.RemoveEntityType(entityType.ClrType));
            Assert.Null(model.FindEntityType(typeof(Customer)));
        }

        [Fact]
        public void Can_add_and_remove_entity_by_name()
        {
            var model = new Model();
            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
            Assert.Null(model.RemoveEntityType(typeof(Customer).FullName));

            var entityType = model.AddEntityType(typeof(Customer).FullName);

            Assert.Null(entityType.ClrType);
            Assert.Equal(typeof(Customer).FullName, entityType.Name);
            Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));
            Assert.Same(model, entityType.Model);

            Assert.Same(entityType, model.GetOrAddEntityType(typeof(Customer).FullName));

            Assert.Equal(new[] { entityType }, model.GetEntityTypes().ToArray());

            Assert.Same(entityType, model.RemoveEntityType(entityType.Name));
            Assert.Null(model.RemoveEntityType(entityType.Name));
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

            orderType.AddForeignKey(customerFk, customerKey, customerType);

            Assert.Equal(
                CoreStrings.EntityTypeInUseByForeignKey(
                    typeof(Customer).Name,
                    "{'" + Order.CustomerIdProperty.Name + "'}",
                    typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(customerType.Name)).Message);
        }

        [Fact]
        public void Cannot_remove_entity_type_when_it_has_derived_types()
        {
            var model = new Model();
            var customerType = model.GetOrAddEntityType(typeof(Customer));
            var specialCustomerType = model.GetOrAddEntityType(typeof(SpecialCustomer));

            specialCustomerType.BaseType = customerType;

            Assert.Equal(
                CoreStrings.EntityTypeInUseByDerived(typeof(Customer).Name, typeof(SpecialCustomer).Name),
                Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(customerType.Name)).Message);
        }

        [Fact]
        public void Adding_duplicate_entity_by_type_throws()
        {
            var model = new Model();
            Assert.Null(model.RemoveEntityType(typeof(Customer).FullName));

            model.AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.DuplicateEntityType(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Customer))).Message);
        }

        [Fact]
        public void Adding_duplicate_entity_by_name_throws()
        {
            var model = new Model();
            Assert.Null(model.RemoveEntityType(typeof(Customer)));

            model.AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.DuplicateEntityType(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Customer).FullName)).Message);
        }

        [Fact]
        public void Can_get_entity_by_type()
        {
            var model = new Model();
            var entityType = model.GetOrAddEntityType(typeof(Customer));

            Assert.Same(entityType, model.FindEntityType(typeof(Customer)));
            Assert.Same(entityType, model.FindEntityType(typeof(Customer)));
            Assert.Null(model.FindEntityType(typeof(string)));
        }

        [Fact]
        public void Can_get_entity_by_name()
        {
            var model = new Model();
            var entityType = model.GetOrAddEntityType(typeof(Customer).FullName);

            Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));
            Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));
            Assert.Null(model.FindEntityType(typeof(string)));
        }

        [Fact]
        public void Entities_are_ordered_by_name()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Order));
            var entityType2 = model.AddEntityType(typeof(Customer));

            Assert.True(new[] { entityType2, entityType1 }.SequenceEqual(model.GetEntityTypes()));
        }

        [Fact]
        public void Can_get_referencing_foreign_keys()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var keyProperty = entityType1.AddProperty("Id", typeof(int));
            var fkProperty = entityType2.AddProperty("CustomerId", typeof(int?));
            var foreignKey = entityType2.GetOrAddForeignKey(fkProperty, entityType1.AddKey(keyProperty), entityType1);

            var referencingForeignKeys = model.FindReferencingForeignKeys(entityType1);

            Assert.Same(foreignKey, referencingForeignKeys.Single());
            Assert.Same(foreignKey, entityType1.FindReferencingForeignKeys().Single());
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");

            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class SpecialCustomer : Customer
        {
        }

        private class Order
        {
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");

            public int Id { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
