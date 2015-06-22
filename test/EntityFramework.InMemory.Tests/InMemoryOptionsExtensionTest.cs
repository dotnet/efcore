// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryOptionsExtensionTest
    {
        private static readonly MethodInfo _applyServices
            = typeof(InMemoryOptionsExtension).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "ApplyServices");

        [Fact]
        public void Adds_in_memory_services()
        {
            var services = new ServiceCollection();
            var builder = new EntityFrameworkServicesBuilder(services);

            _applyServices.Invoke(new InMemoryOptionsExtension(), new object[] { builder });

            Assert.True(services.Any(sd => sd.ServiceType == typeof(IInMemoryDatabase)));
        }
    }
}
