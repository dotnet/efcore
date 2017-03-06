// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Internal
{
    public class ClrPropertyGetterFactoryTest
    {
        [Fact]
        public void Property_is_returned_if_it_implements_IClrPropertyGetter()
        {
            var getterMock = new Mock<IClrPropertyGetter>();
            var propertyMock = getterMock.As<IProperty>();

            var source = new ClrPropertyGetterFactory();

            Assert.Same(getterMock.Object, source.Create(propertyMock.Object));
        }

        [Fact]
        public void Delegate_getter_is_returned_for_IProperty_property()
        {
            var entityType = new Model().AddEntityType(typeof(Customer));
            var idProperty = entityType.AddProperty("Id", typeof(int));

            Assert.Equal(7, new ClrPropertyGetterFactory().Create(idProperty).GetClrValue(new Customer { Id = 7 }));
        }

        [Fact]
        public void Delegate_getter_is_returned_for_property_info()
        {
            Assert.Equal(7, new ClrPropertyGetterFactory().Create(typeof(Customer).GetAnyProperty("Id")).GetClrValue(new Customer { Id = 7 }));
        }

        #region Fixture

        private class Customer
        {
            internal int Id { get; set; }
        }

        #endregion
    }
}
