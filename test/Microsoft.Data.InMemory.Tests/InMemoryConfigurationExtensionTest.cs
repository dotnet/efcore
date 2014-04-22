// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.AspNet.DependencyInjection;
using Xunit;

namespace Microsoft.Data.InMemory.Tests
{
    public class InMemoryConfigurationExtensionTest
    {
        private static readonly MethodInfo _applyServices
            = typeof(InMemoryConfigurationExtension).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "ApplyServices");

        [Fact]
        public void Adds_in_memory_services()
        {
            var services = new ServiceCollection();
            var builder = new EntityServicesBuilder(services);

            _applyServices.Invoke(new InMemoryConfigurationExtension(), new object[] { builder });

            Assert.True(services.Any(sd => sd.ServiceType == typeof(InMemoryDataStore)));
        }
    }
}
