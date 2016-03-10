// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Storage
{
    public class DatabaseProviderSelectorTest
    {
        [Fact]
        public void Selects_single_configured_provider()
        {
            var provider = CreateSource("Database1", configured: true, available: false);
            var serviceProvider = Mock.Of<IServiceProvider>();

            var selector = new DatabaseProviderSelector(
                serviceProvider,
                Mock.Of<IDbContextOptions>(),
                new[] { provider });

            Assert.Same(provider.GetProviderServices(serviceProvider), selector.SelectServices());
        }

        [Fact]
        public void Throws_if_multiple_providers_configured()
        {
            var provider1 = CreateSource("Database1", configured: true, available: false);
            var provider2 = CreateSource("Database2", configured: true, available: false);
            var provider3 = CreateSource("Database3", configured: false, available: true);
            var provider4 = CreateSource("Database4", configured: true, available: false);

            var selector = new DatabaseProviderSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                new[] { provider1, provider2, provider3, provider4 });

            Assert.Equal(CoreStrings.MultipleProvidersConfigured("'Database1' 'Database2' 'Database4' "),
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectServices()).Message);
        }

        [Fact]
        public void Throws_if_no_provider_services_have_been_registered_using_external_service_provider()
        {
            var selector = new DatabaseProviderSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                null);

            Assert.Equal(CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectServices()).Message);
        }

        [Fact]
        public void Throws_if_no_provider_services_have_been_registered_using_implicit_service_provider()
        {
            var selector = new DatabaseProviderSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                null);

            Assert.Equal(CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectServices()).Message);
        }

        [Fact]
        public void Throws_if_multiple_provider_services_are_registered_but_none_are_configured()
        {
            var provider1 = CreateSource("Database1", configured: false, available: true);
            var provider2 = CreateSource("Database2", configured: false, available: false);
            var provider3 = CreateSource("Database3", configured: false, available: false);

            var selector = new DatabaseProviderSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                new[] { provider1, provider2, provider3 });

            Assert.Equal(CoreStrings.MultipleProvidersAvailable("'Database1' 'Database2' 'Database3' "),
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectServices()).Message);
        }

        [Fact]
        public void Throws_if_one_provider_service_is_registered_but_not_configured_and_cannot_be_used_without_configuration()
        {
            var provider = CreateSource("Database1", configured: false, available: false);

            var selector = new DatabaseProviderSelector(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IDbContextOptions>(),
                new[] { provider });

            Assert.Equal(CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(
                    () => selector.SelectServices()).Message);
        }

        private static IDatabaseProvider CreateSource(string name, bool configured, bool available)
        {
            var servicesMock = new Mock<IDatabaseProviderServices>();
            servicesMock.Setup(m => m.InvariantName).Returns(name);

            var providerMock = new Mock<IDatabaseProvider>();
            providerMock.Setup(m => m.IsConfigured(It.IsAny<IDbContextOptions>())).Returns(configured);
            providerMock.Setup(m => m.GetProviderServices(It.IsAny<IServiceProvider>())).Returns(servicesMock.Object);

            return providerMock.Object;
        }
    }
}
