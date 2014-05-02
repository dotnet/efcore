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
    public class ClrPropertyGetterSourceTest
    {
        [Fact]
        public void Property_is_returned_if_it_implements_IClrPropertyGetter()
        {
            var getterMock = new Mock<IClrPropertyGetter>();
            var propertyMock = getterMock.As<IProperty>();

            var source = new ClrPropertyGetterSource();

            Assert.Same(getterMock.Object, source.GetAccessor(propertyMock.Object));
        }

        [Fact]
        public void Delegate_getter_is_returned_for_IProperty_property()
        {
            var entityType = new EntityType(typeof(Customer));
            var idProperty = entityType.AddProperty("Id", typeof(int));

            Assert.Equal(7, new ClrPropertyGetterSource().GetAccessor(idProperty).GetClrValue(new Customer { Id = 7 }));
        }

        [Fact]
        public void Delegate_getter_is_returned_for_property_type_and_name()
        {
            Assert.Equal(7, new ClrPropertyGetterSource().GetAccessor(typeof(Customer), "Id").GetClrValue(new Customer { Id = 7 }));
        }

        [Fact]
        public void Delegate_getter_is_cached_by_type_and_property_name()
        {
            var entityType = new EntityType(typeof(Customer));
            var idProperty = entityType.AddProperty("Id", typeof(int));

            var source = new ClrPropertyGetterSource();

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
