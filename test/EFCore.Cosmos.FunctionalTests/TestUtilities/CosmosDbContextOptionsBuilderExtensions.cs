// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class CosmosDbContextOptionsBuilderExtensions
    {
        public static CosmosDbContextOptionsBuilder ApplyConfiguration(this CosmosDbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ExecutionStrategy(d => new TestCosmosExecutionStrategy(d));
            optionsBuilder.RequestTimeout(TimeSpan.FromMinutes(1));

            return optionsBuilder;
        }
    }
}
