// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public class TestCosmosExecutionStrategy : CosmosExecutionStrategy
{
    protected static new readonly int DefaultMaxRetryCount = 10;

    protected static new readonly TimeSpan DefaultMaxDelay = TimeSpan.FromSeconds(60);

    public TestCosmosExecutionStrategy()
        : base(
            new DbContext(
                new DbContextOptionsBuilder()
                    .EnableServiceProviderCaching(false)
                    .UseCosmos(
                        TestEnvironment.DefaultConnection,
                        TestEnvironment.AuthToken,
                        "NonExistent").Options),
            DefaultMaxRetryCount, DefaultMaxDelay)
    {
    }

    public TestCosmosExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay)
    {
    }
}
