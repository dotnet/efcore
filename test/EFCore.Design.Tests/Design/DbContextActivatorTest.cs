// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

public class DbContextActivatorTest
{
    [ConditionalFact]
    public void CreateInstance_works()
    {
        EF.IsDesignTime = false;

        var result = DbContextActivator.CreateInstance(typeof(TestContext));

        Assert.IsType<TestContext>(result);

        Assert.True(EF.IsDesignTime);
    }

    [ConditionalFact]
    public void CreateInstance_with_arguments_works()
    {
        EF.IsDesignTime = false;

        var result = DbContextActivator.CreateInstance(
            typeof(TestContext),
            null,
            null,
            new[] { "A", "B" });

        Assert.IsType<TestContext>(result);

        Assert.True(EF.IsDesignTime);
    }

    private class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            Assert.True(EF.IsDesignTime);

            options
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(nameof(DbContextActivatorTest));
        }
    }
}
