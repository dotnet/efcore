// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public partial class EntityTypeTest
    {
        [Fact]
        public void Circular_inheritance_should_throw()
        {
            var model = new Model();

            //    A
            //   / \
            //  B   C
            //       \
            //        D

            var a = model.AddEntityType(typeof(A).Name);
            var b = model.AddEntityType(typeof(B).Name);
            var c = model.AddEntityType(typeof(C).Name);
            var d = model.AddEntityType(typeof(D).Name);

            b.HasBaseType(a);
            c.HasBaseType(a);
            d.HasBaseType(c);

            Assert.Equal(
                CoreStrings.CircularInheritance(a.DisplayName(), a.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => a.HasBaseType(a)).Message);

            Assert.Equal(
                CoreStrings.CircularInheritance(a.DisplayName(), b.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => a.HasBaseType(b)).Message);

            Assert.Equal(
                CoreStrings.CircularInheritance(a.DisplayName(), d.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => a.HasBaseType(d)).Message);
        }

        [Fact]
        public void Setting_CLR_base_for_shadow_entity_type_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B).Name);

            Assert.Equal(
                CoreStrings.NonShadowBaseType(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.HasBaseType(a)).Message);
        }

        [Fact]
        public void Setting_shadow_base_for_CLR_entity_type_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A).Name);
            var b = model.AddEntityType(typeof(B));

            Assert.Equal(
                CoreStrings.NonClrBaseType(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.HasBaseType(a)).Message);
        }

        [Fact]
        public void Setting_not_assignable_base_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));

            Assert.Equal(
                CoreStrings.NotAssignableClrBaseType(typeof(A).Name, typeof(B).Name, typeof(A).Name, typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => a.HasBaseType(b)).Message);
        }

        [Fact]
        public void Properties_on_base_type_should_be_inherited()
        {
            var model = new Model();

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

            b.HasBaseType(a);
            c.HasBaseType(a);

            Assert.Equal(new[] { "E", "G" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "F", "H" }, b.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "H", "I" }, c.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1, 2, 3 }, b.GetProperties().Select(p => p.GetIndex()));
            Assert.Equal(new[] { 0, 1, 2, 3 }, c.GetProperties().Select(p => p.GetIndex()));
            Assert.Same(b.FindProperty("E"), a.FindProperty("E"));
        }

        [Fact]
        public void Properties_added_to_base_type_should_be_inherited()
        {
            var model = new Model();

            //    A
            //   / \
            //  B   C

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));
            var c = model.AddEntityType(typeof(C));

            b.HasBaseType(a);
            c.HasBaseType(a);

            a.AddProperty(A.GProperty);
            a.AddProperty(A.EProperty);

            b.AddProperty(B.HProperty);
            b.AddProperty(B.FProperty);

            c.AddProperty(C.HProperty);
            c.AddProperty("I", typeof(string));

            Assert.Equal(new[] { "E", "G" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "F", "H" }, b.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "H", "I" }, c.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1, 2, 3 }, b.GetProperties().Select(p => p.GetIndex()));
            Assert.Equal(new[] { 0, 1, 2, 3 }, c.GetProperties().Select(p => p.GetIndex()));
        }

        [Fact]
        public void Properties_should_be_updated_when_base_type_changes()
        {
            var model = new Model();

            var c = model.AddEntityType(typeof(C));
            c.AddProperty(C.HProperty);
            c.AddProperty(C.FProperty);

            var d = model.AddEntityType(typeof(D));
            d.AddProperty(A.EProperty);
            d.AddProperty(A.GProperty);
            d.HasBaseType(c);

            Assert.Equal(new[] { "F", "H" }, c.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F", "H", "E", "G" }, d.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1 }, c.GetProperties().Select(p => p.GetIndex()));
            Assert.Equal(new[] { 0, 1, 2, 3 }, d.GetProperties().Select(p => p.GetIndex()));

            d.HasBaseType(null);

            Assert.Equal(new[] { "F", "H" }, c.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G" }, d.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1 }, c.GetProperties().Select(p => p.GetIndex()));
            Assert.Equal(new[] { 0, 1 }, d.GetProperties().Select(p => p.GetIndex()));

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.EProperty);
            a.AddProperty(A.GProperty);

            c.HasBaseType(a);

            Assert.Equal(new[] { "E", "G" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "F", "H" }, c.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G" }, d.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1 }, a.GetProperties().Select(p => p.GetIndex()));
            Assert.Equal(new[] { 0, 1, 2, 3 }, c.GetProperties().Select(p => p.GetIndex()));
            Assert.Equal(new[] { 0, 1 }, d.GetProperties().Select(p => p.GetIndex()));
        }

        [Fact]
        public void Adding_property_throws_when_parent_type_has_property_with_same_name()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty);

            var b = model.AddEntityType(typeof(B));
            b.HasBaseType(a);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("G", typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.AddProperty("G")).Message);
        }

        [Fact]
        public void Adding_property_throws_when_grandparent_type_has_property_with_same_name()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty);

            var c = model.AddEntityType(typeof(C));
            c.HasBaseType(a);

            var d = model.AddEntityType(typeof(D));
            d.HasBaseType(c);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("G", typeof(D).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => d.AddProperty("G")).Message);
        }

        [Fact]
        public void Adding_property_throws_when_child_type_has_property_with_same_name()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));

            var b = model.AddEntityType(typeof(B));
            b.HasBaseType(a);

            b.AddProperty(A.GProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("G", typeof(A).Name, typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => a.AddProperty(A.GProperty)).Message);
        }

        [Fact]
        public void Adding_property_throws_when_grandchild_type_has_property_with_same_name()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));

            var c = model.AddEntityType(typeof(C));
            c.HasBaseType(a);

            var d = model.AddEntityType(typeof(D));
            d.HasBaseType(c);

            d.AddProperty(A.GProperty);

            Assert.Equal(
                CoreStrings.ConflictingPropertyOrNavigation("G", typeof(A).Name, typeof(D).Name),
                Assert.Throws<InvalidOperationException>(() => a.AddProperty(A.GProperty)).Message);
        }

        [Fact]
        public void Setting_base_type_throws_when_parent_contains_duplicate_property()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty);

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(A.GProperty);

            Assert.Equal(
                CoreStrings.DuplicatePropertiesOnBase(typeof(B).Name, typeof(A).Name, typeof(B).Name, "G", typeof(A).Name, "G"),
                Assert.Throws<InvalidOperationException>(() => b.HasBaseType(a)).Message);
        }

        [Fact]
        public void Setting_base_type_throws_when_grandparent_contains_duplicate_property()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.EProperty);
            a.AddProperty(A.GProperty);

            var c = model.AddEntityType(typeof(C));
            c.HasBaseType(a);

            var d = model.AddEntityType(typeof(D));
            d.AddProperty(A.EProperty);
            d.AddProperty(A.GProperty);

            Assert.Equal(
                CoreStrings.DuplicatePropertiesOnBase(typeof(D).Name, typeof(C).Name, typeof(D).Name, "E", typeof(A).Name, "E"),
                Assert.Throws<InvalidOperationException>(() => d.HasBaseType(c)).Message);
        }

        [Fact]
        public void Setting_base_type_throws_when_grandchild_contain_duplicate_property()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.EProperty);
            a.AddProperty(A.GProperty);

            var c = model.AddEntityType(typeof(C));

            var d = model.AddEntityType(typeof(D));
            d.AddProperty(A.EProperty);
            d.AddProperty(A.GProperty);
            d.HasBaseType(c);

            Assert.Equal(
                CoreStrings.DuplicatePropertiesOnBase(typeof(C).Name, typeof(A).Name, typeof(D).Name, "E", typeof(A).Name, "E"),
                Assert.Throws<InvalidOperationException>(() => c.HasBaseType(a)).Message);
        }

        [Fact]
        public void Keys_on_base_type_should_be_inherited()
        {
            var model = new Model();

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

            b.HasBaseType(a);

            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "G", "E", "F" }, b.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1, 2 }, b.GetProperties().Select(p => p.GetIndex()));
            Assert.Same(pk, b.FindPrimaryKey(new[] { b.FindProperty("G") }));
            Assert.Same(b.FindKey(b.FindProperty("G")), a.FindKey(a.FindProperty("G")));
        }

        [Fact]
        public void Keys_added_to_base_type_should_be_inherited()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty).IsNullable = false;
            a.AddProperty(A.EProperty).IsNullable = false;

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(B.FProperty);

            b.HasBaseType(a);

            a.SetPrimaryKey(a.FindProperty("G"));
            a.AddKey(a.FindProperty("E"));

            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "G", "E", "F" }, b.GetProperties().Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Keys_should_be_updated_when_base_type_changes()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var g = a.AddProperty(A.GProperty);
            g.IsNullable = false;
            a.SetPrimaryKey(g);
            var e = a.AddProperty(A.EProperty);
            e.IsNullable = false;
            a.AddKey(e);

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(B.FProperty);
            b.HasBaseType(a);

            b.HasBaseType(null);

            Assert.Equal(
                new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                Array.Empty<string[]>(),
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F" }, b.GetProperties().Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Adding_keys_throws_when_there_is_a_parent_type()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));
            b.HasBaseType(a);

            Assert.Equal(
                CoreStrings.DerivedEntityTypeKey(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.SetPrimaryKey(b.AddProperty("G"))).Message);
            Assert.Equal(
                CoreStrings.DerivedEntityTypeKey(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => b.AddKey(b.AddProperty("E"))).Message);
        }

        [Fact]
        public void Setting_base_type_throws_when_child_contains_key()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));
            var h = b.AddProperty(B.HProperty);
            h.IsNullable = false;
            var key = b.AddKey(h);

            Assert.Equal(
                CoreStrings.DerivedEntityCannotHaveKeys(typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => b.HasBaseType(a)).Message);

            b.RemoveKey(key.Properties);
            var f = b.AddProperty(B.FProperty);
            f.IsNullable = false;
            b.SetPrimaryKey(f);

            Assert.Equal(
                CoreStrings.DerivedEntityCannotHaveKeys(typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => b.HasBaseType(a)).Message);
        }

        [Fact]
        public void Setting_base_type_throws_when_mixing_views_and_entities()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddQueryType(typeof(B));

            Assert.Equal(
                CoreStrings.MixedQueryEntityTypeInheritance(typeof(A).Name, typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => b.HasBaseType(a)).Message);

            Assert.Equal(
                CoreStrings.MixedQueryEntityTypeInheritance(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => a.HasBaseType(b)).Message);
        }

        [Fact]
        public void Navigations_on_base_type_should_be_inherited()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(Array.Empty<string>(), specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());

            specialCustomerType.HasBaseType(customerType);

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty);
            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders", "DerivedOrders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders", "DerivedOrders" }, ((IEntityType)specialCustomerType).GetNavigations().Select(p => p.Name).ToArray());
            Assert.Same(customerType.FindNavigation("Orders"), specialCustomerType.FindNavigation("Orders"));
        }

        [Fact]
        public void Navigations_added_to_base_type_should_be_inherited()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.HasBaseType(customerType);

            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty);

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders", "DerivedOrders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Navigations_should_be_updated_when_base_type_changes()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.HasBaseType(customerType);

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty);

            specialCustomerType.HasBaseType(null);

            Assert.Equal(new[] { nameof(Customer.Orders) }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { nameof(SpecialCustomer.DerivedOrders) }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Adding_navigation_throws_when_parent_type_has_navigation_with_same_name()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);
            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.HasBaseType(customerType);

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey(nameof(Customer.Orders), typeof(Customer).Name, "{'Id'}", "{'CustomerId'}"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        specialCustomerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty)).Message);
        }

        [Fact]
        public void Adding_navigation_throws_when_grandparent_type_has_navigation_with_same_name()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);
            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.HasBaseType(customerType);

            var verySpecialCustomerType = model.AddEntityType(typeof(VerySpecialCustomer));
            verySpecialCustomerType.HasBaseType(specialCustomerType);

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, verySpecialCustomerType);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey("Orders", typeof(Customer).Name, "{'Id'}", "{'CustomerId'}"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        specialCustomerForeignKey.HasPrincipalToDependent("Orders")).Message);

            Assert.Equal("Orders", ((IEntityType)verySpecialCustomerType).GetNavigations().Single().Name);
        }

        [Fact]
        public void Adding_navigation_throws_when_child_type_has_navigation_with_same_name()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.HasBaseType(customerType);

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey("Orders", typeof(SpecialCustomer).Name, "{'CustomerId'}", "{'Id'}"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        customerForeignKey.HasPrincipalToDependent("Orders")).Message);
        }

        [Fact]
        public void Adding_navigation_throws_when_grandchild_type_has_navigation_with_same_name()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.HasBaseType(customerType);

            var verySpecialCustomerType = model.AddEntityType(typeof(VerySpecialCustomer));
            verySpecialCustomerType.HasBaseType(specialCustomerType);

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, verySpecialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey(nameof(Customer.Orders), typeof(VerySpecialCustomer).Name, "{'CustomerId'}", "{'Id'}"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty)).Message);

            Assert.Equal(nameof(Customer.Orders), ((IEntityType)verySpecialCustomerType).GetNavigations().Single().Name);
        }

        [Fact]
        public void Setting_base_type_throws_when_parent_contains_duplicate_navigation()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);
            customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var derivedForeignKeyProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.GetOrAddKey(property);
            var specialCustomerForeignKey = specialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            specialCustomerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Equal(
                CoreStrings.DuplicateNavigationsOnBase(typeof(SpecialOrder).Name, typeof(Order).Name, "Customer"),
                Assert.Throws<InvalidOperationException>(() => specialOrderType.HasBaseType(orderType)).Message);
        }

        [Fact]
        public void Setting_base_type_throws_when_grandparent_contains_duplicate_navigation()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);
            customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            var derivedForeignKeyProperty = verySpecialOrderType.GetOrAddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.GetOrAddKey(property);
            var specialCustomerForeignKey = verySpecialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            specialCustomerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);
            verySpecialOrderType.HasBaseType(specialOrderType);

            Assert.Equal(
                CoreStrings.DuplicateNavigationsOnBase(typeof(SpecialOrder).Name, typeof(Order).Name, nameof(Order.Customer)),
                Assert.Throws<InvalidOperationException>(() => specialOrderType.HasBaseType(orderType)).Message);
        }

        [Fact]
        public void Setting_base_type_throws_when_grandchild_contain_duplicate_navigation()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);
            customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            var derivedForeignKeyProperty = verySpecialOrderType.GetOrAddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.GetOrAddKey(property);
            var specialCustomerForeignKey = verySpecialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            specialCustomerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Equal(
                CoreStrings.DuplicateNavigationsOnBase(typeof(VerySpecialOrder).Name, typeof(SpecialOrder).Name, "Customer"),
                Assert.Throws<InvalidOperationException>(() => verySpecialOrderType.HasBaseType(specialOrderType)).Message);
        }

        [Fact]
        public void ForeignKeys_on_base_type_should_be_inherited()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            var derivedForeignKeyProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, customerType);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            specialOrderType.HasBaseType(orderType);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Same(customerForeignKey, specialOrderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType));
        }

        [Fact]
        public void ForeignKeys_added_to_base_type_should_be_inherited()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            specialOrderType.HasBaseType(orderType);
            orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            var derivedForeignKeyProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, customerType);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        [Fact]
        public void ForeignKeys_should_be_updated_when_base_type_changes()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);
            var derivedForeignKeyProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, customerType);

            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            specialOrderType.HasBaseType(null);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        [Fact]
        public void Adding_foreignKey_throws_when_parent_type_has_foreignKey_on_same_properties()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);

            Assert.Equal(
                CoreStrings.DuplicateForeignKey(
                    Property.Format(new[] { foreignKeyProperty }),
                    typeof(SpecialOrder).Name,
                    typeof(Order).Name,
                    Property.Format(customerKey.Properties),
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => specialOrderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        [Fact]
        public void Adding_foreignKey_throws_when_grandparent_type_has_foreignKey_on_same_properties()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.HasBaseType(specialOrderType);

            Assert.Equal(
                CoreStrings.DuplicateForeignKey(
                    Property.Format(new[] { foreignKeyProperty }),
                    typeof(VerySpecialOrder).Name,
                    typeof(Order).Name,
                    Property.Format(customerKey.Properties),
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => verySpecialOrderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        [Fact]
        public void Adding_foreignKey_throws_when_child_type_has_foreignKey_on_same_properties()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);
            specialOrderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.DuplicateForeignKey(
                    Property.Format(new[] { foreignKeyProperty }),
                    typeof(Order).Name,
                    typeof(SpecialOrder).Name,
                    Property.Format(customerKey.Properties),
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        [Fact]
        public void Adding_foreignKey_throws_when_grandchild_type_has_foreignKey_on_same_properties()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.HasBaseType(specialOrderType);
            verySpecialOrderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.DuplicateForeignKey(
                    Property.Format(new[] { foreignKeyProperty }),
                    typeof(Order).Name,
                    typeof(VerySpecialOrder).Name,
                    Property.Format(customerKey.Properties),
                    typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        [Fact]
        public void Index_on_base_type_should_be_inherited()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var index = orderType.GetOrAddIndex(indexProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            var derivedIndexProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddIndex(derivedIndexProperty);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            specialOrderType.HasBaseType(orderType);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Same(index, specialOrderType.GetOrAddIndex(indexProperty));
        }

        [Fact]
        public void Index_added_to_base_type_should_be_inherited()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            specialOrderType.HasBaseType(orderType);
            orderType.GetOrAddIndex(indexProperty);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            var derivedIndexProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddIndex(derivedIndexProperty);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        [Fact]
        public void Indexes_should_be_updated_when_base_type_changes()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);
            var derivedIndexProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddIndex(derivedIndexProperty);

            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.GetOrAddIndex(indexProperty);

            specialOrderType.HasBaseType(null);

            Assert.Equal(
                new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(
                new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        [Fact]
        public void Adding_an_index_throws_if_properties_were_removed()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            entityType.RemoveProperty(idProperty.Name);

            Assert.Equal(
                CoreStrings.IndexPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddIndex(new[] { idProperty })).Message);
        }

        [Fact]
        public void Adding_an_index_throws_if_duplicate_properties()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);

            Assert.Equal(
                CoreStrings.DuplicatePropertyInList("{'" + Customer.IdProperty.Name + "', '" + Customer.IdProperty.Name + "'}", Customer.IdProperty.Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddIndex(new[] { idProperty, idProperty })).Message);
        }

        [Fact]
        public void Adding_an_index_throws_when_parent_type_has_index_on_same_properties()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.AddIndex(indexProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);

            Assert.Equal(
                CoreStrings.DuplicateIndex(Property.Format(new[] { indexProperty }), typeof(SpecialOrder).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => specialOrderType.AddIndex(indexProperty)).Message);
        }

        [Fact]
        public void Adding_an_index_throws_when_grandparent_type_has_index_on_same_properties()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.AddIndex(indexProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.HasBaseType(specialOrderType);

            Assert.Equal(
                CoreStrings.DuplicateIndex(Property.Format(new[] { indexProperty }), typeof(VerySpecialOrder).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => verySpecialOrderType.AddIndex(indexProperty)).Message);
        }

        [Fact]
        public void Adding_an_index_throws_when_child_type_has_index_on_same_properties()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);
            specialOrderType.AddIndex(indexProperty);

            Assert.Equal(
                CoreStrings.DuplicateIndex(Property.Format(new[] { indexProperty }), typeof(Order).Name, typeof(SpecialOrder).Name),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddIndex(indexProperty)).Message);
        }

        [Fact]
        public void Adding_an_index_throws_when_grandchild_type_has_index_on_same_properties()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.HasBaseType(specialOrderType);
            verySpecialOrderType.AddIndex(indexProperty);

            Assert.Equal(
                CoreStrings.DuplicateIndex(Property.Format(new[] { indexProperty }), typeof(Order).Name, typeof(VerySpecialOrder).Name),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddIndex(indexProperty)).Message);
        }
    }
}
