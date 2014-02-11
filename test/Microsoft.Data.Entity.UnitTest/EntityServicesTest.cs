// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Services;
using Xunit;

namespace Microsoft.Data.Entity
{
    public class EntityServicesTest
    {
        [Fact]
        public void Can_create_default_provider()
        {
            var serviceProvider = EntityServices.CreateDefaultProvider();

            Assert.NotNull(serviceProvider);
            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
        }

        [Fact]
        public void Can_surface_default_services_to_consumers()
        {
            var services = new Dictionary<Type, Type>();

            EntityServices.AddDefaultServices(services.Add);

            Assert.Equal(typeof(ConsoleLoggerFactory), services[typeof(ILoggerFactory)]);
        }
    }
}
