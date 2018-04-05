// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class EntityFrameworkServiceCollectionExtensionsTestBase
    {
        private readonly TestHelpers _testHelpers;

        protected EntityFrameworkServiceCollectionExtensionsTestBase(TestHelpers testHelpers)
            => _testHelpers = testHelpers;

        [Fact]
        public void Calling_AddEntityFramework_explicitly_does_not_change_services()
        {
            var services1 = AddServices(new ServiceCollection());
            var services2 = AddServices(new ServiceCollection());

            new EntityFrameworkServicesBuilder(services2).TryAddCoreServices();

            AssertServicesSame(services1, services2);
        }

        [Fact]
        public virtual void Repeated_calls_to_add_do_not_modify_collection()
        {
            AssertServicesSame(
                AddServices(new ServiceCollection()),
                AddServices(AddServices(new ServiceCollection())));
        }

        [Fact]
        public virtual void Required_services_are_registered_with_expected_lifetimes()
        {
            LifetimeTest(EntityFrameworkServicesBuilder.CoreServices);
        }

        protected virtual void LifetimeTest(
            params IDictionary<Type, EntityFrameworkServicesBuilder.ServiceCharacteristics>[] serviceDefinitions)
        {
            var services = AddServices(new ServiceCollection());

            foreach (var coreService in serviceDefinitions.SelectMany(e => e))
            {
                var registered = services.Where(s => s.ServiceType == coreService.Key).ToList();

                if (coreService.Value.MultipleRegistrations)
                {
                    Assert.All(registered, s => Assert.Equal(coreService.Value.Lifetime, s.Lifetime));
                }
                else
                {
                    Assert.Equal(1, registered.Count);
                    Assert.Equal(coreService.Value.Lifetime, registered[0].Lifetime);
                }
            }
        }

        protected virtual void AssertServicesSame(IServiceCollection services1, IServiceCollection services2)
        {
            var sortedServices1 = services1
                .OrderBy(s => s.ServiceType.GetHashCode())
                .ToList();

            var sortedServices2 = services2
                .OrderBy(s => s.ServiceType.GetHashCode())
                .ToList();

            Assert.Equal(sortedServices1.Count, sortedServices2.Count);

            for (var i = 0; i < sortedServices1.Count; i++)
            {
                Assert.Equal(sortedServices1[i].ServiceType, sortedServices2[i].ServiceType);
                Assert.Equal(sortedServices1[i].ImplementationType, sortedServices2[i].ImplementationType);
                Assert.Equal(sortedServices1[i].Lifetime, sortedServices2[i].Lifetime);
            }
        }

        private IServiceCollection AddServices(IServiceCollection serviceCollection)
            => _testHelpers.AddProviderServices(serviceCollection);
    }
}
