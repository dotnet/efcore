// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     An <see cref="IExecutionStrategy" /> implementation for retrying failed executions on SQL Server.
/// </summary>
/// <remarks>
///     <para>
///         This strategy is specifically tailored to SQL Server (including Azure SQL). It is pre-configured with
///         error numbers for transient errors that can be retried. Additional error numbers to retry on can also be supplied.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class SqlServerRetryingExecutionStrategy : ExecutionStrategy
{
    private readonly HashSet<int>? _additionalErrorNumbers;

    /// <summary>
    ///     The default minimum time delay between retries for throttling errors.
    /// </summary>
    protected static readonly TimeSpan DefaultMinDelayThrottling = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     Default values of 6 for the maximum retry count and 30 seconds for the maximum default delay are used.
    /// </remarks>
    /// <param name="context">The context on which the operations will be invoked.</param>
    public SqlServerRetryingExecutionStrategy(
        DbContext context)
        : this(context, DefaultMaxRetryCount)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     Default values of 6 for the maximum retry count and 30 seconds for the maximum default delay are used.
    /// </remarks>
    /// <param name="dependencies">Parameter object containing service dependencies.</param>
    public SqlServerRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies)
        : this(dependencies, DefaultMaxRetryCount)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     A default value 30 seconds for the maximum default delay is used.
    /// </remarks>
    /// <param name="context">The context on which the operations will be invoked.</param>
    /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
    public SqlServerRetryingExecutionStrategy(
        DbContext context,
        int maxRetryCount)
        : this(context, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     A default value 30 seconds for the maximum default delay is used.
    /// </remarks>
    /// <param name="dependencies">Parameter object containing service dependencies.</param>
    /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
    public SqlServerRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        int maxRetryCount)
        : this(dependencies, maxRetryCount, DefaultMaxDelay, errorNumbersToAdd: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     Default values of 6 for the maximum retry count and 30 seconds for the maximum default delay are used.
    /// </remarks>
    /// <param name="dependencies">Parameter object containing service dependencies.</param>
    /// <param name="errorNumbersToAdd">Additional SQL error numbers that should be considered transient.</param>
    public SqlServerRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        IEnumerable<int> errorNumbersToAdd)
        : this(dependencies, DefaultMaxRetryCount, DefaultMaxDelay, errorNumbersToAdd)
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="context">The context on which the operations will be invoked.</param>
    /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
    /// <param name="maxRetryDelay">The maximum delay between retries.</param>
    /// <param name="errorNumbersToAdd">Additional SQL error numbers that should be considered transient.</param>
    public SqlServerRetryingExecutionStrategy(
        DbContext context,
        int maxRetryCount,
        TimeSpan maxRetryDelay,
        IEnumerable<int>? errorNumbersToAdd)
        : base(
            context,
            maxRetryCount,
            maxRetryDelay)
    {
        _additionalErrorNumbers = errorNumbersToAdd?.ToHashSet();
    }

    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerRetryingExecutionStrategy" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing service dependencies.</param>
    /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
    /// <param name="maxRetryDelay">The maximum delay between retries.</param>
    /// <param name="errorNumbersToAdd">Additional SQL error numbers that should be considered transient.</param>
    public SqlServerRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        int maxRetryCount,
        TimeSpan maxRetryDelay,
        IEnumerable<int>? errorNumbersToAdd)
        : base(dependencies, maxRetryCount, maxRetryDelay)
    {
        _additionalErrorNumbers = errorNumbersToAdd?.ToHashSet();
    }

    /// <summary>
    ///     Additional SQL error numbers that should be considered transient.
    /// </summary>
    public virtual IEnumerable<int>? AdditionalErrorNumbers
        => _additionalErrorNumbers;

    /// <summary>
    ///     Determines whether the specified exception represents a transient failure that can be
    ///     compensated by a retry. Additional exceptions to retry on can be passed to the constructor.
    /// </summary>
    /// <param name="exception">The exception object to be verified.</param>
    /// <returns>
    ///     <see langword="true" /> if the specified exception is considered as transient, otherwise <see langword="false" />.
    /// </returns>
    protected override bool ShouldRetryOn(Exception exception)
    {
        if (_additionalErrorNumbers != null
            && exception is SqlException sqlException)
        {
            foreach (SqlError err in sqlException.Errors)
            {
                if (_additionalErrorNumbers.Contains(err.Number))
                {
                    return true;
                }
            }
        }

        return SqlServerTransientExceptionDetector.ShouldRetryOn(exception);
    }

    /// <summary>
    ///     Determines whether the operation should be retried and the delay before the next attempt.
    /// </summary>
    /// <param name="lastException">The exception thrown during the last execution attempt.</param>
    /// <returns>
    ///     Returns the delay indicating how long to wait for before the next execution attempt if the operation should be retried;
    ///     <see langword="null" /> otherwise
    /// </returns>
    protected override TimeSpan? GetNextDelay(Exception lastException)
    {
        var baseDelay = base.GetNextDelay(lastException);
        if (baseDelay == null)
        {
            return null;
        }

        return CallOnWrappedException(lastException, IsMemoryOptimizedError)
            ? TimeSpan.FromMilliseconds(baseDelay.Value.TotalSeconds)
            : CallOnWrappedException(lastException, IsThrottlingError)
                ? baseDelay + DefaultMinDelayThrottling
                : baseDelay;
    }

    private static bool IsMemoryOptimizedError(Exception exception)
    {
        if (exception is SqlException sqlException)
        {
            foreach (SqlError err in sqlException.Errors)
            {
                switch (err.Number)
                {
                    case 41301:
                    case 41302:
                    case 41305:
                    case 41325:
                    case 41839:
                        return true;
                }
            }
        }

        return false;
    }

    private static bool IsThrottlingError(Exception exception)
    {
        if (exception is SqlException sqlException)
        {
            foreach (SqlError err in sqlException.Errors)
            {
                switch (err.Number)
                {
                    case 49977:
                    case 49920:
                    case 49919:
                    case 49918:
                    case 45319:
                    case 45182:
                    case 45161:
                    case 45157:
                    case 45156:
                    case 41840:
                    case 41823:
                    case 40903:
                    case 40890:
                    case 40675:
                    case 40648:
                    case 40642:
                    case 40613:
                    case 40501:
                    case 40189:
                    case 39110:
                    case 39108:
                    case 37327:
                    case 30085:
                    case 25740:
                    case 25738:
                    case 22498:
                    case 22335:
                    case 17889:
                    case 14355:
                    case 10930:
                    case 10929:
                    case 9985:
                    case 3950:
                    case 3935:
                    case 1404:
                    case 1204:
                    case 233:
                    case -2:
                        return true;
                }
            }
        }

        return false;
    }
}
