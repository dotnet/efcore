// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ServiceProviderCacheTest
    {
        [Fact]
        public void Returns_same_provider_for_same_type_of_configured_extensions()
        {
            var config1 = CreateOptions<FakeDbContextOptionsExtension1>();
            var config2 = CreateOptions<FakeDbContextOptionsExtension1>();

            var cache = new ServiceProviderCache();

            Assert.Same(cache.GetOrAdd(config1, true), cache.GetOrAdd(config2, true));
        }

        [Fact]
        public void Returns_different_provider_for_different_type_of_configured_extensions()
        {
            var config1 = CreateOptions<FakeDbContextOptionsExtension1>();
            var config2 = CreateOptions<FakeDbContextOptionsExtension2>();

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1, true), cache.GetOrAdd(config2, true));
        }

        [Fact]
        public void Returns_same_provider_for_same_type_of_configured_extensions_and_replaced_service_types()
        {
            var config1 = CreateOptions<CoreOptionsExtension>();
            config1 = config1.WithExtension(
                config1.FindExtension<CoreOptionsExtension>()
                    .WithReplacedService(typeof(object), typeof(Random)));

            var config2 = CreateOptions<CoreOptionsExtension>();
            config2 = config2.WithExtension(
                config2.FindExtension<CoreOptionsExtension>()
                    .WithReplacedService(typeof(object), typeof(Random)));

            var cache = new ServiceProviderCache();

            Assert.Same(cache.GetOrAdd(config1, true), cache.GetOrAdd(config2, true));
        }

        [Fact]
        public void Returns_different_provider_for_different_replaced_service_types()
        {
            var config1 = CreateOptions<CoreOptionsExtension>();
            config1 = config1.WithExtension(
                config1.FindExtension<CoreOptionsExtension>()
                    .WithReplacedService(typeof(object), typeof(Random)));

            var config2 = CreateOptions<CoreOptionsExtension>();
            config2 = config2.WithExtension(
                config2.FindExtension<CoreOptionsExtension>()
                    .WithReplacedService(typeof(object), typeof(string)));

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1, true), cache.GetOrAdd(config2, true));
        }

        private static DbContextOptions CreateOptions<TExtension>()
            where TExtension : class, IDbContextOptionsExtension, new()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new TExtension());

            return optionsBuilder.Options;
        }

        private class FakeDbContextOptionsExtension1 : IDbContextOptionsExtension
        {
            public virtual bool ApplyServices(IServiceCollection services) => false;

            public virtual long GetServiceProviderHashCode() => 0;

            public virtual void Validate(IDbContextOptions options)
            {
            }

            public virtual string LogFragment => "";
        }

        private class FakeDbContextOptionsExtension2 : IDbContextOptionsExtension
        {
            public virtual bool ApplyServices(IServiceCollection services) => false;

            public virtual long GetServiceProviderHashCode() => 0;

            public virtual void Validate(IDbContextOptions options)
            {
            }

            public virtual string LogFragment => "";
        }
    }
}
