// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage;

public class InMemoryDatabaseProviderTest
{
    [ConditionalFact]
    public void Returns_appropriate_name()
        => Assert.Equal(
            typeof(InMemoryDatabase).Assembly.GetName().Name,
            new DatabaseProvider<InMemoryOptionsExtension>(new DatabaseProviderDependencies()).Name);

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
