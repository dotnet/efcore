// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class ModelTest
    {
        [ConditionalFact]
        public void Use_of_custom_IModel_throws()
        {
            var model = new FakeModel();

            Assert.Equal(
                CoreStrings.CustomMetadata(nameof(Use_of_custom_IModel_throws), nameof(IModel), nameof(FakeModel)),
                Assert.Throws<NotSupportedException>(() => model.AsModel()).Message);
        }

        private class FakeModel : IModel
        {
            public object this[string name]
                => throw new NotImplementedException();

            public IAnnotation FindAnnotation(string name)
                => throw new NotImplementedException();

            public IEnumerable<IAnnotation> GetAnnotations()
                => throw new NotImplementedException();

            public IEnumerable<IEntityType> GetEntityTypes()
                => throw new NotImplementedException();

            public IEntityType FindEntityType(string name)
                => throw new NotImplementedException();

            public IEntityType FindEntityType(string name, string definingNavigationName, IEntityType definingEntityType)
                => throw new NotImplementedException();
        }

        [ConditionalFact]
        public void Snapshot_change_tracking_is_used_by_default()
        {
            Assert.Equal(ChangeTrackingStrategy.Snapshot, CreateModel().GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public void Change_tracking_strategy_can_be_changed()
        {
            var model = CreateModel();
            model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, model.GetChangeTrackingStrategy());

            model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, model.GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public void Can_add_and_remove_entity_by_type()
        {
            var model = CreateModel();
            Assert.Null(model.FindEntityType(typeof(Customer)));
            Assert.Null(model.RemoveEntityType(typeof(Customer)));

            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Equal(typeof(Customer), entityType.ClrType);
            Assert.NotNull(model.FindEntityType(typeof(Customer)));
            Assert.Same(model, entityType.Model);
            Assert.NotNull(((EntityType)entityType).Builder);

            Assert.Same(entityType, model.FindEntityType(typeof(Customer)));

            Assert.Equal(new[] { entityType }, model.GetEntityTypes().ToArray());

            Assert.Same(entityType, model.RemoveEntityType(entityType.ClrType));

            Assert.Null(model.RemoveEntityType(entityType.ClrType));
            Assert.Null(model.FindEntityType(typeof(Customer)));
            Assert.Null(((EntityType)entityType).Builder);
        }

        [ConditionalFact]
        public void Can_add_and_remove_entity_by_name()
        {
            var model = CreateModel();
            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
            Assert.Null(model.RemoveEntityType(typeof(Customer).FullName));

            var entityType = model.AddEntityType(typeof(Customer).FullName);

            Assert.Null(entityType.ClrType);
            Assert.Equal(typeof(Customer).FullName, entityType.Name);
            Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));
            Assert.Same(model, entityType.Model);
            Assert.NotNull(((EntityType)entityType).Builder);

            Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));

            Assert.Equal(new[] { entityType }, model.GetEntityTypes().ToArray());

            Assert.Same(entityType, model.RemoveEntityType(entityType.Name));

            Assert.Null(model.RemoveEntityType(entityType.Name));
            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
            Assert.Null(((EntityType)entityType).Builder);
        }

        [ConditionalFact]
        public void Can_add_and_remove_shared_entity()
        {
            var model = CreateModel();
            var entityTypeName = "SharedCustomer1";
            Assert.Null(model.FindEntityType(typeof(Customer)));
            Assert.Null(model.FindEntityType(entityTypeName));

            var entityType = model.AddEntityType(entityTypeName, typeof(Customer));

            Assert.Equal(typeof(Customer), entityType.ClrType);
            Assert.Equal(entityTypeName, entityType.Name);
            Assert.NotNull(model.FindEntityType(entityTypeName));
            Assert.Same(model, entityType.Model);
            Assert.NotNull(((EntityType)entityType).Builder);

            Assert.Same(entityType, model.FindEntityType(entityTypeName));
            Assert.Null(model.FindEntityType(typeof(Customer)));

            Assert.Equal(new[] { entityType }, model.GetEntityTypes().ToArray());

            Assert.Same(entityType, model.RemoveEntityType(entityType.Name));

            Assert.Null(model.RemoveEntityType(entityType.Name));
            Assert.Null(model.FindEntityType(entityTypeName));
            Assert.Null(((EntityType)entityType).Builder);
        }

        [ConditionalFact]
        public void Can_add_weak_entity_types()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.AddProperty(Customer.IdProperty);
            var customerKey = customerType.AddKey(idProperty);
            var dependentOrderType = model.AddEntityType(typeof(Order), nameof(Customer.Orders), customerType);

            var fkProperty = dependentOrderType.AddProperty("ShadowId", typeof(int));
            var orderKey = dependentOrderType.AddKey(fkProperty);
            var fk = dependentOrderType.AddForeignKey(fkProperty, customerKey, customerType);
            var index = dependentOrderType.AddIndex(fkProperty);

            Assert.Same(fkProperty, dependentOrderType.GetProperties().Single());
            Assert.Same(orderKey, dependentOrderType.GetKeys().Single());
            Assert.Same(fk, dependentOrderType.GetForeignKeys().Single());
            Assert.Same(index, dependentOrderType.GetIndexes().Single());
            Assert.Equal(new[] { customerType, dependentOrderType }, model.GetEntityTypes());
            Assert.True(model.HasEntityTypeWithDefiningNavigation(typeof(Order)));
            Assert.True(model.HasEntityTypeWithDefiningNavigation(typeof(Order).DisplayName()));
            Assert.Same(
                dependentOrderType,
                model.FindEntityType(typeof(Order).DisplayName(), nameof(Customer.Orders), customerType));
            Assert.Same(
                dependentOrderType,
                model.FindEntityType(typeof(Order).DisplayName(), nameof(Customer.Orders), (IEntityType)customerType));

            Assert.Equal(
                CoreStrings.ClashingWeakEntityType(typeof(Order).DisplayName(fullName: false)),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Order))).Message);
            Assert.Equal(
                CoreStrings.ClashingNonWeakEntityType(
                    nameof(Customer)
                    + "."
                    + nameof(Customer.Orders)
                    + "#"
                    + nameof(Order)
                    + "."
                    + nameof(Order.Customer)
                    + "#"
                    + nameof(Customer)),
                Assert.Throws<InvalidOperationException>(
                    () => model.AddEntityType(typeof(Customer), nameof(Order.Customer), dependentOrderType)).Message);

            Assert.Equal(
                CoreStrings.ForeignKeySelfReferencingDependentEntityType(
                    nameof(Customer) + "." + nameof(Customer.Orders) + "#" + nameof(Order)),
                Assert.Throws<InvalidOperationException>(
                    () => dependentOrderType.AddForeignKey(fkProperty, orderKey, dependentOrderType)).Message);

            Assert.Same(
                dependentOrderType, model.RemoveEntityType(
                    typeof(Order), nameof(Customer.Orders), customerType));
            Assert.Null(((EntityType)dependentOrderType).Builder);
            Assert.Empty(customerType.GetReferencingForeignKeys());
        }

        [ConditionalFact]
        public void Cannot_remove_entity_type_when_referenced_by_foreign_key()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.AddProperty(Customer.IdProperty);
            var customerKey = customerType.AddKey(idProperty);
            var orderType = model.AddEntityType(typeof(Order));
            var customerFk = orderType.AddProperty(Order.CustomerIdProperty);

            orderType.AddForeignKey(customerFk, customerKey, customerType);

            Assert.Equal(
                CoreStrings.EntityTypeInUseByReferencingForeignKey(
                    typeof(Customer).Name,
                    "{'" + Order.CustomerIdProperty.Name + "'}",
                    typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(customerType.Name)).Message);
        }

        [ConditionalFact]
        public void Cannot_remove_entity_type_when_it_has_derived_types()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            specialCustomerType.BaseType = customerType;

            Assert.Equal(
                CoreStrings.EntityTypeInUseByDerived(typeof(Customer).Name, typeof(SpecialCustomer).Name),
                Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(customerType.Name)).Message);
        }

        [ConditionalFact]
        public void Adding_duplicate_entity_by_type_throws()
        {
            var model = CreateModel();
            Assert.Null(model.RemoveEntityType(typeof(Customer).FullName));

            model.AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.DuplicateEntityType(nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Customer))).Message);
        }

        [ConditionalFact]
        public void Adding_duplicate_entity_by_name_throws()
        {
            var model = CreateModel();
            Assert.Null(model.RemoveEntityType(typeof(Customer)));

            model.AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.DuplicateEntityType(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Customer).FullName)).Message);
        }

        [ConditionalFact]
        public void Can_get_entity_by_type()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Same(entityType, model.FindEntityType(typeof(Customer)));
            Assert.Same(entityType, model.FindEntityType(typeof(Customer)));
            Assert.Null(model.FindEntityType(typeof(string)));
        }

        [ConditionalFact]
        public void Can_get_entity_by_name()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer).FullName);

            Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));
            Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));
            Assert.Null(model.FindEntityType(typeof(string)));
        }

        [ConditionalFact]
        public void Entities_are_ordered_by_name()
        {
            var model = CreateModel();
            var entityType1 = model.AddEntityType(typeof(Order));
            var entityType2 = model.AddEntityType(typeof(Customer));

            Assert.True(new[] { entityType2, entityType1 }.SequenceEqual(model.GetEntityTypes()));
        }

        [ConditionalFact]
        public void Can_get_referencing_foreign_keys()
        {
            var model = CreateModel();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var keyProperty = entityType1.AddProperty("Id", typeof(int));
            var fkProperty = entityType2.AddProperty("CustomerId", typeof(int));
            var foreignKey = entityType2.AddForeignKey(fkProperty, entityType1.AddKey(keyProperty), entityType1);

            var referencingForeignKeys = entityType1.GetReferencingForeignKeys();

            Assert.Same(foreignKey, referencingForeignKeys.Single());
            Assert.Same(foreignKey, entityType1.GetReferencingForeignKeys().Single());
        }

        private static IMutableModel CreateModel()
            => new Model();

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");

            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<Order> Orders { get; set; }
        }

        private class SpecialCustomer : Customer
        {
        }

        private class Order
        {
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");

            public int Id { get; set; }
            public int CustomerId { get; set; }
            public Customer Customer { get; set; }
        }
    }
}
