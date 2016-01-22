// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ClrPropertySetterFactoryTest
    {
        [Fact]
        public void Property_is_returned_if_it_implements_IClrPropertySetter()
        {
            var setterMock = new Mock<IClrPropertySetter>();
            var propertyMock = setterMock.As<IProperty>();

            var source = new ClrPropertySetterFactory();

            Assert.Same(setterMock.Object, source.Create(propertyMock.Object));
        }

        [Fact]
        public void Delegate_setter_is_returned_for_IProperty_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, 77);

            Assert.Equal(77, customer.Id);
        }

        [Fact]
        public void Delegate_setter_is_returned_for_property_type_and_name()
        {
            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(typeof(Customer).GetAnyProperty("Id")).SetClrValue(customer, 77);

            Assert.Equal(77, customer.Id);
        }

        [Fact]
        public void Delegate_setter_can_set_value_type_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.IdProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, 1);

            Assert.Equal(1, customer.Id);
        }

        [Fact]
        public void Delegate_setter_can_set_reference_type_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.ContentProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, "MyString");

            Assert.Equal("MyString", customer.Content);
        }

        [Fact]
        public void Delegate_setter_can_set_nullable_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.OptionalIntProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, 3);

            Assert.Equal(3, customer.OptionalInt);
        }

        [Fact]
        public void Delegate_setter_can_set_nullable_property_with_null_value()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.OptionalIntProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, null);

            Assert.Null(customer.OptionalInt);
        }

        [Fact]
        public void Delegate_setter_can_set_enum_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.FlagProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, Flag.One);

            Assert.Equal(Flag.One, customer.Flag);
        }

        [Fact]
        public void Delegate_setter_can_set_nullable_enum_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty(Customer.OptionalFlagProperty);

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterFactory().Create(idProperty).SetClrValue(customer, Flag.Two);

            Assert.Equal(Flag.Two, customer.OptionalFlag);
        }

        #region Fixture

        private enum Flag
        {
            One,
            Two
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo OptionalIntProperty = typeof(Customer).GetProperty("OptionalInt");
            public static readonly PropertyInfo ContentProperty = typeof(Customer).GetProperty("Content");
            public static readonly PropertyInfo FlagProperty = typeof(Customer).GetProperty("Flag");
            public static readonly PropertyInfo OptionalFlagProperty = typeof(Customer).GetProperty("OptionalFlag");

            public int Id { get; set; }
            public string Content { get; set; }
            public int? OptionalInt { get; set; }
            public Flag Flag { get; set; }
            public Flag? OptionalFlag { get; set; }
        }

        #endregion
    }
}
