// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Data.Entity.Infrastructure;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ServiceProviderCacheTest
    {
        [Fact]
        public void Returns_same_provider_for_same_set_of_configured_services()
        {
            var serviceInstance = new FakeService4();

            var config1 = BuildConfiguration(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3, FakeService3>();
                    b.ServiceCollection.AddInstance<FakeService4>(serviceInstance);
                });

            var config2 = BuildConfiguration(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3, FakeService3>();
                    b.ServiceCollection.AddInstance<FakeService4>(serviceInstance);
                });

            var cache = new ServiceProviderCache();

            Assert.Same(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        [Fact]
        public void Returns_different_provider_for_configured_services_differing_by_instance()
        {
            var config1 = BuildConfiguration(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3, FakeService3>();
                    b.ServiceCollection.AddInstance<FakeService4>(new FakeService4());
                });

            var config2 = BuildConfiguration(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3, FakeService3>();
                    b.ServiceCollection.AddInstance<FakeService4>(new FakeService4());
                });

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        [Fact]
        public void Returns_different_provider_for_configured_services_differing_by_type()
        {
            var serviceInstance = new FakeService4();

            var config1 = BuildConfiguration(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3, FakeService3>();
                    b.ServiceCollection.AddInstance<FakeService4>(serviceInstance);
                });

            var config2 = BuildConfiguration(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService2>();
                    b.ServiceCollection.AddSingleton<FakeService3, FakeService3>();
                    b.ServiceCollection.AddInstance<FakeService4>(serviceInstance);
                });

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        [Fact]
        public void Returns_different_provider_for_configured_services_differing_by_lifetime()
        {
            var serviceInstance = new FakeService4();

            var config1 = BuildConfiguration(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3, FakeService3>();
                    b.ServiceCollection.AddInstance<FakeService4>(serviceInstance);
                });

            var config2 = BuildConfiguration(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddScoped<FakeService3, FakeService3>();
                    b.ServiceCollection.AddInstance<FakeService4>(serviceInstance);
                });

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        [Fact]
        public void Returns_different_provider_for_configured_services_differing_by_services_provided()
        {
            var serviceInstance = new FakeService4();

            var config1 = BuildConfiguration(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceA, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3, FakeService3>();
                    b.ServiceCollection.AddInstance<FakeService4>(serviceInstance);
                });

            var config2 = BuildConfiguration(b =>
                {
                    b.ServiceCollection.AddSingleton<IFakeServiceB, FakeService1>();
                    b.ServiceCollection.AddSingleton<FakeService3, FakeService3>();
                    b.ServiceCollection.AddInstance<FakeService4>(serviceInstance);
                });

            var cache = new ServiceProviderCache();

            Assert.NotSame(cache.GetOrAdd(config1), cache.GetOrAdd(config2));
        }

        private static ImmutableDbContextOptions BuildConfiguration(Action<EntityServicesBuilder> builderAction)
        {
            var config = (IDbContextOptionsConstruction)new ImmutableDbContextOptions();
            config.AddOrUpdateExtension<FakeEntityConfigurationExtension>(e => e.BuilderActions.Add(builderAction));
            return (ImmutableDbContextOptions)config;
        }

        private class FakeEntityConfigurationExtension : EntityConfigurationExtension
        {
            private readonly List<Action<EntityServicesBuilder>> _builderActions = new List<Action<EntityServicesBuilder>>();

            public List<Action<EntityServicesBuilder>> BuilderActions
            {
                get { return _builderActions; }
            }

            protected internal override void ApplyServices(EntityServicesBuilder builder)
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
