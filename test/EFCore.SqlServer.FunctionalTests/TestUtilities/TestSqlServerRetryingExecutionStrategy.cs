// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestSqlServerRetryingExecutionStrategy : SqlServerRetryingExecutionStrategy
{
    private const bool ErrorNumberDebugMode = false;

    private static readonly int[] _additionalErrorNumbers =
    [
        -1, // Physical connection is not usable
        -2, // Timeout
        42008, // Mirroring (Only when a database is deleted and another one is created in fast succession)
        42019 // CREATE DATABASE operation failed
    ];

    public TestSqlServerRetryingExecutionStrategy()
        : base(
            new DbContext(
                new DbContextOptionsBuilder()
                    .EnableServiceProviderCaching(false)
                    .UseSqlServer(TestEnvironment.DefaultConnection).Options),
            DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
    {
    }

    public TestSqlServerRetryingExecutionStrategy(DbContext context)
        : base(context, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
    {
    }

    public TestSqlServerRetryingExecutionStrategy(DbContext context, TimeSpan maxDelay)
        : base(context, DefaultMaxRetryCount, maxDelay, _additionalErrorNumbers)
    {
    }

    public TestSqlServerRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
    {
    }

    protected override bool ShouldRetryOn(Exception exception)
    {
        if (base.ShouldRetryOn(exception))
        {
            return true;
        }

#pragma warning disable CS0162 // Unreachable code detected
        if (ErrorNumberDebugMode
            && exception is SqlException sqlException)
        {
            var message = "Didn't retry on";
            foreach (SqlError err in sqlException.Errors)
            {
                message += " " + err.Number;
            }

            message += Environment.NewLine;
            throw new InvalidOperationException(message + exception, exception);
        }
#pragma warning restore CS0162 // Unreachable code detected

        return exception is InvalidOperationException { Message: "Internal .Net Framework Data Provider error 6." };
    }

    public new virtual TimeSpan? GetNextDelay(Exception lastException)
    {
        ExceptionsEncountered.Add(lastException);
        return base.GetNextDelay(lastException);
    }
}
