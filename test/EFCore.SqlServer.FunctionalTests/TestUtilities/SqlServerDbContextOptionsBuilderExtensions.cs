// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class SqlServerDbContextOptionsBuilderExtensions
{
    public static SqlServerDbContextOptionsBuilder ApplyConfiguration(this SqlServerDbContextOptionsBuilder optionsBuilder)
    {
        var maxBatch = TestEnvironment.GetInt(nameof(SqlServerDbContextOptionsBuilder.MaxBatchSize));
        if (maxBatch.HasValue)
        {
            optionsBuilder.MaxBatchSize(maxBatch.Value);
        }

        optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);

        optionsBuilder.ExecutionStrategy(d => new TestSqlServerRetryingExecutionStrategy(d));

        optionsBuilder.CommandTimeout(SqlServerTestStore.CommandTimeout);

        return optionsBuilder;
    }
}
