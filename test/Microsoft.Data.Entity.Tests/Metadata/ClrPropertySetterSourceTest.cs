// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using Microsoft.Data.Entity.Metadata;
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
            var entityType = new EntityType(typeof(Customer));
            var idProperty = entityType.AddProperty("Id", typeof(int));

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
            var entityType = new EntityType(typeof(Customer));
            var idProperty = entityType.AddProperty("Id", typeof(int));

            var source = new ClrPropertySetterSource();

            var accessor = source.GetAccessor(typeof(Customer), "Id");
            Assert.Same(accessor, source.GetAccessor(typeof(Customer), "Id"));
            Assert.Same(accessor, source.GetAccessor(idProperty));
        }

        #region Fixture

        private class Customer
        {
            internal int Id { get; set; }
        }

        #endregion
    }
}
