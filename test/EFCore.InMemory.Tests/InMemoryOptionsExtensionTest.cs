// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryOptionsExtensionTest
    {
        private static readonly MethodInfo _applyServices
            = typeof(InMemoryOptionsExtension).GetTypeInfo().DeclaredMethods.Single(m => m.Name == "ApplyServices");

        [ConditionalFact]
        public void Adds_in_memory_services()
        {
            var services = new ServiceCollection();

            _applyServices.Invoke(new InMemoryOptionsExtension(), new object[] { services });

            Assert.Contains(services, sd => sd.ServiceType == typeof(IInMemoryDatabase));
        }
    }
}
