// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
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
            using (var context = TestHelpers.CreateContext())
            {
                var scopedProvider = ((IDbContextServices)context).ScopedServiceProvider;

                Assert.NotNull(scopedProvider.GetRequiredService<InMemoryDataStore>());
                Assert.NotNull(scopedProvider.GetRequiredService<DataStoreSource>());
            }
        }
    }
}
