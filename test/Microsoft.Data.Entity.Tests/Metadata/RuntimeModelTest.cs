// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class RuntimeModelTest
    {
        [Fact]
        public void Can_get_key_factory()
        {
            var factory = Mock.Of<EntityKeyFactory>();
            var factoryMock = new Mock<EntityKeyFactorySource>();
            var keyProperties = new[] { Mock.Of<IProperty>() };
            factoryMock.Setup(m => m.GetKeyFactory(keyProperties)).Returns(factory);

            var model = new RuntimeModel(Mock.Of<IModel>(), factoryMock.Object);

            Assert.Same(factory, model.GetKeyFactory(keyProperties));
        }
    }
}
