// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDatabaseProviderTest
    {
        [Fact]
        public void Returns_appropriate_name()
        {
            Assert.Equal("In-Memory Database", new InMemoryDatabaseProvider().Name);
        }

        [Fact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            Assert.True(new InMemoryDatabaseProvider().IsConfigured(optionsBuilder.Options));
        }

        [Fact]
        public void Can_be_auto_configured()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            var provider = new InMemoryDatabaseProvider();
            provider.AutoConfigure(optionsBuilder);

            Assert.True(provider.IsConfigured(optionsBuilder.Options));
        }

        [Fact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(new InMemoryDatabaseProvider().IsConfigured(optionsBuilder.Options));
        }
    }
}
