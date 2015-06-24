// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDatabaseProviderTest
    {
        [Fact]
        public void Returns_appropriate_name()
        {
            Assert.Equal(
                typeof(InMemoryDatabase).GetTypeInfo().Assembly.GetName().Name,
                new InMemoryDatabaseProviderServices(InMemoryTestHelpers.Instance.CreateServiceProvider()).InvariantName);
        }

        [Fact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            Assert.True(new DatabaseProvider<InMemoryDatabaseProviderServices, InMemoryOptionsExtension>().IsConfigured(optionsBuilder.Options));
        }

        [Fact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(new DatabaseProvider<InMemoryDatabaseProviderServices, InMemoryOptionsExtension>().IsConfigured(optionsBuilder.Options));
        }
    }
}
