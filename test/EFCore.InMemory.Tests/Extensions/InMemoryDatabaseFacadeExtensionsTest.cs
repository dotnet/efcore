// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class InMemoryDatabaseFacadeExtensionsTest
{
    [ConditionalFact]
    public void IsInMemory_when_using_in_memory()
    {
        using var context = new ProviderContext();
        Assert.True(context.Database.IsInMemory());
    }

    private class ProviderContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase("Maltesers");
    }
}
