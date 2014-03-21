// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityServicesTest
    {
        [Fact]
        public void Can_get_default_services()
        {
            var services = EntityServices.GetDefaultServices().ToList();

            Assert.True(services.Any(sd => sd.ServiceType == typeof(ILoggerFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IdentityGeneratorFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ActiveIdentityGenerators)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(StateManagerFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IModelSource)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(EntitySetFinder)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(EntitySetInitializer)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IEntityStateListener)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(StateEntryFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(EntityKeyFactorySource)));
        }

        [Fact]
        public void ActiveIdentityGenerators_is_configured_with_IdentityGeneratorFactory()
        {
            var serviceProvider = EntityServices.GetDefaultServices().BuildServiceProvider();

            var generators = serviceProvider.GetService<ActiveIdentityGenerators>();

            var property = new Property("Foo", typeof(Guid), shadowProperty: false) { ValueGenerationStrategy = ValueGenerationStrategy.Client };

            Assert.IsType<GuidIdentityGenerator>(generators.GetOrAdd(property));
        }
    }
}
