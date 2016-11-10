// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Moq;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable ImplicitlyCapturedClosure
namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Internal
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
        public void Use_of_custom_IEntityType_throws()
        {
            Assert.Equal(
                CoreStrings.CustomMetadata(nameof(Use_of_custom_IEntityType_throws), nameof(IEntityType), "IEntityTypeProxy"),
                Assert.Throws<NotSupportedException>(() => Mock.Of<IEntityType>().AsEntityType()).Message);
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
                CoreStrings.CircularInheritance(a.DisplayName(), a.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => { a.HasBaseType(a); }).Message);

            Assert.Equal(
                CoreStrings.CircularInheritance(a.DisplayName(), b.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => { a.HasBaseType(b); }).Message);

            Assert.Equal(
                CoreStrings.CircularInheritance(a.DisplayName(), d.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => { a.HasBaseType(d); }).Message);
        }

        [Fact]
        public void Setting_CLR_base_for_shadow_entity_type_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B).Name);

            Assert.Equal(
                CoreStrings.NonShadowBaseType(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => { b.HasBaseType(a); }).Message);
        }

        [Fact]
        public void Setting_shadow_base_for_CLR_entity_type_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A).Name);
            var b = model.AddEntityType(typeof(B));

            Assert.Equal(
                CoreStrings.NonClrBaseType(typeof(B).Name, typeof(A).Name),
                Assert.Throws<InvalidOperationException>(() => { b.HasBaseType(a); }).Message);
        }

        [Fact]
        public void Setting_not_assignable_base_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));

            Assert.Equal(
                CoreStrings.NotAssignableClrBaseType(typeof(A).Name, typeof(B).Name, typeof(A).Name, typeof(B).Name),
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
                CoreStrings.DuplicatePropertiesOnBase(typeof(B).Name, typeof(A).Name, typeof(B).Name, "G", typeof(A).Name, "G"),
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
                CoreStrings.DuplicatePropertiesOnBase(typeof(D).Name, typeof(C).Name, typeof(D).Name, "E", typeof(A).Name, "E"),
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
                CoreStrings.DuplicatePropertiesOnBase(typeof(C).Name, typeof(A).Name, typeof(D).Name, "E", typeof(A).Name, "E"),
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
                CoreStrings.DerivedEntityCannotHaveKeys(typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => { b.HasBaseType(a); }).Message);

            b.RemoveKey(key.Properties);
            var f = b.AddProperty(B.FProperty);
            f.IsNullable = false;
            b.SetPrimaryKey(f);

            Assert.Equal(
                CoreStrings.DerivedEntityCannotHaveKeys(typeof(B).Name),
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

            customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            Assert.Equal(new[] { "Orders" }, customerType.GetNavigations().Select(p => p.Name).ToArray());
            Assert.Equal(new string[0], specialCustomerType.GetNavigations().Select(p => p.Name).ToArray());

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
                Assert.Throws<InvalidOperationException>(() =>
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
            specialCustomerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

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
            specialCustomerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.Equal(
                CoreStrings.NavigationForWrongForeignKey(nameof(Customer.Orders), typeof(VerySpecialCustomer).Name, "{'CustomerId'}", "{'Id'}"),
                Assert.Throws<InvalidOperationException>(() =>
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
                CoreStrings.DuplicateForeignKey(
                    Property.Format(new[] { foreignKeyProperty }),
                    typeof(SpecialOrder).Name,
                    typeof(Order).Name,
                    Property.Format(customerKey.Properties),
                    typeof(Customer).Name),
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
                CoreStrings.DuplicateForeignKey(
                    Property.Format(new[] { foreignKeyProperty }),
                    typeof(VerySpecialOrder).Name,
                    typeof(Order).Name,
                    Property.Format(customerKey.Properties),
                    typeof(Customer).Name),
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
                CoreStrings.DuplicateForeignKey(
                    Property.Format(new[] { foreignKeyProperty }),
                    typeof(Order).Name,
                    typeof(SpecialOrder).Name,
                    Property.Format(customerKey.Properties),
                    typeof(Customer).Name),
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
                CoreStrings.DuplicateForeignKey(
                    Property.Format(new[] { foreignKeyProperty }),
                    typeof(Order).Name,
                    typeof(VerySpecialOrder).Name,
                    Property.Format(customerKey.Properties),
                    typeof(Customer).Name),
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
                Assert.Throws<InvalidOperationException>(() => entityType.AddIndex(new[] { idProperty })).Message);
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
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Metadata.Internal.EntityTypeTest", new Model().AddEntityType(typeof(EntityTypeTest)).Name);
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Metadata.Internal.EntityTypeTest+Customer", new Model().AddEntityType(typeof(Customer)).Name);
            Assert.Equal("System.Collections.Generic.List<Microsoft.EntityFrameworkCore.Tests.Metadata.Internal.EntityTypeTest+Customer>", new Model().AddEntityType(typeof(List<Customer>)).Name);
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
                Assert.Throws<InvalidOperationException>(() => entityType1.SetPrimaryKey(idProperty)).Message);
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
                Assert.Throws<InvalidOperationException>(() => entityType1.AddKey(idProperty)).Message);
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
                Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty })).Message);
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
            Assert.False(idProperty.IsKey());
            Assert.Empty(idProperty.GetContainingKeys());

            var key1 = entityType.GetOrSetPrimaryKey(new[] { idProperty, nameProperty });
            var key2 = entityType.GetOrAddKey(idProperty);

            Assert.NotNull(key1.Builder);
            Assert.NotNull(key2.Builder);
            Assert.Equal(new[] { key2, key1 }, entityType.GetKeys().ToArray());
            Assert.True(idProperty.IsKey());
            Assert.Equal(new[] { key1, key2 }, idProperty.GetContainingKeys().ToArray());

            Assert.Same(key1, entityType.RemoveKey(key1.Properties));
            Assert.Null(entityType.RemoveKey(key1.Properties));

            Assert.Equal(new[] { key2 }, entityType.GetKeys().ToArray());

            Assert.Same(key2, entityType.RemoveKey(new[] { idProperty }));

            Assert.Null(key1.Builder);
            Assert.Null(key2.Builder);
            Assert.Empty(entityType.GetKeys());
            Assert.False(idProperty.IsKey());
            Assert.Empty(idProperty.GetContainingKeys());
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
                CoreStrings.KeyInUse("{'" + Customer.IdProperty.Name + "'}", nameof(Customer), nameof(Order)),
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
                CoreStrings.KeyPropertyMustBeReadOnly(Customer.NameProperty.Name, nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => nameProperty.IsReadOnlyAfterSave = false).Message);

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
        public void Can_add_a_foreign_key_targetting_different_key()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey1 = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));
            var orderType = model.AddEntityType(typeof(Order));
            var customerFkProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

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

        [Fact]
        public void Can_add_a_foreign_key_targetting_different_entity_type()
        {
            var model = new Model();
            var baseType = model.AddEntityType(typeof(BaseType));
            var customerType = model.AddEntityType(typeof(Customer));
            customerType.HasBaseType(baseType);
            var customerKey1 = baseType.GetOrAddKey(baseType.GetOrAddProperty(Customer.IdProperty));
            var orderType = model.AddEntityType(typeof(Order));
            var customerFkProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

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
                CoreStrings.DuplicateForeignKey(
                    "{'" + Order.CustomerIdProperty.Name + "'}",
                    typeof(Order).Name,
                    typeof(Order).Name,
                    "{'" + Customer.IdProperty.Name + "'}",
                    typeof(Customer).Name),
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
                Assert.Throws<InvalidOperationException>(() => entityType1.AddForeignKey(new[] { fkProperty }, entityType2.GetOrAddKey(idProperty), entityType2)).Message);
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
                Assert.Throws<InvalidOperationException>(() => entityType.AddForeignKey(new[] { fkProperty }, key, entityType)).Message);
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
                CoreStrings.ForeignKeyPropertyInKey(
                    Customer.IdProperty.Name,
                    typeof(Customer).Name,
                    "{'" + Customer.IdProperty.Name + "'" + ", 'id2'}",
                    typeof(BaseType).Name),
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
                CoreStrings.ForeignKeyReferencedEntityKeyMismatch("{'" + Customer.IdProperty.Name + "'}", nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() => entityType.AddForeignKey(new[] { fkProperty }, key, entityType)).Message);
        }

        [Fact]
        public void Adding_a_foreign_key_throws_if_related_entity_is_from_different_model()
        {
            var dependentEntityType = new Model().AddEntityType(typeof(Customer));
            var fkProperty = dependentEntityType.GetOrAddProperty(Customer.IdProperty);
            var principalEntityType = new Model().AddEntityType(typeof(Order));
            var idProperty = principalEntityType.GetOrAddProperty(Order.IdProperty);

            Assert.Equal(
                CoreStrings.EntityTypeModelMismatch(nameof(Customer), nameof(Order)),
                Assert.Throws<InvalidOperationException>(() => dependentEntityType.AddForeignKey(new[] { fkProperty }, principalEntityType.GetOrAddKey(idProperty), principalEntityType)).Message);
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
            Assert.False(customerFk1.IsForeignKey());
            Assert.Empty(customerFk1.GetContainingForeignKeys());

            var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);
            var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);

            Assert.NotNull(fk1.Builder);
            Assert.NotNull(fk2.Builder);
            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());
            Assert.True(customerFk1.IsForeignKey());
            Assert.Same(fk1, customerFk1.GetContainingForeignKeys().Single());

            Assert.Same(fk1, orderType.RemoveForeignKey(fk1.Properties, fk1.PrincipalKey, fk1.PrincipalEntityType));
            Assert.Null(orderType.RemoveForeignKey(fk1.Properties, fk1.PrincipalKey, fk1.PrincipalEntityType));

            Assert.Equal(new[] { fk2 }, orderType.GetForeignKeys().ToArray());
            Assert.False(customerFk1.IsForeignKey());
            Assert.Empty(customerFk1.GetContainingForeignKeys());

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

            fk.HasDependentToPrincipal(Order.CustomerProperty);
            fk.HasPrincipalToDependent(Customer.OrdersProperty);

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

        [Fact]
        public void Can_add_new_navigations_or_get_existing_navigations()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);
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

        [Fact]
        public void Can_get_navigation_and_can_try_get_navigation()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);
            var customerNavigation = customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

            Assert.Same(customerNavigation, orderType.FindNavigation(nameof(Order.Customer)));
            Assert.Same(customerNavigation, orderType.FindNavigation(nameof(Order.Customer)));

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
        public void Can_add_a_navigation_to_shadow_entity()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.AddProperty("Id", typeof(int)));

            var orderType = model.AddEntityType("Order");
            var foreignKeyProperty = orderType.AddProperty("CustomerId", typeof(int));
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.NotNull(customerForeignKey.HasDependentToPrincipal("Customer"));
        }

        [Fact]
        public void Adding_a_navigation_on_non_shadow_entity_type_pointing_to_a_shadow_entity_type_throws()
        {
            var model = new Model();
            var customerType = model.AddEntityType("Customer");
            var customerKey = customerType.GetOrAddKey(customerType.AddProperty("Id", typeof(int)));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty("CustomerId", typeof(int));
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NavigationToShadowEntity(nameof(Order.Customer), typeof(Order).Name, "Customer"),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty)).Message);
        }

        [Fact]
        public void Adding_a_shadow_navigation_on_a_non_shadow_entity_type_throws()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.AddProperty("Id", typeof(int)));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.AddProperty("CustomerId", typeof(int));
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                CoreStrings.NoClrNavigation("Navigation", typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal("Navigation")).Message);
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
                    nameof(Customer.NotCollectionOrders), typeof(Customer).Name, typeof(Order).Name, typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasPrincipalToDependent(Customer.NotCollectionOrdersProperty)).Message);
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
                    nameof(SpecialCustomer.DerivedOrders),
                    typeof(SpecialCustomer).Name,
                    typeof(IEnumerable<SpecialOrder>).ShortDisplayName(),
                    typeof(Order).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty)).Message);
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

            var ordersNavigation = customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);

            Assert.Equal(nameof(Customer.Orders), ordersNavigation.Name);
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
                CoreStrings.NavigationSingleWrongClrType("OrderCustomer", typeof(Order).Name, typeof(Order).Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal(Order.OrderCustomerProperty)).Message);
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
                    nameof(SpecialOrder.DerivedCustomer), typeof(SpecialOrder).Name, typeof(SpecialCustomer).Name, typeof(Customer).Name),
                Assert.Throws<InvalidOperationException>(
                    () => customerForeignKey.HasDependentToPrincipal(SpecialOrder.DerivedCustomerProperty)).Message);
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

            var customerNavigation = customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

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

            var navigationToDependent = fk.HasPrincipalToDependent(SelfRef.SelfRef1Property);
            var navigationToPrincipal = fk.HasDependentToPrincipal(SelfRef.SelfRef2Property);

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

            fk.HasPrincipalToDependent(SelfRef.SelfRef1Property);
            Assert.Equal(CoreStrings.DuplicateNavigation(nameof(SelfRef.SelfRef1), typeof(SelfRef).Name, typeof(SelfRef).Name),
                Assert.Throws<InvalidOperationException>(() => fk.HasDependentToPrincipal(SelfRef.SelfRef1Property)).Message);
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

            var navigation2 = customerForeignKey.HasPrincipalToDependent(Customer.OrdersProperty);
            var navigation1 = specialCustomerForeignKey.HasPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty);

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
            Assert.False(property1.IsIndex());
            Assert.Empty(property1.GetContainingIndexes());

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
            Assert.True(property1.IsIndex());
            Assert.Equal(new[] { index1, index2 }, property1.GetContainingIndexes().ToArray());

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
            Assert.False(property1.IsIndex());
            Assert.Empty(property1.GetContainingIndexes());
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
                Assert.Throws<InvalidOperationException>(
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

            Assert.False(property1.IsShadowProperty);
            Assert.Equal("Id", property1.Name);
            Assert.Same(typeof(int), property1.ClrType);
            Assert.False(((IProperty)property1).IsConcurrencyToken);
            Assert.Same(entityType, property1.DeclaringEntityType);

            var property2 = entityType.AddProperty("Name", typeof(string));

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

            Assert.False(idProperty.IsShadowProperty);
            Assert.Equal("Id", idProperty.Name);
            Assert.Same(typeof(int), idProperty.ClrType);
            Assert.Same(entityType, idProperty.DeclaringEntityType);

            Assert.Same(idProperty, entityType.GetOrAddProperty(Customer.IdProperty));
            Assert.Same(idProperty, entityType.FindProperty("Id"));
            Assert.False(idProperty.IsShadowProperty);

            var nameProperty = entityType.AddProperty("Name");

            Assert.False(((IProperty)nameProperty).IsShadowProperty);
            Assert.Equal("Name", nameProperty.Name);
            Assert.Same(typeof(string), nameProperty.ClrType);
            Assert.Same(entityType, nameProperty.DeclaringEntityType);

            Assert.Same(nameProperty, entityType.GetOrAddProperty(Customer.NameProperty));
            Assert.Same(nameProperty, entityType.FindProperty("Name"));
            Assert.False(nameProperty.IsShadowProperty);

            Assert.True(new[] { idProperty, nameProperty }.SequenceEqual(entityType.GetProperties()));
        }

        [Fact]
        public void AddProperty_throws_if_shadow_entity_type()
        {
            var entityType = new Model().AddEntityType("Customer");

            Assert.Equal(CoreStrings.ClrPropertyOnShadowEntity(nameof(Customer.Name), "Customer"),
                Assert.Throws<InvalidOperationException>(() =>
                    entityType.AddProperty(Customer.NameProperty)).Message);
        }

        [Fact]
        public void AddProperty_throws_if_no_clr_property_or_field()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));

            Assert.Equal(CoreStrings.NoPropertyType("_foo", nameof(Customer)),
                Assert.Throws<InvalidOperationException>(() =>
                    entityType.AddProperty("_foo")).Message);
        }

        [Fact]
        public void AddProperty_throws_if_clr_type_does_not_match()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));

            Assert.Equal(CoreStrings.PropertyWrongClrType(
                nameof(Customer.Name), nameof(Customer), typeof(string).DisplayName(), typeof(int).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() =>
                    entityType.AddProperty(nameof(Customer.Name), typeof(int))).Message);
        }

        [Fact]
        public void Cannot_remove_property_when_used_by_primary_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.GetOrAddProperty(Customer.IdProperty);

            entityType.GetOrSetPrimaryKey(property);

            Assert.Equal(
                CoreStrings.PropertyInUseKey("Id", typeof(Customer).Name, "{'Id'}"),
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
                CoreStrings.PropertyInUseKey("Id", typeof(Customer).Name, "{'Id'}"),
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
                CoreStrings.PropertyInUseForeignKey("CustomerId", typeof(Order).Name, "{'CustomerId'}", typeof(Order).Name),
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
                CoreStrings.PropertyInUseIndex("Id", typeof(Customer).Name, "{'Id'}", typeof(Customer).Name),
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
            entityType.AddProperty("Mane_", typeof(int));

            Assert.False(entityType.FindProperty("Name").IsShadowProperty);
            Assert.False(entityType.FindProperty("Id").IsShadowProperty);
            Assert.True(entityType.FindProperty("Mane_").IsShadowProperty);
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

            customerForeignKey.HasDependentToPrincipal(Order.CustomerProperty);

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
            entityType.AddProperty("Id_", typeof(int));
            entityType.AddProperty("Mane_", typeof(int));

            Assert.Equal(0, entityType.FindProperty("Id_").GetIndex());
            Assert.Equal(1, entityType.FindProperty("Mane_").GetIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetIndex());

            Assert.Equal(0, entityType.FindProperty("Id_").GetShadowIndex());
            Assert.Equal(1, entityType.FindProperty("Mane_").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetShadowIndex());

            Assert.Equal(2, entityType.ShadowPropertyCount());
        }

        [Fact]
        public void Indexes_are_rebuilt_when_more_properties_added_or_relevant_state_changes()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications;

            Assert.Equal(0, entityType.FindProperty("Id").GetIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetIndex());
            Assert.Equal(3, entityType.FindProperty("Token").GetIndex());
            Assert.Equal(0, entityType.FindNavigation("CollectionNav").GetIndex());
            Assert.Equal(1, entityType.FindNavigation("ReferenceNav").GetIndex());

            Assert.Equal(-1, entityType.FindProperty("Id").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("AnotherEntityId").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetShadowIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetStoreGeneratedIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindNavigation("ReferenceNav").GetStoreGeneratedIndex());

            Assert.Equal(0, entityType.ShadowPropertyCount());
            Assert.Equal(3, entityType.OriginalValueCount());
            Assert.Equal(3, entityType.RelationshipPropertyCount());
            Assert.Equal(2, entityType.StoreGeneratedCount());

            var gameProperty = entityType.AddProperty("Game", typeof(int));
            gameProperty.IsConcurrencyToken = true;

            var maneProperty = entityType.AddProperty("Mane", typeof(int));
            maneProperty.IsConcurrencyToken = true;

            Assert.Equal(0, entityType.FindProperty("Id").GetIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetIndex());
            Assert.Equal(2, entityType.FindProperty("Game").GetIndex());
            Assert.Equal(3, entityType.FindProperty("Mane").GetIndex());
            Assert.Equal(4, entityType.FindProperty("Name").GetIndex());
            Assert.Equal(5, entityType.FindProperty("Token").GetIndex());
            Assert.Equal(0, entityType.FindNavigation("CollectionNav").GetIndex());
            Assert.Equal(1, entityType.FindNavigation("ReferenceNav").GetIndex());

            Assert.Equal(-1, entityType.FindProperty("Id").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("AnotherEntityId").GetShadowIndex());
            Assert.Equal(0, entityType.FindProperty("Game").GetShadowIndex());
            Assert.Equal(1, entityType.FindProperty("Mane").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetShadowIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Game").GetOriginalValueIndex());
            Assert.Equal(3, entityType.FindProperty("Mane").GetOriginalValueIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(4, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Game").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Mane").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetStoreGeneratedIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindProperty("Game").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindProperty("Mane").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindNavigation("ReferenceNav").GetStoreGeneratedIndex());

            Assert.Equal(2, entityType.ShadowPropertyCount());
            Assert.Equal(5, entityType.OriginalValueCount());
            Assert.Equal(3, entityType.RelationshipPropertyCount());
            Assert.Equal(2, entityType.StoreGeneratedCount());

            gameProperty.IsConcurrencyToken = false;
            entityType.FindProperty("Name").IsConcurrencyToken = true;

            Assert.Equal(0, entityType.FindProperty("Id").GetIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetIndex());
            Assert.Equal(2, entityType.FindProperty("Game").GetIndex());
            Assert.Equal(3, entityType.FindProperty("Mane").GetIndex());
            Assert.Equal(4, entityType.FindProperty("Name").GetIndex());
            Assert.Equal(5, entityType.FindProperty("Token").GetIndex());
            Assert.Equal(0, entityType.FindNavigation("CollectionNav").GetIndex());
            Assert.Equal(1, entityType.FindNavigation("ReferenceNav").GetIndex());

            Assert.Equal(-1, entityType.FindProperty("Id").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("AnotherEntityId").GetShadowIndex());
            Assert.Equal(0, entityType.FindProperty("Game").GetShadowIndex());
            Assert.Equal(1, entityType.FindProperty("Mane").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetShadowIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetShadowIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(-1, entityType.FindProperty("Game").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Mane").GetOriginalValueIndex());
            Assert.Equal(3, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(4, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Game").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Mane").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(0, entityType.FindProperty("Id").GetStoreGeneratedIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindProperty("Game").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindProperty("Mane").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetStoreGeneratedIndex());
            Assert.Equal(-1, entityType.FindNavigation("ReferenceNav").GetStoreGeneratedIndex());

            Assert.Equal(2, entityType.ShadowPropertyCount());
            Assert.Equal(5, entityType.OriginalValueCount());
            Assert.Equal(3, entityType.RelationshipPropertyCount());
            Assert.Equal(2, entityType.StoreGeneratedCount());
        }

        [Fact]
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

                Assert.Equal(3, type.PropertyCount());
                Assert.Equal(2, type.NavigationCount());
                Assert.Equal(1, type.ShadowPropertyCount());
                Assert.Equal(3, type.OriginalValueCount());
                Assert.Equal(4, type.RelationshipPropertyCount());
                Assert.Equal(2, type.StoreGeneratedCount());

                type = context.Model.FindEntityType(typeof(Level2));

                Assert.Equal(0, type.FindProperty("Id").GetIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetIndex());
                Assert.Equal(2, type.FindProperty("Prop1").GetIndex());
                Assert.Equal(3, type.FindProperty("Level2ReferenceId").GetIndex());
                Assert.Equal(4, type.FindProperty("Prop2").GetIndex());
                Assert.Equal(0, type.FindNavigation("Level1Collection").GetIndex());
                Assert.Equal(1, type.FindNavigation("Level1Reference").GetIndex());
                Assert.Equal(2, type.FindNavigation("Level2Collection").GetIndex());
                Assert.Equal(3, type.FindNavigation("Level2Reference").GetIndex());

                Assert.Equal(-1, type.FindProperty("Id").GetShadowIndex());
                Assert.Equal(0, type.FindProperty("Level1ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetShadowIndex());
                Assert.Equal(1, type.FindProperty("Level2ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop2").GetShadowIndex());

                Assert.Equal(0, type.FindProperty("Id").GetOriginalValueIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetOriginalValueIndex());
                Assert.Equal(2, type.FindProperty("Prop1").GetOriginalValueIndex());
                Assert.Equal(3, type.FindProperty("Level2ReferenceId").GetOriginalValueIndex());
                Assert.Equal(4, type.FindProperty("Prop2").GetOriginalValueIndex());

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

                Assert.Equal(5, type.PropertyCount());
                Assert.Equal(4, type.NavigationCount());
                Assert.Equal(2, type.ShadowPropertyCount());
                Assert.Equal(5, type.OriginalValueCount());
                Assert.Equal(7, type.RelationshipPropertyCount());
                Assert.Equal(3, type.StoreGeneratedCount());

                type = context.Model.FindEntityType(typeof(Level3));

                Assert.Equal(0, type.FindProperty("Id").GetIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetIndex());
                Assert.Equal(2, type.FindProperty("Prop1").GetIndex());
                Assert.Equal(3, type.FindProperty("Level2ReferenceId").GetIndex());
                Assert.Equal(4, type.FindProperty("Prop2").GetIndex());
                Assert.Equal(5, type.FindProperty("Level3ReferenceId").GetIndex());
                Assert.Equal(6, type.FindProperty("Prop3").GetIndex());
                Assert.Equal(0, type.FindNavigation("Level1Collection").GetIndex());
                Assert.Equal(1, type.FindNavigation("Level1Reference").GetIndex());
                Assert.Equal(2, type.FindNavigation("Level2Collection").GetIndex());
                Assert.Equal(3, type.FindNavigation("Level2Reference").GetIndex());
                Assert.Equal(4, type.FindNavigation("Level3Collection").GetIndex());
                Assert.Equal(5, type.FindNavigation("Level3Reference").GetIndex());

                Assert.Equal(-1, type.FindProperty("Id").GetShadowIndex());
                Assert.Equal(0, type.FindProperty("Level1ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop1").GetShadowIndex());
                Assert.Equal(1, type.FindProperty("Level2ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop2").GetShadowIndex());
                Assert.Equal(2, type.FindProperty("Level3ReferenceId").GetShadowIndex());
                Assert.Equal(-1, type.FindProperty("Prop3").GetShadowIndex());

                Assert.Equal(0, type.FindProperty("Id").GetOriginalValueIndex());
                Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetOriginalValueIndex());
                Assert.Equal(2, type.FindProperty("Prop1").GetOriginalValueIndex());
                Assert.Equal(3, type.FindProperty("Level2ReferenceId").GetOriginalValueIndex());
                Assert.Equal(4, type.FindProperty("Prop2").GetOriginalValueIndex());
                Assert.Equal(5, type.FindProperty("Level3ReferenceId").GetOriginalValueIndex());
                Assert.Equal(6, type.FindProperty("Prop3").GetOriginalValueIndex());

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

                Assert.Equal(7, type.PropertyCount());
                Assert.Equal(6, type.NavigationCount());
                Assert.Equal(3, type.ShadowPropertyCount());
                Assert.Equal(7, type.OriginalValueCount());
                Assert.Equal(10, type.RelationshipPropertyCount());
                Assert.Equal(4, type.StoreGeneratedCount());
            }
        }

        private class Levels : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase();

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Level1>().HasOne(e => e.Level1Reference).WithMany(e => e.Level1Collection);
                modelBuilder.Entity<Level2>().HasOne(e => e.Level2Reference).WithMany(e => e.Level2Collection);
                modelBuilder.Entity<Level3>().HasOne(e => e.Level3Reference).WithMany(e => e.Level3Collection);
            }
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
        public void Change_tracking_from_model_is_used_by_default_regardless_of_CLR_type()
        {
            var model = BuildFullNotificationEntityModel();
            var entityType = model.FindEntityType(typeof(FullNotificationEntity));

            Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

            model.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;

            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());
        }

        [Fact]
        public void Change_tracking_from_model_is_used_by_default_for_shadow_entities()
        {
            var model = new Model();
            var entityType = model.AddEntityType("Z'ha'dum");

            Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.ChangeTrackingStrategy);

            model.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;

            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.ChangeTrackingStrategy);
        }

        [Fact]
        public void Change_tracking_can_be_set_to_anything_for_full_notification_entities()
        {
            var model = BuildFullNotificationEntityModel();
            model.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;

            var entityType = model.FindEntityType(typeof(FullNotificationEntity));

            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());

            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.Snapshot;
            Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;
            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());

            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications;
            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.GetChangeTrackingStrategy());

            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues;
            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues, entityType.GetChangeTrackingStrategy());
        }

        [Fact]
        public void Change_tracking_can_be_set_to_snapshot_or_changed_only_for_changed_only_entities()
        {
            var model = new Model { ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications };
            var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));

            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.ChangeTrackingStrategy);

            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.Snapshot;
            Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.ChangeTrackingStrategy);

            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;
            Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.ChangeTrackingStrategy);

            Assert.Equal(
                CoreStrings.ChangeTrackingInterfaceMissing("ChangedOnlyEntity", "ChangingAndChangedNotifications", "INotifyPropertyChanging"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications).Message);

            Assert.Equal(
                CoreStrings.ChangeTrackingInterfaceMissing("ChangedOnlyEntity", "ChangingAndChangedNotificationsWithOriginalValues", "INotifyPropertyChanging"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues).Message);
        }

        [Fact]
        public void Change_tracking_can_be_set_to_snapshot_only_for_non_notifying_entities()
        {
            var model = new Model { ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications };
            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.ChangeTrackingStrategy);

            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.Snapshot;
            Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.ChangeTrackingStrategy);

            Assert.Equal(
                CoreStrings.ChangeTrackingInterfaceMissing("Customer", "ChangedNotifications", "INotifyPropertyChanged"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications).Message);

            Assert.Equal(
                CoreStrings.ChangeTrackingInterfaceMissing("Customer", "ChangingAndChangedNotifications", "INotifyPropertyChanged"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications).Message);

            Assert.Equal(
                CoreStrings.ChangeTrackingInterfaceMissing("Customer", "ChangingAndChangedNotificationsWithOriginalValues", "INotifyPropertyChanged"),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues).Message);
        }

        [Fact]
        public void All_properties_have_original_value_indexes_when_using_snapshot_change_tracking()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.Snapshot;

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(3, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(4, entityType.OriginalValueCount());
        }

        [Fact]
        public void All_relationship_properties_have_relationship_indexes_when_using_snapshot_change_tracking()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.Snapshot;

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(3, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(4, entityType.RelationshipPropertyCount());
        }

        [Fact]
        public void All_properties_have_original_value_indexes_when_using_changed_only_tracking()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(3, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(4, entityType.OriginalValueCount());
        }

        [Fact]
        public void Collections_dont_have_relationship_indexes_when_using_changed_only_change_tracking()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangedNotifications;

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(3, entityType.RelationshipPropertyCount());
        }

        [Fact]
        public void Only_concurrency_and_key_properties_have_original_value_indexes_when_using_full_notifications()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications;

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(3, entityType.OriginalValueCount());
        }

        [Fact]
        public void Collections_dont_have_relationship_indexes_when_using_full_notifications()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications;

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(3, entityType.RelationshipPropertyCount());
        }

        [Fact]
        public void All_properties_have_original_value_indexes_when_full_notifications_with_original_values()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues;

            Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
            Assert.Equal(2, entityType.FindProperty("Name").GetOriginalValueIndex());
            Assert.Equal(3, entityType.FindProperty("Token").GetOriginalValueIndex());

            Assert.Equal(4, entityType.OriginalValueCount());
        }

        [Fact]
        public void Collections_dont_have_relationship_indexes_when_full_notifications_with_original_values()
        {
            var entityType = BuildFullNotificationEntityModel().FindEntityType(typeof(FullNotificationEntity));
            entityType.ChangeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues;

            Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
            Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
            Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
            Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

            Assert.Equal(3, entityType.RelationshipPropertyCount());
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
            public static readonly PropertyInfo NotCollectionOrdersProperty = typeof(Customer).GetProperty(nameof(NotCollectionOrders));

            public int AlternateId { get; set; }
            public Guid Unique { get; set; }
            public string Name { get; set; }
            public string Mane { get; set; }
            public ICollection<Order> Orders { get; set; }

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

        private static Model BuildFullNotificationEntityModel()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

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
