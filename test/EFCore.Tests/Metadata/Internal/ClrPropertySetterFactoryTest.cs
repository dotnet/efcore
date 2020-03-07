// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class ClrPropertySetterFactoryTest
    {
        [ConditionalFact]
        public void Property_is_returned_if_it_implements_IClrPropertySetter()
        {
            var property = new FakeProperty();

            Assert.Same(property, new ClrPropertySetterFactory().Create(property));
        }

        private class FakeProperty : IProperty, IClrPropertySetter
        {
            public void SetClrValue(object instance, object value) => throw new NotImplementedException();
            public object this[string name] => throw new NotImplementedException();
            public IAnnotation FindAnnotation(string name) => throw new NotImplementedException();
            public IEnumerable<IAnnotation> GetAnnotations() => throw new NotImplementedException();
            public string Name { get; }
            public ITypeBase DeclaringType { get; }
            public Type ClrType { get; }
            public IEntityType DeclaringEntityType { get; }
            public bool IsNullable { get; }
            public bool IsReadOnlyBeforeSave { get; }
            public bool IsReadOnlyAfterSave { get; }
            public bool IsStoreGeneratedAlways { get; }
            public ValueGenerated ValueGenerated { get; }
            public bool IsConcurrencyToken { get; }
            public PropertyInfo PropertyInfo { get; }
            public FieldInfo FieldInfo { get; }
        }

        [ConditionalFact]
        public void Delegate_setter_is_returned_for_IProperty_property()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, 77);

            Assert.Equal(77, customer.Id);
        }

        [ConditionalFact]
        public void Delegate_setter_is_returned_for_property_type_and_name()
        {
            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(typeof(Customer).GetAnyProperty("Id")).SetClrValue(customer, 77);

            Assert.Equal(77, customer.Id);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_value_type_property()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, 1);

            Assert.Equal(1, customer.Id);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_reference_type_property()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.ContentProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, "MyString");

            Assert.Equal("MyString", customer.Content);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_nullable_property()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.OptionalIntProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, 3);

            Assert.Equal(3, customer.OptionalInt);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_nullable_property_with_null_value()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.OptionalIntProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, null);

            Assert.Null(customer.OptionalInt);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_enum_property()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.FlagProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, Flag.One);

            Assert.Equal(Flag.One, customer.Flag);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_nullable_enum_property()
        {
            var entityType = CreateModel().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.OptionalFlagProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, Flag.Two);

            Assert.Equal(Flag.Two, customer.OptionalFlag);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_on_virtual_privatesetter_property_override_singlebasetype()
        {
            var entityType = CreateModel().AddEntityType(typeof(ConcreteEntity1));
            var property = entityType.AddProperty(
                typeof(ConcreteEntity1).GetProperty(nameof(ConcreteEntity1.VirtualPrivateProperty_Override)));
            var entity = new ConcreteEntity1();

            new ClrPropertySetterFactory().Create(property).SetClrValue(entity, 100);
            Assert.Equal(100, entity.VirtualPrivateProperty_Override);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_on_virtual_privatesetter_property_override_multiplebasetypes()
        {
            var entityType = CreateModel().AddEntityType(typeof(ConcreteEntity2));
            var property = entityType.AddProperty(
                typeof(ConcreteEntity2).GetProperty(nameof(ConcreteEntity2.VirtualPrivateProperty_Override)));
            var entity = new ConcreteEntity2();

            new ClrPropertySetterFactory().Create(property).SetClrValue(entity, 100);
            Assert.Equal(100, entity.VirtualPrivateProperty_Override);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_on_virtual_privatesetter_property_no_override_singlebasetype()
        {
            var entityType = CreateModel().AddEntityType(typeof(ConcreteEntity1));
            var property = entityType.AddProperty(
                typeof(ConcreteEntity1).GetProperty(nameof(ConcreteEntity1.VirtualPrivateProperty_NoOverride)));
            var entity = new ConcreteEntity1();

            new ClrPropertySetterFactory().Create(property).SetClrValue(entity, 100);
            Assert.Equal(100, entity.VirtualPrivateProperty_NoOverride);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_on_virtual_privatesetter_property_no_override_multiplebasetypes()
        {
            var entityType = CreateModel().AddEntityType(typeof(ConcreteEntity2));
            var property = entityType.AddProperty(
                typeof(ConcreteEntity2).GetProperty(nameof(ConcreteEntity2.VirtualPrivateProperty_NoOverride)));
            var entity = new ConcreteEntity2();

            new ClrPropertySetterFactory().Create(property).SetClrValue(entity, 100);
            Assert.Equal(100, entity.VirtualPrivateProperty_NoOverride);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_on_privatesetter_property_singlebasetype()
        {
            var entityType = CreateModel().AddEntityType(typeof(ConcreteEntity1));
            var property = entityType.AddProperty(typeof(ConcreteEntity1).GetProperty(nameof(ConcreteEntity1.PrivateProperty)));
            var entity = new ConcreteEntity1();

            new ClrPropertySetterFactory().Create(property).SetClrValue(entity, 100);
            Assert.Equal(100, entity.PrivateProperty);
        }

        [ConditionalFact]
        public void Delegate_setter_can_set_on_privatesetter_property_multiplebasetypes()
        {
            var entityType = CreateModel().AddEntityType(typeof(ConcreteEntity2));
            var property = entityType.AddProperty(typeof(ConcreteEntity2).GetProperty(nameof(ConcreteEntity2.PrivateProperty)));
            var entity = new ConcreteEntity2();

            new ClrPropertySetterFactory().Create(property).SetClrValue(entity, 100);
            Assert.Equal(100, entity.PrivateProperty);
        }

        [ConditionalFact]
        public void Delegate_setter_throws_if_no_setter_found()
        {
            var entityType = CreateModel().AddEntityType(typeof(ConcreteEntity1));
            var property = entityType.AddProperty(typeof(ConcreteEntity1).GetProperty(nameof(ConcreteEntity1.NoSetterProperty)));

            Assert.Throws<InvalidOperationException>(
                () => new ClrPropertySetterFactory().Create(property));

            entityType = CreateModel().AddEntityType(typeof(ConcreteEntity2));
            property = entityType.AddProperty(typeof(ConcreteEntity2).GetProperty(nameof(ConcreteEntity2.NoSetterProperty)));

            Assert.Throws<InvalidOperationException>(
                () => new ClrPropertySetterFactory().Create(property));
        }

        private IMutableModel CreateModel()
            => new Model();

        #region Fixture

        private enum Flag
        {
            One,
            Two
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty(nameof(Id));
            public static readonly PropertyInfo OptionalIntProperty = typeof(Customer).GetProperty(nameof(OptionalInt));
            public static readonly PropertyInfo ContentProperty = typeof(Customer).GetProperty(nameof(Content));
            public static readonly PropertyInfo FlagProperty = typeof(Customer).GetProperty(nameof(Flag));
            public static readonly PropertyInfo OptionalFlagProperty = typeof(Customer).GetProperty(nameof(OptionalFlag));

            public int Id { get; set; }
            public string Content { get; set; }
            public int? OptionalInt { get; set; }
            public Flag Flag { get; set; }
            public Flag? OptionalFlag { get; set; }
        }

        private class ConcreteEntity2 : ConcreteEntity1
        {
            // ReSharper disable once RedundantOverriddenMember
            public override int VirtualPrivateProperty_Override => base.VirtualPrivateProperty_Override;
        }

        private class ConcreteEntity1 : BaseEntity
        {
            // ReSharper disable once RedundantOverriddenMember
            public override int VirtualPrivateProperty_Override => base.VirtualPrivateProperty_Override;
        }

        private class BaseEntity
        {
            public virtual int VirtualPrivateProperty_Override { get; private set; }
            public virtual int VirtualPrivateProperty_NoOverride { get; private set; }
            public int PrivateProperty { get; private set; }
            public int NoSetterProperty { get; }
        }

        #endregion
    }
}
