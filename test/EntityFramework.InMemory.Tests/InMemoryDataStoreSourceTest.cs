// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDataStoreSourceTest
    {
        [Fact]
        public void Returns_appropriate_name()
        {
            Assert.Equal(
                typeof(InMemoryDataStore).Name, 
                new InMemoryDataStoreSource(Mock.Of<DbContextConfiguration>(), new LazyRef<IDbContextOptions>(() => null)).Name);
        }

        [Fact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            IDbContextOptions options = new DbContextOptions();
            options.AddOrUpdateExtension<InMemoryOptionsExtension>(e => { });

            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.ContextOptions).Returns(options);

            Assert.True(new InMemoryDataStoreSource(configurationMock.Object, new LazyRef<IDbContextOptions>(options)).IsConfigured);
        }

        [Fact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            IDbContextOptions options = new DbContextOptions();

            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.ContextOptions).Returns(options);

            Assert.False(new InMemoryDataStoreSource(configurationMock.Object, new LazyRef<IDbContextOptions>(options)).IsConfigured);
        }

        [Fact]
        public void Is_always_available()
        {
            Assert.True(new InMemoryDataStoreSource(Mock.Of<DbContextConfiguration>(), new LazyRef<IDbContextOptions>(() => null)).IsAvailable);
        }
    }
}
