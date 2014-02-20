// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Identity;
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
            Assert.True(services.Any(sd => sd.ServiceType == typeof(IIdentityGenerator<Guid>)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {
            var serviceProvider = new ServiceProvider().Add(EntityServices.GetDefaultServices());

            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
            Assert.NotNull(serviceProvider.GetService<IIdentityGenerator<Guid>>());
        }
    }
}
