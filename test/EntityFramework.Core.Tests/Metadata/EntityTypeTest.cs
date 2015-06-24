// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable ImplicitlyCapturedClosure

namespace Microsoft.Data.Entity.Tests.Metadata
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

            b.BaseType = a;
            c.BaseType = a;
            d.BaseType = c;

            Assert.Equal(
                Strings.CircularInheritance(a, a),
                Assert.Throws<InvalidOperationException>(() => a.BaseType = a).Message);

            Assert.Equal(
                Strings.CircularInheritance(a, b),
                Assert.Throws<InvalidOperationException>(() => a.BaseType = b).Message);

            Assert.Equal(
                Strings.CircularInheritance(a, d),
                Assert.Throws<InvalidOperationException>(() => a.BaseType = d).Message);
        }

        [Fact]
        public void Setting_CLR_base_for_shadow_entity_type_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B).Name);

            Assert.Equal(
                Strings.NonShadowBaseType(b, a),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);
        }

        [Fact]
        public void Setting_shadow_base_for_CLR_entity_type_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A).Name);
            var b = model.AddEntityType(typeof(B));

            Assert.Equal(
                Strings.NonClrBaseType(b, a),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);
        }

        [Fact]
        public void Setting_not_assignable_base_should_throw()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));

            Assert.Equal(
                Strings.NotAssignableClrBaseType(a, b, typeof(A).Name, typeof(B).Name),
                Assert.Throws<InvalidOperationException>(() => a.BaseType = b).Message);
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

            Assert.Equal(new[] { "E", "G" }, a.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F", "H" }, b.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "H", "I" }, c.Properties.Select(p => p.Name).ToArray());

            b.BaseType = a;
            c.BaseType = a;

            Assert.Equal(new[] { "E", "G" }, a.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "F", "H" }, b.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "H", "I" }, c.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1, 2, 3 }, b.Properties.Select(p => p.Index));
            Assert.Equal(new[] { 0, 1, 2, 3 }, c.Properties.Select(p => p.Index));
            Assert.Same(b.GetProperty("E"), a.GetProperty("E"));
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

            b.BaseType = a;
            c.BaseType = a;

            a.AddProperty(A.GProperty);
            a.AddProperty(A.EProperty);

            b.AddProperty(B.HProperty);
            b.AddProperty(B.FProperty);

            c.AddProperty(C.HProperty);
            c.AddProperty("I", typeof(string));

            Assert.Equal(new[] { "E", "G" }, a.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "F", "H" }, b.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "H", "I" }, c.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1, 2, 3 }, b.Properties.Select(p => p.Index));
            Assert.Equal(new[] { 0, 1, 2, 3 }, c.Properties.Select(p => p.Index));
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
            d.BaseType = c;

            Assert.Equal(new[] { "F", "H" }, c.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F", "H", "E", "G" }, d.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1 }, c.Properties.Select(p => p.Index));
            Assert.Equal(new[] { 0, 1, 2, 3 }, d.Properties.Select(p => p.Index));

            d.BaseType = null;

            Assert.Equal(new[] { "F", "H" }, c.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G" }, d.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1 }, c.Properties.Select(p => p.Index));
            Assert.Equal(new[] { 0, 1 }, d.Properties.Select(p => p.Index));

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.EProperty);
            a.AddProperty(A.GProperty);

            c.BaseType = a;

            Assert.Equal(new[] { "E", "G" }, a.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G", "F", "H" }, c.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "E", "G" }, d.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1 }, a.Properties.Select(p => p.Index));
            Assert.Equal(new[] { 0, 1, 2, 3 }, c.Properties.Select(p => p.Index));
            Assert.Equal(new[] { 0, 1 }, d.Properties.Select(p => p.Index));
        }

        [Fact]
        public void Adding_property_throws_when_parent_type_has_property_with_same_name()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty);

            var b = model.AddEntityType(typeof(B));
            b.BaseType = a;

            Assert.Equal(Strings.DuplicateProperty("G", typeof(B).FullName),
                Assert.Throws<InvalidOperationException>(() => b.AddProperty("G")).Message);
        }

        [Fact]
        public void Adding_property_throws_when_grandparent_type_has_property_with_same_name()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty);

            var c = model.AddEntityType(typeof(C));
            c.BaseType = a;

            var d = model.AddEntityType(typeof(D));
            d.BaseType = c;

            Assert.Equal(
                Strings.DuplicateProperty("G", typeof(D).FullName),
                Assert.Throws<InvalidOperationException>(() => d.AddProperty("G")).Message);
        }

        [Fact]
        public void Adding_property_throws_when_child_type_has_property_with_same_name()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));

            var b = model.AddEntityType(typeof(B));
            b.BaseType = a;

            b.AddProperty(A.GProperty);

            Assert.Equal(
                Strings.DuplicateProperty("G", typeof(A).FullName),
                Assert.Throws<InvalidOperationException>(() => a.AddProperty(A.GProperty)).Message);
        }

        [Fact]
        public void Adding_property_throws_when_grandchild_type_has_property_with_same_name()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));

            var c = model.AddEntityType(typeof(C));
            c.BaseType = a;

            var d = model.AddEntityType(typeof(D));
            d.BaseType = c;

            d.AddProperty(A.GProperty);

            Assert.Equal(
                Strings.DuplicateProperty("G", typeof(A).FullName),
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
                Strings.DuplicatePropertiesOnBase(typeof(B).FullName, typeof(A).FullName, "G"),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);
        }

        [Fact]
        public void Setting_base_type_throws_when_grandparent_contains_duplicate_property()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.EProperty);
            a.AddProperty(A.GProperty);

            var c = model.AddEntityType(typeof(C));
            c.BaseType = a;

            var d = model.AddEntityType(typeof(D));
            d.AddProperty(A.EProperty);
            d.AddProperty(A.GProperty);

            Assert.Equal(
                Strings.DuplicatePropertiesOnBase(typeof(D).FullName, typeof(C).FullName, "E, G"),
                Assert.Throws<InvalidOperationException>(() => d.BaseType = c).Message);
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
            d.BaseType = c;

            Assert.Equal(
                Strings.DuplicatePropertiesOnBase(typeof(C).FullName, typeof(A).FullName, "E, G"),
                Assert.Throws<InvalidOperationException>(() => c.BaseType = a).Message);
        }

        [Fact]
        public void Keys_on_base_type_should_be_inherited()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var pk = a.SetPrimaryKey(a.AddProperty(A.GProperty));
            a.AddKey(a.AddProperty(A.EProperty));

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(B.FProperty);

            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new string[0][],
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F" }, b.Properties.Select(p => p.Name).ToArray());

            b.BaseType = a;

            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "G", "E", "F" }, b.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { 0, 1, 2 }, b.Properties.Select(p => p.Index));
            Assert.Same(pk, b.FindPrimaryKey(new[] { b.GetProperty("G") }));
            Assert.Same(b.GetKey(b.GetProperty("G")), a.GetKey(a.GetProperty("G")));
        }

        [Fact]
        public void Keys_added_to_base_type_should_be_inherited()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.AddProperty(A.GProperty);
            a.AddProperty(A.EProperty);

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(B.FProperty);

            b.BaseType = a;

            a.SetPrimaryKey(a.GetProperty("G"));
            a.AddKey(a.GetProperty("E"));

            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "G", "E", "F" }, b.Properties.Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Keys_should_be_updated_when_base_type_changes()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            a.SetPrimaryKey(a.AddProperty(A.GProperty));
            a.AddKey(a.AddProperty(A.EProperty));

            var b = model.AddEntityType(typeof(B));
            b.AddProperty(B.FProperty);
            b.BaseType = a;

            b.BaseType = null;

            Assert.Equal(new[] { new[] { "E" }, new[] { "G" } },
                a.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new string[0][],
                b.GetKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { "G", "E" }, a.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "F" }, b.Properties.Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Adding_keys_throws_when_there_is_a_parent_type()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));
            b.BaseType = a;

            Assert.Equal(
                Strings.DerivedEntityTypeKey(typeof(B).FullName),
                Assert.Throws<InvalidOperationException>(() => b.SetPrimaryKey(b.AddProperty("G"))).Message);
            Assert.Equal(
                Strings.DerivedEntityTypeKey(typeof(B).FullName),
                Assert.Throws<InvalidOperationException>(() => b.AddKey(b.AddProperty("E"))).Message);
        }

        [Fact]
        public void Setting_base_type_throws_when_child_contains_key()
        {
            var model = new Model();

            var a = model.AddEntityType(typeof(A));
            var b = model.AddEntityType(typeof(B));
            var key = b.AddKey(b.AddProperty(B.HProperty));

            Assert.Equal(
                Strings.DerivedEntityCannotHaveKeys(typeof(B).FullName),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);

            b.RemoveKey(key);
            b.SetPrimaryKey(b.AddProperty(B.FProperty));

            Assert.Equal(
                Strings.DerivedEntityCannotHaveKeys(typeof(B).FullName),
                Assert.Throws<InvalidOperationException>(() => b.BaseType = a).Message);
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

            customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            Assert.Equal(new[] { "Orders" }, customerType.Navigations.Select(p => p.Name).ToArray());
            Assert.Equal(new string[0], specialCustomerType.Navigations.Select(p => p.Name).ToArray());

            specialCustomerType.BaseType = customerType;

            Assert.Equal(new[] { "Orders" }, customerType.Navigations.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders" }, specialCustomerType.Navigations.Select(p => p.Name).ToArray());

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerType.AddNavigation("DerivedOrders", specialCustomerForeignKey, pointsToPrincipal: false);

            Assert.Equal(new[] { "Orders" }, customerType.Navigations.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders", "DerivedOrders" }, specialCustomerType.Navigations.Select(p => p.Name).ToArray());
            Assert.Same(customerType.GetOrAddNavigation("Orders", customerForeignKey, pointsToPrincipal: false),
                specialCustomerType.GetOrAddNavigation("Orders", customerForeignKey, pointsToPrincipal: false));
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
            specialCustomerType.BaseType = customerType;

            customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false);

            Assert.Equal(new[] { "Orders" }, customerType.Navigations.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders" }, specialCustomerType.Navigations.Select(p => p.Name).ToArray());

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerType.AddNavigation("DerivedOrders", specialCustomerForeignKey, pointsToPrincipal: false);

            Assert.Equal(new[] { "Orders" }, customerType.Navigations.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "Orders", "DerivedOrders" }, specialCustomerType.Navigations.Select(p => p.Name).ToArray());
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

            customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.BaseType = customerType;

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerType.AddNavigation("DerivedOrders", specialCustomerForeignKey, pointsToPrincipal: false);

            specialCustomerType.BaseType = null;

            Assert.Equal(new[] { "Orders" }, customerType.Navigations.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { "DerivedOrders" }, specialCustomerType.Navigations.Select(p => p.Name).ToArray());
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

            customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.BaseType = customerType;

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);

            Assert.Equal(
                Strings.DuplicateNavigation("Orders", typeof(SpecialCustomer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    specialCustomerType.AddNavigation("Orders", specialCustomerForeignKey, pointsToPrincipal: false)).Message);
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

            customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false);

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));
            specialCustomerType.BaseType = customerType;

            var verySpecialCustomerType = model.AddEntityType(typeof(VerySpecialCustomer));
            verySpecialCustomerType.BaseType = specialCustomerType;

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, verySpecialCustomerType);

            Assert.Equal(
                Strings.DuplicateNavigation("Orders", typeof(VerySpecialCustomer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    verySpecialCustomerType.AddNavigation("Orders", specialCustomerForeignKey, pointsToPrincipal: false)).Message);
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
            specialCustomerType.BaseType = customerType;

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, specialCustomerType);
            specialCustomerType.AddNavigation("Orders", specialCustomerForeignKey, pointsToPrincipal: false);

            Assert.Equal(
                Strings.DuplicateNavigation("Orders", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false)).Message);
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
            specialCustomerType.BaseType = customerType;

            var verySpecialCustomerType = model.AddEntityType(typeof(VerySpecialCustomer));
            verySpecialCustomerType.BaseType = specialCustomerType;

            var derivedForeignKeyProperty = orderType.GetOrAddProperty(Order.IdProperty);
            var specialCustomerForeignKey = orderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, verySpecialCustomerType);
            verySpecialCustomerType.AddNavigation("Orders", specialCustomerForeignKey, pointsToPrincipal: false);

            Assert.Equal(
                Strings.DuplicateNavigation("Orders", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false)).Message);
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
            orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var derivedForeignKeyProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.GetOrAddKey(property);
            var specialCustomerForeignKey = specialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            specialOrderType.AddNavigation("Customer", specialCustomerForeignKey, pointsToPrincipal: true);

            Assert.Equal(
                Strings.DuplicateNavigationsOnBase(typeof(SpecialOrder).FullName, typeof(Order).FullName, "Customer"),
                Assert.Throws<InvalidOperationException>(() => specialOrderType.BaseType = orderType).Message);
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
            orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            var derivedForeignKeyProperty = verySpecialOrderType.GetOrAddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.GetOrAddKey(property);
            var specialCustomerForeignKey = verySpecialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            verySpecialOrderType.AddNavigation("Customer", specialCustomerForeignKey, pointsToPrincipal: true);
            verySpecialOrderType.BaseType = specialOrderType;

            Assert.Equal(
                Strings.DuplicateNavigationsOnBase(typeof(SpecialOrder).FullName, typeof(Order).FullName, "Customer"),
                Assert.Throws<InvalidOperationException>(() => specialOrderType.BaseType = orderType).Message);
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
            orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            var derivedForeignKeyProperty = verySpecialOrderType.GetOrAddProperty(Order.IdProperty);
            var property = specialCustomerType.AddProperty("AltId", typeof(int));
            var specialCustomerKey = specialCustomerType.GetOrAddKey(property);
            var specialCustomerForeignKey = verySpecialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, specialCustomerKey, specialCustomerType);
            verySpecialOrderType.AddNavigation("Customer", specialCustomerForeignKey, pointsToPrincipal: true);

            Assert.Equal(
                Strings.DuplicateNavigationsOnBase(typeof(VerySpecialOrder).FullName, typeof(SpecialOrder).FullName, "Customer"),
                Assert.Throws<InvalidOperationException>(() => verySpecialOrderType.BaseType = specialOrderType).Message);
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

            specialOrderType.BaseType = orderType;

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

            specialOrderType.BaseType = orderType;
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
            specialOrderType.BaseType = orderType;
            var derivedForeignKeyProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddForeignKey(derivedForeignKeyProperty, customerKey, customerType);

            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            specialOrderType.BaseType = null;

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.GetForeignKeys().Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        // Issue #2514
        //[Fact]
        public void Adding_foreignKey_throws_when_parent_type_has_foreignKey_on_same_properties()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            Assert.Equal(
                Strings.DuplicateForeignKey(Property.Format(new[] { foreignKeyProperty }), typeof(SpecialOrder).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    specialOrderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        // Issue #2514
        //[Fact]
        public void Adding_foreignKey_throws_when_grandparent_type_has_foreignKey_on_same_properties()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.BaseType = specialOrderType;

            Assert.Equal(
                Strings.DuplicateForeignKey(Property.Format(new[] { foreignKeyProperty }), typeof(VerySpecialOrder).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    verySpecialOrderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        // Issue #2514
        //[Fact]
        public void Adding_foreignKey_throws_when_child_type_has_foreignKey_on_same_properties()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);
            specialOrderType.BaseType = orderType;

            Assert.Equal(
                Strings.DuplicateForeignKey(Property.Format(new[] { foreignKeyProperty }), typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
        }

        // Issue #2514
        //[Fact]
        public void Adding_foreignKey_throws_when_grandchild_type_has_foreignKey_on_same_properties()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.BaseType = specialOrderType;
            verySpecialOrderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                Strings.DuplicateForeignKey(Property.Format(new[] { foreignKeyProperty }), typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType)).Message);
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
                orderType.Indexes.Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.Indexes.Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            specialOrderType.BaseType = orderType;

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.Indexes.Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
                specialOrderType.Indexes.Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Same(index, specialOrderType.GetOrAddIndex(indexProperty));
        }

        [Fact]
        public void Index_added_to_base_type_should_be_inherited()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));

            specialOrderType.BaseType = orderType;
            orderType.GetOrAddIndex(indexProperty);

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.Indexes.Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                specialOrderType.Indexes.Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());

            var derivedIndexProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddIndex(derivedIndexProperty);

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.Indexes.Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name }, new[] { Order.IdProperty.Name } },
                specialOrderType.Indexes.Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        [Fact]
        public void Indexes_should_be_updated_when_base_type_changes()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;
            var derivedIndexProperty = specialOrderType.GetOrAddProperty(Order.IdProperty);
            specialOrderType.GetOrAddIndex(derivedIndexProperty);

            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.GetOrAddIndex(indexProperty);

            specialOrderType.BaseType = null;

            Assert.Equal(new[] { new[] { Order.CustomerIdProperty.Name } },
                orderType.Indexes.Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
            Assert.Equal(new[] { new[] { Order.IdProperty.Name } },
                specialOrderType.Indexes.Select(fk => fk.Properties.Select(p => p.Name).ToArray()).ToArray());
        }

        [Fact]
        public void Adding_an_index_throws_if_properties_were_removed()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            entityType.RemoveProperty(idProperty);

            Assert.Equal(
                Strings.IndexPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<ArgumentException>(() => entityType.AddIndex(new[] { idProperty })).Message);
        }

        // Issue #2514
        //[Fact]
        public void Adding_an_index_throws_when_parent_type_has_index_on_same_properties()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.GetOrAddIndex(indexProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            Assert.Equal(
                Strings.DuplicateIndex(Property.Format(new[] { indexProperty }), typeof(SpecialOrder).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    specialOrderType.GetOrAddIndex(indexProperty)).Message);
        }

        // Issue #2514
        //[Fact]
        public void Adding_an_index_throws_when_grandparent_type_has_index_on_same_properties()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            orderType.GetOrAddIndex(indexProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.BaseType = specialOrderType;

            Assert.Equal(
                Strings.DuplicateIndex(Property.Format(new[] { indexProperty }), typeof(VerySpecialOrder).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    verySpecialOrderType.GetOrAddIndex(indexProperty)).Message);
        }

        // Issue #2514
        //[Fact]
        public void Adding_an_index_throws_when_child_type_has_index_on_same_properties()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.GetOrAddIndex(indexProperty);
            specialOrderType.BaseType = orderType;

            Assert.Equal(
                Strings.DuplicateIndex(Property.Format(new[] { indexProperty }), typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    orderType.GetOrAddIndex(indexProperty)).Message);
        }

        // Issue #2514
        //[Fact]
        public void Adding_an_index_throws_when_grandchild_type_has_index_on_same_properties()
        {
            var model = new Model();

            var orderType = model.AddEntityType(typeof(Order));
            var indexProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);

            var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
            specialOrderType.BaseType = orderType;

            var verySpecialOrderType = model.AddEntityType(typeof(VerySpecialOrder));
            verySpecialOrderType.BaseType = specialOrderType;
            specialOrderType.GetOrAddIndex(indexProperty);

            Assert.Equal(
                Strings.DuplicateIndex(Property.Format(new[] { indexProperty }), typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    orderType.GetOrAddIndex(indexProperty)).Message);
        }

        [Fact]
        public void Can_create_entity_type()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Equal(typeof(Customer).FullName, entityType.Name);
            Assert.Same(typeof(Customer), entityType.ClrType);
        }

        [Fact]
        public void Display_name_is_prettified_CLR_name()
        {
            Assert.Equal("EntityTypeTest", new EntityType(typeof(EntityTypeTest), new Model()).DisplayName());
            Assert.Equal("Customer", new EntityType(typeof(Customer), new Model()).DisplayName());
            Assert.Equal("List<Customer>", new EntityType(typeof(List<Customer>), new Model()).DisplayName());
        }

        [Fact]
        public void Display_name_is_part_of_name_following_final_separator_when_no_CLR_type()
        {
            Assert.Equal("Everything", new EntityType("Everything", new Model()).DisplayName());
            Assert.Equal("Is", new EntityType("Everything.Is", new Model()).DisplayName());
            Assert.Equal("Awesome", new EntityType("Everything.Is.Awesome", new Model()).DisplayName());
            Assert.Equal("WhenWe`reLivingOurDream", new EntityType("Everything.Is.Awesome+WhenWe`reLivingOurDream", new Model()).DisplayName());
        }

        [Fact]
        public void Name_is_prettified_CLR_full_name()
        {
            Assert.Equal("Microsoft.Data.Entity.Tests.Metadata.EntityTypeTest", new EntityType(typeof(EntityTypeTest), new Model()).Name);
            Assert.Equal("Microsoft.Data.Entity.Tests.Metadata.EntityTypeTest+Customer", new EntityType(typeof(Customer), new Model()).Name);
            Assert.Equal("System.Collections.Generic.List<Microsoft.Data.Entity.Tests.Metadata.EntityTypeTest+Customer>", new EntityType(typeof(List<Customer>), new Model()).Name);
        }

        [Fact]
        public void Can_set_reset_and_clear_primary_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);

            var key1 = entityType.SetPrimaryKey(new[] { idProperty, nameProperty });

            Assert.NotNull(key1);
            Assert.Same(key1, entityType.GetPrimaryKey());

            Assert.Same(key1, entityType.FindPrimaryKey());
            Assert.Same(key1, entityType.GetKeys().Single());

            var key2 = entityType.SetPrimaryKey(idProperty);

            Assert.NotNull(key2);
            Assert.Same(key2, entityType.GetPrimaryKey());
            Assert.Same(key2, entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());

            Assert.Same(key1, entityType.GetKey(key1.Properties));
            Assert.Same(key2, entityType.GetKey(key2.Properties));

            Assert.Null(entityType.SetPrimaryKey((Property)null));

            Assert.Null(entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());

            Assert.Null(entityType.SetPrimaryKey(new Property[0]));

            Assert.Null(entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());

            Assert.Equal(
                Strings.EntityRequiresKey(typeof(Customer).FullName),
                Assert.Throws<ModelItemNotFoundException>(() => entityType.GetPrimaryKey()).Message);
        }

        [Fact]
        public void Setting_primary_key_throws_if_properties_from_different_type()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var idProperty = entityType2.GetOrAddProperty(Customer.IdProperty);

            Assert.Equal(
                Strings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<ArgumentException>(() => entityType1.SetPrimaryKey(idProperty)).Message);
        }

        [Fact]
        public void Can_get_set_reset_and_clear_primary_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);

            var key1 = entityType.GetOrSetPrimaryKey(new[] { idProperty, nameProperty });

            Assert.NotNull(key1);
            Assert.Same(key1, entityType.GetOrSetPrimaryKey(new[] { idProperty, nameProperty }));
            Assert.Same(key1, entityType.GetPrimaryKey());

            Assert.Same(key1, entityType.FindPrimaryKey());
            Assert.Same(key1, entityType.GetKeys().Single());

            var key2 = entityType.GetOrSetPrimaryKey(idProperty);

            Assert.NotNull(key2);
            Assert.NotEqual(key1, key2);
            Assert.Same(key2, entityType.GetOrSetPrimaryKey(idProperty));
            Assert.Same(key2, entityType.GetPrimaryKey());
            Assert.Same(key2, entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());
            Assert.Same(key1, entityType.GetKey(key1.Properties));
            Assert.Same(key2, entityType.GetKey(key2.Properties));

            Assert.Null(entityType.GetOrSetPrimaryKey((Property)null));

            Assert.Null(entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());

            Assert.Null(entityType.GetOrSetPrimaryKey(new Property[0]));

            Assert.Null(entityType.FindPrimaryKey());
            Assert.Equal(2, entityType.GetKeys().Count());
            Assert.Equal(
                Strings.EntityRequiresKey(typeof(Customer).FullName),
                Assert.Throws<ModelItemNotFoundException>(() => entityType.GetPrimaryKey()).Message);
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

            entityType.SetPrimaryKey((Property)null);

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

            entityType.SetPrimaryKey(entityType.GetOrAddProperty(Customer.NameProperty));

            Assert.Equal(2, entityType.GetKeys().Count());
            Assert.Same(customerPk, entityType.FindKey(idProperty));
            Assert.NotSame(customerPk, entityType.GetPrimaryKey());
            Assert.Same(customerPk, fk.PrincipalKey);
        }

        [Fact]
        public void Can_add_and_get_a_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);

            var key1 = entityType.AddKey(new[] { idProperty, nameProperty });

            Assert.NotNull(key1);
            Assert.Same(key1, entityType.GetOrAddKey(new[] { idProperty, nameProperty }));
            Assert.Same(key1, entityType.GetKeys().Single());

            var key2 = entityType.GetOrAddKey(idProperty);

            Assert.NotNull(key2);
            Assert.Same(key2, entityType.GetKey(idProperty));
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
                Strings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<ArgumentException>(() => entityType1.AddKey(idProperty)).Message);
        }

        [Fact]
        public void Adding_a_key_throws_if_duplicated()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);
            entityType.GetOrAddKey(new[] { idProperty, nameProperty });

            Assert.Equal(
                Strings.DuplicateKey("{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty, nameProperty })).Message);
        }

        [Fact]
        public void Adding_a_key_throws_if_properties_were_removed()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            entityType.RemoveProperty(idProperty);

            Assert.Equal(
                Strings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<ArgumentException>(() => entityType.AddKey(new[] { idProperty })).Message);
        }

        [Fact]
        public void Adding_a_key_throws_if_same_as_primary()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);
            entityType.GetOrSetPrimaryKey(new[] { idProperty, nameProperty });

            Assert.Equal(
                Strings.DuplicateKey("{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty, nameProperty })).Message);
        }

        [Fact]
        public void Can_remove_keys()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = entityType.GetOrAddProperty(Customer.NameProperty);

            Assert.Equal(
                Strings.KeyNotFound("{'" + idProperty.Name + "', '" + nameProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<ModelItemNotFoundException>(() => entityType.GetKey(new[] { idProperty, nameProperty })).Message);
            Assert.Null(entityType.RemoveKey(new Key(new[] { idProperty })));

            var key1 = entityType.GetOrSetPrimaryKey(new[] { idProperty, nameProperty });
            var key2 = entityType.GetOrAddKey(idProperty);

            Assert.Equal(new[] { key2, key1 }, entityType.GetKeys().ToArray());

            Assert.Same(key1, entityType.RemoveKey(key1));
            Assert.Null(entityType.RemoveKey(key1));

            Assert.Equal(
                Strings.KeyNotFound("{'" + idProperty.Name + "', '" + nameProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<ModelItemNotFoundException>(() => entityType.GetKey(new[] { idProperty, nameProperty })).Message);

            Assert.Equal(new[] { key2 }, entityType.GetKeys().ToArray());

            Assert.Same(key2, entityType.RemoveKey(new Key(new[] { idProperty })));

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
                Strings.KeyInUse("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).FullName, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() => customerType.RemoveKey(customerKey)).Message);
        }

        [Fact]
        public void Keys_are_ordered_by_property_count_then_property_names()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.AddProperty(Customer.IdProperty);
            var nameProperty = customerType.AddProperty(Customer.NameProperty);
            var otherNameProperty = customerType.AddProperty("OtherNameProperty", typeof(string));

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
                Strings.KeyPropertyMustBeReadOnly(Customer.NameProperty.Name, typeof(Customer).FullName),
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
            nameProperty.StoreGeneratedAlways = true;

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
            Assert.Same(fk1, orderType.GetForeignKey(customerFk1));
            Assert.Same(fk1, orderType.FindForeignKey(customerFk1));
            Assert.Same(fk1, orderType.GetOrAddForeignKey(customerFk1, new Key(new[] { idProperty }), orderType));
            Assert.Same(fk1, orderType.GetForeignKeys().Single());

            var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);

            Assert.Same(fk2, orderType.GetForeignKey(customerFk2));
            Assert.Same(fk2, orderType.FindForeignKey(customerFk2));
            Assert.Same(fk2, orderType.GetOrAddForeignKey(customerFk2, new Key(new[] { idProperty }), orderType));
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
                Strings.DuplicateForeignKey("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Order).FullName),
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
                Strings.ForeignKeyPropertiesWrongEntity("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Customer).FullName),
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
            entityType.RemoveProperty(fkProperty);

            Assert.Equal(
                Strings.ForeignKeyPropertiesWrongEntity("{'fk'}", typeof(Customer).FullName),
                Assert.Throws<ArgumentException>(() => entityType.AddForeignKey(new[] { fkProperty }, key, entityType)).Message);
        }

        [Fact]
        public void Adding_a_foreign_key_throws_if_key_was_removed()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty(Customer.IdProperty);
            var key = entityType.GetOrAddKey(idProperty);
            entityType.RemoveKey(key);
            var fkProperty = entityType.AddProperty("fk", typeof(int));

            Assert.Equal(
                Strings.ForeignKeyReferencedEntityKeyMismatch("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).FullName),
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
                Strings.EntityTypeModelMismatch(typeof(Customer).FullName, typeof(Order).FullName),
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
            Assert.Same(fk2, orderType.GetForeignKey(customerFk2));
            Assert.Same(fk2, orderType.FindForeignKey(customerFk2));
            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());
            Assert.Same(fk2, orderType.GetOrAddForeignKey(customerFk2, customerKey, customerType));
            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());
        }

        [Fact]
        public void TryGetForeignKey_finds_foreign_key_matching_principal_type_name_plus_PK_name()
        {
            var fkProperty = DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey(), PrincipalType);

            Assert.Same(fk, DependentType.FindForeignKey(
                PrincipalType,
                "SomeNav",
                "SomeInverse",
                null,
                null,
                unique: false));
        }

        [Fact]
        public void TryGetForeignKey_finds_foreign_key_matching_given_properties()
        {
            DependentType.AddProperty("SomeNavID", typeof(int));
            DependentType.AddProperty("SomeNavPeEKaY", typeof(int));
            DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));
            var fkProperty = DependentType.AddProperty("HeToldMeYouKilledMyFk", typeof(int));

            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey(), PrincipalType);

            Assert.Same(
                fk,
                DependentType.FindForeignKey(
                    PrincipalType,
                    "SomeNav",
                    "SomeInverse",
                    new[] { fkProperty },
                    new Property[0],
                    unique: false));
        }

        [Fact]
        public void TryGetForeignKey_finds_foreign_key_matching_given_property()
        {
            DependentType.AddProperty("SomeNavID", typeof(int));
            DependentType.AddProperty("SomeNavPeEKaY", typeof(int));
            DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));
            var fkProperty1 = DependentType.AddProperty("No", typeof(int));
            var fkProperty2 = DependentType.AddProperty("IAmYourFk", typeof(int));

            var fk = DependentType.GetOrAddForeignKey(new[] { fkProperty1, fkProperty2 }, PrincipalType.GetOrAddKey(
                new[]
                {
                    PrincipalType.AddProperty("Id1", typeof(int)),
                    PrincipalType.AddProperty("Id2", typeof(int))
                }),
                PrincipalType);

            Assert.Same(
                fk,
                DependentType.FindForeignKey(
                    PrincipalType,
                    "SomeNav",
                    "SomeInverse",
                    new[] { fkProperty1, fkProperty2 },
                    new Property[0],
                    unique: false));
        }

        [Fact]
        public void TryGetForeignKey_finds_foreign_key_matching_navigation_plus_Id()
        {
            var fkProperty = DependentType.AddProperty("SomeNavID", typeof(int));
            DependentType.AddProperty("SomeNavPeEKaY", typeof(int));
            DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey(), PrincipalType);

            Assert.Same(
                fk,
                DependentType.FindForeignKey(
                    PrincipalType,
                    "SomeNav",
                    "SomeInverse",
                    null,
                    null,
                    unique: false));
        }

        [Fact]
        public void TryGetForeignKey_finds_foreign_key_matching_navigation_plus_PK_name()
        {
            var fkProperty = DependentType.AddProperty("SomeNavPeEKaY", typeof(int));
            DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey(), PrincipalType);

            Assert.Same(
                fk,
                DependentType.FindForeignKey(
                    PrincipalType,
                    "SomeNav",
                    "SomeInverse",
                    null,
                    null,
                    unique: false));
        }

        [Fact]
        public void TryGetForeignKey_finds_foreign_key_matching_principal_type_name_plus_Id()
        {
            var fkProperty = DependentType.AddProperty("PrincipalEntityID", typeof(int));
            DependentType.AddProperty("PrincipalEntityPeEKaY", typeof(int));

            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey(), PrincipalType);

            Assert.Same(
                fk,
                DependentType.FindForeignKey(
                    PrincipalType,
                    "SomeNav",
                    "SomeInverse",
                    null,
                    null,
                    unique: false));
        }

        [Fact]
        public void TryGetForeignKey_does_not_find_existing_FK_if_FK_has_different_navigation_to_principal()
        {
            var fkProperty = DependentType.AddProperty("SharedFk", typeof(int));
            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey(), PrincipalType);
            DependentType.AddNavigation("AnotherNav", fk, pointsToPrincipal: true);

            var newFk = DependentType.FindForeignKey(
                PrincipalType,
                "SomeNav",
                "SomeInverse",
                new[] { fkProperty },
                new Property[0],
                unique: false);

            Assert.Null(newFk);
        }

        [Fact]
        public void TryGetForeignKey_does_not_find_existing_FK_if_FK_has_different_navigation_to_dependent()
        {
            var fkProperty = DependentType.AddProperty("SharedFk", typeof(int));
            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey(), PrincipalType);
            PrincipalType.AddNavigation("AnotherNav", fk, pointsToPrincipal: false);

            var newFk = DependentType.FindForeignKey(
                PrincipalType,
                "SomeNav",
                "SomeInverse",
                new[] { fkProperty },
                new Property[0],
                unique: false);

            Assert.Null(newFk);
        }

        [Fact]
        public void TryGetForeignKey_does_not_find_existing_FK_if_FK_has_different_uniqueness()
        {
            var fkProperty = DependentType.AddProperty("SharedFk", typeof(int));
            var fk = DependentType.GetOrAddForeignKey(fkProperty, PrincipalType.GetPrimaryKey(), PrincipalType);
            fk.IsUnique = true;

            var newFk = DependentType.FindForeignKey(
                PrincipalType,
                "SomeNav",
                "SomeInverse",
                new[] { fkProperty },
                new Property[0],
                unique: false);

            Assert.Null(newFk);
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

        private EntityType DependentType => _model.GetEntityType(typeof(DependentEntity));

        private EntityType PrincipalType => _model.GetEntityType(typeof(PrincipalEntity));

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

            Assert.Equal(
                Strings.ForeignKeyNotFound("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Order).FullName),
                Assert.Throws<ModelItemNotFoundException>(() => orderType.GetForeignKey(customerFk1)).Message);
            Assert.Null(orderType.RemoveForeignKey(new ForeignKey(new[] { customerFk2 }, customerKey, orderType, customerType)));

            var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);
            var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);

            Assert.Equal(new[] { fk1, fk2 }, orderType.GetForeignKeys().ToArray());

            Assert.Same(fk1, orderType.RemoveForeignKey(fk1));
            Assert.Null(orderType.RemoveForeignKey(fk1));

            Assert.Equal(
                Strings.ForeignKeyNotFound("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Order).FullName),
                Assert.Throws<ModelItemNotFoundException>(() => orderType.GetForeignKey(customerFk1)).Message);
            Assert.Equal(new[] { fk2 }, orderType.GetForeignKeys().ToArray());

            Assert.Same(fk2, orderType.RemoveForeignKey(new ForeignKey(new[] { customerFk2 }, customerKey, orderType, customerType)));

            Assert.Empty(orderType.GetForeignKeys());
        }

        [Fact]
        public void Removing_a_foreign_key_throws_if_it_referenced_from_a_navigation_in_the_model()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var customerFk = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var fk = orderType.GetOrAddForeignKey(customerFk, customerKey, customerType);

            orderType.AddNavigation("Customer", fk, pointsToPrincipal: true);

            Assert.Equal(
                Strings.ForeignKeyInUse("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Order).FullName, "Customer", typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() => orderType.RemoveForeignKey(fk)).Message);

            customerType.AddNavigation("Orders", fk, pointsToPrincipal: false);

            Assert.Equal(
                Strings.ForeignKeyInUse("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Order).FullName, "Orders", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => orderType.RemoveForeignKey(fk)).Message);
        }

        [Fact]
        public void Foreign_keys_are_ordered_by_property_count_then_property_names()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var idProperty = customerType.GetOrAddProperty(Customer.IdProperty);
            var nameProperty = customerType.GetOrAddProperty(Customer.NameProperty);
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
            Assert.Null(orderType.RemoveNavigation(new Navigation("Customer", customerForeignKey)));

            var customerNavigation = orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true);
            var ordersNavigation = customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false);

            Assert.Equal("Customer", customerNavigation.Name);
            Assert.Same(orderType, customerNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
            Assert.True(customerNavigation.PointsToPrincipal());
            Assert.False(customerNavigation.IsCollection());
            Assert.Same(customerType, customerNavigation.GetTargetType());
            Assert.Same(customerNavigation, customerForeignKey.DependentToPrincipal);

            Assert.Equal("Orders", ordersNavigation.Name);
            Assert.Same(customerType, ordersNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, ordersNavigation.ForeignKey);
            Assert.False(ordersNavigation.PointsToPrincipal());
            Assert.True(ordersNavigation.IsCollection());
            Assert.Same(orderType, ordersNavigation.GetTargetType());
            Assert.Same(ordersNavigation, customerForeignKey.PrincipalToDependent);

            Assert.Same(customerNavigation, orderType.Navigations.Single());
            Assert.Same(ordersNavigation, customerType.Navigations.Single());

            Assert.Same(customerNavigation, orderType.RemoveNavigation(customerNavigation));
            Assert.Null(orderType.RemoveNavigation(customerNavigation));
            Assert.Empty(orderType.Navigations);

            Assert.Same(ordersNavigation, customerType.RemoveNavigation(new Navigation("Orders", customerForeignKey)));
            Assert.Empty(customerType.Navigations);
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

            var customerNavigation = orderType.GetOrAddNavigation("Customer", customerForeignKey, pointsToPrincipal: true);

            Assert.Equal("Customer", customerNavigation.Name);
            Assert.Same(orderType, customerNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
            Assert.True(customerNavigation.PointsToPrincipal());
            Assert.False(customerNavigation.IsCollection());
            Assert.Same(customerType, customerNavigation.GetTargetType());

            Assert.Same(customerNavigation, orderType.GetOrAddNavigation("Customer", customerForeignKey, pointsToPrincipal: false));
            Assert.True(customerNavigation.PointsToPrincipal());
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

            var customerNavigation = orderType.GetOrAddNavigation("Customer", customerForeignKey, pointsToPrincipal: true);

            Assert.Same(customerNavigation, orderType.FindNavigation("Customer"));
            Assert.Same(customerNavigation, orderType.GetNavigation("Customer"));

            Assert.Null(orderType.FindNavigation("Nose"));

            Assert.Equal(
                Strings.NavigationNotFound("Nose", typeof(Order).FullName),
                Assert.Throws<ModelItemNotFoundException>(() => orderType.GetNavigation("Nose")).Message);
        }

        [Fact]
        public void Adding_a_new_navigation_with_a_name_that_already_exists_throws()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true);

            Assert.Equal(
                Strings.DuplicateNavigation("Customer", typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true)).Message);
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
                Strings.ConflictingProperty("Customer", typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true)).Message);
        }

        [Fact]
        public void Adding_a_navigation_that_belongs_to_a_different_type_throws()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            Assert.Equal(
                Strings.NavigationOnWrongEntityType("Customer", typeof(Customer).FullName, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() => customerType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true)).Message);
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
                Strings.NavigationOnShadowEntity("Customer", "Order"),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true)).Message);
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
                Strings.NavigationToShadowEntity("Customer", typeof(Order).FullName, "Customer"),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true)).Message);
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
                Strings.NoClrNavigation("Snook", typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddNavigation("Snook", customerForeignKey, pointsToPrincipal: true)).Message);
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
                Strings.NavigationCollectionWrongClrType("NotCollectionOrders", typeof(Customer).FullName, typeof(Order).FullName, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => customerType.AddNavigation("NotCollectionOrders", customerForeignKey, pointsToPrincipal: false)).Message);
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
                Strings.NavigationCollectionWrongClrType("DerivedOrders", typeof(SpecialCustomer).FullName, typeof(IEnumerable<SpecialOrder>).FullName, typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => customerType.AddNavigation("DerivedOrders", customerForeignKey, pointsToPrincipal: false)).Message);
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

            var ordersNavigation = customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false);

            Assert.Equal("Orders", ordersNavigation.Name);
            Assert.Same(customerType, ordersNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, ordersNavigation.ForeignKey);
            Assert.False(ordersNavigation.PointsToPrincipal());
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
                Strings.NavigationSingleWrongClrType("OrderCustomer", typeof(Order).FullName, typeof(Order).FullName, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddNavigation("OrderCustomer", customerForeignKey, pointsToPrincipal: true)).Message);
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
                Strings.NavigationSingleWrongClrType("DerivedCustomer", typeof(SpecialOrder).FullName, typeof(SpecialCustomer).FullName, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => orderType.AddNavigation("DerivedCustomer", customerForeignKey, pointsToPrincipal: true)).Message);
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

            var customerNavigation = orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true);

            Assert.Equal("Customer", customerNavigation.Name);
            Assert.Same(orderType, customerNavigation.DeclaringEntityType);
            Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
            Assert.True(customerNavigation.PointsToPrincipal());
            Assert.False(customerNavigation.IsCollection());
            Assert.Same(customerType, customerNavigation.GetTargetType());
        }

        [Fact]
        public void Multiple_sets_of_navigations_using_the_same_foreign_key_are_not_allowed()
        {
            var model = new Model();
            var customerType = model.AddEntityType(typeof(Customer));
            var customerKey = customerType.GetOrAddKey(customerType.GetOrAddProperty(Customer.IdProperty));

            var orderType = model.AddEntityType(typeof(Order));
            var foreignKeyProperty = orderType.GetOrAddProperty(Order.CustomerIdProperty);
            var customerForeignKey = orderType.GetOrAddForeignKey(foreignKeyProperty, customerKey, customerType);

            customerType.AddNavigation("EnumerableOrders", customerForeignKey, pointsToPrincipal: false);

            Assert.Equal(
                Strings.MultipleNavigations("Orders", "EnumerableOrders", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false)).Message);
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

            var navigationToDependent = entityType.AddNavigation("SelfRef1", fk, pointsToPrincipal: false);
            var navigationToPrincipal = entityType.AddNavigation("SelfRef2", fk, pointsToPrincipal: true);

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

            entityType.AddNavigation("SelfRef1", fk, pointsToPrincipal: false);
            Assert.Equal(Strings.DuplicateNavigation("SelfRef1", typeof(SelfRef).FullName),
                Assert.Throws<InvalidOperationException>(() => entityType.AddNavigation("SelfRef1", fk, pointsToPrincipal: true)).Message);
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

            var navigation2 = customerType.AddNavigation("Orders", customerForeignKey, pointsToPrincipal: false);
            var navigation1 = customerType.AddNavigation("DerivedOrders", specialCustomerForeignKey, pointsToPrincipal: false);

            Assert.True(new[] { navigation1, navigation2 }.SequenceEqual(customerType.Navigations));
        }

        [Fact]
        public void Can_add_retrieve_and_remove_indexes()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Order));
            var property1 = entityType.AddProperty(Order.IdProperty);
            var property2 = entityType.AddProperty(Order.CustomerIdProperty);

            Assert.Equal(0, entityType.Indexes.Count());
            Assert.Null(entityType.RemoveIndex(new Index(new[] { property1 })));

            var index1 = entityType.GetOrAddIndex(property1);

            Assert.Equal(1, index1.Properties.Count);
            Assert.Same(index1, entityType.GetIndex(property1));
            Assert.Same(index1, entityType.FindIndex(property1));
            Assert.Same(property1, index1.Properties[0]);

            var index2 = entityType.AddIndex(new[] { property1, property2 });

            Assert.Equal(2, index2.Properties.Count);
            Assert.Same(index2, entityType.GetOrAddIndex(new[] { property1, property2 }));
            Assert.Same(index2, entityType.FindIndex(new[] { property1, property2 }));
            Assert.Same(property1, index2.Properties[0]);
            Assert.Same(property2, index2.Properties[1]);

            Assert.Equal(2, entityType.Indexes.Count());
            Assert.Same(index1, entityType.Indexes.First());
            Assert.Same(index2, entityType.Indexes.Last());

            Assert.Same(index1, entityType.RemoveIndex(index1));
            Assert.Null(entityType.RemoveIndex(index1));

            Assert.Equal(1, entityType.Indexes.Count());
            Assert.Same(index2, entityType.Indexes.Single());

            Assert.Same(index2, entityType.RemoveIndex(new Index(new[] { property1, property2 })));

            Assert.Equal(0, entityType.Indexes.Count());
        }

        [Fact]
        public void AddIndex_throws_if_not_from_same_entity()
        {
            var model = new Model();
            var entityType1 = model.AddEntityType(typeof(Customer));
            var entityType2 = model.AddEntityType(typeof(Order));
            var property1 = entityType1.AddProperty(Customer.IdProperty);
            var property2 = entityType1.AddProperty(Customer.NameProperty);

            Assert.Equal(Strings.IndexPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Order).FullName),
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

            Assert.Equal(Strings.DuplicateIndex("{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(
                    () => entityType.AddIndex(new[] { property1, property2 })).Message);
        }

        [Fact]
        public void GetIndex_throws_if_index_not_found()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property1 = entityType.AddProperty(Customer.IdProperty);
            var property2 = entityType.AddProperty(Customer.NameProperty);

            Assert.Equal(Strings.IndexNotFound("{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<ModelItemNotFoundException>(
                    () => entityType.GetIndex(new[] { property1, property2 })).Message);

            entityType.AddIndex(property1);

            Assert.Equal(Strings.IndexNotFound("{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).FullName),
                Assert.Throws<ModelItemNotFoundException>(
                    () => entityType.GetIndex(new[] { property1, property2 })).Message);
        }

        [Fact]
        public void Can_add_and_remove_properties()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            Assert.Null(entityType.RemoveProperty(new Property("Id", entityType)));

            var property1 = entityType.AddProperty("Id", typeof(int));
            property1.IsShadowProperty = false;

            Assert.False(property1.IsShadowProperty);
            Assert.Equal("Id", property1.Name);
            Assert.Same(typeof(int), property1.ClrType);
            Assert.False(((IProperty)property1).IsConcurrencyToken);
            Assert.Same(entityType, property1.DeclaringEntityType);

            var property2 = entityType.AddProperty("Name", typeof(string));
            property2.IsShadowProperty = false;

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));

            Assert.Same(property1, entityType.RemoveProperty(property1));
            Assert.Null(entityType.RemoveProperty(property1));

            Assert.True(new[] { property2 }.SequenceEqual(entityType.Properties));

            Assert.Same(property2, entityType.RemoveProperty(new Property("Name", entityType)));

            Assert.Empty(entityType.Properties);
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
            Assert.Same(nameProperty, entityType.GetProperty("Name"));
            Assert.False(nameProperty.IsShadowProperty);

            Assert.True(new[] { idProperty, nameProperty }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Cannot_remove_property_when_used_by_primary_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.GetOrAddProperty(Customer.IdProperty);

            entityType.GetOrSetPrimaryKey(property);

            Assert.Equal(
                Strings.PropertyInUse("Id", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property)).Message);
        }

        [Fact]
        public void Cannot_remove_property_when_used_by_non_primary_key()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.GetOrAddProperty(Customer.IdProperty);

            entityType.GetOrAddKey(property);

            Assert.Equal(
                Strings.PropertyInUse("Id", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property)).Message);
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
                Strings.PropertyInUse("CustomerId", typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() => orderType.RemoveProperty(customerFk)).Message);
        }

        [Fact]
        public void Cannot_remove_property_when_used_by_an_index()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.GetOrAddProperty(Customer.IdProperty);

            entityType.GetOrAddIndex(property);

            Assert.Equal(
                Strings.PropertyInUse("Id", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property)).Message);
        }

        [Fact]
        public void Properties_are_ordered_by_name()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            var property2 = entityType.AddProperty(Customer.NameProperty);
            var property1 = entityType.AddProperty(Customer.IdProperty);

            Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Primary_key_properties_precede_others()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            var aProperty = entityType.AddProperty("A", typeof(int));
            var pkProperty = entityType.AddProperty(Customer.IdProperty);

            entityType.SetPrimaryKey(pkProperty);

            Assert.True(new[] { pkProperty, aProperty }.SequenceEqual(entityType.Properties));
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

            Assert.True(new[] { pkProperty1, pkProperty2, aProperty }.SequenceEqual(entityType.Properties));
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
            childType.BaseType = parentType;

            Assert.True(new[] { property1, property2, property3, property4 }.SequenceEqual(childType.Properties));
        }

        [Fact]
        public void Properties_are_properly_ordered_when_primary_key_changes()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            var aProperty = entityType.AddProperty("A", typeof(int));
            var bProperty = entityType.AddProperty("B", typeof(int));

            entityType.SetPrimaryKey(bProperty);

            Assert.True(new[] { bProperty, aProperty }.SequenceEqual(entityType.Properties));

            entityType.SetPrimaryKey(aProperty);

            Assert.True(new[] { aProperty, bProperty }.SequenceEqual(entityType.Properties));
        }

        [Fact]
        public void Can_get_property_and_can_try_get_property()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var property = entityType.GetOrAddProperty(Customer.IdProperty);

            Assert.Same(property, entityType.FindProperty(Customer.IdProperty));
            Assert.Same(property, entityType.FindProperty("Id"));
            Assert.Same(property, entityType.GetProperty(Customer.IdProperty));
            Assert.Same(property, entityType.GetProperty("Id"));

            Assert.Null(entityType.FindProperty("Nose"));

            Assert.Equal(
                Strings.PropertyNotFound("Nose", typeof(Customer).FullName),
                Assert.Throws<ModelItemNotFoundException>(() => entityType.GetProperty("Nose")).Message);
        }

        [Fact]
        public void Shadow_properties_have_CLR_flag_set_to_false()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            entityType.GetOrAddProperty(Customer.NameProperty);
            entityType.AddProperty(Customer.IdProperty);
            entityType.AddProperty("Mane", typeof(int));

            Assert.False(entityType.GetProperty("Name").IsShadowProperty);
            Assert.False(entityType.GetProperty("Id").IsShadowProperty);
            Assert.Null(entityType.GetProperty("Mane").IsShadowProperty);
        }

        [Fact]
        public void Adding_a_new_property_with_a_name_that_already_exists_throws()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            entityType.AddProperty(Customer.IdProperty);

            Assert.Equal(
                Strings.DuplicateProperty("Id", typeof(Customer).FullName),
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

            orderType.AddNavigation("Customer", customerForeignKey, pointsToPrincipal: true);

            Assert.Equal(
                Strings.ConflictingNavigation("Customer", typeof(Order).FullName),
                Assert.Throws<InvalidOperationException>(() => orderType.AddProperty("Customer")).Message);
        }

        [Fact]
        public void Adding_a_CLR_property_from_wrong_CLR_type_throws()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));

            Assert.Equal(
                Strings.PropertyWrongEntityClrType(Order.CustomerIdProperty.Name, typeof(Customer).FullName, typeof(Order).Name),
                Assert.Throws<ArgumentException>(() => entityType.GetOrAddProperty(Order.CustomerIdProperty)).Message);
        }

        [Fact]
        public void Adding_a_CLR_property_to_shadow_type_throws()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer).Name);

            Assert.Equal(
                Strings.ClrPropertyOnShadowEntity(Order.CustomerIdProperty.Name, typeof(Customer).Name),
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

            Assert.Equal(0, entityType.GetProperty("Id").Index);
            Assert.Equal(1, entityType.GetProperty("Mane").Index);
            Assert.Equal(2, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Id").GetShadowIndex());
            Assert.Equal(1, entityType.GetProperty("Mane").GetShadowIndex());
            Assert.Equal(-1, entityType.GetProperty("Name").GetShadowIndex());

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

            Assert.Equal(0, entityType.GetProperty("Id").Index);
            Assert.Equal(1, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Id").GetShadowIndex());
            Assert.Equal(-1, entityType.GetProperty("Name").GetShadowIndex());

            Assert.Equal(0, entityType.GetProperty("Id").GetOriginalValueIndex());
            Assert.Equal(-1, entityType.GetProperty("Name").GetOriginalValueIndex());

            Assert.Equal(1, entityType.ShadowPropertyCount());
            Assert.Equal(1, entityType.OriginalValueCount());

            var gameProperty = entityType.AddProperty("Game", typeof(int));
            gameProperty.IsConcurrencyToken = true;

            var maneProperty = entityType.AddProperty("Mane", typeof(int));
            maneProperty.IsConcurrencyToken = true;

            Assert.Equal(0, entityType.GetProperty("Game").Index);
            Assert.Equal(1, entityType.GetProperty("Id").Index);
            Assert.Equal(2, entityType.GetProperty("Mane").Index);
            Assert.Equal(3, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Game").GetShadowIndex());
            Assert.Equal(1, entityType.GetProperty("Id").GetShadowIndex());
            Assert.Equal(2, entityType.GetProperty("Mane").GetShadowIndex());
            Assert.Equal(-1, entityType.GetProperty("Name").GetShadowIndex());

            Assert.Equal(0, entityType.GetProperty("Game").GetOriginalValueIndex());
            Assert.Equal(1, entityType.GetProperty("Id").GetOriginalValueIndex());
            Assert.Equal(2, entityType.GetProperty("Mane").GetOriginalValueIndex());
            Assert.Equal(-1, entityType.GetProperty("Name").GetOriginalValueIndex());

            Assert.Equal(3, entityType.ShadowPropertyCount());
            Assert.Equal(3, entityType.OriginalValueCount());

            gameProperty.IsConcurrencyToken = false;
            nameProperty.IsConcurrencyToken = true;

            Assert.Equal(0, entityType.GetProperty("Game").Index);
            Assert.Equal(1, entityType.GetProperty("Id").Index);
            Assert.Equal(2, entityType.GetProperty("Mane").Index);
            Assert.Equal(3, entityType.GetProperty("Name").Index);

            Assert.Equal(0, entityType.GetProperty("Game").GetShadowIndex());
            Assert.Equal(1, entityType.GetProperty("Id").GetShadowIndex());
            Assert.Equal(2, entityType.GetProperty("Mane").GetShadowIndex());
            Assert.Equal(-1, entityType.GetProperty("Name").GetShadowIndex());

            Assert.Equal(-1, entityType.GetProperty("Game").GetOriginalValueIndex());
            Assert.Equal(0, entityType.GetProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.GetProperty("Mane").GetOriginalValueIndex());
            Assert.Equal(2, entityType.GetProperty("Name").GetOriginalValueIndex());

            Assert.Equal(3, entityType.ShadowPropertyCount());
            Assert.Equal(3, entityType.OriginalValueCount());

            gameProperty.IsShadowProperty = false;
            nameProperty.IsShadowProperty = true;

            Assert.Equal(0, entityType.GetProperty("Game").Index);
            Assert.Equal(1, entityType.GetProperty("Id").Index);
            Assert.Equal(2, entityType.GetProperty("Mane").Index);
            Assert.Equal(3, entityType.GetProperty("Name").Index);

            Assert.Equal(-1, entityType.GetProperty("Game").GetShadowIndex());
            Assert.Equal(0, entityType.GetProperty("Id").GetShadowIndex());
            Assert.Equal(1, entityType.GetProperty("Mane").GetShadowIndex());
            Assert.Equal(2, entityType.GetProperty("Name").GetShadowIndex());

            Assert.Equal(-1, entityType.GetProperty("Game").GetOriginalValueIndex());
            Assert.Equal(0, entityType.GetProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.GetProperty("Mane").GetOriginalValueIndex());
            Assert.Equal(2, entityType.GetProperty("Name").GetOriginalValueIndex());

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

            Assert.True(new[] { i1, i2, i3, i4 }.SequenceEqual(customerType.Indexes));
        }

        [Fact]
        public void Lazy_original_values_are_used_for_full_notification_and_shadow_enties()
        {
            Assert.False(new EntityType(typeof(FullNotificationEntity), new Model()).UseEagerSnapshots);
        }

        [Fact]
        public void Lazy_original_values_are_used_for_shadow_enties()
        {
            Assert.False(new EntityType("Z'ha'dum", new Model()).UseEagerSnapshots);
        }

        [Fact]
        public void Eager_original_values_are_used_for_enties_that_only_implement_INotifyPropertyChanged()
        {
            Assert.True(new EntityType(typeof(ChangedOnlyEntity), new Model()).UseEagerSnapshots);
        }

        [Fact]
        public void Eager_original_values_are_used_for_enties_that_do_no_notification()
        {
            Assert.True(new EntityType(typeof(Customer), new Model()).UseEagerSnapshots);
        }

        [Fact]
        public void Lazy_original_values_can_be_switched_off()
        {
            Assert.False(new EntityType(typeof(FullNotificationEntity), new Model()) { UseEagerSnapshots = false }.UseEagerSnapshots);
        }

        [Fact]
        public void Lazy_original_values_can_be_switched_on_but_only_if_entity_does_not_require_eager_values()
        {
            var entityType = new EntityType(typeof(FullNotificationEntity), new Model()) { UseEagerSnapshots = true };
            entityType.UseEagerSnapshots = false;
            Assert.False(entityType.UseEagerSnapshots);

            Assert.Equal(
                Strings.EagerOriginalValuesRequired(typeof(ChangedOnlyEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => new EntityType(typeof(ChangedOnlyEntity), new Model()) { UseEagerSnapshots = false }).Message);
        }

        [Fact]
        public void All_properties_have_original_value_indexes_when_using_eager_original_values()
        {
            var entityType = new EntityType(typeof(FullNotificationEntity), new Model()) { UseEagerSnapshots = true };

            entityType.AddProperty(FullNotificationEntity.NameProperty);
            entityType.AddProperty(FullNotificationEntity.IdProperty);

            Assert.Equal(0, entityType.GetProperty("Id").GetOriginalValueIndex());
            Assert.Equal(1, entityType.GetProperty("Name").GetOriginalValueIndex());

            Assert.Equal(2, entityType.OriginalValueCount());
        }

        [Fact]
        public void Only_required_properties_have_original_value_indexes_when_using_lazy_original_values()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(FullNotificationEntity));

            entityType.AddProperty(FullNotificationEntity.NameProperty).IsConcurrencyToken = true;
            entityType.AddProperty(FullNotificationEntity.IdProperty);

            Assert.Equal(-1, entityType.GetProperty("Id").GetOriginalValueIndex());
            Assert.Equal(0, entityType.GetProperty("Name").GetOriginalValueIndex());

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

            private void NotifyChanged([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            private void NotifyChanging([CallerMemberName] String propertyName = "")
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

            private void NotifyChanged([CallerMemberName] String propertyName = "")
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
