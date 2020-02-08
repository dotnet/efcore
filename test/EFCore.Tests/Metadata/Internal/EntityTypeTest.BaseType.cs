// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public partial class EntityTypeTest
    {
        [ConditionalFact]
        public void Circular_inheritance_should_throw()
        {
            var model = CreateModel();

            //    A
            //   / \
            //  B   C
            //       \
            //        D

            var a = model.AddEntityType(typeof(A).Name);
            var b = model.AddEntityType(typeof(B).Name);
            var c = model.AddEntityType(typeof(C).Name);
            var d = model.AddEntityType(typeof(D).Name);

            b.BaseType = a;
            c.BaseType = a;
            d.BaseType = c;

            Assert.Equal(
                CoreStrings.CircularInheritance(a.DisplayName(), a.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => a.BaseType = a).Message);

            Assert.Equal(
                CoreStrings.CircularInheritance(a.DisplayName(), b.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => a.BaseType = b).Message);

            Assert.Equal(
                CoreStrings.CircularInheritance(a.DisplayName(), d.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => a.BaseType = d).Message);
        }

        [ConditionalFact]
        public void Setting_CLR_base_for_shadow_entity_type_should_throw()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B).Name);

            Assert.Equal(
                CoreStrings.NonShadowBaseType(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);
        }

        [ConditionalFact]
        public void Setting_shadow_base_for_CLR_entity_type_should_throw()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A).Name);
            var b = model.AddEntityType(typeof(B));

            Assert.Equal(
                CoreStrings.NonClrBaseType(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);
        }

        [ConditionalFact]
        public void Setting_not_assignable_base_should_throw()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));

            Assert.Equal(
                CoreStrings.NotAssignableClrBaseType(typeof(A).Name, typeof(B).Name, typeof(A).Name, typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => a.BaseType = b).Message);
        }

        [ConditionalFact]
        public void Properties_on_base_type_should_be_inherited()
        {
            var model = CreateModel();

            //    A
            //   / \
            //  B   C

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty);
            a.AddProperty(A.EProperty);

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(B.HProperty);
            b.AddProperty(B.FProperty);

            var c = model.AddEntityType(typeof(C));
            c.AddProperty(C.HProperty);
            c.AddProperty("I", typeof(string));

            Assert.Equal(new[] { "E", "G" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F", "H" }, b.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "H", "I" }, c.GetProperties().Select(p => p.Name).ToArray());

            b.BaseType = a;
            c.BaseType = a;

            model.FinalizeModel();

            Assert.Equal(new[] { "E", "G" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "F", "H" }, b.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "H", "I" }, c.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1, 2, 3 }, b.GetProperties().Select(p => p.GetIndex()));
            Assert.Equal(new[] { 0, 1, 2, 3 }, c.GetProperties().Select(p => p.GetIndex()));
            Assert.Same(b.FindProperty("E"), a.FindProperty("E"));
        }

        [ConditionalFact]
        public void Properties_added_to_base_type_should_be_inherited()
        {
            var model = CreateModel();

            //    A
            //   / \
            //  B   C

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));
            var c = model.AddEntityType(typeof(C));

            b.BaseType = a;
            c.BaseType = a;

            a.AddProperty(A.GProperty);
            a.AddProperty(A.EProperty);

            b.AddProperty(B.HProperty);
            b.AddProperty(B.FProperty);

            c.AddProperty(C.HProperty);
            c.AddProperty("I", typeof(string));

            model.FinalizeModel();

            Assert.Equal(new[] { "E", "G" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "F", "H" }, b.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "H", "I" }, c.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1, 2, 3 }, b.GetProperties().Select(p => p.GetIndex()));
            Assert.Equal(new[] { 0, 1, 2, 3 }, c.GetProperties().Select(p => p.GetIndex()));
        }

        [ConditionalFact]
        public void Properties_should_be_updated_when_base_type_changes()
        {
            var model = CreateModel();

            var c = model.AddEntityType(typeof(C));
            c.AddProperty(C.HProperty);
            c.AddProperty(C.FProperty);

            var d = model.AddEntityType(typeof(D));
            d.AddProperty(A.EProperty);
            d.AddProperty(A.GProperty);
            d.BaseType = c;

            Assert.Equal(new[] { "F", "H" }, c.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F", "H", "E", "G" }, d.GetProperties().Select(p => p.Name).ToArray());

            d.BaseType = null;

            Assert.Equal(new[] { "F", "H" }, c.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G" }, d.GetProperties().Select(p => p.Name).ToArray());

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.EProperty);
            a.AddProperty(A.GProperty);

            c.BaseType = a;

            model.FinalizeModel();

            Assert.Equal(new[] { "E", "G" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "F", "H" }, c.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G" }, d.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1 }, a.GetProperties().Select(p => p.GetIndex()));
            Assert.Equal(new[] { 0, 1, 2, 3 }, c.GetProperties().Select(p => p.GetIndex()));
            Assert.Equal(new[] { 0, 1 }, d.GetProperties().Select(p => p.GetIndex()));
        }

        [ConditionalFact]
        public void Adding_property_throws_when_parent_type_has_property_with_same_name()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty);

            var b = model.AddEntityType(typeof(B));
            b.BaseType = a;

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("G", typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.AddProperty("G")).Message);
        }

        [ConditionalFact]
        public void Adding_property_throws_when_grandparent_type_has_property_with_same_name()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty);

            var c = model.AddEntityType(typeof(C));
            c.BaseType = a;

            var d = model.AddEntityType(typeof(D));
            d.BaseType = c;

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("G", typeof(D).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => d.AddProperty("G")).Message);
        }

        [ConditionalFact]
        public void Adding_property_throws_when_child_type_has_property_with_same_name()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));

            var b = model.AddEntityType(typeof(B));
            b.BaseType = a;

            b.AddProperty(A.GProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("G", typeof(A).Name, typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => a.AddProperty(A.GProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_property_throws_when_grandchild_type_has_property_with_same_name()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));

            var c = model.AddEntityType(typeof(C));
            c.BaseType = a;

            var d = model.AddEntityType(typeof(D));
            d.BaseType = c;

            d.AddProperty(A.GProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("G", typeof(A).Name, typeof(D).Name),
                Assert.Throws<InvalidOperationException>(() => a.AddProperty(A.GProperty)).Message);
        }

        [ConditionalFact]
        public void Setting_base_type_throws_when_parent_contains_duplicate_property()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty);

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(A.GProperty);

            Assert.Equal(
                CoreStrings.DuplicatePropertiesOnBase(typeof(B).Name, typeof(A).Name, typeof(B).Name, "G", typeof(A).Name, "G"),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);
        }

        [ConditionalFact]
        public void Setting_base_type_throws_when_grandparent_contains_duplicate_property()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.EProperty);
            a.AddProperty(A.GProperty);

            var c = model.AddEntityType(typeof(C));
            c.BaseType = a;

            var d = model.AddEntityType(typeof(D));
            d.AddProperty(A.EProperty);
            d.AddProperty(A.GProperty);

            Assert.Equal(
                CoreStrings.DuplicatePropertiesOnBase(typeof(D).Name, typeof(C).Name, typeof(D).Name, "E", typeof(A).Name, "E"),
                Assert.Throws<InvalidOperationException>(() => d.BaseType = c).Message);
        }

        [ConditionalFact]
        public void Setting_base_type_throws_when_grandchild_contain_duplicate_property()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.EProperty);
            a.AddProperty(A.GProperty);

            var c = model.AddEntityType(typeof(C));

            var d = model.AddEntityType(typeof(D));
            d.AddProperty(A.EProperty);
            d.AddProperty(A.GProperty);
            d.BaseType = c;

            Assert.Equal(
                CoreStrings.DuplicatePropertiesOnBase(typeof(C).Name, typeof(A).Name, typeof(D).Name, "E", typeof(A).Name, "E"),
                Assert.Throws<InvalidOperationException>(() => c.BaseType = a).Message);
        }

        [ConditionalFact]
        public void Keys_on_base_type_should_be_inherited()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            var g = a.AddProperty(A.GProperty);
            g.IsNullable = false;
            var e = a.AddProperty(A.EProperty);
            e.IsNullable = false;
            var pk = a.SetPrimaryKey(g);
            a.AddKey(e);

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(B.FProperty);

            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                Array.Empty<string[]>(),
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F" }, b.GetProperties().Select(p => p.Name).ToArray());

            b.BaseType = a;

            model.FinalizeModel();

            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "G", "E", "F" }, b.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1, 2 }, b.GetProperties().Select(p => p.GetIndex()));
            Assert.Same(pk, b.FindProperty("G").FindContainingPrimaryKey());
            Assert.Same(b.FindKey(b.FindProperty("G")), a.FindKey(a.FindProperty("G")));
        }

        [ConditionalFact]
        public void Keys_added_to_base_type_should_be_inherited()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty).IsNullable = false;
            a.AddProperty(A.EProperty).IsNullable = false;

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(B.FProperty);

            b.BaseType = a;

            a.SetPrimaryKey(a.FindProperty("G"));
            a.AddKey(a.FindProperty("E"));

            model.FinalizeModel();

            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "G", "E", "F" }, b.GetProperties().Select(p => p.Name).ToArray());
        }

        [ConditionalFact]
        public void Keys_should_be_updated_when_base_type_changes()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            var g = a.AddProperty(A.GProperty);
            g.IsNullable = false;
            a.SetPrimaryKey(g);
            var e = a.AddProperty(A.EProperty);
            e.IsNullable = false;
            a.AddKey(e);

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(B.FProperty);
            b.BaseType = a;

            b.BaseType = null;

            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                Array.Empty<string[]>(),
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F" }, b.GetProperties().Select(p => p.Name).ToArray());
        }

        [ConditionalFact]
        public void Adding_keys_throws_when_there_is_a_parent_type()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));
            b.BaseType = a;

            Assert.Equal(
                CoreStrings.DerivedEntityTypeKey(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.SetPrimaryKey(b.AddProperty("G"))).Message);
            Assert.Equal(
                CoreStrings.DerivedEntityTypeKey(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.AddKey(b.AddProperty("E"))).Message);
        }

        [ConditionalFact]
        public void Setting_base_type_throws_when_child_contains_key()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));
            var h = b.AddProperty(B.HProperty);
            h.IsNullable = false;
            var key = b.AddKey(h);

            Assert.Equal(
                CoreStrings.DerivedEntityCannotHaveKeys(typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);

            b.RemoveKey(key.Properties);
            var f = b.AddProperty(B.FProperty);
            f.IsNullable = false;
            b.SetPrimaryKey(f);

            Assert.Equal(
                CoreStrings.DerivedEntityCannotHaveKeys(typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);
        }

        [ConditionalFact]
        public void Setting_base_type_throws_on_keyless_type()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));
            b.IsKeyless = true;

            Assert.Equal(
                CoreStrings.DerivedEntityCannotBeKeyless(typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);
        }

        [ConditionalFact]
        public void HasNoKey_on_derived_type_throws()
        {
            var model = CreateModel();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));
            b.BaseType = a;

            Assert.Equal(
                CoreStrings.DerivedEntityTypeHasNoKey(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.IsKeyless = true).Message);
        }

        [ConditionalFact]
        public void Navigations_on_base_type_should_be_inherited()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(Array.Empty<string>(), specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());

            specialCustomerType.BaseType = customerType;

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());

            var derivedForeignKeyProperty = orderType.AddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.AddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty);
            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders", "DerivedOrders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(
                new[] { "Orders", "DerivedOrders" }, ((IEntityType)specialCustomerType).GetNavigations().Select(p => p.Name).ToArray());
            Assert.Same(customerType.FindNavigation("Orders"), specialCustomerType.FindNavigation("Orders"));
        }

        [ConditionalFact]
        public void Navigations_added_to_base_type_should_be_inherited()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.BaseType = customerType;

            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());

            var derivedForeignKeyProperty = orderType.AddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.AddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty);

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders", "DerivedOrders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());
        }

        [ConditionalFact]
        public void Navigations_should_be_updated_when_base_type_changes()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.BaseType = customerType;

            var derivedForeignKeyProperty = orderType.AddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.AddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty);

            specialCustomerType.BaseType = null;

            Assert.Equal(new[] { nameof(Customer.Orders) }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(
                new[] { nameof(SpecialCustomer.DerivedOrders) }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());
        }

        [ConditionalFact]
        public void Adding_navigation_throws_when_parent_type_has_navigation_with_same_name()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);
            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.BaseType = customerType;

            var derivedForeignKeyProperty = orderType.AddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.AddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey(nameof(Customer.Orders), typeof(Customer).Name, "{'Id'}", "{'CustomerId'}"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        specialCustomerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_navigation_throws_when_grandparent_type_has_navigation_with_same_name()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);
            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.BaseType = customerType;

            var verySpecialCustomerType = model.AddEntityType(typeof(VerySpecialCustomer));
            verySpecialCustomerType.BaseType = specialCustomerType;

            var derivedForeignKeyProperty = orderType.AddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.AddForeignKey(derivedForeignKeyProperty, customerKey, verySpecialCustomerType);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey("Orders", typeof(Customer).Name, "{'Id'}", "{'CustomerId'}"),
                Assert.Throws<InvalidOperationException>(
                    () => specialCustomerForeignKey.HasPrincipalToDependent("Orders")).Message);

            Assert.Equal("Orders", ((IEntityType)verySpecialCustomerType).GetNavigations().Single().Name);
        }

        [ConditionalFact]
        public void Adding_navigation_throws_when_child_type_has_navigation_with_same_name()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.BaseType = customerType;

            var derivedForeignKeyProperty = orderType.AddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.AddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey("Orders", typeof(SpecialCustomer).Name, "{'CustomerId'}", "{'Id'}"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        customerForeignKey.HasPrincipalToDependent("Orders")).Message);
        }

        [ConditionalFact]
        public void Adding_navigation_throws_when_grandchild_type_has_navigation_with_same_name()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.BaseType = customerType;

            var verySpecialCustomerType = model.AddEntityType(typeof(VerySpecialCustomer));
            verySpecialCustomerType.BaseType = specialCustomerType;

            var derivedForeignKeyProperty = orderType.AddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.AddForeignKey(derivedForeignKeyProperty, customerKey, verySpecialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey(
                    nameof(Customer.Orders), typeof(VerySpecialCustomer).Name, "{'CustomerId'}", "{'Id'}"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty)).Message);

            Assert.Equal(nameof(Customer.Orders), ((IEntityType)verySpecialCustomerType).GetNavigations().Single().Name);
        }

        [ConditionalFact]
        public void Setting_base_type_throws_when_parent_contains_duplicate_navigation()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);
            customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var derivedForeignKeyProperty = specialOrderType.AddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.AddKey(property);
            var specialCustomerForeignKey = specialOrderType.AddForeignKey(
                derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            specialCustomerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Equal(CoreStrings.DuplicatePropertiesOnBase(nameof(SpecialOrder), nameof(Order),
                nameof(SpecialOrder), nameof(Order.Customer), nameof(Order), nameof(Order.Customer)),
                Assert.Throws<InvalidOperationException>(() => specialOrderType.BaseType = orderType).Message);
        }

        [ConditionalFact]
        public void Setting_base_type_throws_when_grandparent_contains_duplicate_navigation()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);
            customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            var derivedForeignKeyProperty = verySpecialOrderType.AddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.AddKey(property);
            var specialCustomerForeignKey = verySpecialOrderType.AddForeignKey(
                derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            specialCustomerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);
            verySpecialOrderType.BaseType = specialOrderType;

            Assert.Equal(CoreStrings.DuplicatePropertiesOnBase(nameof(SpecialOrder), nameof(Order),
                    nameof(VerySpecialOrder), nameof(Order.Customer), nameof(Order), nameof(Order.Customer)),
                Assert.Throws<InvalidOperationException>(() => specialOrderType.BaseType = orderType).Message);
        }

        [ConditionalFact]
        public void Setting_base_type_throws_when_grandchild_contain_duplicate_navigation()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);
            customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            var derivedForeignKeyProperty = verySpecialOrderType.AddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.AddKey(property);
            var specialCustomerForeignKey = verySpecialOrderType.AddForeignKey(
                derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            specialCustomerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Equal(
                CoreStrings.DuplicatePropertiesOnBase(nameof(VerySpecialOrder), nameof(SpecialOrder),
                    nameof(VerySpecialOrder), nameof(Order.Customer), nameof(Order), nameof(Order.Customer)),
                Assert.Throws<InvalidOperationException>(() => verySpecialOrderType.BaseType = specialOrderType).Message);
        }

        [ConditionalFact]
        public void ForeignKeys_on_base_type_should_be_inherited()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            var derivedForeignKeyProperty = specialOrderType.AddProperty(Order.IdProperty);
            specialOrderType.AddForeignKey(derivedForeignKeyProperty, customerKey, customerType);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            specialOrderType.BaseType = orderType;

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Same(customerForeignKey, specialOrderType.FindForeignKey(foreignKeyProperty, customerKey, customerType));
        }

        [ConditionalFact]
        public void ForeignKeys_added_to_base_type_should_be_inherited()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            specialOrderType.BaseType = orderType;
            orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            var derivedForeignKeyProperty = specialOrderType.AddProperty(Order.IdProperty);
            specialOrderType.AddForeignKey(derivedForeignKeyProperty, customerKey, customerType);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        [ConditionalFact]
        public void ForeignKeys_should_be_updated_when_base_type_changes()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;
            var derivedForeignKeyProperty = specialOrderType.AddProperty(Order.IdProperty);
            specialOrderType.AddForeignKey(derivedForeignKeyProperty, customerKey, customerType);

            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            specialOrderType.BaseType = null;

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        [ConditionalFact]
        public void Adding_foreignKey_throws_when_parent_type_has_foreignKey_on_same_properties()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            Assert.Equal(
                CoreStrings.DuplicateForeignKey(
                    new[] { foreignKeyProperty }.Format(),
                    typeof(SpecialOrder).Name,
                    typeof(Order).Name,
                    customerKey.Properties.Format(),
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => specialOrderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        [ConditionalFact]
        public void Adding_foreignKey_throws_when_grandparent_type_has_foreignKey_on_same_properties()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.BaseType = specialOrderType;

            Assert.Equal(
                CoreStrings.DuplicateForeignKey(
                    new[] { foreignKeyProperty }.Format(),
                    typeof(VerySpecialOrder).Name,
                    typeof(Order).Name,
                    customerKey.Properties.Format(),
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => verySpecialOrderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        [ConditionalFact]
        public void Adding_foreignKey_throws_when_child_type_has_foreignKey_on_same_properties()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;
            specialOrderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.DuplicateForeignKey(
                    new[] { foreignKeyProperty }.Format(),
                    typeof(Order).Name,
                    typeof(SpecialOrder).Name,
                    customerKey.Properties.Format(),
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        [ConditionalFact]
        public void Adding_foreignKey_throws_when_grandchild_type_has_foreignKey_on_same_properties()
        {
            var model = CreateModel();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.BaseType = specialOrderType;
            verySpecialOrderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.DuplicateForeignKey(
                    new[] { foreignKeyProperty }.Format(),
                    typeof(Order).Name,
                    typeof(VerySpecialOrder).Name,
                    customerKey.Properties.Format(),
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        [ConditionalFact]
        public void Index_on_base_type_should_be_inherited()
        {
            var model = CreateModel();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var index = orderType.AddIndex(indexProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            var derivedIndexProperty = specialOrderType.AddProperty(Order.IdProperty);
            specialOrderType.AddIndex(derivedIndexProperty);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            specialOrderType.BaseType = orderType;

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Same(index, specialOrderType.FindIndex(indexProperty));
        }

        [ConditionalFact]
        public void Index_added_to_base_type_should_be_inherited()
        {
            var model = CreateModel();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.AddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            specialOrderType.BaseType = orderType;
            orderType.AddIndex(indexProperty);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            var derivedIndexProperty = specialOrderType.AddProperty(Order.IdProperty);
            specialOrderType.AddIndex(derivedIndexProperty);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        [ConditionalFact]
        public void Indexes_should_be_updated_when_base_type_changes()
        {
            var model = CreateModel();

            var orderType = model.AddEntityType(typeof(Order));

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;
            var derivedIndexProperty = specialOrderType.AddProperty(Order.IdProperty);
            specialOrderType.AddIndex(derivedIndexProperty);

            var indexProperty = orderType.AddProperty(Order.CustomerIdProperty);
            orderType.AddIndex(indexProperty);

            specialOrderType.BaseType = null;

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        [ConditionalFact]
        public void Adding_an_index_throws_if_properties_were_removed()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            entityType.RemoveProperty(idProperty.Name);

            Assert.Equal(
                CoreStrings.IndexPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddIndex(new[] { idProperty })).Message);
        }

        [ConditionalFact]
        public void Adding_an_index_throws_if_duplicate_properties()
        {
            var model = CreateModel();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);

            Assert.Equal(
                CoreStrings.DuplicatePropertyInList(
                    "{'" + Customer.IdProperty.Name + "', '" + Customer.IdProperty.Name + "'}", Customer.IdProperty.Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddIndex(new[] { idProperty, idProperty })).Message);
        }

        [ConditionalFact]
        public void Adding_an_index_throws_when_parent_type_has_index_on_same_properties()
        {
            var model = CreateModel();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.AddProperty(Order.CustomerIdProperty);
            orderType.AddIndex(indexProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            Assert.Equal(
                CoreStrings.DuplicateIndex(new[] { indexProperty }.Format(), typeof(SpecialOrder).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => specialOrderType.AddIndex(indexProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_an_index_throws_when_grandparent_type_has_index_on_same_properties()
        {
            var model = CreateModel();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.AddProperty(Order.CustomerIdProperty);
            orderType.AddIndex(indexProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.BaseType = specialOrderType;

            Assert.Equal(
                CoreStrings.DuplicateIndex(new[] { indexProperty }.Format(), typeof(VerySpecialOrder).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => verySpecialOrderType.AddIndex(indexProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_an_index_throws_when_child_type_has_index_on_same_properties()
        {
            var model = CreateModel();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.AddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;
            specialOrderType.AddIndex(indexProperty);

            Assert.Equal(
                CoreStrings.DuplicateIndex(new[] { indexProperty }.Format(), typeof(Order).Name, typeof(SpecialOrder).Name),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddIndex(indexProperty)).Message);
        }

        [ConditionalFact]
        public void Adding_an_index_throws_when_grandchild_type_has_index_on_same_properties()
        {
            var model = CreateModel();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.AddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.BaseType = specialOrderType;
            verySpecialOrderType.AddIndex(indexProperty);

            Assert.Equal(
                CoreStrings.DuplicateIndex(new[] { indexProperty }.Format(), typeof(Order).Name, typeof(VerySpecialOrder).Name),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddIndex(indexProperty)).Message);
        }

        private static IMutableModel CreateModel() => new Model();
    }
}
