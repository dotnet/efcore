// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.InMemory.Tests
{
    public class InMemoryEntityServicesBuilderExtensionsTest
    {
        [Fact]
        public void Can_get_default_services()
        {
            var services = new ServiceCollection().AddEntityFramework(s => s.AddInMemoryStore());

            Assert.True(services.Any(sd => sd.ServiceType == typeof(IdentityGeneratorFactory)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryDataStore)));
            Assert.True(services.Any(sd => sd.ServiceType == typeof(DataStoreSource)));
        }

        [Fact]
        public void Services_wire_up_correctly()
        {

            var serviceProvider = new ServiceCollection().AddEntityFramework(s => s.AddInMemoryStore()).BuildServiceProvider();

            using (var context = new EntityContext(new EntityConfigurationBuilder(serviceProvider).BuildConfiguration()))
            {
                var scopedProvider = context.Configuration.Services.ServiceProvider;

                Assert.NotNull(scopedProvider.GetService<IdentityGeneratorFactory>());
                Assert.NotNull(scopedProvider.GetService<InMemoryDataStore>());
                Assert.NotNull(scopedProvider.GetService<DataStoreSource>());
            }
        }
    }
}
