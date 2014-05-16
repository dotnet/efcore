// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryEntityServicesBuilderExtensionsTest
    {
        [Fact]
        public void Can_get_default_services()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore();

            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryDataStore)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DataStoreSource)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore();
            var serviceProvider = services.BuildServiceProvider();

            using (var context = new DbContext(serviceProvider, new DbContextOptions().BuildConfiguration()))
            {
                var scopedProvider = context.Configuration.Services.ServiceProvider;

                Assert.NotNull(scopedProvider.GetService<InMemoryDataStore>());
                Assert.NotNull(scopedProvider.GetService<DataStoreSource>());
            }
        }
    }
}
