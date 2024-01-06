// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

// The tests interact with global state and should never be run in parallel
[Collection("DesignTimeFlagTest")]
public class DesignTimeFlagTest
{
    [ConditionalFact]
    public void Operations_have_design_time_flag_set()
    {
        EF.IsDesignTime = false;

        var handler = new OperationResultHandler();

        new MockOperation<string>(
            handler, () =>
            {
                Assert.True(EF.IsDesignTime);
                return "Twilight Sparkle";
            });

        Assert.False(EF.IsDesignTime);
        Assert.Equal("Twilight Sparkle", handler.Result);
    }

    [ConditionalFact]
    public void CreateInstance_sets_design_time_flag()
    {
        EF.IsDesignTime = false;

        Assert.IsType<TestContext>(DbContextActivator.CreateInstance(typeof(TestContext)));

        Assert.True(EF.IsDesignTime);
    }

    [ConditionalFact]
    public void CreateInstance_with_arguments_sets_design_time_flag()
    {
        EF.IsDesignTime = false;

        Assert.IsType<TestContext>(
            DbContextActivator.CreateInstance(
                typeof(TestContext),
                null,
                null,
                ["A", "B"]));

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

    private class MockOperation<T> : OperationExecutor.OperationBase
    {
        public MockOperation(IOperationResultHandler resultHandler, Func<T> action)
            : base(resultHandler)
        {
            Execute(action);
        }
    }
}

[CollectionDefinition("DesignTimeFlagTest", DisableParallelization = true)]
public class DesignTimeFlagTestCollection;
