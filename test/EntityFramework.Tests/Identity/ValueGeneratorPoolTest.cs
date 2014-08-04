// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Identity
{
    public class ValueGeneratorPoolTest
    {
        [Fact]
        public void Lazily_creates_generators_as_requested_up_to_pool_size()
        {
            var property = CreateProperty();

            var factoryMock = new Mock<IValueGeneratorFactory>();
            factoryMock.Setup(m => m.Create(property)).Returns(CreateValueGeneratorCallback);

            var pool = new ValueGeneratorPool(factoryMock.Object, property, poolSize: 3);

            factoryMock.Verify(m => m.Create(It.IsAny<Property>()), Times.Never);

            var generator1 = pool.GetGenerator();

            factoryMock.Verify(m => m.Create(It.IsAny<Property>()), Times.Once);
            factoryMock.Verify(m => m.Create(property), Times.Once);

            var generator2 = pool.GetGenerator();

            factoryMock.Verify(m => m.Create(It.IsAny<Property>()), Times.Exactly(2));
            factoryMock.Verify(m => m.Create(property), Times.Exactly(2));
            Assert.NotSame(generator1, generator2);

            var generator3 = pool.GetGenerator();

            factoryMock.Verify(m => m.Create(It.IsAny<Property>()), Times.Exactly(3));
            factoryMock.Verify(m => m.Create(property), Times.Exactly(3));
            Assert.NotSame(generator2, generator3);

            Assert.Same(generator1, pool.GetGenerator());
            Assert.Same(generator2, pool.GetGenerator());
            Assert.Same(generator3, pool.GetGenerator());
            Assert.Same(generator1, pool.GetGenerator());
            Assert.Same(generator2, pool.GetGenerator());
            Assert.Same(generator3, pool.GetGenerator());

            factoryMock.Verify(m => m.Create(It.IsAny<Property>()), Times.Exactly(3));
            factoryMock.Verify(m => m.Create(property), Times.Exactly(3));
        }

        private static TemporaryValueGenerator CreateValueGeneratorCallback()
        {
            return new TemporaryValueGenerator();
        }

        private static Property CreateProperty()
        {
            var entityType = new EntityType("Led");
            return entityType.AddProperty("Zeppelin", typeof(int));
        }
    }
}
