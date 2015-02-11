// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.DependencyInjection;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ServiceProviderCacheTest
    {
        [Fact]
        public void Returns_same_provider_for_same_set_of_configured_services()
        {
            var serviceInstance = new FakeService4();

            var config1 = CreateOptions(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3>();
                    b.ServiceCollection.AddInstance(serviceInstance);
                });

            var config2 = CreateOptions(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3>();
                    b.ServiceCollection.AddInstance(serviceInstance);
                });

            var cache = new ServiceProviderCache();

            Assert.Same(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        [Fact]
        public void Returns_different_provider_for_configured_services_differing_by_instance()
        {
            var config1 = CreateOptions(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3>();
                    b.ServiceCollection.AddInstance(new FakeService4());
                });

            var config2 = CreateOptions(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3>();
                    b.ServiceCollection.AddInstance(new FakeService4());
                });

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        [Fact]
        public void Returns_different_provider_for_configured_services_differing_by_type()
        {
            var serviceInstance = new FakeService4();

            var config1 = CreateOptions(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3>();
                    b.ServiceCollection.AddInstance(serviceInstance);
                });

            var config2 = CreateOptions(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService2>();
                    b.ServiceCollection.AddSingleton<FakeService3>();
                    b.ServiceCollection.AddInstance(serviceInstance);
                });

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        [Fact]
        public void Returns_different_provider_for_configured_services_differing_by_lifetime()
        {
            var serviceInstance = new FakeService4();

            var config1 = CreateOptions(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3>();
                    b.ServiceCollection.AddInstance(serviceInstance);
                });

            var config2 = CreateOptions(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddScoped<FakeService3>();
                    b.ServiceCollection.AddInstance(serviceInstance);
                });

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        [Fact]
        public void Returns_different_provider_for_configured_services_differing_by_services_provided()
        {
            var serviceInstance = new FakeService4();

            var config1 = CreateOptions(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3>();
                    b.ServiceCollection.AddInstance(serviceInstance);
                });

            var config2 = CreateOptions(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceB, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3>();
                    b.ServiceCollection.AddInstance(serviceInstance);
                });

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        private static DbContextOptions CreateOptions(Action<EntityFrameworkServicesBuilder> builderAction)
        {
            IDbContextOptions options = new DbContextOptions();
            options.AddOrUpdateExtension<FakeDbContextOptionsExtension>(e => e.BuilderActions.Add(builderAction));
            return (DbContextOptions)options;
        }

        private class FakeDbContextOptionsExtension : DbContextOptionsExtension
        {
            private readonly List<Action<EntityFrameworkServicesBuilder>> _builderActions = new List<Action<EntityFrameworkServicesBuilder>>();

            public List<Action<EntityFrameworkServicesBuilder>> BuilderActions
            {
                get { return _builderActions; }
            }

            protected internal override void ApplyServices(EntityFrameworkServicesBuilder builder)
            {
                foreach (var builderAction in _builderActions)
                {
                    builderAction(builder);
                }
            }
        }

        private interface IFakeServiceA
        {
        }

        private interface IFakeServiceB
        {
        }

        private class FakeService1 : IFakeServiceA, IFakeServiceB
        {
        }

        private class FakeService2 : IFakeServiceA, IFakeServiceB
        {
        }

        private class FakeService3
        {
        }

        private class FakeService4
        {
        }
    }
}
