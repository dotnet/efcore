// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity.Identity;
using Xunit;

namespace Microsoft.Data.InMemory.Tests
{
    public class InMemoryServicesTest
    {
        [Fact]
        public void CanGetDefaultServices()
        {
            var services = InMemoryServices.GetDefaultServices().ToList();

            Assert.True(services.Any(sd => sd.ServiceType == typeof(IdentityGeneratorFactory)));
        }

        [Fact]
        public void ServicesWireUpCorrectly()
        {
            var serviceProvider = InMemoryServices.GetDefaultServices().BuildServiceProvider();

            Assert.NotNull(serviceProvider.GetService<IdentityGeneratorFactory>());
        }
    }
}
