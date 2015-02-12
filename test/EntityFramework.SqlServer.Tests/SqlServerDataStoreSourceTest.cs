// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerDataStoreSourceTest
    {
        [Fact]
        public void Returns_appropriate_name()
        {
            Assert.Equal(
                typeof(SqlServerDataStore).Name,
                new SqlServerDataStoreSource(Mock.Of<DbContextServices>(), new DbContextService<IDbContextOptions>(() => null)).Name);
        }

        [Fact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            IDbContextOptions options = new DbContextOptions();
            options.AddOrUpdateExtension<SqlServerOptionsExtension>(e => { });

            var configurationMock = new Mock<DbContextServices>();
            configurationMock.Setup(m => m.ContextOptions).Returns(options);

            Assert.True(new SqlServerDataStoreSource(configurationMock.Object, new DbContextService<IDbContextOptions>(options)).IsConfigured);
        }

        [Fact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var options = new DbContextOptions();

            var configurationMock = new Mock<DbContextServices>();
            configurationMock.Setup(m => m.ContextOptions).Returns(options);

            Assert.False(new SqlServerDataStoreSource(configurationMock.Object, new DbContextService<IDbContextOptions>(options)).IsConfigured);
        }

        [Fact]
        public void Is_available_when_configured()
        {
            IDbContextOptions options = new DbContextOptions();
            options.AddOrUpdateExtension<SqlServerOptionsExtension>(e => { });

            var configurationMock = new Mock<DbContextServices>();
            configurationMock.Setup(m => m.ContextOptions).Returns(options);

            Assert.True(new SqlServerDataStoreSource(configurationMock.Object, new DbContextService<IDbContextOptions>(options)).IsAvailable);
        }

        [Fact]
        public void Is_not_available_when_not_configured()
        {
            var options = new DbContextOptions();

            var configurationMock = new Mock<DbContextServices>();
            configurationMock.Setup(m => m.ContextOptions).Returns(options);

            Assert.False(new SqlServerDataStoreSource(configurationMock.Object, new DbContextService<IDbContextOptions>(options)).IsAvailable);
        }
    }
}
