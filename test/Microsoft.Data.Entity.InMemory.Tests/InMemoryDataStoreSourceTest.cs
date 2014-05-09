// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDataStoreSourceTest
    {
        [Fact]
        public void Returns_appropriate_name()
        {
            Assert.Equal(typeof(InMemoryDataStore).Name, new InMemoryDataStoreSource().Name);
        }

        [Fact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            var configuration = new DbContextOptions()
                .AddBuildAction(c => c.AddOrUpdateExtension<InMemoryConfigurationExtension>(e => { }))
                .BuildConfiguration();

            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.ContextOptions).Returns(configuration);

            Assert.True(new InMemoryDataStoreSource().IsConfigured(configurationMock.Object));
        }

        [Fact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var configuration = new DbContextOptions().BuildConfiguration();

            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.ContextOptions).Returns(configuration);

            Assert.False(new InMemoryDataStoreSource().IsConfigured(configurationMock.Object));
        }

        [Fact]
        public void Is_always_available()
        {
            Assert.True(new InMemoryDataStoreSource().IsAvailable(Mock.Of<DbContextConfiguration>()));
        }
    }
}
