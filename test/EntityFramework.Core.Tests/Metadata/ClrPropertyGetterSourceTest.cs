// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
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
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty("Id", typeof(int));

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
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.GetOrAddProperty("Id", typeof(int));

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
