// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryDatabaseProviderTest
    {
        [ConditionalFact]
        public void Returns_appropriate_name()
        {
            Assert.Equal(
                typeof(InMemoryDatabase).Assembly.GetName().Name,
                new DatabaseProvider<InMemoryOptionsExtension>(new DatabaseProviderDependencies()).Name);
        }

        [ConditionalFact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            Assert.True(
                new DatabaseProvider<InMemoryOptionsExtension>(new DatabaseProviderDependencies()).IsConfigured(optionsBuilder.Options));
        }

        [ConditionalFact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            Assert.False(
                new DatabaseProvider<InMemoryOptionsExtension>(new DatabaseProviderDependencies()).IsConfigured(optionsBuilder.Options));
        }
    }
}
