// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class ClrPropertySetterSourceTest
    {
        [Fact]
        public void Property_is_returned_if_it_implements_IClrPropertySetter()
        {
            var setterMock = new Mock<IClrPropertySetter>();
            var propertyMock = setterMock.As<IProperty>();

            var source = new ClrPropertySetterSource();

            Assert.Same(setterMock.Object, source.GetAccessor(propertyMock.Object));
        }

        [Fact]
        public void Delegate_setter_is_returned_for_IProperty_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty("Id", typeof(int));

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterSource().GetAccessor(idProperty).SetClrValue(customer, 77);

            Assert.Equal(77, customer.Id);
        }

        [Fact]
        public void Delegate_setter_is_returned_for_property_type_and_name()
        {
            var customer = new Customer { Id = 7 };

            new ClrPropertySetterSource().GetAccessor(typeof(Customer), "Id").SetClrValue(customer, 77);

            Assert.Equal(77, customer.Id);
        }

        [Fact]
        public void Delegate_setter_is_cached_by_type_and_property_name()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty("Id", typeof(int));

            var source = new ClrPropertySetterSource();

            var accessor = source.GetAccessor(typeof(Customer), "Id");
            Assert.Same(accessor, source.GetAccessor(typeof(Customer), "Id"));
            Assert.Same(accessor, source.GetAccessor(idProperty));
        }

        [Fact]
        public void Delegate_setter_can_set_value_type_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty("Id", typeof(int));

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterSource().GetAccessor(idProperty).SetClrValue(customer, 1);

            Assert.Equal(1, customer.Id);
        }

        [Fact]
        public void Delegate_setter_can_set_reference_type_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty("Content", typeof(string));

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterSource().GetAccessor(idProperty).SetClrValue(customer, "MyString");

            Assert.Equal("MyString", customer.Content);
        }

        [Fact]
        public void Delegate_setter_can_set_nullable_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty("OptionalInt", typeof(int?));

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterSource().GetAccessor(idProperty).SetClrValue(customer, 3);

            Assert.Equal(3, customer.OptionalInt);
        }

        [Fact]
        public void Delegate_setter_can_set_nullable_property_with_null_value()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty("OptionalInt", typeof(int?));

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterSource().GetAccessor(idProperty).SetClrValue(customer, null);

            Assert.Null(customer.OptionalInt);
        }

        [Fact]
        public void Delegate_setter_can_set_enum_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty("Flag", typeof(Flag));

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterSource().GetAccessor(idProperty).SetClrValue(customer, Flag.One);

            Assert.Equal(Flag.One, customer.Flag);
        }

        [Fact]
        public void Delegate_setter_can_set_nullable_enum_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty("OptionalFlag", typeof(Flag?));

            var customer = new Customer { Id = 7 };

            new ClrPropertySetterSource().GetAccessor(idProperty).SetClrValue(customer, Flag.Two);

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
            internal int Id { get; set; }
            internal string Content { get; set; }
            internal int? OptionalInt { get; set; }
            internal Flag Flag { get; set; }
            internal Flag? OptionalFlag { get; set; }
        }

        #endregion
    }
}
