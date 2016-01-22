// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static SqlServerDbContextOptionsBuilder ApplyConfiguration(this SqlServerDbContextOptionsBuilder optionsBuilder)
        {
            var maxBatch = TestEnvironment.GetInt(nameof(SqlServerDbContextOptionsBuilder.MaxBatchSize));

            if (maxBatch.HasValue)
            {
                optionsBuilder.MaxBatchSize(maxBatch.Value);
            }

            var offsetSupport = TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsOffset)) ?? true;

            if (!offsetSupport)
            {
                optionsBuilder.UseRowNumberForPaging();
            }

            return optionsBuilder;
        }
    }
}
