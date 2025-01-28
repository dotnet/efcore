// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class SqliteDatabaseFacadeExtensionsTest
{
    [ConditionalFact]
    public void IsSqlite_when_using_SQLite()
    {
        using var context = new ProviderContext(
            new DbContextOptionsBuilder()
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .BuildServiceProvider(validateScopes: true))
                .UseSqlite("Database=Maltesers").Options);
        Assert.True(context.Database.IsSqlite());
    }

    [ConditionalFact]
    public void Not_IsSqlite_when_using_different_provider()
    {
        using var context = new ProviderContext(
            new DbContextOptionsBuilder()
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase("Maltesers").Options);
        Assert.False(context.Database.IsSqlite());
    }

    private class ProviderContext(DbContextOptions options) : DbContext(options);
}
