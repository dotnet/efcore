// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable ImplicitlyCapturedClosure

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class EntityTypeTest
    {
        private readonly Model _model = BuildModel();

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
                CoreStrings.CircularInheritance(a, a),
                Assert.Throws<InvalidOperationException>(() => { a.HasBaseType(a); }).Message);

            Assert.Equal(
                CoreStrings.CircularInheritance(a, b),
                Assert.Throws<InvalidOperationException>(() => { a.HasBaseType(b); }).Message);

            Assert.Equal(
                CoreStrings.CircularInheritance(a, d),
                Assert.Throws<InvalidOperationException>(() => { a.HasBaseType(d); }).Message);
        }

        [Fact]
        public void Setting_CLR_base_for_shadow_entity_type_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B).Name);

            Assert.Equal(
                CoreStrings.NonShadowBaseType(b, a),
                Assert.Throws<InvalidOperationException>(() => { b.HasBaseType(a); }).Message);
        }

        [Fact]
        public void Setting_shadow_base_for_CLR_entity_type_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A).Name);
            var b = model.AddEntityType(typeof(B));

            Assert.Equal(
                CoreStrings.NonClrBaseType(b, a),
                Assert.Throws<InvalidOperationException>(() => { b.HasBaseType(a); }).Message);
        }

        [Fact]
        public void Setting_not_assignable_base_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));

            Assert.Equal(
                CoreStrings.NotAssignableClrBaseType(a, b, typeof(A).Name, typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => { a.HasBaseType(b); }).Message);
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

            Assert.Equal(CoreStrings.DuplicateProperty("G", typeof(B).Name, typeof(A).Name),
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
                CoreStrings.DuplicateProperty("G", typeof(D).Name, typeof(A).Name),
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
                CoreStrings.DuplicateProperty("G", typeof(A).Name, typeof(B).Name),
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
                CoreStrings.DuplicateProperty("G", typeof(A).Name, typeof(D).Name),
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
                CoreStrings.DuplicatePropertiesOnBase(typeof(B).FullName, typeof(A).FullName, "G"),
                Assert.Throws<InvalidOperationException>(() => { b.HasBaseType(a); }).Message);
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
                CoreStrings.DuplicatePropertiesOnBase(typeof(D).FullName, typeof(C).FullName, "E, G"),
                Assert.Throws<InvalidOperationException>(() => { d.HasBaseType(c); }).Message);
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
                CoreStrings.DuplicatePropertiesOnBase(typeof(C).FullName, typeof(A).FullName, "E, G"),
                Assert.Throws<InvalidOperationException>(() => { c.HasBaseType(a); }).Message);
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

            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new string[0][],
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.GetProperties().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F" }, b.GetProperties().Select(p => p.Name).ToArray());

            b.HasBaseType(a);

            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
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

            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
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

            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new string[0][],
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
                CoreStrings.DerivedEntityCannotHaveKeys(typeof(B).FullName),
                Assert.Throws<InvalidOperationException>(() => { b.HasBaseType(a); }).Message);

            b.RemoveKey(key.Properties);
            var f = b.AddProperty(B.FProperty);
            f.IsNullable = false;
            b.SetPrimaryKey(f);

            Assert.Equal(
                CoreStrings.DerivedEntityCannotHaveKeys(typeof(B).FullName),
                Assert.Throws<InvalidOperationException>(() => { b.HasBaseType(a); }).Message);
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

            customerForeignKey.HasPrincipalToDependent("Orders");

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new string[0], specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());

            specialCustomerType.HasBaseType(customerType);

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent("DerivedOrders");
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

            customerForeignKey.HasPrincipalToDependent("Orders");

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent("DerivedOrders");

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

            customerForeignKey.HasPrincipalToDependent("Orders");

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.HasBaseType(customerType);

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerForeignKey.HasPrincipalToDependent("DerivedOrders");

            specialCustomerType.HasBaseType(null);

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "DerivedOrders" }, specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());
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
            customerForeignKey.HasPrincipalToDependent("Orders");

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.HasBaseType(customerType);

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey("Orders", typeof(Customer).Name, "{'Id'}", "{'CustomerId'}"),
                Assert.Throws<InvalidOperationException>(() =>
                    specialCustomerForeignKey.HasPrincipalToDependent("Orders")).Message);
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
            customerForeignKey.HasPrincipalToDependent("Orders");

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.HasBaseType(customerType);

            var verySpecialCustomerType = model.AddEntityType(typeof(VerySpecialCustomer));
            verySpecialCustomerType.HasBaseType(specialCustomerType);

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, verySpecialCustomerType);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey("Orders", typeof(Customer).Name, "{'Id'}", "{'CustomerId'}"),
                Assert.Throws<InvalidOperationException>(() =>
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
            specialCustomerForeignKey.HasPrincipalToDependent("Orders");

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey("Orders", typeof(SpecialCustomer).Name, "{'CustomerId'}", "{'Id'}"),
                Assert.Throws<InvalidOperationException>(() =>
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
            specialCustomerForeignKey.HasPrincipalToDependent("Orders");

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey("Orders", typeof(VerySpecialCustomer).Name, "{'CustomerId'}", "{'Id'}"),
                Assert.Throws<InvalidOperationException>(() =>
                    customerForeignKey.HasPrincipalToDependent("Orders")).Message);

            Assert.Equal("Orders", ((IEntityType)verySpecialCustomerType).GetNavigations().Single().Name);
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
            customerForeignKey.HasDependentToPrincipal("Customer");

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var derivedForeignKeyProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.GetOrAddKey(property);
            var specialCustomerForeignKey = specialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            specialCustomerForeignKey.HasDependentToPrincipal("Customer");

            Assert.Equal(
                CoreStrings.DuplicateNavigationsOnBase(typeof(SpecialOrder).FullName, typeof(Order).FullName, "Customer"),
                Assert.Throws<InvalidOperationException>(() => { specialOrderType.HasBaseType(orderType); }).Message);
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
            customerForeignKey.HasDependentToPrincipal("Customer");

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            var derivedForeignKeyProperty = verySpecialOrderType.GetOrAddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.GetOrAddKey(property);
            var specialCustomerForeignKey = verySpecialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            specialCustomerForeignKey.HasDependentToPrincipal("Customer");
            verySpecialOrderType.HasBaseType(specialOrderType);

            Assert.Equal(
                CoreStrings.DuplicateNavigationsOnBase(typeof(SpecialOrder).FullName, typeof(Order).FullName, "Customer"),
                Assert.Throws<InvalidOperationException>(() => { specialOrderType.HasBaseType(orderType); }).Message);
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
            customerForeignKey.HasDependentToPrincipal("Customer");

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.HasBaseType(orderType);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            var derivedForeignKeyProperty = verySpecialOrderType.GetOrAddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.GetOrAddKey(property);
            var specialCustomerForeignKey = verySpecialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            specialCustomerForeignKey.HasDependentToPrincipal("Customer");

            Assert.Equal(
                CoreStrings.DuplicateNavigationsOnBase(typeof(VerySpecialOrder).FullName, typeof(SpecialOrder).FullName, "Customer"),
                Assert.Throws<InvalidOperationException>(() => { verySpecialOrderType.HasBaseType(specialOrderType); }).Message);
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

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            specialOrderType.HasBaseType(orderType);

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
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

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            var derivedForeignKeyProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, customerType);

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
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

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.IdProperty.Name } },
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
                CoreStrings.DuplicateForeignKey(Property.Format(new[] { foreignKeyProperty }), typeof(SpecialOrder).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(() =>
                    specialOrderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
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
                CoreStrings.DuplicateForeignKey(Property.Format(new[] { foreignKeyProperty }), typeof(VerySpecialOrder).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(() =>
                    verySpecialOrderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
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
                CoreStrings.DuplicateForeignKey(Property.Format(new[] { foreignKeyProperty }), typeof(Order).Name, typeof(SpecialOrder).Name),
                Assert.Throws<InvalidOperationException>(() =>
                    orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
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
                CoreStrings.DuplicateForeignKey(Property.Format(new[] { foreignKeyProperty }), typeof(Order).Name, typeof(VerySpecialOrder).Name),
                Assert.Throws<InvalidOperationException>(() =>
                    orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
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

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            specialOrderType.HasBaseType(orderType);

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
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

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                specialOrderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            var derivedIndexProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddIndex(derivedIndexProperty);

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
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

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetIndexes().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.IdProperty.Name } },
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
                Assert.Throws<ArgumentException>(() => entityType.AddIndex(new[] { idProperty })).Message);
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
                Assert.Throws<InvalidOperationException>(() =>
                    specialOrderType.AddIndex(indexProperty)).Message);
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
                Assert.Throws<InvalidOperationException>(() =>
                    verySpecialOrderType.AddIndex(indexProperty)).Message);
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
                Assert.Throws<InvalidOperationException>(() =>
                    orderType.AddIndex(indexProperty)).Message);
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
                Assert.Throws<InvalidOperationException>(() =>
                    orderType.AddIndex(indexProperty)).Message);
        }

        [Fact]
        public void Can_create_entity_type()
        {
            var entityType = new Model().AddEntityType(typeof(Customer), ConfigurationSource.Convention);

            Assert.Equal(typeof(Customer).FullName, entityType.Name);
            Assert.Same(typeof(Customer), entityType.ClrType);
            Assert.Equal(ConfigurationSource.Convention, entityType.GetConfigurationSource());

            entityType.UpdateConfigurationSource(ConfigurationSource.DataAnnotation);

            Assert.Equal(ConfigurationSource.DataAnnotation, entityType.GetConfigurationSource());
        }

        [Fact]
        public void Can_set_and_reset_CLR_type()
        {
            var entityType = new Model().AddEntityType(typeof(Customer).DisplayName());

            Assert.Equal(typeof(Customer).FullName, entityType.Name);
            Assert.Null(entityType.ClrType);

            entityType.ClrType = typeof(Customer);

            Assert.Equal(typeof(Customer).FullName, entityType.Name);
            Assert.Same(typeof(Customer), entityType.ClrType);

            entityType.ClrType = null;

            Assert.Equal(typeof(Customer).FullName, entityType.Name);
            Assert.Null(entityType.ClrType);
        }

        [Fact]
        public void Cannot_set_CLR_type_if_name_does_not_match()
        {
            var entityType = new Model().AddEntityType(typeof(Customer).Name);

            Assert.Equal(CoreStrings.ClrTypeWrongName(typeof(Customer).DisplayName(), typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.ClrType = typeof(Customer)).Message);
        }

        [Fact]
        public void Cannot_set_CLR_type_if_base_type_derived_type_or_properties_set()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer).DisplayName());

            entityType.AddProperty("Blah");
            Assert.Equal(CoreStrings.EntityTypeInUse(entityType.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => entityType.ClrType = typeof(Customer)).Message);

            entityType.RemoveProperty("Blah");
            entityType.HasBaseType(model.AddEntityType("Base"));
            Assert.Equal(CoreStrings.EntityTypeInUse(entityType.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => entityType.ClrType = typeof(Customer)).Message);

            entityType.HasBaseType(null);
            model.AddEntityType("Derived").HasBaseType(entityType);
            Assert.Equal(CoreStrings.EntityTypeInUse(entityType.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => entityType.ClrType = typeof(Customer)).Message);
        }

        [Fact]
        public void Display_name_is_prettified_CLR_name()
        {
            Assert.Equal("EntityTypeTest", new Model().AddEntityType(typeof(EntityTypeTest)).DisplayName());
            Assert.Equal("Customer", new Model().AddEntityType(typeof(Customer)).DisplayName());
            Assert.Equal("List<Customer>", new Model().AddEntityType(typeof(List<Customer>)).DisplayName());
        }

        [Fact]
        public void Display_name_is_entity_type_name_when_no_CLR_type()
            => Assert.Equal(
                "Everything.Is+Awesome<When.We, re.Living<Our.Dream>>",
                new Model().AddEntityType("Everything.Is+Awesome<When.We, re.Living<Our.Dream>>").DisplayName());

        [Fact]
        public void Name_is_prettified_CLR_full_name()
        {
            Assert.Equal("Microsoft.EntityFrameworkCore.Metadata.Internal.EntityTypeTest", new Model().AddEntityType(typeof(EntityTypeTest)).Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Metadata.Internal.EntityTypeTest+Customer", new Model().AddEntityType(typeof(Customer)).Name);
            Assert.Equal("System.Collections.Generic.List<Microsoft.EntityFrameworkCore.Metadata.Internal.EntityTypeTest+Customer>", new Model().AddEntityType(typeof(List<Customer>)).Name);
        }

        [Fact]
        public void Can_set_reset_and_clear_primary_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);
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

            Assert.Null(entityType.SetPrimaryKey(new Property[0]));

            Assert.Null(entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());
        }

        [Fact]
        public void Setting_primary_key_throws_if_properties_from_different_type()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var idProperty = entityType2.GetOrAddProperty(Customer.IdProperty);

            Assert.Equal(
                CoreStrings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
                Assert.Throws<ArgumentException>(() => entityType1.SetPrimaryKey(idProperty)).Message);
        }

        [Fact]
        public void Can_get_set_reset_and_clear_primary_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;

            var key1 = entityType.GetOrSetPrimaryKey(new[] { idProperty, nameProperty });

            Assert.NotNull(key1);
            Assert.Same(key1, entityType.GetOrSetPrimaryKey(new[] { idProperty, nameProperty }));
            Assert.Same(key1, entityType.FindPrimaryKey());

            Assert.Same(key1, entityType.FindPrimaryKey());
            Assert.Same(key1, entityType.GetKeys().Single());

            var key2 = entityType.GetOrSetPrimaryKey(idProperty);

            Assert.NotNull(key2);
            Assert.NotEqual(key1, key2);
            Assert.Same(key2, entityType.GetOrSetPrimaryKey(idProperty));
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

        [Fact]
        public void Can_clear_the_primary_key_if_it_is_referenced_from_a_foreign_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var customerPk = entityType.GetOrSetPrimaryKey(idProperty);

            var orderType = model.AddEntityType(typeof(Order));
            var fk = orderType.GetOrAddForeignKey(orderType.GetOrAddProperty(Order.CustomerIdProperty), customerPk, entityType);

            entityType.SetPrimaryKey(null);

            Assert.Equal(1, entityType.GetKeys().Count());
            Assert.Same(customerPk, entityType.FindKey(idProperty));
            Assert.Null(entityType.FindPrimaryKey());
            Assert.Same(customerPk, fk.PrincipalKey);
        }

        [Fact]
        public void Can_change_the_primary_key_if_it_is_referenced_from_a_foreign_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);
            var customerPk = entityType.GetOrSetPrimaryKey(idProperty);

            var orderType = model.AddEntityType(typeof(Order));
            var fk = orderType.GetOrAddForeignKey(orderType.GetOrAddProperty(Order.CustomerIdProperty), customerPk, entityType);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;
            entityType.SetPrimaryKey(nameProperty);

            Assert.Equal(2, entityType.GetKeys().Count());
            Assert.Same(customerPk, entityType.FindKey(idProperty));
            Assert.NotSame(customerPk, entityType.FindPrimaryKey());
            Assert.Same(customerPk, fk.PrincipalKey);
        }

        [Fact]
        public void Can_add_and_get_a_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;

            var key1 = entityType.AddKey(new[] { idProperty, nameProperty });

            Assert.NotNull(key1);
            Assert.Same(key1, entityType.GetOrAddKey(new[] { idProperty, nameProperty }));
            Assert.Same(key1, entityType.GetKeys().Single());

            var key2 = entityType.GetOrAddKey(idProperty);

            Assert.NotNull(key2);
            Assert.Same(key2, entityType.FindKey(idProperty));
            Assert.Equal(2, entityType.GetKeys().Count());
            Assert.Contains(key1, entityType.GetKeys());
            Assert.Contains(key2, entityType.GetKeys());
        }

        [Fact]
        public void Adding_a_key_throws_if_properties_from_different_type()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var idProperty = entityType2.GetOrAddProperty(Customer.IdProperty);

            Assert.Equal(
                CoreStrings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
                Assert.Throws<ArgumentException>(() => entityType1.AddKey(idProperty)).Message);
        }

        [Fact]
        public void Adding_a_key_throws_if_duplicated()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;
            entityType.GetOrAddKey(new[] { idProperty, nameProperty });

            Assert.Equal(
                CoreStrings.DuplicateKey("{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty, nameProperty })).Message);
        }

        [Fact]
        public void Adding_a_key_throws_if_properties_were_removed()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            entityType.RemoveProperty(idProperty.Name);

            Assert.Equal(
                CoreStrings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
                Assert.Throws<ArgumentException>(() => entityType.AddKey(new[] { idProperty })).Message);
        }

        [Fact]
        public void Adding_a_key_throws_if_same_as_primary()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;
            entityType.GetOrSetPrimaryKey(new[] { idProperty, nameProperty });

            Assert.Equal(
                CoreStrings.DuplicateKey("{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty, nameProperty })).Message);
        }

        [Fact]
        public void Adding_a_key_throws_if_any_properties_are_part_of_derived_foreign_key()
        {
            var model = new Model();
            var baseType = model.AddEntityType(typeof(BaseType));
            var idProperty = baseType.GetOrAddProperty(Customer.IdProperty);
            var fkProperty = baseType.AddProperty("fk", typeof(int));
            var key = baseType.GetOrAddKey(new[] { idProperty });
            IMutableEntityType entityType = model.AddEntityType(typeof(Customer));
            entityType.BaseType = baseType;
            entityType.AddForeignKey(new[] { fkProperty }, key, entityType);

            Assert.Equal(
                CoreStrings.KeyPropertyInForeignKey("fk", typeof(BaseType).Name),
                Assert.Throws<InvalidOperationException>(() => baseType.GetOrAddKey(new[] { fkProperty })).Message);
        }

        [Fact]
        public void Can_remove_keys()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;

            Assert.Null(entityType.RemoveKey(new[] { idProperty }));

            var key1 = entityType.GetOrSetPrimaryKey(new[] { idProperty, nameProperty });
            var key2 = entityType.GetOrAddKey(idProperty);

            Assert.NotNull(key1.Builder);
            Assert.NotNull(key2.Builder);
            Assert.Equal(new[] { key2, key1 }, entityType.GetKeys().ToArray());

            Assert.Same(key1, entityType.RemoveKey(key1.Properties));
            Assert.Null(entityType.RemoveKey(key1.Properties));

            Assert.Equal(new[] { key2 }, entityType.GetKeys().ToArray());

            Assert.Same(key2, entityType.RemoveKey(new[] { idProperty }));

            Assert.Null(key1.Builder);
            Assert.Null(key2.Builder);
            Assert.Empty(entityType.GetKeys());
        }

        [Fact]
        public void Removing_a_key_throws_if_it_referenced_from_a_foreign_key_in_the_model()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var customerFk = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.GetOrAddForeignKey(customerFk, customerKey, customerType);

            Assert.Equal(
                CoreStrings.KeyInUse("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).FullName, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() => customerType.RemoveKey(customerKey.Properties)).Message);
        }

        [Fact]
        public void Keys_are_ordered_by_property_count_then_property_names()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.AddProperty(Customer.IdProperty);
            var nameProperty = customerType.AddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;
            var otherNameProperty = customerType.AddProperty("OtherNameProperty", typeof(string));
            otherNameProperty.IsNullable = false;

            var k2 = customerType.GetOrAddKey(nameProperty);
            var k4 = customerType.GetOrAddKey(new[] { idProperty, otherNameProperty });
            var k3 = customerType.GetOrAddKey(new[] { idProperty, nameProperty });
            var k1 = customerType.GetOrAddKey(idProperty);

            Assert.True(new[] { k1, k2, k3, k4 }.SequenceEqual(customerType.GetKeys()));
        }

        [Fact]
        public void Key_properties_are_always_read_only_after_save()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;

            Assert.False(((IProperty)idProperty).IsReadOnlyAfterSave);
            Assert.False(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.False(((IProperty)idProperty).IsReadOnlyBeforeSave);
            Assert.False(((IProperty)nameProperty).IsReadOnlyBeforeSave);

            entityType.GetOrAddKey(new[] { idProperty, nameProperty });

            Assert.True(((IProperty)idProperty).IsReadOnlyAfterSave);
            Assert.True(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.False(((IProperty)idProperty).IsReadOnlyBeforeSave);
            Assert.False(((IProperty)nameProperty).IsReadOnlyBeforeSave);

            nameProperty.IsReadOnlyAfterSave = true;

            Assert.Equal(
                CoreStrings.KeyPropertyMustBeReadOnly(Customer.NameProperty.Name, typeof(Customer).FullName),
                Assert.Throws<NotSupportedException>(() => nameProperty.IsReadOnlyAfterSave = false).Message);

            nameProperty.IsReadOnlyBeforeSave = true;

            Assert.True(((IProperty)idProperty).IsReadOnlyAfterSave);
            Assert.True(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.False(((IProperty)idProperty).IsReadOnlyBeforeSave);
            Assert.True(((IProperty)nameProperty).IsReadOnlyBeforeSave);
        }

        [Fact]
        public void Store_computed_values_are_read_only_before_and_after_save_by_default()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);

            Assert.False(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.False(((IProperty)nameProperty).IsReadOnlyBeforeSave);

            nameProperty.ValueGenerated = ValueGenerated.OnAddOrUpdate;

            Assert.True(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.True(((IProperty)nameProperty).IsReadOnlyBeforeSave);

            nameProperty.IsReadOnlyBeforeSave = false;

            Assert.True(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.False(((IProperty)nameProperty).IsReadOnlyBeforeSave);

            nameProperty.IsReadOnlyAfterSave = false;

            Assert.False(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.False(((IProperty)nameProperty).IsReadOnlyBeforeSave);
        }

        [Fact]
        public void Store_always_computed_values_are_not_read_only_before_and_after_save_by_default()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);

            Assert.False(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.False(((IProperty)nameProperty).IsReadOnlyBeforeSave);

            nameProperty.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            nameProperty.IsStoreGeneratedAlways = true;

            Assert.False(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.False(((IProperty)nameProperty).IsReadOnlyBeforeSave);

            nameProperty.IsReadOnlyBeforeSave = true;

            Assert.False(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.True(((IProperty)nameProperty).IsReadOnlyBeforeSave);

            nameProperty.IsReadOnlyAfterSave = true;

            Assert.True(((IProperty)nameProperty).IsReadOnlyAfterSave);
            Assert.True(((IProperty)nameProperty).IsReadOnlyBeforeSave);
        }

        [Fact]
        public void Can_add_a_foreign_key()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.GetOrAddProperty(Customer.IdProperty);
            var customerKey = customerType.GetOrAddKey(idProperty);
            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.GetOrAddProperty(Order.CustomerIdProperty);
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

        [Fact]
        public void Adding_a_foreign_key_throws_if_duplicate()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));
            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.AddForeignKey(customerFk1, customerKey, customerType);

            Assert.Equal(
                CoreStrings.DuplicateForeignKey("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Order).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(() => orderType.AddForeignKey(customerFk1, customerKey, customerType)).Message);
        }

        [Fact]
        public void Adding_a_foreign_key_throws_if_properties_from_different_type()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var idProperty = entityType2.GetOrAddProperty(Order.IdProperty);
            var fkProperty = entityType2.GetOrAddProperty(Order.CustomerIdProperty);

            Assert.Equal(
                CoreStrings.ForeignKeyPropertiesWrongEntity("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Customer).Name),
                Assert.Throws<ArgumentException>(() => entityType1.AddForeignKey(new[] { fkProperty }, entityType2.GetOrAddKey(idProperty), entityType2)).Message);
        }

        [Fact]
        public void Adding_a_foreign_key_throws_if_properties_were_removed()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var key = entityType.GetOrAddKey(idProperty);
            var fkProperty = entityType.AddProperty("fk", typeof(int));
            entityType.RemoveProperty(fkProperty.Name);

            Assert.Equal(
                CoreStrings.ForeignKeyPropertiesWrongEntity("{'fk'}", typeof(Customer).Name),
                Assert.Throws<ArgumentException>(() => entityType.AddForeignKey(new[] { fkProperty }, key, entityType)).Message);
        }

        [Fact]
        public void Adding_a_foreign_key_throws_if_any_properties_are_part_of_inherited_key()
        {
            var model = new Model();
            var baseType = model.AddEntityType(typeof(BaseType));
            var idProperty = baseType.GetOrAddProperty(Customer.IdProperty);
            var idProperty2 = baseType.GetOrAddProperty("id2", typeof(int));
            var key = baseType.GetOrAddKey(new[] { idProperty, idProperty2 });
            IMutableEntityType entityType = model.AddEntityType(typeof(Customer));
            entityType.BaseType = baseType;
            var fkProperty = entityType.AddProperty("fk", typeof(int));

            Assert.Equal(
                CoreStrings.ForeignKeyPropertyInKey(Customer.IdProperty.Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddForeignKey(new[] { fkProperty, idProperty }, key, entityType)).Message);
        }

        [Fact]
        public void Adding_a_foreign_key_throws_if_key_was_removed()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var key = entityType.GetOrAddKey(idProperty);
            entityType.RemoveKey(key.Properties);
            var fkProperty = entityType.AddProperty("fk", typeof(int));

            Assert.Equal(
                CoreStrings.ForeignKeyReferencedEntityKeyMismatch("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<ArgumentException>(() => entityType.AddForeignKey(new[] { fkProperty }, key, entityType)).Message);
        }

        [Fact]
        public void Adding_a_foreign_key_throws_if_related_entity_is_from_different_model()
        {
            var dependentEntityType = new Model().AddEntityType(typeof(Customer));
            var fkProperty = dependentEntityType.GetOrAddProperty(Customer.IdProperty);
            var principalEntityType = new Model().AddEntityType(typeof(Order));
            var idProperty = principalEntityType.GetOrAddProperty(Order.IdProperty);

            Assert.Equal(
                CoreStrings.EntityTypeModelMismatch(typeof(Customer).FullName, typeof(Order).FullName),
                Assert.Throws<ArgumentException>(() => dependentEntityType.AddForeignKey(new[] { fkProperty }, principalEntityType.GetOrAddKey(idProperty), principalEntityType)).Message);
        }

        [Fact]
        public void Can_get_or_add_a_foreign_key()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.GetOrAddProperty(Customer.IdProperty);
            var customerKey = customerType.GetOrAddKey(idProperty);
            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerFk2 = orderType.AddProperty("IdAgain", typeof(int));
            var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);

            var fk2 = orderType.GetOrAddForeignKey(customerFk2, customerKey, customerType);

            Assert.NotNull(fk2);
            Assert.NotEqual(fk1, fk2);
            Assert.Same(fk2, orderType.FindForeignKeys(customerFk2).Single());
            Assert.Same(fk2, orderType.FindForeignKey(customerFk2, customerKey, customerType));
            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());
            Assert.Same(fk2, orderType.GetOrAddForeignKey(customerFk2, customerKey, customerType));
            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());
        }

        private static Model BuildModel()
        {
            var model = new Model();

            var principalType = model.AddEntityType(typeof(PrincipalEntity));
            var property1 = principalType.AddProperty("PeeKay", typeof(int));
            property1.IsShadowProperty = false;
            principalType.GetOrSetPrimaryKey(property1);

            var dependentType = model.AddEntityType(typeof(DependentEntity));
            var property = dependentType.AddProperty("KayPee", typeof(int));
            dependentType.GetOrSetPrimaryKey(property);

            return model;
        }

        private EntityType DependentType => _model.FindEntityType(typeof(DependentEntity));

        private EntityType PrincipalType => _model.FindEntityType(typeof(PrincipalEntity));

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

        [Fact]
        public void Can_remove_foreign_keys()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));
            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerFk2 = orderType.AddProperty("IdAgain", typeof(int));

            Assert.Null(orderType.RemoveForeignKey(new[] { customerFk2 }, customerKey, customerType));

            var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);
            var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);

            Assert.NotNull(fk1.Builder);
            Assert.NotNull(fk2.Builder);
            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());

            Assert.Same(fk1, orderType.RemoveForeignKey(fk1.Properties, fk1.PrincipalKey, fk1.PrincipalEntityType));
            Assert.Null(orderType.RemoveForeignKey(fk1.Properties, fk1.PrincipalKey, fk1.PrincipalEntityType));

            Assert.Equal(new[] { fk2 }, orderType.GetForeignKeys().ToArray());

            Assert.Same(fk2, orderType.RemoveForeignKey(new[] { customerFk2 }, customerKey, customerType));

            Assert.Null(fk1.Builder);
            Assert.Null(fk2.Builder);
            Assert.Empty(orderType.GetForeignKeys());
        }

        [Fact]
        public void Can_remove_a_foreign_key_if_it_is_referenced_from_a_navigation_in_the_model()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var customerFk = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var fk = orderType.GetOrAddForeignKey(customerFk, customerKey, customerType);

            fk.HasDependentToPrincipal("Customer");
            fk.HasPrincipalToDependent("Orders");

            Assert.NotNull(orderType.RemoveForeignKey(fk.Properties, fk.PrincipalKey, fk.PrincipalEntityType));
            Assert.Empty(orderType.GetNavigations());
            Assert.Empty(customerType.GetNavigations());
        }

        [Fact]
        public void Foreign_keys_are_ordered_by_property_count_then_property_names()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = customerType.GetOrAddProperty(Customer.NameProperty);
            nameProperty.IsNullable = false;
            var customerKey = customerType.GetOrAddKey(idProperty);
            var otherCustomerKey = customerType.GetOrAddKey(new[] { idProperty, nameProperty });

            var orderType = model.AddEntityType(typeof(Order));
            var customerFk1 = orderType.GetOrAddProperty(Order.CustomerIdProperty);
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

        [Fact]
        public void Can_add_and_remove_navigations()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            var customerNavigation = customerForeignKey.HasDependentToPrincipal("Customer");
            var ordersNavigation = customerForeignKey.HasPrincipalToDependent("Orders");

            Assert.Equal("Customer", customerNavigation.Name);
            Assert.Same(orderType, customerNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
            Assert.True(customerNavigation.IsDependentToPrincipal());
            Assert.False(customerNavigation.IsCollection());
            Assert.Same(customerType, customerNavigation.GetTargetType());
            Assert.Same(customerNavigation, customerForeignKey.DependentToPrincipal);

            Assert.Equal("Orders", ordersNavigation.Name);
            Assert.Same(customerType, ordersNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, ordersNavigation.ForeignKey);
            Assert.False(ordersNavigation.IsDependentToPrincipal());
            Assert.True(ordersNavigation.IsCollection());
            Assert.Same(orderType, ordersNavigation.GetTargetType());
            Assert.Same(ordersNavigation, customerForeignKey.PrincipalToDependent);

            Assert.Same(customerNavigation, orderType.GetNavigations().Single());
            Assert.Same(ordersNavigation, customerType.GetNavigations().Single());

            Assert.Same(customerNavigation, customerForeignKey.HasDependentToPrincipal(null));
            Assert.Null(customerForeignKey.HasDependentToPrincipal(null));
            Assert.Empty(orderType.GetNavigations());
            Assert.Empty(((IEntityType)orderType).GetNavigations());

            Assert.Same(ordersNavigation, customerForeignKey.HasPrincipalToDependent(null));
            Assert.Null(customerForeignKey.HasPrincipalToDependent(null));
            Assert.Empty(customerType.GetNavigations());
            Assert.Empty(((IEntityType)customerType).GetNavigations());
        }

        [Fact]
        public void Can_add_new_navigations_or_get_existing_navigations()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);
            var customerNavigation = customerForeignKey.HasDependentToPrincipal("Customer");

            Assert.Equal("Customer", customerNavigation.Name);
            Assert.Same(orderType, customerNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
            Assert.True(customerNavigation.IsDependentToPrincipal());
            Assert.False(customerNavigation.IsCollection());
            Assert.Same(customerType, customerNavigation.GetTargetType());

            Assert.Same(customerNavigation, orderType.FindNavigation("Customer"));
            Assert.True(customerNavigation.IsDependentToPrincipal());
        }

        [Fact]
        public void Can_get_navigation_and_can_try_get_navigation()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);
            var customerNavigation = customerForeignKey.HasDependentToPrincipal("Customer");

            Assert.Same(customerNavigation, orderType.FindNavigation("Customer"));
            Assert.Same(customerNavigation, orderType.FindNavigation("Customer"));

            Assert.Null(orderType.FindNavigation("Nose"));
        }

        [Fact]
        public void Adding_a_new_navigation_with_a_name_that_conflicts_with_a_property_throws()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            orderType.AddProperty("Customer");

            Assert.Equal(
                CoreStrings.ConflictingProperty("Customer", typeof(Order).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal("Customer")).Message);
        }

        [Fact]
        public void Adding_a_navigation_to_a_shadow_entity_type_throws()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.AddProperty("Id", typeof(int)));

            var orderType = model.AddEntityType("Order");
            var foreignKeyProperty = orderType.AddProperty("CustomerId", typeof(int));
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationOnShadowEntity("Customer", "Order"),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal("Customer")).Message);
        }

        [Fact]
        public void Adding_a_navigation_pointing_to_a_shadow_entity_type_throws()
        {
            var model = new Model();
            var customerType = model.AddEntityType("Customer");
            var customerKey = customerType.GetOrAddKey(customerType.AddProperty("Id", typeof(int)));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty("CustomerId", typeof(int));
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationToShadowEntity("Customer", typeof(Order).Name, "Customer"),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal("Customer")).Message);
        }

        [Fact]
        public void Adding_a_navigation_that_doesnt_match_a_CLR_property_throws()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NoClrNavigation("Snook", typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal("Snook")).Message);
        }

        [Fact]
        public void Collection_navigation_properties_must_be_IEnumerables_of_the_target_type()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationCollectionWrongClrType(
                    "NotCollectionOrders", typeof(Customer).Name, typeof(Order).FullName, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasPrincipalToDependent("NotCollectionOrders")).Message);
        }

        [Fact]
        public void Collection_navigation_properties_cannot_be_IEnumerables_of_derived_target_type()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationCollectionWrongClrType(
                    "DerivedOrders", typeof(SpecialCustomer).Name, typeof(IEnumerable<SpecialOrder>).FullName, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasPrincipalToDependent("DerivedOrders")).Message);
        }

        [Fact]
        public void Collection_navigation_properties_can_be_IEnumerables_of_base_target_type()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            var ordersNavigation = customerForeignKey.HasPrincipalToDependent("Orders");

            Assert.Equal("Orders", ordersNavigation.Name);
            Assert.Same(customerType, ordersNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, ordersNavigation.ForeignKey);
            Assert.False(ordersNavigation.IsDependentToPrincipal());
            Assert.True(ordersNavigation.IsCollection());
            Assert.Same(orderType, ordersNavigation.GetTargetType());
            Assert.Same(ordersNavigation, customerForeignKey.PrincipalToDependent);
        }

        [Fact]
        public void Reference_navigation_properties_must_be_of_the_target_type()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationSingleWrongClrType("OrderCustomer", typeof(Order).Name, typeof(Order).FullName, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal("OrderCustomer")).Message);
        }

        [Fact]
        public void Reference_navigation_properties_cannot_be_of_derived_type()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationSingleWrongClrType(
                    "DerivedCustomer", typeof(SpecialOrder).Name, typeof(SpecialCustomer).FullName, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal("DerivedCustomer")).Message);
        }

        [Fact]
        public void Reference_navigation_properties_can_be_of_base_type()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(SpecialOrder));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            var customerNavigation = customerForeignKey.HasDependentToPrincipal("Customer");

            Assert.Equal("Customer", customerNavigation.Name);
            Assert.Same(orderType, customerNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
            Assert.True(customerNavigation.IsDependentToPrincipal());
            Assert.False(customerNavigation.IsCollection());
            Assert.Same(customerType, customerNavigation.GetTargetType());
        }

        [Fact]
        public void Can_create_self_referencing_navigations()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(SelfRef));
            var fkProperty = entityType.AddProperty(SelfRef.ForeignKeyProperty);
            var principalKeyProperty = entityType.AddProperty(SelfRef.IdProperty);
            var referencedKey = entityType.SetPrimaryKey(principalKeyProperty);
            var fk = entityType.AddForeignKey(fkProperty, referencedKey, entityType);
            fk.IsUnique = true;

            var navigationToDependent = fk.HasPrincipalToDependent("SelfRef1");
            var navigationToPrincipal = fk.HasDependentToPrincipal("SelfRef2");

            Assert.Same(fk.PrincipalToDependent, navigationToDependent);
            Assert.Same(fk.DependentToPrincipal, navigationToPrincipal);
        }

        [Fact]
        public void Throws_when_adding_same_self_referencing_navigation_twice()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(SelfRef));
            var fkProperty = entityType.AddProperty(SelfRef.ForeignKeyProperty);
            var principalKeyProperty = entityType.AddProperty(SelfRef.IdProperty);
            var referencedKey = entityType.SetPrimaryKey(principalKeyProperty);
            var fk = entityType.AddForeignKey(fkProperty, referencedKey, entityType);
            fk.IsUnique = true;

            fk.HasPrincipalToDependent("SelfRef1");
            Assert.Equal(CoreStrings.DuplicateNavigation("SelfRef1", typeof(SelfRef).Name, typeof(SelfRef).Name),
                Assert.Throws<InvalidOperationException>(() => fk.HasDependentToPrincipal("SelfRef1")).Message);
        }

        [Fact]
        public void Navigations_are_ordered_by_name()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(SpecialCustomer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var customerForeignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.AddForeignKey(customerForeignKeyProperty, customerKey, customerType);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            var specialCustomerForeignKeyProperty = specialOrderType.AddProperty(Order.CustomerIdProperty);
            var specialCustomerForeignKey = specialOrderType.AddForeignKey(specialCustomerForeignKeyProperty, customerKey, customerType);

            var navigation2 = customerForeignKey.HasPrincipalToDependent("Orders");
            var navigation1 = specialCustomerForeignKey.HasPrincipalToDependent("DerivedOrders");

            Assert.True(new[] { navigation1, navigation2 }.SequenceEqual(customerType.GetNavigations()));
            Assert.True(new[] { navigation1, navigation2 }.SequenceEqual(((IEntityType)customerType).GetNavigations()));
        }

        [Fact]
        public void Can_add_retrieve_and_remove_indexes()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Order));
            var property1 = entityType.AddProperty(Order.IdProperty);
            var property2 = entityType.AddProperty(Order.CustomerIdProperty);

            Assert.Equal(0, entityType.GetIndexes().Count());
            Assert.Null(entityType.RemoveIndex(new[] { property1 }));

            var index1 = entityType.GetOrAddIndex(property1);

            Assert.Equal(1, index1.Properties.Count);
            Assert.Same(index1, entityType.FindIndex(property1));
            Assert.Same(index1, entityType.FindIndex(property1));
            Assert.Same(property1, index1.Properties[0]);

            var index2 = entityType.AddIndex(new[] { property1, property2 });

            Assert.NotNull(index1.Builder);
            Assert.NotNull(index2.Builder);
            Assert.Equal(2, index2.Properties.Count);
            Assert.Same(index2, entityType.GetOrAddIndex(new[] { property1, property2 }));
            Assert.Same(index2, entityType.FindIndex(new[] { property1, property2 }));
            Assert.Same(property1, index2.Properties[0]);
            Assert.Same(property2, index2.Properties[1]);

            Assert.Equal(2, entityType.GetIndexes().Count());
            Assert.Same(index1, entityType.GetIndexes().First());
            Assert.Same(index2, entityType.GetIndexes().Last());

            Assert.Same(index1, entityType.RemoveIndex(index1.Properties));
            Assert.Null(entityType.RemoveIndex(index1.Properties));

            Assert.Equal(1, entityType.GetIndexes().Count());
            Assert.Same(index2, entityType.GetIndexes().Single());

            Assert.Same(index2, entityType.RemoveIndex(new[] { property1, property2 }));

            Assert.Null(index1.Builder);
            Assert.Null(index2.Builder);
            Assert.Equal(0, entityType.GetIndexes().Count());
        }

        [Fact]
        public void AddIndex_throws_if_not_from_same_entity()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var property1 = entityType1.AddProperty(Customer.IdProperty);
            var property2 = entityType1.AddProperty(Customer.NameProperty);

            Assert.Equal(CoreStrings.IndexPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Order).Name),
                Assert.Throws<ArgumentException>(
                    () => entityType2.AddIndex(new[] { property1, property2 })).Message);
        }

        [Fact]
        public void AddIndex_throws_if_duplicate()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);
            entityType.AddIndex(new[] { property1, property2 });

            Assert.Equal(CoreStrings.DuplicateIndex("{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.AddIndex(new[] { property1, property2 })).Message);
        }

        [Fact]
        public void Can_add_and_remove_properties()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            Assert.Null(entityType.RemoveProperty("Id"));

            var property1 = entityType.AddProperty("Id", typeof(int));
            property1.IsShadowProperty = false;

            Assert.False(property1.IsShadowProperty);
            Assert.Equal("Id", property1.Name);
            Assert.Same(typeof(int), property1.ClrType);
            Assert.False(((IProperty)property1).IsConcurrencyToken);
            Assert.Same(entityType, property1.DeclaringEntityType);

            var property2 = entityType.AddProperty("Name", typeof(string));
            property2.IsShadowProperty = false;

            Assert.NotNull(property1.Builder);
            Assert.NotNull(property2.Builder);
            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.GetProperties()));

            Assert.Same(property1, entityType.RemoveProperty(property1.Name));
            Assert.Null(entityType.RemoveProperty(property1.Name));

            Assert.True(new[] { property2 }.SequenceEqual(entityType.GetProperties()));

            Assert.Same(property2, entityType.RemoveProperty("Name"));

            Assert.Null(property1.Builder);
            Assert.Null(property2.Builder);
            Assert.Empty(entityType.GetProperties());
        }

        [Fact]
        public void Can_add_new_properties_or_get_existing_properties_using_PropertyInfo_or_name()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            var idProperty = entityType.AddProperty("Id", typeof(int));
            idProperty.IsShadowProperty = false;

            Assert.False(idProperty.IsShadowProperty);
            Assert.Equal("Id", idProperty.Name);
            Assert.Same(typeof(int), idProperty.ClrType);
            Assert.Same(entityType, idProperty.DeclaringEntityType);

            Assert.Same(idProperty, entityType.GetOrAddProperty(Customer.IdProperty));
            Assert.Same(idProperty, entityType.GetOrAddProperty("Id"));
            Assert.False(idProperty.IsShadowProperty);

            var nameProperty = entityType.GetOrAddProperty("Name");
            nameProperty.ClrType = typeof(string);

            Assert.True(((IProperty)nameProperty).IsShadowProperty);
            Assert.Equal("Name", nameProperty.Name);
            Assert.Same(typeof(string), nameProperty.ClrType);
            Assert.Same(entityType, nameProperty.DeclaringEntityType);

            Assert.Same(nameProperty, entityType.GetOrAddProperty(Customer.NameProperty));
            Assert.Same(nameProperty, entityType.FindProperty("Name"));
            Assert.False(nameProperty.IsShadowProperty);

            Assert.True(new[] { idProperty, nameProperty }.SequenceEqual(entityType.GetProperties()));
        }

        [Fact]
        public void Cannot_remove_property_when_used_by_primary_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.GetOrAddProperty(Customer.IdProperty);

            entityType.GetOrSetPrimaryKey(property);

            Assert.Equal(
                CoreStrings.PropertyInUse("Id", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property.Name)).Message);
        }

        [Fact]
        public void Cannot_remove_property_when_used_by_non_primary_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.GetOrAddProperty(Customer.IdProperty);

            entityType.GetOrAddKey(property);

            Assert.Equal(
                CoreStrings.PropertyInUse("Id", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property.Name)).Message);
        }

        [Fact]
        public void Cannot_remove_property_when_used_by_foreign_key()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerPk = customerType.GetOrSetPrimaryKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var customerFk = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.GetOrAddForeignKey(customerFk, customerPk, customerType);

            Assert.Equal(
                CoreStrings.PropertyInUse("CustomerId", typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() => orderType.RemoveProperty(customerFk.Name)).Message);
        }

        [Fact]
        public void Cannot_remove_property_when_used_by_an_index()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.GetOrAddProperty(Customer.IdProperty);

            entityType.GetOrAddIndex(property);

            Assert.Equal(
                CoreStrings.PropertyInUse("Id", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property.Name)).Message);
        }

        [Fact]
        public void Properties_are_ordered_by_name()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            var property2 = entityType.AddProperty(Customer.NameProperty);
            var property1 = entityType.AddProperty(Customer.IdProperty);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.GetProperties()));
        }

        [Fact]
        public void Primary_key_properties_precede_others()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            var aProperty = entityType.AddProperty("A", typeof(int));
            var pkProperty = entityType.AddProperty(Customer.IdProperty);

            entityType.SetPrimaryKey(pkProperty);

            Assert.True(new[] { pkProperty, aProperty }.SequenceEqual(entityType.GetProperties()));
        }

        [Fact]
        public void Composite_primary_key_properties_are_listed_in_key_order()
        {
            var model = new Model();
            var entityType = model.AddEntityType("CompositeKeyType");

            var aProperty = entityType.AddProperty("A", typeof(int));
            var pkProperty2 = entityType.AddProperty("aPK", typeof(int));
            var pkProperty1 = entityType.AddProperty("bPK", typeof(int));

            entityType.SetPrimaryKey(new[] { pkProperty1, pkProperty2 });

            Assert.True(new[] { pkProperty1, pkProperty2, aProperty }.SequenceEqual(entityType.GetProperties()));
        }

        [Fact]
        public void Properties_on_base_type_are_listed_before_derived_properties()
        {
            var model = new Model();

            var parentType = model.AddEntityType("Parent");
            var property2 = parentType.AddProperty("D", typeof(int));
            var property1 = parentType.AddProperty("C", typeof(int));

            var childType = model.AddEntityType("Child");
            var property4 = childType.AddProperty("B", typeof(int));
            var property3 = childType.AddProperty("A", typeof(int));
            childType.HasBaseType(parentType);

            Assert.True(new[] { property1, property2, property3, property4 }.SequenceEqual(childType.GetProperties()));
        }

        [Fact]
        public void Properties_are_properly_ordered_when_primary_key_changes()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            var aProperty = entityType.AddProperty("A", typeof(int));
            var bProperty = entityType.AddProperty("B", typeof(int));

            entityType.SetPrimaryKey(bProperty);

            Assert.True(new[] { bProperty, aProperty }.SequenceEqual(entityType.GetProperties()));

            entityType.SetPrimaryKey(aProperty);

            Assert.True(new[] { aProperty, bProperty }.SequenceEqual(entityType.GetProperties()));
        }

        [Fact]
        public void Can_get_property_and_can_try_get_property()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.GetOrAddProperty(Customer.IdProperty);

            Assert.Same(property, entityType.FindProperty(Customer.IdProperty));
            Assert.Same(property, entityType.FindProperty("Id"));
            Assert.Same(property, entityType.FindProperty(Customer.IdProperty));
            Assert.Same(property, entityType.FindProperty("Id"));

            Assert.Null(entityType.FindProperty("Nose"));
        }

        [Fact]
        public void Shadow_properties_have_CLR_flag_set_to_false()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            entityType.GetOrAddProperty(Customer.NameProperty);
            entityType.AddProperty(Customer.IdProperty);
            entityType.AddProperty("Mane", typeof(int));

            Assert.False(entityType.FindProperty("Name").IsShadowProperty);
            Assert.False(entityType.FindProperty("Id").IsShadowProperty);
            Assert.True(entityType.FindProperty("Mane").IsShadowProperty);
        }

        [Fact]
        public void Adding_a_new_property_with_a_name_that_already_exists_throws()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            entityType.AddProperty(Customer.IdProperty);

            Assert.Equal(
                CoreStrings.DuplicateProperty("Id", typeof(Customer).Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.AddProperty("Id")).Message);
        }

        [Fact]
        public void Adding_a_new_property_with_a_name_that_conflicts_with_a_navigation_throws()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            customerForeignKey.HasDependentToPrincipal("Customer");

            Assert.Equal(
                CoreStrings.ConflictingNavigation("Customer", typeof(Order).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(() => orderType.AddProperty("Customer")).Message);
        }

        [Fact]
        public void Adding_a_CLR_property_from_wrong_CLR_type_throws()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Equal(
                CoreStrings.PropertyWrongEntityClrType(Order.CustomerIdProperty.Name, typeof(Customer).Name, typeof(Order).Name),
                Assert.Throws<ArgumentException>(() => entityType.GetOrAddProperty(Order.CustomerIdProperty)).Message);
        }

        [Fact]
        public void Adding_a_CLR_property_to_shadow_type_throws()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer).Name);

            Assert.Equal(
                CoreStrings.ClrPropertyOnShadowEntity(Order.CustomerIdProperty.Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(() => entityType.GetOrAddProperty(Order.CustomerIdProperty)).Message);
        }

        [Fact]
        public void Can_get_property_indexes()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            entityType.GetOrAddProperty(Customer.NameProperty);
            entityType.AddProperty("Id", typeof(int));
            entityType.AddProperty("Mane", typeof(int));

            Assert.Equal(0, entityType.FindProperty("Id").GetIndex());
            Assert.Equal(1, entityType.FindProperty("Mane").GetIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetShadowIndex());
            Assert.Equal(1, entityType.FindProperty("Mane").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetShadowIndex());

            Assert.Equal(2, entityType.ShadowPropertyCount());
        }

        [Fact]
        public void Indexes_are_rebuilt_when_more_properties_added_or_relevant_state_changes()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(FullNotificationEntity));

            var nameProperty = entityType.AddProperty("Name", typeof(string));
            nameProperty.IsShadowProperty = false;
            var property = entityType.AddProperty("Id", typeof(int)).IsConcurrencyToken = true;

            Assert.Equal(0, entityType.FindProperty("Id").GetIndex());
            Assert.Equal(1, entityType.FindProperty("Name").GetIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetShadowIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetOriginalValueIndex());

            Assert.Equal(1, entityType.ShadowPropertyCount());
            Assert.Equal(1, entityType.OriginalValueCount());

            var gameProperty = entityType.AddProperty("Game", typeof(int));
            gameProperty.IsConcurrencyToken = true;

            var maneProperty = entityType.AddProperty("Mane", typeof(int));
            maneProperty.IsConcurrencyToken = true;

            Assert.Equal(0, entityType.FindProperty("Game").GetIndex());
            Assert.Equal(1, entityType.FindProperty("Id").GetIndex());
            Assert.Equal(2, entityType.FindProperty("Mane").GetIndex());
            Assert.Equal(3, entityType.FindProperty("Name").GetIndex());

            Assert.Equal(0, entityType.FindProperty("Game").GetShadowIndex());
            Assert.Equal(1, entityType.FindProperty("Id").GetShadowIndex());
            Assert.Equal(2, entityType.FindProperty("Mane").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetShadowIndex());

            Assert.Equal(0, entityType.FindProperty("Game").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Mane").GetOriginalValueIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetOriginalValueIndex());

            Assert.Equal(3, entityType.ShadowPropertyCount());
            Assert.Equal(3, entityType.OriginalValueCount());

            gameProperty.IsConcurrencyToken = false;
            nameProperty.IsConcurrencyToken = true;

            Assert.Equal(0, entityType.FindProperty("Game").GetIndex());
            Assert.Equal(1, entityType.FindProperty("Id").GetIndex());
            Assert.Equal(2, entityType.FindProperty("Mane").GetIndex());
            Assert.Equal(3, entityType.FindProperty("Name").GetIndex());

            Assert.Equal(0, entityType.FindProperty("Game").GetShadowIndex());
            Assert.Equal(1, entityType.FindProperty("Id").GetShadowIndex());
            Assert.Equal(2, entityType.FindProperty("Mane").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetShadowIndex());

            Assert.Equal(-1, entityType.FindProperty("Game").GetOriginalValueIndex());
            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("Mane").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetOriginalValueIndex());

            Assert.Equal(3, entityType.ShadowPropertyCount());
            Assert.Equal(3, entityType.OriginalValueCount());

            gameProperty.IsShadowProperty = false;
            nameProperty.IsShadowProperty = true;

            Assert.Equal(0, entityType.FindProperty("Game").GetIndex());
            Assert.Equal(1, entityType.FindProperty("Id").GetIndex());
            Assert.Equal(2, entityType.FindProperty("Mane").GetIndex());
            Assert.Equal(3, entityType.FindProperty("Name").GetIndex());

            Assert.Equal(-1, entityType.FindProperty("Game").GetShadowIndex());
            Assert.Equal(0, entityType.FindProperty("Id").GetShadowIndex());
            Assert.Equal(1, entityType.FindProperty("Mane").GetShadowIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetShadowIndex());

            Assert.Equal(-1, entityType.FindProperty("Game").GetOriginalValueIndex());
            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("Mane").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetOriginalValueIndex());

            Assert.Equal(3, entityType.ShadowPropertyCount());
            Assert.Equal(3, entityType.OriginalValueCount());
        }

        [Fact]
        public void Indexes_are_ordered_by_property_count_then_property_names()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = customerType.GetOrAddProperty(Customer.NameProperty);
            var otherProperty = customerType.AddProperty("OtherProperty", typeof(string));

            var i2 = customerType.AddIndex(nameProperty);
            var i4 = customerType.AddIndex(new[] { idProperty, otherProperty });
            var i3 = customerType.AddIndex(new[] { idProperty, nameProperty });
            var i1 = customerType.AddIndex(idProperty);

            Assert.True(new[] { i1, i2, i3, i4 }.SequenceEqual(customerType.GetIndexes()));
        }

        [Fact]
        public void Lazy_original_values_are_used_for_full_notification_and_shadow_enties()
        {
            Assert.False(new Model().AddEntityType(typeof(FullNotificationEntity)).UseEagerSnapshots);
        }

        [Fact]
        public void Lazy_original_values_are_used_for_shadow_enties()
        {
            Assert.False(new Model().AddEntityType("Z'ha'dum").UseEagerSnapshots);
        }

        [Fact]
        public void Lazy_original_values_are_used_for_enties_that_are_made_shadow()
        {
            var entityType = new Model().AddEntityType(typeof(ChangedOnlyEntity));
            entityType.ClrType = null;
            Assert.False(entityType.UseEagerSnapshots);
        }

        [Fact]
        public void Eager_original_values_are_used_for_enties_that_only_implement_INotifyPropertyChanged()
        {
            Assert.True(new Model().AddEntityType(typeof(ChangedOnlyEntity)).UseEagerSnapshots);
        }

        [Fact]
        public void Eager_original_values_are_used_for_enties_that_do_no_notification()
        {
            Assert.True(new Model().AddEntityType(typeof(Customer)).UseEagerSnapshots);
        }

        [Fact]
        public void Lazy_original_values_can_be_switched_off()
        {
            var entityType = new Model().AddEntityType(typeof(FullNotificationEntity));
            entityType.UseEagerSnapshots = false;
            Assert.False(entityType.UseEagerSnapshots);
        }

        [Fact]
        public void Lazy_original_values_can_be_switched_on_but_only_if_entity_does_not_require_eager_values()
        {
            var entityType = new Model().AddEntityType(typeof(FullNotificationEntity));
            entityType.UseEagerSnapshots = true;
            entityType.UseEagerSnapshots = false;
            Assert.False(entityType.UseEagerSnapshots);

            entityType = new Model().AddEntityType(typeof(ChangedOnlyEntity));
            Assert.Equal(
                CoreStrings.EagerOriginalValuesRequired(typeof(ChangedOnlyEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => entityType.UseEagerSnapshots = false).Message);
        }

        [Fact]
        public void All_properties_have_original_value_indexes_when_using_eager_original_values()
        {
            var entityType = new Model().AddEntityType(typeof(FullNotificationEntity));
            entityType.UseEagerSnapshots = true;
            entityType.AddProperty(FullNotificationEntity.NameProperty);
            entityType.AddProperty(FullNotificationEntity.IdProperty);

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("Name").GetOriginalValueIndex());

            Assert.Equal(2, entityType.OriginalValueCount());
        }

        [Fact]
        public void Only_required_properties_have_original_value_indexes_when_using_lazy_original_values()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(FullNotificationEntity));

            entityType.AddProperty(FullNotificationEntity.NameProperty).IsConcurrencyToken = true;
            entityType.AddProperty(FullNotificationEntity.IdProperty);

            Assert.Equal(-1, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(0, entityType.FindProperty("Name").GetOriginalValueIndex());

            Assert.Equal(1, entityType.OriginalValueCount());
        }

        [Fact]
        public void FK_properties_are_marked_as_requiring_original_values()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(FullNotificationEntity));
            var key = entityType.GetOrSetPrimaryKey(entityType.AddProperty(FullNotificationEntity.IdProperty));
            var fkProperty = entityType.AddProperty("Fk", typeof(int));

            Assert.Equal(-1, fkProperty.GetOriginalValueIndex());

            entityType.GetOrAddForeignKey(new[] { fkProperty }, key, entityType);

            Assert.Equal(0, fkProperty.GetOriginalValueIndex());
        }

        private class BaseType
        {
            public int Id { get; set; }
        }

        private class Customer : BaseType
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public Guid Unique { get; set; }
            public string Name { get; set; }
            public string Mane { get; set; }
            public ICollection<Order> Orders { get; set; }

            public IEnumerable<Order> EnumerableOrders { get; set; }
            public Order NotCollectionOrders { get; set; }
        }

        private class SpecialCustomer : Customer
        {
            public IEnumerable<SpecialOrder> DerivedOrders { get; set; }
        }

        private class VerySpecialCustomer : SpecialCustomer
        {
        }

        private class Order : BaseType
        {
            public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");
            public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty("CustomerUnique");

            public int CustomerId { get; set; }
            public Guid CustomerUnique { get; set; }
            public Customer Customer { get; set; }

            public Order OrderCustomer { get; set; }
        }

        private class SpecialOrder : Order
        {
            public SpecialCustomer DerivedCustomer { get; set; }
        }

        private class VerySpecialOrder : SpecialOrder
        {
        }

        private class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            public static readonly PropertyInfo IdProperty = typeof(FullNotificationEntity).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(FullNotificationEntity).GetProperty("Name");

            private int _id;
            private string _name;
            private int _game;

            public int Id
            {
                get { return _id; }
                set
                {
                    if (_id != value)
                    {
                        NotifyChanging();
                        _id = value;
                        NotifyChanged();
                    }
                }
            }

            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name != value)
                    {
                        NotifyChanging();
                        _name = value;
                        NotifyChanged();
                    }
                }
            }

            public int Game
            {
                get { return _game; }
                set
                {
                    if (_game != value)
                    {
                        NotifyChanging();
                        _game = value;
                        NotifyChanged();
                    }
                }
            }

            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            private void NotifyChanging([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanging != null)
                {
                    PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
                }
            }
        }

        private class ChangedOnlyEntity : INotifyPropertyChanged
        {
            private int _id;
            private string _name;

            public int Id
            {
                get { return _id; }
                set
                {
                    if (_id != value)
                    {
                        _id = value;
                        NotifyChanged();
                    }
                }
            }

            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name != value)
                    {
                        _name = value;
                        NotifyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        private class SelfRef
        {
            public static readonly PropertyInfo IdProperty = typeof(SelfRef).GetProperty("Id");
            public static readonly PropertyInfo ForeignKeyProperty = typeof(SelfRef).GetProperty("ForeignKey");

            public int Id { get; set; }
            public SelfRef SelfRef1 { get; set; }
            public SelfRef SelfRef2 { get; set; }
            public int ForeignKey { get; set; }
        }
    }
}
