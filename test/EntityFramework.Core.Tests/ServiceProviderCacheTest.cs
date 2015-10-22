// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ServiceProviderCacheTest
    {
        [Fact]
        public void Returns_same_provider_for_same_tyoe_of_configured_extensions()
        {
            var config1 = CreateOptions(new FakeDbContextOptionsExtension1());
            var config2 = CreateOptions(new FakeDbContextOptionsExtension1());

            var cache = new ServiceProviderCache();

            Assert.Same(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        [Fact]
        public void Returns_different_provider_for_different_tyoe_of_configured_extensions()
        {
            var config1 = CreateOptions(new FakeDbContextOptionsExtension1());
            var config2 = CreateOptions(new FakeDbContextOptionsExtension2());

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        private static DbContextOptions CreateOptions(IDbContextOptionsExtension extension)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder.Options;
        }

        private class FakeDbContextOptionsExtension1 : IDbContextOptionsExtension
        {
            public virtual void ApplyServices(EntityFrameworkServicesBuilder builder)
            {
            }
        }

        private class FakeDbContextOptionsExtension2 : IDbContextOptionsExtension
        {
            public virtual void ApplyServices(EntityFrameworkServicesBuilder builder)
            {
            }
        }
    }
}
