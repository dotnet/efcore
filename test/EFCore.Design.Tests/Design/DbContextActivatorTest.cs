// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

public class DbContextActivatorTest
{
    [ConditionalFact]
    public void CreateInstance_works()
    {
        Assert.IsType<TestContext>(DbContextActivator.CreateInstance(typeof(TestContext)));
    }

    [ConditionalFact]
    public void CreateInstance_with_arguments_works()
    {
        Assert.IsType<TestContext>(DbContextActivator.CreateInstance(
            typeof(TestContext),
            null,
            null,
            new[] { "A", "B" }));
    }

    private class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(nameof(DbContextActivatorTest));
    }
}
