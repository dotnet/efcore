// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Design;

public class DbContextActivatorTest
{
    [ConditionalFact]
    public void CreateInstance_works()
    {
        var result = DbContextActivator.CreateInstance(typeof(TestContext));

        Assert.IsType<TestContext>(result);
    }

    [ConditionalFact]
    public void CreateInstance_with_arguments_works()
    {
        var result = DbContextActivator.CreateInstance(
            typeof(TestContext),
            null,
            null,
            new[] { "A", "B" });

        Assert.IsType<TestContext>(result);
    }

    private class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(nameof(DbContextActivatorTest));
    }
}
