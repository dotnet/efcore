// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class EntityKeyFactorySourceTest
    {
        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_property_key()
        {
            var keyMock = new Mock<IProperty>();
            keyMock.Setup(m => m.PropertyType).Returns(typeof(int));

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.Key).Returns(new[] { keyMock.Object });

            Assert.IsType<SimpleEntityKeyFactory<int>>(new EntityKeyFactorySource().GetKeyFactory(typeMock.Object));
        }

        [Fact]
        public void Returns_same_simple_entity_key_factory_for_same_key_type()
        {
            var keyMock1 = new Mock<IProperty>();
            keyMock1.Setup(m => m.PropertyType).Returns(typeof(Guid));

            var typeMock1 = new Mock<IEntityType>();
            typeMock1.Setup(m => m.Key).Returns(new[] { keyMock1.Object });

            var keyMock2 = new Mock<IProperty>();
            keyMock2.Setup(m => m.PropertyType).Returns(typeof(Guid));

            var typeMock2 = new Mock<IEntityType>();
            typeMock2.Setup(m => m.Key).Returns(new[] { keyMock2.Object });

            var factorySource = new EntityKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(typeMock1.Object), factorySource.GetKeyFactory(typeMock2.Object));
        }

        [Fact]
        public void Returns_a_composite_entity_key_factory_for_composite_property_key()
        {
            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.Key).Returns(new[] { new Mock<IProperty>().Object, new Mock<IProperty>().Object });

            Assert.IsType<CompositeEntityKeyFactory>(new EntityKeyFactorySource().GetKeyFactory(typeMock.Object));
        }
    }
}
