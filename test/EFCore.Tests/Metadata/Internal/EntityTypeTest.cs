// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable ImplicitlyCapturedClosure
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public partial class EntityTypeTest
    {
        [ConditionalFact]
        public void Invalid_filter_expressions_throws()
        {
            var model = CreateModel();

            var entityTypeA = model.AddEntityType(typeof(A).Name);

            Expression<Func<B, bool>> badExpression1 = b => false;

            Assert.Equal(
                CoreStrings.BadFilterExpression(badExpression1, entityTypeA.DisplayName(), entityTypeA.ClrType),
                Assert.Throws<InvalidOperationException>(() => entityTypeA.SetQueryFilter(badExpression1)).Message);

            Expression<Func<A, string>> badExpression2 = a => "";

            Assert.Equal(
                CoreStrings.BadFilterExpression(badExpression2, entityTypeA.DisplayName(), entityTypeA.ClrType),
                Assert.Throws<InvalidOperationException>(() => entityTypeA.SetQueryFilter(badExpression2)).Message);
        }

        [ConditionalFact]
        public void Use_of_custom_IEntityType_throws()
        {
            var type = new FakeEntityType();

            Assert.Equal(
                CoreStrings.CustomMetadata(nameof(Use_of_custom_IEntityType_throws), nameof(IEntityType), nameof(FakeEntityType)),
                Assert.Throws<NotSupportedException>(() => type.AsEntityType()).Message);
        }

        private class FakeEntityType : IEntityType
        {
            public object this[string name] => throw new NotImplementedException();
            public IAnnotation FindAnnotation(string name) => throw new NotImplementedException();
            public IEnumerable<IAnnotation> GetAnnotations() => throw new NotImplementedException();
            public IModel Model { get; }
            public string Name { get; }
            public Type ClrType { get; }
            public IEntityType BaseType { get; }
            public string DefiningNavigationName { get; }
            public IEntityType DefiningEntityType { get; }
            public LambdaExpression QueryFilter { get; }
            public IKey FindPrimaryKey() => throw new NotImplementedException();
            public IKey FindKey(IReadOnlyList<IProperty> properties) => throw new NotImplementedException();
            public IEnumerable<IKey> GetKeys() => throw new NotImplementedException();

            public IForeignKey FindForeignKey(IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType) =>
                throw new NotImplementedException();

            public IEnumerable<IForeignKey> GetForeignKeys() => throw new NotImplementedException();
            public IIndex FindIndex(IReadOnlyList<IProperty> properties) => throw new NotImplementedException();
            public IEnumerable<IIndex> GetIndexes() => throw new NotImplementedException();
            public IProperty FindProperty(string name) => throw new NotImplementedException();
            public IEnumerable<IProperty> GetProperties() => throw new NotImplementedException();
            public IServiceProperty FindServiceProperty(string name) => throw new NotImplementedException();
            public IEnumerable<IServiceProperty> GetServiceProperties() => throw new NotImplementedException();
            public IEnumerable<IDictionary<string, object>> GetSeedData() => throw new NotImplementedException();
        }

        [ConditionalFact]
        public void Display_name_is_prettified_CLR_name()
        {
            Assert.Equal("EntityTypeTest", CreateModel().AddEntityType(typeof(EntityTypeTest)).DisplayName());
            Assert.Equal("Customer", CreateModel().AddEntityType(typeof(Customer)).DisplayName());
            Assert.Equal("List<Customer>", CreateModel().AddEntityType(typeof(List<Customer>)).DisplayName());
        }

        [ConditionalFact]
        public void Display_name_is_entity_type_name_when_no_CLR_type()
            => Assert.Equal(
                "Everything.Is+Awesome<When.We, re.Living<Our.Dream>>",
                CreateModel().AddEntityType("Everything.Is+Awesome<When.We, re.Living<Our.Dream>>").DisplayName());

        [ConditionalFact]
        public void Name_is_prettified_CLR_full_name()
        {
            Assert.Equal(
                "Microsoft.EntityFrameworkCore.Metadata.Internal.EntityTypeTest", CreateModel().AddEntityType(typeof(EntityTypeTest)).Name);
            Assert.Equal(
                "Microsoft.EntityFrameworkCore.Metadata.Internal.EntityTypeTest+Customer",
                CreateModel().AddEntityType(typeof(Customer)).Name);
            Assert.Equal(
                "System.Collections.Generic.List<Microsoft.EntityFrameworkCore.Metadata.Internal.EntityTypeTest+Customer>",
                CreateModel().AddEntityType(typeof(List<Customer>)).Name);
        }

        [ConditionalFact]
        public void Can_set_reset_and_clear_primary_key()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var nameProperty = entityType.AddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;

            var key1 = entityType.SetPrimaryKey(new[] { idProperty, nameProperty });

            Assert.NotNull(key1);
            Assert.Same(key1, entityType.FindPrimaryKey());

            Assert.Same(key1, entityType.FindPrimaryKey());
            Assert.Same(key1, entityType.GetKeys().Single());

            var key2 = entityType.SetPrimaryKey(idProperty);

            Assert.NotNull(key2);
            Assert.Same(key2, entityType.FindPrimaryKey());
            Assert.Same(key2, entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());

            Assert.Same(key1, entityType.FindKey(key1.Properties));
            Assert.Same(key2, entityType.FindKey(key2.Properties));

            Assert.Null(entityType.SetPrimaryKey(null));

            Assert.Null(entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());

            Assert.Null(entityType.SetPrimaryKey(Array.Empty<Property>()));

            Assert.Null(entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());
        }

        [ConditionalFact]
        public void Setting_primary_key_throws_if_properties_from_different_type()
        {
            var model = CreateModel();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var idProperty = entityType2.AddProperty(Customer.IdProperty);

            Assert.Equal(
                CoreStrings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType1.SetPrimaryKey(idProperty)).Message);
        }

        [ConditionalFact]
        public void Can_get_set_reset_and_clear_primary_key()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var nameProperty = entityType.AddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;

            var key1 = entityType.SetPrimaryKey(new[] { idProperty, nameProperty });

            Assert.NotNull(key1);
            Assert.Same(key1, entityType.SetPrimaryKey(new[] { idProperty, nameProperty }));
            Assert.Same(key1, entityType.FindPrimaryKey());

            Assert.Same(key1, entityType.FindPrimaryKey());
            Assert.Same(key1, entityType.GetKeys().Single());

            var key2 = entityType.SetPrimaryKey(idProperty);

            Assert.NotNull(key2);
            Assert.NotEqual(key1, key2);
            Assert.Same(key2, entityType.SetPrimaryKey(idProperty));
            Assert.Same(key2, entityType.FindPrimaryKey());
            Assert.Same(key2, entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());
            Assert.Same(key1, entityType.FindKey(key1.Properties));
            Assert.Same(key2, entityType.FindKey(key2.Properties));

            Assert.Null(entityType.SetPrimaryKey(null));

            Assert.Null(entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());

            Assert.Null(entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());
        }

        [ConditionalFact]
        public void Can_clear_the_primary_key_if_it_is_referenced_from_a_foreign_key()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var customerPk = entityType.SetPrimaryKey(idProperty);

            var orderType = model.AddEntityType(typeof(Order));
            var fk = orderType.AddForeignKey(orderType.AddProperty(Order.CustomerIdProperty), customerPk, entityType);

            entityType.SetPrimaryKey(null);

            Assert.Single(entityType.GetKeys());
            Assert.Same(customerPk, entityType.FindKey(idProperty));
            Assert.Null(entityType.FindPrimaryKey());
            Assert.Same(customerPk, fk.PrincipalKey);
        }

        [ConditionalFact]
        public void Can_change_the_primary_key_if_it_is_referenced_from_a_foreign_key()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var customerPk = entityType.SetPrimaryKey(idProperty);

            var orderType = model.AddEntityType(typeof(Order));
            var fk = orderType.AddForeignKey(orderType.AddProperty(Order.CustomerIdProperty), customerPk, entityType);
            var nameProperty = entityType.AddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;
            entityType.SetPrimaryKey(nameProperty);

            Assert.Equal(2, entityType.GetKeys().Count());
            Assert.Same(customerPk, entityType.FindKey(idProperty));
            Assert.NotSame(customerPk, entityType.FindPrimaryKey());
            Assert.Same(customerPk, fk.PrincipalKey);
        }

        [ConditionalFact]
        public void Can_add_and_get_a_key()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var nameProperty = entityType.AddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;

            var key1 = entityType.AddKey(new[] { idProperty, nameProperty });

            Assert.NotNull(key1);
            Assert.Same(key1, entityType.FindKey(new[] { idProperty, nameProperty }));
            Assert.Same(key1, entityType.GetKeys().Single());

            var key2 = entityType.AddKey(idProperty);

            Assert.NotNull(key2);
            Assert.Same(key2, entityType.FindKey(idProperty));
            Assert.Equal(2, entityType.GetKeys().Count());
            Assert.Contains(key1, entityType.GetKeys());
            Assert.Contains(key2, entityType.GetKeys());
        }

        [ConditionalFact]
        public void Adding_a_key_throws_if_properties_from_different_type()
        {
            var model = CreateModel();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var idProperty = entityType2.AddProperty(Customer.IdProperty);

            Assert.Equal(
                CoreStrings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType1.AddKey(idProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_a_key_throws_if_duplicated()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var nameProperty = entityType.AddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;
            entityType.AddKey(new[] { idProperty, nameProperty });

            Assert.Equal(
                CoreStrings.DuplicateKey(
                    "{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).Name,
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty, nameProperty })).Message);
        }

        [ConditionalFact]
        public void Adding_a_key_throws_if_duplicated_properties()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);

            Assert.Equal(
                CoreStrings.DuplicatePropertyInList(
                    "{'" + Customer.IdProperty.Name + "', '" + Customer.IdProperty.Name + "'}", Customer.IdProperty.Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty, idProperty })).Message);
        }

        [ConditionalFact]
        public void Adding_a_key_throws_if_properties_were_removed()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            entityType.RemoveProperty(idProperty.Name);

            Assert.Equal(
                CoreStrings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty })).Message);
        }

        [ConditionalFact]
        public void Adding_a_key_throws_if_same_as_primary()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var nameProperty = entityType.AddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;
            entityType.SetPrimaryKey(new[] { idProperty, nameProperty });

            Assert.Equal(
                CoreStrings.DuplicateKey(
                    "{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).Name,
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty, nameProperty })).Message);
        }

        [ConditionalFact]
        public void Can_add_a_key_if_any_properties_are_part_of_derived_foreign_key()
        {
            var model = CreateModel();
            var baseType = model.AddEntityType(typeof(BaseType));
            var idProperty = baseType.AddProperty(Customer.IdProperty);
            var fkProperty = baseType.AddProperty("fk", typeof(int));
            var key = baseType.AddKey(new[] { idProperty });
            var entityType = model.AddEntityType(typeof(Customer));
            entityType.BaseType = baseType;
            entityType.AddForeignKey(new[] { fkProperty }, key, entityType);

            Assert.NotNull(baseType.AddKey(new[] { fkProperty }));
        }

        [ConditionalFact]
        public void Adding_a_key_with_value_generation_throws_if_any_properties_are_part_of_derived_foreign_key()
        {
            var model = CreateModel();
            var baseType = model.AddEntityType(typeof(BaseType));
            var idProperty = baseType.AddProperty(Customer.IdProperty);
            var fkProperty = baseType.AddProperty("fk", typeof(int));
            fkProperty.ValueGenerated = ValueGenerated.OnAdd;
            var key = baseType.AddKey(new[] { idProperty });
            var entityType = model.AddEntityType(typeof(Customer));
            entityType.BaseType = baseType;
            entityType.AddForeignKey(new[] { fkProperty }, key, entityType);

            Assert.Equal(
                CoreStrings.KeyPropertyInForeignKey("fk", typeof(BaseType).Name),
                Assert.Throws<InvalidOperationException>(() => baseType.AddKey(new[] { fkProperty })).Message);
        }

        [ConditionalFact]
        public void Can_remove_keys()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var nameProperty = entityType.AddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;

            Assert.Null(entityType.RemoveKey(new[] { idProperty }));
            Assert.False(idProperty.IsKey());
            Assert.Empty(idProperty.GetContainingKeys());

            var key1 = entityType.SetPrimaryKey(new[] { idProperty, nameProperty });
            var key2 = entityType.AddKey(idProperty);

            Assert.NotNull(((Key)key1).Builder);
            Assert.NotNull(((Key)key2).Builder);
            Assert.Equal(new[] { key2, key1 }, entityType.GetKeys().ToArray());
            Assert.True(idProperty.IsKey());
            Assert.Equal(new[] { key1, key2 }, idProperty.GetContainingKeys().ToArray());

            Assert.Same(key1, entityType.RemoveKey(key1.Properties));
            Assert.Null(entityType.RemoveKey(key1.Properties));

            Assert.Equal(new[] { key2 }, entityType.GetKeys().ToArray());

            Assert.Same(key2, entityType.RemoveKey(new[] { idProperty }));

            Assert.Null(((Key)key1).Builder);
            Assert.Null(((Key)key2).Builder);
            Assert.Empty(entityType.GetKeys());
            Assert.False(idProperty.IsKey());
            Assert.Empty(idProperty.GetContainingKeys());
        }

        [ConditionalFact]
        public void Removing_a_key_throws_if_it_referenced_from_a_foreign_key_in_the_model()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var customerFk = orderType.AddProperty(Order.CustomerIdProperty);
            orderType.AddForeignKey(customerFk, customerKey, customerType);

            Assert.Equal(
                CoreStrings.KeyInUse("{'" + Customer.IdProperty.Name + "'}", nameof(Customer), nameof(Order)),
                Assert.Throws<InvalidOperationException>(() => customerType.RemoveKey(customerKey.Properties)).Message);
        }

        [ConditionalFact]
        public void Keys_are_ordered_by_property_count_then_property_names()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.AddProperty(Customer.IdProperty);
            var nameProperty = customerType.AddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;
            var otherNameProperty = customerType.AddProperty("OtherNameProperty", typeof(string));
            otherNameProperty.IsNullable = false;

            var k2 = customerType.AddKey(nameProperty);
            var k4 = customerType.AddKey(new[] { idProperty, otherNameProperty });
            var k3 = customerType.AddKey(new[] { idProperty, nameProperty });
            var k1 = customerType.AddKey(idProperty);

            Assert.True(new[] { k1, k2, k3, k4 }.SequenceEqual(customerType.GetKeys()));
        }

        [ConditionalFact]
        public void Store_computed_values_are_ignored_before_and_after_save_by_default()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var nameProperty = entityType.AddProperty(Customer.NameProperty);

            Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
            Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetAfterSaveBehavior());

            nameProperty.ValueGenerated = ValueGenerated.OnAddOrUpdate;

            Assert.Equal(PropertySaveBehavior.Ignore, nameProperty.GetBeforeSaveBehavior());
            Assert.Equal(PropertySaveBehavior.Ignore, nameProperty.GetAfterSaveBehavior());

            nameProperty.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

            Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
            Assert.Equal(PropertySaveBehavior.Ignore, nameProperty.GetAfterSaveBehavior());

            nameProperty.SetAfterSaveBehavior(PropertySaveBehavior.Save);

            Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
            Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetAfterSaveBehavior());
        }

        [ConditionalFact]
        public void Store_computed_values_are_ignored_after_save_by_default()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var nameProperty = entityType.AddProperty(Customer.NameProperty);

            Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
            Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetAfterSaveBehavior());

            nameProperty.ValueGenerated = ValueGenerated.OnUpdate;

            Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
            Assert.Equal(PropertySaveBehavior.Ignore, nameProperty.GetAfterSaveBehavior());

            nameProperty.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

            Assert.Equal(PropertySaveBehavior.Throw, nameProperty.GetBeforeSaveBehavior());
            Assert.Equal(PropertySaveBehavior.Ignore, nameProperty.GetAfterSaveBehavior());

            nameProperty.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

            Assert.Equal(PropertySaveBehavior.Throw, nameProperty.GetBeforeSaveBehavior());
            Assert.Equal(PropertySaveBehavior.Throw, nameProperty.GetAfterSaveBehavior());
        }

        [ConditionalFact]
        public void Can_add_a_foreign_key()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.AddProperty(Customer.IdProperty);
            var customerKey = customerType.AddKey(idProperty);
            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);
            var customerFk2 = orderType.AddProperty("IdAgain", typeof(int));

            var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);

            Assert.NotNull(fk1);
            Assert.Same(fk1, orderType.FindForeignKeys(customerFk1).Single());
            Assert.Same(fk1, orderType.FindForeignKey(customerFk1, customerKey, customerType));
            Assert.Same(fk1, orderType.GetForeignKeys().Single());

            var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);

            Assert.Same(fk2, orderType.FindForeignKeys(customerFk2).Single());
            Assert.Same(fk2, orderType.FindForeignKey(customerFk2, customerKey, customerType));
            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());
        }

        [ConditionalFact]
        public void Can_add_a_foreign_key_targeting_different_key()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey1 = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));
            var orderType = model.AddEntityType(typeof(Order));
            var customerFkProperty = orderType.AddProperty(Order.CustomerIdProperty);

            var fk1 = orderType.AddForeignKey(customerFkProperty, customerKey1, customerType);

            Assert.NotNull(fk1);
            Assert.Same(fk1, orderType.FindForeignKeys(customerFkProperty).Single());
            Assert.Same(fk1, orderType.FindForeignKey(customerFkProperty, customerKey1, customerType));
            Assert.Same(fk1, orderType.GetForeignKeys().Single());

            var altKeyProperty = customerType.AddProperty(nameof(Customer.AlternateId), typeof(int));
            var customerKey2 = customerType.AddKey(altKeyProperty);
            var fk2 = orderType.AddForeignKey(customerFkProperty, customerKey2, customerType);

            Assert.Equal(2, orderType.FindForeignKeys(customerFkProperty).Count());
            Assert.Same(fk2, orderType.FindForeignKey(customerFkProperty, customerKey2, customerType));
            Assert.Equal(new[] { fk2, fk1 }, orderType.GetForeignKeys().ToArray());
        }

        [ConditionalFact]
        public void Can_add_a_foreign_key_targeting_different_entity_type()
        {
            var model = CreateModel();
            var baseType = model.AddEntityType(typeof(BaseType));
            var customerType = model.AddEntityType(typeof(Customer));
            customerType.BaseType = baseType;
            var customerKey1 = baseType.AddKey(baseType.AddProperty(Customer.IdProperty));
            var orderType = model.AddEntityType(typeof(Order));
            var customerFkProperty = orderType.AddProperty(Order.CustomerIdProperty);

            var fk1 = orderType.AddForeignKey(customerFkProperty, customerKey1, baseType);

            Assert.NotNull(fk1);
            Assert.Same(fk1, orderType.FindForeignKeys(customerFkProperty).Single());
            Assert.Same(fk1, orderType.FindForeignKey(customerFkProperty, customerKey1, baseType));
            Assert.Same(fk1, orderType.GetForeignKeys().Single());

            var fk2 = orderType.AddForeignKey(customerFkProperty, customerKey1, customerType);

            Assert.Equal(2, orderType.FindForeignKeys(customerFkProperty).Count());
            Assert.Same(fk2, orderType.FindForeignKey(customerFkProperty, customerKey1, customerType));
            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());
        }

        [ConditionalFact]
        public void Adding_a_foreign_key_throws_if_duplicate()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));
            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);
            orderType.AddForeignKey(customerFk1, customerKey, customerType);

            Assert.Equal(
                CoreStrings.DuplicateForeignKey(
                    "{'" + Order.CustomerIdProperty.Name + "'}",
                    typeof(Order).Name,
                    typeof(Order).Name,
                    "{'" + Customer.IdProperty.Name + "'}",
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => orderType.AddForeignKey(customerFk1, customerKey, customerType)).Message);
        }

        [ConditionalFact]
        public void Adding_a_foreign_key_throws_if_duplicated_properties()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(
                new[]
                {
                    customerType.AddProperty(nameof(Customer.Id), typeof(int)),
                    customerType.AddProperty(nameof(Customer.AlternateId), typeof(int))
                });
            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);

            Assert.Equal(
                CoreStrings.DuplicatePropertyInList(
                    "{'" + Order.CustomerIdProperty.Name + "', '" + Order.CustomerIdProperty.Name + "'}",
                    Order.CustomerIdProperty.Name),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddForeignKey(new[] { customerFk1, customerFk1 }, customerKey, customerType)).Message);
        }

        [ConditionalFact]
        public void Adding_a_foreign_key_throws_if_properties_from_different_type()
        {
            var model = CreateModel();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var idProperty = entityType2.AddProperty(Order.IdProperty);
            var fkProperty = entityType2.AddProperty(Order.CustomerIdProperty);

            Assert.Equal(
                CoreStrings.ForeignKeyPropertiesWrongEntity("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => entityType1.AddForeignKey(new[] { fkProperty }, entityType2.AddKey(idProperty), entityType2)).Message);
        }

        [ConditionalFact]
        public void Adding_a_foreign_key_throws_if_properties_were_removed()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var key = entityType.AddKey(idProperty);
            var fkProperty = entityType.AddProperty("fk", typeof(int));
            entityType.RemoveProperty(fkProperty.Name);

            Assert.Equal(
                CoreStrings.ForeignKeyPropertiesWrongEntity("{'fk'}", typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddForeignKey(new[] { fkProperty }, key, entityType)).Message);
        }

        [ConditionalFact]
        public void Can_add_a_foreign_key_if_any_properties_are_part_of_inherited_key()
        {
            var model = CreateModel();
            var baseType = model.AddEntityType(typeof(BaseType));
            var idProperty = baseType.AddProperty(Customer.IdProperty);
            var idProperty2 = baseType.AddProperty("id2", typeof(int));
            var key = baseType.AddKey(new[] { idProperty, idProperty2 });
            var entityType = model.AddEntityType(typeof(Customer));
            entityType.BaseType = baseType;
            var fkProperty = entityType.AddProperty("fk", typeof(int));

            Assert.NotNull(entityType.AddForeignKey(new[] { fkProperty, idProperty }, key, entityType));
        }

        [ConditionalFact]
        public void Can_add_a_foreign_key_if_any_properties_are_part_of_inherited_key_with_value_generation()
        {
            var model = CreateModel();
            var baseType = model.AddEntityType(typeof(BaseType));
            var idProperty = baseType.AddProperty(Customer.IdProperty);
            idProperty.ValueGenerated = ValueGenerated.OnAdd;
            var idProperty2 = baseType.AddProperty("id2", typeof(int));
            var key = baseType.AddKey(new[] { idProperty, idProperty2 });
            var entityType = model.AddEntityType(typeof(Customer));
            entityType.BaseType = baseType;
            var fkProperty = entityType.AddProperty("fk", typeof(int));

            Assert.NotNull(entityType.AddForeignKey(new[] { fkProperty, idProperty }, key, entityType));
        }

        [ConditionalFact]
        public void Adding_a_foreign_key_throws_if_key_was_removed()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var key = entityType.AddKey(idProperty);
            entityType.RemoveKey(key.Properties);
            var fkProperty = entityType.AddProperty("fk", typeof(int));

            Assert.Equal(
                CoreStrings.ForeignKeyReferencedEntityKeyMismatch("{'" + Customer.IdProperty.Name + "'}", nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => entityType.AddForeignKey(new[] { fkProperty }, key, entityType)).Message);
        }

        [ConditionalFact]
        public void Adding_a_foreign_key_throws_if_related_entity_is_from_different_model()
        {
            var dependentEntityType = CreateModel().AddEntityType(typeof(Customer));
            var fkProperty = dependentEntityType.AddProperty(Customer.IdProperty);
            var principalEntityType = CreateModel().AddEntityType(typeof(Order));
            var idProperty = principalEntityType.AddProperty(Order.IdProperty);

            Assert.Equal(
                CoreStrings.EntityTypeModelMismatch(nameof(Customer), nameof(Order)),
                Assert.Throws<InvalidOperationException>(
                    () => dependentEntityType.AddForeignKey(
                        new[] { fkProperty }, principalEntityType.AddKey(idProperty), principalEntityType)).Message);
        }

        [ConditionalFact]
        public void Can_get_or_add_a_foreign_key()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.AddProperty(Customer.IdProperty);
            var customerKey = customerType.AddKey(idProperty);
            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);
            var customerFk2 = orderType.AddProperty("IdAgain", typeof(int));
            var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);

            var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);

            Assert.NotNull(fk2);
            Assert.NotEqual(fk1, fk2);
            Assert.Same(fk2, orderType.FindForeignKeys(customerFk2).Single());
            Assert.Same(fk2, orderType.FindForeignKey(customerFk2, customerKey, customerType));
            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());
        }

        private static IMutableModel BuildModel()
        {
            var model = CreateModel();

            var principalType = model.AddEntityType(typeof(PrincipalEntity));
            var property1 = principalType.AddProperty("PeeKay", typeof(int));
            principalType.SetPrimaryKey(property1);

            var dependentType = model.AddEntityType(typeof(DependentEntity));
            var property = dependentType.AddProperty("KayPee", typeof(int));
            dependentType.SetPrimaryKey(property);

            return model;
        }

        [ConditionalFact]
        public void Can_remove_foreign_keys()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));
            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);
            var customerFk2 = orderType.AddProperty("IdAgain", typeof(int));

            Assert.Null(orderType.RemoveForeignKey(new[] { customerFk2 }, customerKey, customerType));
            Assert.False(customerFk1.IsForeignKey());
            Assert.Empty(customerFk1.GetContainingForeignKeys());

            var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);
            var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);

            Assert.NotNull(((ForeignKey)fk1).Builder);
            Assert.NotNull(((ForeignKey)fk2).Builder);
            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());
            Assert.True(customerFk1.IsForeignKey());
            Assert.Same(fk1, customerFk1.GetContainingForeignKeys().Single());

            Assert.Same(fk1, orderType.RemoveForeignKey(fk1.Properties, fk1.PrincipalKey, fk1.PrincipalEntityType));
            Assert.Null(orderType.RemoveForeignKey(fk1.Properties, fk1.PrincipalKey, fk1.PrincipalEntityType));

            Assert.Equal(new[] { fk2 }, orderType.GetForeignKeys().ToArray());
            Assert.False(customerFk1.IsForeignKey());
            Assert.Empty(customerFk1.GetContainingForeignKeys());

            Assert.Same(fk2, orderType.RemoveForeignKey(new[] { customerFk2 }, customerKey, customerType));

            Assert.Null(((ForeignKey)fk1).Builder);
            Assert.Null(((ForeignKey)fk2).Builder);
            Assert.Empty(orderType.GetForeignKeys());
        }

        [ConditionalFact]
        public void Can_remove_a_foreign_key_if_it_is_referenced_from_a_navigation_in_the_model()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var customerFk = orderType.AddProperty(Order.CustomerIdProperty);
            var fk = orderType.AddForeignKey(customerFk, customerKey, customerType);

            fk.HasDependentToPrincipal(Order.CustomerProperty);
            fk.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.NotNull(orderType.RemoveForeignKey(fk.Properties, fk.PrincipalKey, fk.PrincipalEntityType));
            Assert.Empty(orderType.GetNavigations());
            Assert.Empty(customerType.GetNavigations());
        }

        [ConditionalFact]
        public void Foreign_keys_are_ordered_by_property_count_then_property_names()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.AddProperty(Customer.IdProperty);
            var nameProperty = customerType.AddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;
            var customerKey = customerType.AddKey(idProperty);
            var otherCustomerKey = customerType.AddKey(new[] { idProperty, nameProperty });

            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);
            var customerFk2 = orderType.AddProperty("IdAgain", typeof(int));
            var customerFk3A = orderType.AddProperty("OtherId1", typeof(int));
            var customerFk3B = orderType.AddProperty("OtherId2", typeof(string));
            var customerFk4B = orderType.AddProperty("OtherId3", typeof(string));

            var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);
            var fk4 = orderType.AddForeignKey(new[] { customerFk3A, customerFk4B }, otherCustomerKey, customerType);
            var fk3 = orderType.AddForeignKey(new[] { customerFk3A, customerFk3B }, otherCustomerKey, customerType);
            var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);

            Assert.True(new[] { fk1, fk2, fk3, fk4 }.SequenceEqual(orderType.GetForeignKeys()));
        }

        [ConditionalFact]
        public void Can_add_and_remove_navigations()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var customerNavigation = customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);
            var ordersNavigation = customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.Equal(nameof(Order.Customer), customerNavigation.Name);
            Assert.Same(orderType, customerNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
            Assert.True(customerNavigation.IsDependentToPrincipal());
            Assert.False(customerNavigation.IsCollection());
            Assert.Same(customerType, customerNavigation.GetTargetType());
            Assert.Same(customerNavigation, customerForeignKey.DependentToPrincipal);

            Assert.Equal(nameof(Customer.Orders), ordersNavigation.Name);
            Assert.Same(customerType, ordersNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, ordersNavigation.ForeignKey);
            Assert.False(ordersNavigation.IsDependentToPrincipal());
            Assert.True(ordersNavigation.IsCollection());
            Assert.Same(orderType, ordersNavigation.GetTargetType());
            Assert.Same(ordersNavigation, customerForeignKey.PrincipalToDependent);

            Assert.Same(customerNavigation, orderType.GetNavigations().Single());
            Assert.Same(ordersNavigation, customerType.GetNavigations().Single());

            Assert.Same(customerNavigation, customerForeignKey.HasDependentToPrincipal((string)null));
            Assert.Null(customerForeignKey.HasDependentToPrincipal((string)null));
            Assert.Empty(orderType.GetNavigations());
            Assert.Empty(((IEntityType)orderType).GetNavigations());

            Assert.Same(ordersNavigation, customerForeignKey.HasPrincipalToDependent((string)null));
            Assert.Null(customerForeignKey.HasPrincipalToDependent((string)null));
            Assert.Empty(customerType.GetNavigations());
            Assert.Empty(((IEntityType)customerType).GetNavigations());
        }

        [ConditionalFact]
        public void Can_add_new_navigations_or_get_existing_navigations()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);
            var customerNavigation = customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Equal(nameof(Order.Customer), customerNavigation.Name);
            Assert.Same(orderType, customerNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
            Assert.True(customerNavigation.IsDependentToPrincipal());
            Assert.False(customerNavigation.IsCollection());
            Assert.Same(customerType, customerNavigation.GetTargetType());

            Assert.Same(customerNavigation, orderType.FindNavigation(nameof(Order.Customer)));
            Assert.True(customerNavigation.IsDependentToPrincipal());
        }

        [ConditionalFact]
        public void Can_get_navigation_and_can_try_get_navigation()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);
            var customerNavigation = customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Same(customerNavigation, orderType.FindNavigation(nameof(Order.Customer)));
            Assert.Same(customerNavigation, orderType.FindNavigation(nameof(Order.Customer)));

            Assert.Null(orderType.FindNavigation("Nose"));
        }

        [ConditionalFact]
        public void Adding_a_new_navigation_with_a_name_that_conflicts_with_a_property_throws()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            orderType.AddProperty("Customer");

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("Customer", typeof(Order).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal("Customer")).Message);
        }

        [ConditionalFact]
        public void Adding_a_new_navigation_with_a_name_that_conflicts_with_a_service_property_throws()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            orderType.AddServiceProperty(Order.CustomerProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation(nameof(Order.Customer), nameof(Order), nameof(Order)),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal(nameof(Order.Customer))).Message);
        }

        [ConditionalFact]
        public void Can_add_a_navigation_to_shadow_entity()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty("Id", typeof(int)));

            var orderType = model.AddEntityType("Order");
            var foreignKeyProperty = orderType.AddProperty("CustomerId", typeof(int));
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.NotNull(customerForeignKey.HasDependentToPrincipal("Customer"));
        }

        [ConditionalFact]
        public void Adding_a_navigation_on_non_shadow_entity_type_pointing_to_a_shadow_entity_type_throws()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType("Customer");
            var customerKey = customerType.AddKey(customerType.AddProperty("Id", typeof(int)));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty("CustomerId", typeof(int));
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationToShadowEntity(nameof(Order.Customer), typeof(Order).Name, "Customer"),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty)).Message);
        }

        [ConditionalFact]
        public void Collection_navigation_properties_must_be_IEnumerables_of_the_target_type()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationCollectionWrongClrType(
                    nameof(Customer.NotCollectionOrders), typeof(Customer).Name, typeof(Order).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasPrincipalToDependent(Customer.NotCollectionOrdersProperty)).Message);
        }

        [ConditionalFact]
        public void Collection_navigation_properties_cannot_be_IEnumerables_of_derived_target_type()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationCollectionWrongClrType(
                    nameof(SpecialCustomer.DerivedOrders),
                    typeof(SpecialCustomer).Name,
                    typeof(IEnumerable<SpecialOrder>).ShortDisplayName(),
                    typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty)).Message);
        }

        [ConditionalFact]
        public void Collection_navigation_properties_can_be_IEnumerables_of_base_target_type()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var ordersNavigation = customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.Equal(nameof(Customer.Orders), ordersNavigation.Name);
            Assert.Same(customerType, ordersNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, ordersNavigation.ForeignKey);
            Assert.False(ordersNavigation.IsDependentToPrincipal());
            Assert.True(ordersNavigation.IsCollection());
            Assert.Same(orderType, ordersNavigation.GetTargetType());
            Assert.Same(ordersNavigation, customerForeignKey.PrincipalToDependent);
        }

        [ConditionalFact]
        public void Reference_navigation_properties_must_be_of_the_target_type()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationSingleWrongClrType("OrderCustomer", typeof(Order).Name, typeof(Order).Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal(Order.OrderCustomerProperty)).Message);
        }

        [ConditionalFact]
        public void Reference_navigation_properties_cannot_be_of_derived_type()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationSingleWrongClrType(
                    nameof(SpecialOrder.DerivedCustomer), typeof(SpecialOrder).Name, typeof(SpecialCustomer).Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal(SpecialOrder.DerivedCustomerProperty)).Message);
        }

        [ConditionalFact]
        public void Reference_navigation_properties_can_be_of_base_type()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var customerNavigation = customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Equal("Customer", customerNavigation.Name);
            Assert.Same(orderType, customerNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
            Assert.True(customerNavigation.IsDependentToPrincipal());
            Assert.False(customerNavigation.IsCollection());
            Assert.Same(customerType, customerNavigation.GetTargetType());
        }

        [ConditionalFact]
        public void Can_create_self_referencing_navigations()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(SelfRef));
            var fkProperty = entityType.AddProperty(SelfRef.ForeignKeyProperty);
            var principalKeyProperty = entityType.AddProperty(SelfRef.IdProperty);
            var referencedKey = entityType.SetPrimaryKey(principalKeyProperty);
            var fk = entityType.AddForeignKey(fkProperty, referencedKey, entityType);
            fk.IsUnique = true;

            var navigationToDependent = fk.HasPrincipalToDependent(SelfRef.SelfRef1Property);
            var navigationToPrincipal = fk.HasDependentToPrincipal(SelfRef.SelfRef2Property);

            Assert.Same(fk.PrincipalToDependent, navigationToDependent);
            Assert.Same(fk.DependentToPrincipal, navigationToPrincipal);
        }

        [ConditionalFact]
        public void Throws_when_adding_same_self_referencing_navigation_twice()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(SelfRef));
            var fkProperty = entityType.AddProperty(SelfRef.ForeignKeyProperty);
            var principalKeyProperty = entityType.AddProperty(SelfRef.IdProperty);
            var referencedKey = entityType.SetPrimaryKey(principalKeyProperty);
            var fk = entityType.AddForeignKey(fkProperty, referencedKey, entityType);
            fk.IsUnique = true;

            fk.HasPrincipalToDependent(SelfRef.SelfRef1Property);
            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation(nameof(SelfRef.SelfRef1), typeof(SelfRef).Name, typeof(SelfRef).Name),
                Assert.Throws<InvalidOperationException>(() => fk.HasDependentToPrincipal(SelfRef.SelfRef1Property)).Message);
        }

        [ConditionalFact]
        public void Navigations_are_ordered_by_name()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var customerForeignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(customerForeignKeyProperty, customerKey, customerType);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            var specialCustomerForeignKeyProperty = specialOrderType.AddProperty(Order.CustomerIdProperty);
            var specialCustomerForeignKey = specialOrderType.AddForeignKey(specialCustomerForeignKeyProperty, customerKey, customerType);

            var navigation2 = customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);
            var navigation1 = specialCustomerForeignKey.HasPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty);

            Assert.True(new[] { navigation1, navigation2 }.SequenceEqual(customerType.GetNavigations()));
            Assert.True(new[] { navigation1, navigation2 }.SequenceEqual(((IEntityType)customerType).GetNavigations()));
        }

        [ConditionalFact]
        public void Can_add_retrieve_and_remove_indexes()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Order));
            var property1 = entityType.AddProperty(Order.IdProperty);
            var property2 = entityType.AddProperty(Order.CustomerIdProperty);

            Assert.Empty(entityType.GetIndexes());
            Assert.Null(entityType.RemoveIndex(new[] { property1 }));
            Assert.False(property1.IsIndex());
            Assert.Empty(property1.GetContainingIndexes());

            var index1 = entityType.AddIndex(property1);

            Assert.Equal(1, index1.Properties.Count);
            Assert.Same(index1, entityType.FindIndex(property1));
            Assert.Same(index1, entityType.FindIndex(property1));
            Assert.Same(property1, index1.Properties[0]);

            var index2 = entityType.AddIndex(new[] { property1, property2 });

            Assert.NotNull(((IConventionIndex)index1).Builder);
            Assert.NotNull(((IConventionIndex)index2).Builder);
            Assert.Equal(2, index2.Properties.Count);
            Assert.Same(index2, entityType.FindIndex(new[] { property1, property2 }));
            Assert.Same(property1, index2.Properties[0]);
            Assert.Same(property2, index2.Properties[1]);
            Assert.True(property1.IsIndex());
            Assert.Equal(new[] { index1, index2 }, property1.GetContainingIndexes().ToArray());

            Assert.Equal(2, entityType.GetIndexes().Count());
            Assert.Same(index1, entityType.GetIndexes().First());
            Assert.Same(index2, entityType.GetIndexes().Last());

            Assert.Same(index1, entityType.RemoveIndex(index1.Properties));
            Assert.Null(entityType.RemoveIndex(index1.Properties));

            Assert.Single(entityType.GetIndexes());
            Assert.Same(index2, entityType.GetIndexes().Single());

            Assert.Same(index2, entityType.RemoveIndex(new[] { property1, property2 }));

            Assert.Null(((Index)index1).Builder);
            Assert.Null(((Index)index2).Builder);
            Assert.Empty(entityType.GetIndexes());
            Assert.False(property1.IsIndex());
            Assert.Empty(property1.GetContainingIndexes());
        }

        [ConditionalFact]
        public void AddIndex_throws_if_not_from_same_entity()
        {
            var model = CreateModel();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var property1 = entityType1.AddProperty(Customer.IdProperty);
            var property2 = entityType1.AddProperty(Customer.NameProperty);

            Assert.Equal(
                CoreStrings.IndexPropertiesWrongEntity(
                    "{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => entityType2.AddIndex(new[] { property1, property2 })).Message);
        }

        [ConditionalFact]
        public void AddIndex_throws_if_duplicate()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);
            entityType.AddIndex(new[] { property1, property2 });

            Assert.Equal(
                CoreStrings.DuplicateIndex(
                    "{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).Name,
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.AddIndex(new[] { property1, property2 })).Message);
        }

        [ConditionalFact]
        public void Can_add_and_remove_properties()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            Assert.Null(entityType.RemoveProperty("Id"));

            var property1 = entityType.AddProperty("Id", typeof(int));

            Assert.False(property1.IsShadowProperty());
            Assert.Equal("Id", property1.Name);
            Assert.Same(typeof(int), property1.ClrType);
            Assert.False(((IProperty)property1).IsConcurrencyToken);
            Assert.Same(entityType, property1.DeclaringEntityType);

            var property2 = entityType.AddProperty("Name", typeof(string));

            Assert.NotNull(((Property)property1).Builder);
            Assert.NotNull(((Property)property2).Builder);
            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.GetProperties()));

            Assert.Same(property1, entityType.RemoveProperty(property1.Name));
            Assert.Null(entityType.RemoveProperty(property1.Name));

            Assert.True(new[] { property2 }.SequenceEqual(entityType.GetProperties()));

            Assert.Same(property2, entityType.RemoveProperty("Name"));

            Assert.Null(((Property)property1).Builder);
            Assert.Null(((Property)property2).Builder);
            Assert.Empty(entityType.GetProperties());
        }

        [ConditionalFact]
        public void Can_add_new_properties_or_get_existing_properties_using_PropertyInfo_or_name()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));

            var idProperty = entityType.AddProperty("Id", typeof(int));

            Assert.False(idProperty.IsShadowProperty());
            Assert.Equal("Id", idProperty.Name);
            Assert.Same(typeof(int), idProperty.ClrType);
            Assert.Same(entityType, idProperty.DeclaringEntityType);

            Assert.Same(idProperty, entityType.FindProperty(Customer.IdProperty));
            Assert.Same(idProperty, entityType.FindProperty("Id"));
            Assert.False(idProperty.IsShadowProperty());

            var nameProperty = entityType.AddProperty("Name");

            Assert.False(nameProperty.IsShadowProperty());
            Assert.Equal("Name", nameProperty.Name);
            Assert.Same(typeof(string), nameProperty.ClrType);
            Assert.Same(entityType, nameProperty.DeclaringEntityType);

            Assert.Same(nameProperty, entityType.FindProperty(Customer.NameProperty));
            Assert.Same(nameProperty, entityType.FindProperty("Name"));
            Assert.False(nameProperty.IsShadowProperty());

            Assert.True(new[] { idProperty, nameProperty }.SequenceEqual(entityType.GetProperties()));
        }

        [ConditionalFact]
        public void Can_add_new_properties_using_name_of_property_in_base_class()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(HiddenField));

            var property = entityType.AddProperty("Raisin");

            Assert.False(property.IsShadowProperty());
            Assert.Equal("Raisin", property.Name);
            Assert.Same(typeof(string), property.ClrType);
            Assert.Same(entityType, property.DeclaringEntityType);
            Assert.Same(HiddenFieldBase.RaisinProperty, property.PropertyInfo);
            Assert.Null(property.FieldInfo);
        }

        [ConditionalFact]
        public void Can_add_new_properties_using_name_of_field_in_base_class()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(HiddenField));

            var property = entityType.AddProperty("_date");

            Assert.False(property.IsShadowProperty());
            Assert.Equal("_date", property.Name);
            Assert.Same(typeof(string), property.ClrType);
            Assert.Same(entityType, property.DeclaringEntityType);
            Assert.Same(HiddenFieldBase.DateField, property.FieldInfo);
            Assert.Null(property.PropertyInfo);
        }

        private class HiddenField : HiddenFieldBase
        {
            public int Id { get; set; }
        }

        public class HiddenFieldBase
        {
            public static readonly FieldInfo DateField
                = typeof(HiddenFieldBase).GetRuntimeFields().Single(f => f.Name == nameof(_date));

            public static readonly PropertyInfo RaisinProperty
                = typeof(HiddenFieldBase).GetRuntimeProperties().Single(p => p.Name == nameof(Raisin));

            private string _date;
            private string Raisin { get; set; }

            public DateTime Date
            {
                get => DateTime.Parse(_date);
                set => _date = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        [ConditionalFact]
        public void AddProperty_throws_if_shadow_entity_type()
        {
            var entityType = CreateModel().AddEntityType("Customer");

            Assert.Equal(
                CoreStrings.ClrPropertyOnShadowEntity(nameof(Customer.Name), "Customer"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        entityType.AddProperty(Customer.NameProperty)).Message);
        }

        [ConditionalFact]
        public void AddProperty_throws_if_no_clr_property_or_field()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.NoPropertyType("_foo", nameof(Customer)),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.AddProperty("_foo")).Message);
        }

        [ConditionalFact]
        public void AddProperty_throws_if_clr_type_does_not_match()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.PropertyWrongClrType(
                    nameof(Customer.Name), nameof(Customer), typeof(string).DisplayName(), typeof(int).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        entityType.AddProperty(nameof(Customer.Name), typeof(int))).Message);
        }

        [ConditionalFact]
        public void AddProperty_throws_if_name_does_not_match()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.PropertyWrongName(
                    nameof(Customer.Id), nameof(Customer), nameof(Customer.Name)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        entityType.AddProperty(nameof(Customer.Id), typeof(int), Customer.NameProperty)).Message);
        }

        [ConditionalFact]
        public void AddProperty_ignores_clr_type_if_implicit()
        {
            var entityType = (IConventionEntityType)CreateModel().AddEntityType(typeof(Customer));

            var property = entityType.AddProperty(nameof(Customer.Name), typeof(int), setTypeConfigurationSource: false);

            Assert.Equal(typeof(string), property.ClrType);
        }

        [ConditionalFact]
        public void Cannot_remove_property_when_used_by_primary_key()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.AddProperty(Customer.IdProperty);

            entityType.SetPrimaryKey(property);

            Assert.Equal(
                CoreStrings.PropertyInUseKey("Id", typeof(Customer).Name, "{'Id'}"),
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property.Name)).Message);
        }

        [ConditionalFact]
        public void Cannot_remove_property_when_used_by_non_primary_key()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.AddProperty(Customer.IdProperty);

            entityType.AddKey(property);

            Assert.Equal(
                CoreStrings.PropertyInUseKey("Id", typeof(Customer).Name, "{'Id'}"),
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property.Name)).Message);
        }

        [ConditionalFact]
        public void Cannot_remove_property_when_used_by_foreign_key()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerPk = customerType.SetPrimaryKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var customerFk = orderType.AddProperty(Order.CustomerIdProperty);
            orderType.AddForeignKey(customerFk, customerPk, customerType);

            Assert.Equal(
                CoreStrings.PropertyInUseForeignKey("CustomerId", typeof(Order).Name, "{'CustomerId'}", typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(() => orderType.RemoveProperty(customerFk.Name)).Message);
        }

        [ConditionalFact]
        public void Cannot_remove_property_when_used_by_an_index()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.AddProperty(Customer.IdProperty);

            entityType.AddIndex(property);

            Assert.Equal(
                CoreStrings.PropertyInUseIndex("Id", typeof(Customer).Name, "{'Id'}", typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property.Name)).Message);
        }

        [ConditionalFact]
        public void Properties_are_ordered_by_name()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));

            var property2 = entityType.AddProperty(Customer.NameProperty);
            var property1 = entityType.AddProperty(Customer.IdProperty);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.GetProperties()));
        }

        [ConditionalFact]
        public void Primary_key_properties_precede_others()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));

            var aProperty = entityType.AddProperty("A", typeof(int));
            var pkProperty = entityType.AddProperty(Customer.IdProperty);

            entityType.SetPrimaryKey(pkProperty);

            Assert.True(new[] { pkProperty, aProperty }.SequenceEqual(entityType.GetProperties()));
        }

        [ConditionalFact]
        public void Composite_primary_key_properties_are_listed_in_key_order()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType("CompositeKeyType");

            var aProperty = entityType.AddProperty("A", typeof(int));
            var pkProperty2 = entityType.AddProperty("aPK", typeof(int));
            var pkProperty1 = entityType.AddProperty("bPK", typeof(int));

            entityType.SetPrimaryKey(new[] { pkProperty1, pkProperty2 });

            Assert.True(new[] { pkProperty1, pkProperty2, aProperty }.SequenceEqual(entityType.GetProperties()));
        }

        [ConditionalFact]
        public void Properties_on_base_type_are_listed_before_derived_properties()
        {
            var model = CreateModel();

            var parentType = model.AddEntityType("Parent");
            var property2 = parentType.AddProperty("D", typeof(int));
            var property1 = parentType.AddProperty("C", typeof(int));

            var childType = model.AddEntityType("Child");
            var property4 = childType.AddProperty("B", typeof(int));
            var property3 = childType.AddProperty("A", typeof(int));
            childType.BaseType = parentType;

            Assert.True(new[] { property1, property2, property3, property4 }.SequenceEqual(childType.GetProperties()));
        }

        [ConditionalFact]
        public void Properties_are_properly_ordered_when_primary_key_changes()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));

            var aProperty = entityType.AddProperty("A", typeof(int));
            var bProperty = entityType.AddProperty("B", typeof(int));

            entityType.SetPrimaryKey(bProperty);

            Assert.True(new[] { bProperty, aProperty }.SequenceEqual(entityType.GetProperties()));

            entityType.SetPrimaryKey(aProperty);

            Assert.True(new[] { aProperty, bProperty }.SequenceEqual(entityType.GetProperties()));
        }

        [ConditionalFact]
        public void Can_get_property_and_can_try_get_property()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.AddProperty(Customer.IdProperty);

            Assert.Same(property, entityType.FindProperty(Customer.IdProperty));
            Assert.Same(property, entityType.FindProperty("Id"));
            Assert.Same(property, entityType.FindProperty(Customer.IdProperty));
            Assert.Same(property, entityType.FindProperty("Id"));

            Assert.Null(entityType.FindProperty("Nose"));
        }

        [ConditionalFact]
        public void Shadow_properties_have_CLR_flag_set_to_false()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));

            entityType.AddProperty(Customer.NameProperty);
            entityType.AddProperty(Customer.IdProperty);
            entityType.AddProperty("Mane_", typeof(int));

            Assert.False(entityType.FindProperty("Name").IsShadowProperty());
            Assert.False(entityType.FindProperty("Id").IsShadowProperty());
            Assert.True(entityType.FindProperty("Mane_").IsShadowProperty());
        }

        [ConditionalFact]
        public void Adding_a_new_property_with_a_name_that_already_exists_throws()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            entityType.AddProperty(Customer.IdProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("Id", typeof(Customer).Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddProperty("Id")).Message);
        }

        [ConditionalFact]
        public void Adding_a_new_property_with_a_name_that_conflicts_with_a_navigation_throws()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("Customer", typeof(Order).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(() => orderType.AddProperty("Customer")).Message);
        }

        [ConditionalFact]
        public void Adding_a_new_property_with_a_name_that_conflicts_with_a_service_property_throws()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation(nameof(Order.Customer), nameof(Order), nameof(Order)),
                Assert.Throws<InvalidOperationException>(() => orderType.AddServiceProperty(Order.CustomerProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_a_new_service_property_with_a_name_that_conflicts_with_a_property_throws()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            entityType.AddProperty(Customer.OrdersProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation(nameof(Customer.Orders), nameof(Customer), nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => entityType.AddServiceProperty(Customer.OrdersProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_a_new_service_property_with_a_name_that_conflicts_with_a_navigation_throws()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation(nameof(Order.Customer), nameof(Order), nameof(Order)),
                Assert.Throws<InvalidOperationException>(() => orderType.AddServiceProperty(Order.CustomerProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_a_new_service_property_with_a_name_that_already_exists_throws()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            entityType.AddServiceProperty(Customer.OrdersProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation(nameof(Customer.Orders), nameof(Customer), nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => entityType.AddServiceProperty(Customer.OrdersProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_a_new_service_property_with_a_type_that_already_exists_throws()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            entityType.AddServiceProperty(Customer.OrdersProperty);

            Assert.Equal(
                CoreStrings.DuplicateServicePropertyType(
                    nameof(Customer.MoreOrders),
                    "ICollection<Order>",
                    nameof(Customer),
                    nameof(Customer.Orders),
                    nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => entityType.AddServiceProperty(Customer.MoreOrdersProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_a_CLR_property_from_wrong_CLR_type_throws()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.PropertyWrongEntityClrType(Order.CustomerIdProperty.Name, typeof(Customer).Name, typeof(Order).Name),
                Assert.Throws<ArgumentException>(() => entityType.AddProperty(Order.CustomerIdProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_a_CLR_property_to_shadow_type_throws()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer).Name);

            Assert.Equal(
                CoreStrings.ClrPropertyOnShadowEntity(Order.CustomerIdProperty.Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddProperty(Order.CustomerIdProperty)).Message);
        }

        [ConditionalFact]
        public void Can_get_property_indexes()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));

            entityType.AddProperty(Customer.NameProperty);
            entityType.AddProperty("Id_", typeof(int));
            entityType.AddProperty("Mane_", typeof(int));

            ((Model)entityType.Model).FinalizeModel();

            Assert.Equal(0, entityType.FindProperty("Id_").GetIndex());
            Assert.Equal(1, entityType.FindProperty("Mane_").GetIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetIndex());

            Assert.Equal(0, entityType.FindProperty("Id_").GetShadowIndex());
            Assert.Equal(1, entityType.FindProperty("Mane_").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetShadowIndex());

            Assert.Equal(2, entityType.ShadowPropertyCount());
        }

        [ConditionalFact]
        public void Indexes_for_derived_types_are_calculated_correctly()
        {
            using (var context = new Levels())
            {
                var type = context.Model.FindEntityType(typeof(Level1));

                Assert.Equal(0, type.FindProperty("Id").GetIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetIndex());
                Assert.Equal(2, type.FindProperty("Prop1").GetIndex());
                Assert.Equal(0, type.FindNavigation("Level1Collection").GetIndex());
                Assert.Equal(1, type.FindNavigation("Level1Reference").GetIndex());

                Assert.Equal(-1, type.FindProperty("Id").GetShadowIndex());
                Assert.Equal(0, type.FindProperty("Level1ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetShadowIndex());

                Assert.Equal(0, type.FindProperty("Id").GetOriginalValueIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetOriginalValueIndex());
                Assert.Equal(2, type.FindProperty("Prop1").GetOriginalValueIndex());

                Assert.Equal(0, type.FindProperty("Id").GetRelationshipIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetRelationshipIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetRelationshipIndex());
                Assert.Equal(2, type.FindNavigation("Level1Collection").GetRelationshipIndex());
                Assert.Equal(3, type.FindNavigation("Level1Reference").GetRelationshipIndex());

                Assert.Equal(0, type.FindProperty("Id").GetStoreGeneratedIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level1Collection").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level1Reference").GetStoreGeneratedIndex());

                Assert.Equal(4, type.PropertyCount());
                Assert.Equal(2, type.NavigationCount());
                Assert.Equal(2, type.ShadowPropertyCount());
                Assert.Equal(4, type.OriginalValueCount());
                Assert.Equal(4, type.RelationshipPropertyCount());
                Assert.Equal(2, type.StoreGeneratedCount());

                type = context.Model.FindEntityType(typeof(Level2));

                Assert.Equal(0, type.FindProperty("Id").GetIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetIndex());
                Assert.Equal(2, type.FindProperty("Prop1").GetIndex());
                Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetIndex());
                Assert.Equal(5, type.FindProperty("Prop2").GetIndex());
                Assert.Equal(0, type.FindNavigation("Level1Collection").GetIndex());
                Assert.Equal(1, type.FindNavigation("Level1Reference").GetIndex());
                Assert.Equal(2, type.FindNavigation("Level2Collection").GetIndex());
                Assert.Equal(3, type.FindNavigation("Level2Reference").GetIndex());

                Assert.Equal(-1, type.FindProperty("Id").GetShadowIndex());
                Assert.Equal(0, type.FindProperty("Level1ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetShadowIndex());
                Assert.Equal(2, type.FindProperty("Level2ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop2").GetShadowIndex());

                Assert.Equal(0, type.FindProperty("Id").GetOriginalValueIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetOriginalValueIndex());
                Assert.Equal(2, type.FindProperty("Prop1").GetOriginalValueIndex());
                Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetOriginalValueIndex());
                Assert.Equal(5, type.FindProperty("Prop2").GetOriginalValueIndex());

                Assert.Equal(0, type.FindProperty("Id").GetRelationshipIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetRelationshipIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetRelationshipIndex());
                Assert.Equal(2, type.FindNavigation("Level1Collection").GetRelationshipIndex());
                Assert.Equal(3, type.FindNavigation("Level1Reference").GetRelationshipIndex());
                Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetRelationshipIndex());
                Assert.Equal(-1, type.FindProperty("Prop2").GetRelationshipIndex());
                Assert.Equal(5, type.FindNavigation("Level2Collection").GetRelationshipIndex());
                Assert.Equal(6, type.FindNavigation("Level2Reference").GetRelationshipIndex());

                Assert.Equal(0, type.FindProperty("Id").GetStoreGeneratedIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetStoreGeneratedIndex());
                Assert.Equal(2, type.FindProperty("Level2ReferenceId").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindProperty("Prop2").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level1Collection").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level1Reference").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level2Collection").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level2Reference").GetStoreGeneratedIndex());

                Assert.Equal(6, type.PropertyCount());
                Assert.Equal(4, type.NavigationCount());
                Assert.Equal(3, type.ShadowPropertyCount());
                Assert.Equal(6, type.OriginalValueCount());
                Assert.Equal(7, type.RelationshipPropertyCount());
                Assert.Equal(3, type.StoreGeneratedCount());

                type = context.Model.FindEntityType(typeof(Level3));

                Assert.Equal(0, type.FindProperty("Id").GetIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetIndex());
                Assert.Equal(2, type.FindProperty("Prop1").GetIndex());
                Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetIndex());
                Assert.Equal(5, type.FindProperty("Prop2").GetIndex());
                Assert.Equal(6, type.FindProperty("Level3ReferenceId").GetIndex());
                Assert.Equal(7, type.FindProperty("Prop3").GetIndex());
                Assert.Equal(0, type.FindNavigation("Level1Collection").GetIndex());
                Assert.Equal(1, type.FindNavigation("Level1Reference").GetIndex());
                Assert.Equal(2, type.FindNavigation("Level2Collection").GetIndex());
                Assert.Equal(3, type.FindNavigation("Level2Reference").GetIndex());
                Assert.Equal(4, type.FindNavigation("Level3Collection").GetIndex());
                Assert.Equal(5, type.FindNavigation("Level3Reference").GetIndex());

                Assert.Equal(-1, type.FindProperty("Id").GetShadowIndex());
                Assert.Equal(0, type.FindProperty("Level1ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetShadowIndex());
                Assert.Equal(2, type.FindProperty("Level2ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop2").GetShadowIndex());
                Assert.Equal(3, type.FindProperty("Level3ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop3").GetShadowIndex());

                Assert.Equal(0, type.FindProperty("Id").GetOriginalValueIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetOriginalValueIndex());
                Assert.Equal(2, type.FindProperty("Prop1").GetOriginalValueIndex());
                Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetOriginalValueIndex());
                Assert.Equal(5, type.FindProperty("Prop2").GetOriginalValueIndex());
                Assert.Equal(6, type.FindProperty("Level3ReferenceId").GetOriginalValueIndex());
                Assert.Equal(7, type.FindProperty("Prop3").GetOriginalValueIndex());

                Assert.Equal(0, type.FindProperty("Id").GetRelationshipIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetRelationshipIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetRelationshipIndex());
                Assert.Equal(2, type.FindNavigation("Level1Collection").GetRelationshipIndex());
                Assert.Equal(3, type.FindNavigation("Level1Reference").GetRelationshipIndex());
                Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetRelationshipIndex());
                Assert.Equal(-1, type.FindProperty("Prop2").GetRelationshipIndex());
                Assert.Equal(5, type.FindNavigation("Level2Collection").GetRelationshipIndex());
                Assert.Equal(6, type.FindNavigation("Level2Reference").GetRelationshipIndex());
                Assert.Equal(7, type.FindProperty("Level3ReferenceId").GetRelationshipIndex());
                Assert.Equal(-1, type.FindProperty("Prop3").GetRelationshipIndex());
                Assert.Equal(8, type.FindNavigation("Level3Collection").GetRelationshipIndex());
                Assert.Equal(9, type.FindNavigation("Level3Reference").GetRelationshipIndex());

                Assert.Equal(0, type.FindProperty("Id").GetStoreGeneratedIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetStoreGeneratedIndex());
                Assert.Equal(2, type.FindProperty("Level2ReferenceId").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindProperty("Prop2").GetStoreGeneratedIndex());
                Assert.Equal(3, type.FindProperty("Level3ReferenceId").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindProperty("Prop3").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level1Collection").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level1Reference").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level2Collection").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level2Reference").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level3Collection").GetStoreGeneratedIndex());
                Assert.Equal(-1, type.FindNavigation("Level3Reference").GetStoreGeneratedIndex());

                Assert.Equal(8, type.PropertyCount());
                Assert.Equal(6, type.NavigationCount());
                Assert.Equal(4, type.ShadowPropertyCount());
                Assert.Equal(8, type.OriginalValueCount());
                Assert.Equal(10, type.RelationshipPropertyCount());
                Assert.Equal(4, type.StoreGeneratedCount());
            }
        }

        private class Levels : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Level1>().HasOne(e => e.Level1Reference).WithMany(e => e.Level1Collection);
                modelBuilder.Entity<Level2>().HasOne(e => e.Level2Reference).WithMany(e => e.Level2Collection);
                modelBuilder.Entity<Level3>().HasOne(e => e.Level3Reference).WithMany(e => e.Level3Collection);

                modelBuilder.Entity<Level1>().HasDiscriminator<string>("Z");
            }
        }

        [ConditionalFact]
        public void Indexes_are_ordered_by_property_count_then_property_names()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.AddProperty(Customer.IdProperty);
            var nameProperty = customerType.AddProperty(Customer.NameProperty);
            var otherProperty = customerType.AddProperty("OtherProperty", typeof(string));

            var i2 = customerType.AddIndex(nameProperty);
            var i4 = customerType.AddIndex(new[] { idProperty, otherProperty });
            var i3 = customerType.AddIndex(new[] { idProperty, nameProperty });
            var i1 = customerType.AddIndex(idProperty);

            model.FinalizeModel();

            Assert.True(new[] { i1, i2, i3, i4 }.SequenceEqual(customerType.GetIndexes()));
        }

        [ConditionalFact]
        public void Adding_inheritance_to_weak_entity_types_throws()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var baseType = model.AddEntityType(typeof(BaseType), nameof(Customer.Orders), customerType);
            var orderType = model.AddEntityType(typeof(Order), nameof(Customer.Orders), customerType);
            var derivedType = model.AddEntityType(typeof(SpecialOrder), nameof(Customer.Orders), customerType);

            Assert.Equal(
                CoreStrings.WeakDerivedType(
                    nameof(Customer) + "." + nameof(Customer.Orders) + "#" + nameof(Order)),
                Assert.Throws<InvalidOperationException>(() => orderType.BaseType = baseType).Message);
            Assert.Equal(
                CoreStrings.WeakDerivedType(
                    nameof(Customer) + "." + nameof(Customer.Orders) + "#" + nameof(SpecialOrder)),
                Assert.Throws<InvalidOperationException>(() => derivedType.BaseType = orderType).Message);
        }

        [ConditionalFact]
        public void Adding_non_delegated_inheritance_to_delegated_identity_definition_entity_types_throws()
        {
            var model = CreateModel();
            var customerType = model.AddEntityType(typeof(Customer));
            var baseType = model.AddEntityType(typeof(BaseType));
            var orderType = model.AddEntityType(typeof(Order), nameof(Customer.Orders), customerType);
            var derivedType = model.AddEntityType(typeof(SpecialOrder));

            Assert.Equal(
                CoreStrings.WeakDerivedType(
                    nameof(Customer) + "." + nameof(Customer.Orders) + "#" + nameof(Order)),
                Assert.Throws<InvalidOperationException>(() => orderType.BaseType = baseType).Message);
            Assert.Equal(
                CoreStrings.WeakBaseType(
                    typeof(SpecialOrder).DisplayName(fullName: false),
                    nameof(Customer) + "." + nameof(Customer.Orders) + "#" + nameof(Order)),
                Assert.Throws<InvalidOperationException>(() => derivedType.BaseType = orderType).Message);
        }

        [ConditionalFact]
        public void Change_tracking_from_model_is_used_by_default_regardless_of_CLR_type()
        {
            var model = BuildFullNotificationEntityModel();
            var entityType = model.FindEntityType(typeof(FullNotificationEntity));

            Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

            model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public void Change_tracking_from_model_is_used_by_default_for_shadow_entities()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType("Z'ha'dum");

            Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

            model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public void Change_tracking_can_be_set_to_anything_for_full_notification_entities()
        {
            var model = BuildFullNotificationEntityModel();
            model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

            var entityType = model.FindEntityType(typeof(FullNotificationEntity));

            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());

            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
            Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());

            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.GetChangeTrackingStrategy());

            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues, entityType.GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public void Change_tracking_can_be_set_to_snapshot_or_changed_only_for_changed_only_entities()
        {
            var model = CreateModel();
            model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));

            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.GetChangeTrackingStrategy());

            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
            Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());

            Assert.Equal(
                CoreStrings.ChangeTrackingInterfaceMissing(
                    "ChangedOnlyEntity", "ChangingAndChangedNotifications", "INotifyPropertyChanging"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications)).Message);

            Assert.Equal(
                CoreStrings.ChangeTrackingInterfaceMissing(
                    "ChangedOnlyEntity", "ChangingAndChangedNotificationsWithOriginalValues", "INotifyPropertyChanging"),
                Assert.Throws<InvalidOperationException>(
                        () => entityType.SetChangeTrackingStrategy(
                            ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues))
                    .Message);
        }

        [ConditionalFact]
        public void Change_tracking_can_be_set_to_snapshot_only_for_non_notifying_entities()
        {
            var model = CreateModel();
            model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.GetChangeTrackingStrategy());

            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
            Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

            Assert.Equal(
                CoreStrings.ChangeTrackingInterfaceMissing("Customer", "ChangedNotifications", "INotifyPropertyChanged"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications)).Message);

            Assert.Equal(
                CoreStrings.ChangeTrackingInterfaceMissing("Customer", "ChangingAndChangedNotifications", "INotifyPropertyChanged"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications)).Message);

            Assert.Equal(
                CoreStrings.ChangeTrackingInterfaceMissing(
                    "Customer", "ChangingAndChangedNotificationsWithOriginalValues", "INotifyPropertyChanged"),
                Assert.Throws<InvalidOperationException>(
                        () => entityType.SetChangeTrackingStrategy(
                            ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues))
                    .Message);
        }

        [ConditionalFact]
        public void Entity_type_with_deeply_nested_owned_weak_types_builds_correctly()
        {
            using (var context = new RejectionContext(nameof(RejectionContext)))
            {
                var entityTypes = context.Model.GetEntityTypes();

                Assert.Equal(
                    new[]
                    {
                        "Application",
                        "ApplicationVersion",
                        "Rejection",
                        "Application.Attitude#Attitude",
                        "ApplicationVersion.Attitude#Attitude",
                        "Rejection.FirstTest#FirstTest",
                        "Application.Attitude#Attitude.FirstTest#FirstTest",
                        "ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest",
                        "Rejection.FirstTest#FirstTest.Tester#SpecialistStaff",
                        "Application.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff",
                        "ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff"
                    }, entityTypes.Select(e => e.DisplayName()).ToList());
            }
        }

        //
        //          ApplicationVersion             Application
        //            |            |                   |
        //         Attitude`     Rejection          Attitude``
        //            |            |                   |
        //         FirstTest`    FirstTest``       FirstTest```
        //            |            |                   |
        // SpecialistStaff`    SpecialistStaff``   SpecialistStaff```
        //
        // ApplicationVersion   = ApplicationVersion
        // Attitude`            = ApplicationVersion.Attitude#Attitude
        // FirstTest`           = Application.Attitude#Attitude.FirstTest#FirstTest
        // SpecialistStaff`     = ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff
        //
        // Rejection            = Rejection
        // FirstTest``          = Rejection.FirstTest#FirstTest
        // SpecialistStaff``    = Rejection.FirstTest#FirstTest.Tester#SpecialistStaff
        //
        // Application          = Application
        // Attitude``           = Application.Attitude#Attitude
        // FistTest```          = ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest
        // SpecialistStaff```   = Application.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff
        //

        private class Application
        {
            public Guid Id { get; protected set; }
            public Attitude Attitude { get; set; }
            public Rejection Rejection { get; set; }
        }

        private class ApplicationVersion
        {
            public Guid Id { get; protected set; }
            public Attitude Attitude { get; set; }
        }

        private class Rejection
        {
            public FirstTest FirstTest { get; set; }
        }

        private class Attitude
        {
            public FirstTest FirstTest { get; set; }
        }

        private class FirstTest
        {
            public SpecialistStaff Tester { get; set; }
        }

        private class SpecialistStaff
        {
        }

        private class RejectionContext : DbContext
        {
            private readonly string _databaseName;

            public RejectionContext(string databaseName)
            {
                _databaseName = databaseName;
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase(_databaseName);

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                List<string> GetTypeNames()
                    => modelBuilder.Model.GetEntityTypes().Select(e => e.DisplayName()).ToList();

                modelBuilder.Entity<Application>(
                    entity =>
                    {
                        entity.OwnsOne(
                            x => x.Attitude,
                            amb =>
                            {
                                amb.OwnsOne(
                                    x => x.FirstTest, mb =>
                                    {
                                        mb.OwnsOne(a => a.Tester);
                                    });
                            });

                        entity.OwnsOne(
                            x => x.Rejection,
                            amb =>
                            {
                                amb.OwnsOne(
                                    x => x.FirstTest, mb =>
                                    {
                                        mb.OwnsOne(a => a.Tester);
                                    });
                            });
                    });

                Assert.Equal(
                    new[]
                    {
                        "Application",
                        "Attitude",
                        "Rejection",
                        "Attitude.FirstTest#FirstTest", // FirstTest is weak
                        "Rejection.FirstTest#FirstTest", // FirstTest is weak
                        "Attitude.FirstTest#FirstTest.Tester#SpecialistStaff", // SpecialistStaff is weak
                        "Rejection.FirstTest#FirstTest.Tester#SpecialistStaff" // SpecialistStaff is weak
                    }, GetTypeNames());

                modelBuilder.Entity<ApplicationVersion>(
                    entity =>
                    {
                        Assert.Equal(
                            new[]
                            {
                                "Application",
                                "ApplicationVersion",
                                "Attitude",
                                "Rejection",
                                "Attitude.FirstTest#FirstTest",
                                "Rejection.FirstTest#FirstTest",
                                "Attitude.FirstTest#FirstTest.Tester#SpecialistStaff",
                                "Rejection.FirstTest#FirstTest.Tester#SpecialistStaff"
                            }, GetTypeNames());

                        entity.OwnsOne(
                            x => x.Attitude,
                            amb =>
                            {
                                var typeNames = GetTypeNames();
                                Assert.Equal(
                                    new[]
                                    {
                                        "Application",
                                        "ApplicationVersion",
                                        "Rejection",
                                        "Application.Attitude#Attitude", // Attitude becomes weak
                                        "ApplicationVersion.Attitude#Attitude", // Attitude becomes weak
                                        "Rejection.FirstTest#FirstTest",
                                        "Application.Attitude#Attitude.FirstTest#FirstTest", // Attitude becomes weak
                                        "ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest", // Attitude becomes weak
                                        "Rejection.FirstTest#FirstTest.Tester#SpecialistStaff",
                                        "Application.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff", // Attitude becomes weak
                                        "ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff" // Attitude becomes weak
                                    }, typeNames);

                                amb.OwnsOne(
                                    x => x.FirstTest, mb =>
                                    {
                                        mb.OwnsOne(a => a.Tester);
                                    });
                            });
                    });

                Assert.Equal(
                    new[]
                    {
                        "Application",
                        "ApplicationVersion",
                        "Rejection",
                        "Application.Attitude#Attitude",
                        "ApplicationVersion.Attitude#Attitude",
                        "Rejection.FirstTest#FirstTest",
                        "Application.Attitude#Attitude.FirstTest#FirstTest",
                        "ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest",
                        "Rejection.FirstTest#FirstTest.Tester#SpecialistStaff",
                        "Application.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff",
                        "ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff"
                    }, GetTypeNames());
            }
        }

        [ConditionalFact]
        public void All_properties_have_original_value_indexes_when_using_snapshot_change_tracking()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
            ((Model)entityType.Model).FinalizeModel();

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(3, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(4, entityType.OriginalValueCount());
        }

        [ConditionalFact]
        public void All_relationship_properties_have_relationship_indexes_when_using_snapshot_change_tracking()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
            ((Model)entityType.Model).FinalizeModel();

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(3, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(4, entityType.RelationshipPropertyCount());
        }

        [ConditionalFact]
        public void All_properties_have_original_value_indexes_when_using_changed_only_tracking()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
            ((Model)entityType.Model).FinalizeModel();

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(3, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(4, entityType.OriginalValueCount());
        }

        [ConditionalFact]
        public void Collections_dont_have_relationship_indexes_when_using_changed_only_change_tracking()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
            ((Model)entityType.Model).FinalizeModel();

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(3, entityType.RelationshipPropertyCount());
        }

        [ConditionalFact]
        public void Only_concurrency_and_key_properties_have_original_value_indexes_when_using_full_notifications()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            ((Model)entityType.Model).FinalizeModel();

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(3, entityType.OriginalValueCount());
        }

        [ConditionalFact]
        public void Collections_dont_have_relationship_indexes_when_using_full_notifications()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            ((Model)entityType.Model).FinalizeModel();

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(3, entityType.RelationshipPropertyCount());
        }

        [ConditionalFact]
        public void All_properties_have_original_value_indexes_when_full_notifications_with_original_values()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
            ((Model)entityType.Model).FinalizeModel();

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(3, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(4, entityType.OriginalValueCount());
        }

        [ConditionalFact]
        public void Collections_dont_have_relationship_indexes_when_full_notifications_with_original_values()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
            ((Model)entityType.Model).FinalizeModel();

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(3, entityType.RelationshipPropertyCount());
        }

        private readonly IMutableModel _model = BuildModel();

        private IMutableEntityType DependentType => _model.FindEntityType(typeof(DependentEntity));

        private IMutableEntityType PrincipalType => _model.FindEntityType(typeof(PrincipalEntity));

        private class PrincipalEntity
        {
            public int PeeKay { get; set; }
            public IEnumerable<DependentEntity> AnotherNav { get; set; }
        }

        private class DependentEntity
        {
            public PrincipalEntity Navigator { get; set; }
            public PrincipalEntity AnotherNav { get; set; }
        }

        private class A
        {
            public static readonly PropertyInfo EProperty = typeof(A).GetProperty("E");
            public static readonly PropertyInfo GProperty = typeof(A).GetProperty("G");

            public string E { get; set; }
            public string G { get; set; }
        }

        private class B : A
        {
            public static readonly PropertyInfo FProperty = typeof(B).GetProperty("F");
            public static readonly PropertyInfo HProperty = typeof(B).GetProperty("H");

            public string F { get; set; }
            public string H { get; set; }
        }

        private class C : A
        {
            public static readonly PropertyInfo FProperty = typeof(C).GetProperty("F");
            public static readonly PropertyInfo HProperty = typeof(C).GetProperty("H");

            public string F { get; set; }
            public string H { get; set; }
        }

        private class D : C
        {
        }

        private class Level1
        {
            public int Id { get; set; }
            public int Prop1 { get; set; }
            public Level1 Level1Reference { get; set; }
            public ICollection<Level1> Level1Collection { get; set; }
        }

        private class Level2 : Level1
        {
            public int Prop2 { get; set; }
            public Level2 Level2Reference { get; set; }
            public ICollection<Level2> Level2Collection { get; set; }
        }

        private class Level3 : Level2
        {
            public int Prop3 { get; set; }
            public Level3 Level3Reference { get; set; }
            public ICollection<Level3> Level3Collection { get; set; }
        }

        private class BaseType
        {
            public int Id { get; set; }
        }

        private class Customer : BaseType
        {
            public static readonly PropertyInfo IdProperty = typeof(BaseType).GetProperty(nameof(Id));
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty(nameof(Name));
            public static readonly PropertyInfo OrdersProperty = typeof(Customer).GetProperty(nameof(Orders));
            public static readonly PropertyInfo MoreOrdersProperty = typeof(Customer).GetProperty(nameof(MoreOrders));
            public static readonly PropertyInfo NotCollectionOrdersProperty = typeof(Customer).GetProperty(nameof(NotCollectionOrders));

            public int AlternateId { get; set; }
            public Guid Unique { get; set; }
            public string Name { get; set; }
            public string Mane { get; set; }
            public object this[string name] => null;

            public ICollection<Order> Orders { get; set; }
            public ICollection<Order> MoreOrders { get; set; }

            public IEnumerable<Order> EnumerableOrders { get; set; }
            public Order NotCollectionOrders { get; set; }
        }

        private class SpecialCustomer : Customer
        {
            public static readonly PropertyInfo DerivedOrdersProperty = typeof(SpecialCustomer).GetProperty(nameof(DerivedOrders));

            public IEnumerable<SpecialOrder> DerivedOrders { get; set; }
        }

        private class VerySpecialCustomer : SpecialCustomer
        {
        }

        private class Order : BaseType
        {
            public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty(nameof(Id));
            public static readonly PropertyInfo CustomerProperty = typeof(Order).GetProperty(nameof(Customer));
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty(nameof(CustomerId));
            public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty(nameof(CustomerUnique));
            public static readonly PropertyInfo OrderCustomerProperty = typeof(Order).GetProperty(nameof(OrderCustomer));

            public int CustomerId { get; set; }
            public Guid CustomerUnique { get; set; }
            public Customer Customer { get; set; }

            public Order OrderCustomer { get; set; }
        }

        private class SpecialOrder : Order
        {
            public static readonly PropertyInfo DerivedCustomerProperty = typeof(SpecialOrder).GetProperty(nameof(DerivedCustomer));

            public SpecialCustomer DerivedCustomer { get; set; }
        }

        private class VerySpecialOrder : SpecialOrder
        {
        }

        private static IMutableModel BuildFullNotificationEntityModel()
        {
            var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<FullNotificationEntity>(
                b =>
                {
                    b.HasOne(e => e.ReferenceNav)
                        .WithMany()
                        .HasForeignKey(e => e.AnotherEntityId);

                    b.HasMany(e => e.CollectionNav)
                        .WithOne();

                    b.Property(e => e.Token).IsConcurrencyToken();
                });

            return (Model)builder.Model;
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        private class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Token { get; set; }

            public AnotherEntity ReferenceNav { get; set; }
            public int AnotherEntityId { get; set; }

            public ICollection<AnotherEntity> CollectionNav { get; set; }

#pragma warning disable 67
            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        private class AnotherEntity
        {
            public int Id { get; set; }
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        private class ChangedOnlyEntity : INotifyPropertyChanged
        {
            public int Id { get; set; }
            public string Name { get; set; }

#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        private class SelfRef
        {
            public static readonly PropertyInfo IdProperty = typeof(SelfRef).GetProperty("Id");
            public static readonly PropertyInfo ForeignKeyProperty = typeof(SelfRef).GetProperty("ForeignKey");
            public static readonly PropertyInfo SelfRef1Property = typeof(SelfRef).GetProperty(nameof(SelfRef1));
            public static readonly PropertyInfo SelfRef2Property = typeof(SelfRef).GetProperty(nameof(SelfRef2));

            public int Id { get; set; }
            public SelfRef SelfRef1 { get; set; }
            public SelfRef SelfRef2 { get; set; }
            public int ForeignKey { get; set; }
        }
    }
}
