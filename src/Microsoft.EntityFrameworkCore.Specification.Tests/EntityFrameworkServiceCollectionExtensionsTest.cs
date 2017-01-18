// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class EntityFrameworkServiceCollectionExtensionsTest
    {
        private readonly TestHelpers _testHelpers;

        protected EntityFrameworkServiceCollectionExtensionsTest(TestHelpers testHelpers)
        {
            _testHelpers = testHelpers;
        }

        [Fact]
        public virtual void Repeated_calls_to_add_do_not_modify_collection()
        {
            var expectedCollection = AddServices(new ServiceCollection());

            var actualCollection = AddServices(AddServices(new ServiceCollection()));

            AssertServicesSame(expectedCollection, actualCollection);
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
