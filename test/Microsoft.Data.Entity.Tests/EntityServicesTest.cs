// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity
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
            Assert.True(services.Any(sd => sd.ServiceType == typeof(ChangeTrackerFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IModelSource)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {
            var serviceProvider = new ServiceProvider().Add(EntityServices.GetDefaultServices());

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);

            // TODO: Currently Singleton returns a new instance each time
            //Assert.Same(loggerFactory, serviceProvider.GetService<ILoggerFactory>());

            var identityGeneratorFactory = serviceProvider.GetService<IdentityGeneratorFactory>();
            Assert.NotNull(identityGeneratorFactory);
            // TODO: Currently Singleton returns a new instance each time
            //Assert.Same(identityGeneratorFactory, serviceProvider.GetService<IdentityGeneratorFactory>());

            var activeIdentityGenerators = serviceProvider.GetService<ActiveIdentityGenerators>();
            Assert.NotNull(activeIdentityGenerators);
            // TODO: Currently Singleton returns a new instance each time
            //Assert.Same(activeIdentityGenerators, serviceProvider.GetService<ActiveIdentityGenerators>());

            var changeTrackerFactory = serviceProvider.GetService<ChangeTrackerFactory>();
            Assert.NotNull(changeTrackerFactory);
            // TODO: Currently Scoped returns a new instance each time
            //Assert.Same(changeTrackerFactory, serviceProvider.GetService<ChangeTrackerFactory>());

            var scaoped = serviceProvider.GetService<IServiceProvider>();
            Assert.NotNull(scaoped);

            // TODO: Currently scoping not working
            //Assert.Same(loggerFactory, scaoped.GetService<ILoggerFactory>());
            //Assert.Same(identityGeneratorFactory, scaoped.GetService<IdentityGeneratorFactory>());
            //Assert.Same(activeIdentityGenerators, scaoped.GetService<ActiveIdentityGenerators>());
            Assert.NotSame(changeTrackerFactory, scaoped.GetService<ChangeTrackerFactory>());
        }

        [Fact]
        public void ActiveIdentityGenerators_is_configured_with_IdentityGeneratorFactory()
        {
            var serviceProvider = new ServiceProvider().Add(EntityServices.GetDefaultServices());

            var generators = serviceProvider.GetService<ActiveIdentityGenerators>();

            var property = new Property("Foo", typeof(Guid)) { ValueGenerationStrategy = ValueGenerationStrategy.Client };

            Assert.IsType<GuidIdentityGenerator>(generators.GetOrAdd(property));
        }
    }
}
