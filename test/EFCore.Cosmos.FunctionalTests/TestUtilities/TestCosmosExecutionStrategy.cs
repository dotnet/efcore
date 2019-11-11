// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.TestUtilities
{
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
}
