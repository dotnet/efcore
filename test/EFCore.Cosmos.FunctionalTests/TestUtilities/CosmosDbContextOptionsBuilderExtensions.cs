// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class CosmosDbContextOptionsBuilderExtensions
    {
        public static CosmosDbContextOptionsBuilder ApplyConfiguration(this CosmosDbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ExecutionStrategy(d => new TestCosmosExecutionStrategy(d));

            return optionsBuilder;
        }
    }
}
