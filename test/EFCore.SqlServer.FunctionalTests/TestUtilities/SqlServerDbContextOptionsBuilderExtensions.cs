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

    public static AzureSqlDbContextOptionsBuilder ApplyConfiguration(this AzureSqlDbContextOptionsBuilder optionsBuilder)
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

    public static AzureSynapseDbContextOptionsBuilder ApplyConfiguration(this AzureSynapseDbContextOptionsBuilder optionsBuilder)
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

    /// <summary>
    ///     Configures the SQL Server or Azure SQL provider with the specified compatibility level.
    ///     This method automatically chooses between UseAzureSql and UseSqlServer based on the current test environment.
    /// </summary>
    /// <param name="optionsBuilder">The options builder to configure.</param>
    /// <param name="compatibilityLevel">The compatibility level to use.</param>
    /// <returns>The configured options builder.</returns>
    public static DbContextOptionsBuilder UseSqlServerCompatibilityLevel(this DbContextOptionsBuilder optionsBuilder, int compatibilityLevel)
        => TestEnvironment.IsAzureSql
            ? optionsBuilder.UseAzureSql(b => b.UseCompatibilityLevel(compatibilityLevel))
            : optionsBuilder.UseSqlServer(b => b.UseCompatibilityLevel(compatibilityLevel));
}
