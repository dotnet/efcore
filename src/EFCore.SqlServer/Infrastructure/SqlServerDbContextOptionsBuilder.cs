// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Allows SQL Server specific configuration to be performed on <see cref="DbContextOptions" />.
/// </summary>
/// <remarks>
///     Instances of this class are returned from a call to <see cref="O:SqlServerDbContextOptionsExtensions.UseSqlServer" />
///     and it is not designed to be directly constructed in your application code.
/// </remarks>
public class SqlServerDbContextOptionsBuilder
    : RelationalDbContextOptionsBuilder<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlServerDbContextOptionsBuilder" /> class.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    public SqlServerDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        : base(optionsBuilder)
    {
    }

    /// <summary>
    ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This strategy is specifically tailored to SQL Server (including Azure SQL). It is pre-configured with
    ///         error numbers for transient errors that can be retried.
    ///     </para>
    ///     <para>
    ///         Default values of 6 for the maximum retry count and 30 seconds for the maximum default delay are used.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    public virtual SqlServerDbContextOptionsBuilder EnableRetryOnFailure()
        => ExecutionStrategy(c => new SqlServerRetryingExecutionStrategy(c));

    /// <summary>
    ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This strategy is specifically tailored to SQL Server (including Azure SQL). It is pre-configured with
    ///         error numbers for transient errors that can be retried.
    ///     </para>
    ///     <para>
    ///         A default value 30 seconds for the maximum default delay is used.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    public virtual SqlServerDbContextOptionsBuilder EnableRetryOnFailure(int maxRetryCount)
        => ExecutionStrategy(c => new SqlServerRetryingExecutionStrategy(c, maxRetryCount));

    /// <summary>
    ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This strategy is specifically tailored to SQL Server (including Azure SQL). It is pre-configured with
    ///         error numbers for transient errors that can be retried.
    ///     </para>
    ///     <para>
    ///         Default values of 6 for the maximum retry count and 30 seconds for the maximum default delay are used.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="errorNumbersToAdd">Additional SQL error numbers that should be considered transient.</param>
    public virtual SqlServerDbContextOptionsBuilder EnableRetryOnFailure(ICollection<int> errorNumbersToAdd)
        => ExecutionStrategy(c => new SqlServerRetryingExecutionStrategy(c, errorNumbersToAdd));

    /// <summary>
    ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This strategy is specifically tailored to SQL Server (including Azure SQL). It is pre-configured with
    ///         error numbers for transient errors that can be retried, but additional error numbers can also be supplied.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
    /// <param name="maxRetryDelay">The maximum delay between retries.</param>
    /// <param name="errorNumbersToAdd">Additional SQL error numbers that should be considered transient.</param>
    public virtual SqlServerDbContextOptionsBuilder EnableRetryOnFailure(
        int maxRetryCount,
        TimeSpan maxRetryDelay,
        IEnumerable<int>? errorNumbersToAdd)
        => ExecutionStrategy(c => new SqlServerRetryingExecutionStrategy(c, maxRetryCount, maxRetryDelay, errorNumbersToAdd));

    /// <summary>
    ///     Sets the SQL Server compatibility level that EF Core will use when interacting with the database. This allows configuring EF
    ///     Core to work with older (or newer) versions of SQL Server. Defaults to <c>160</c> (SQL Server 2022).
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://learn.microsoft.com/sql/t-sql/statements/alter-database-transact-sql-compatibility-level">
    ///         SQL Server
    ///         documentation on compatibility level
    ///     </see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="compatibilityLevel"><see langword="false" /> to have null resource</param>
    public virtual SqlServerDbContextOptionsBuilder UseCompatibilityLevel(int compatibilityLevel)
        => WithOption(e => e.WithCompatibilityLevel(compatibilityLevel));

    /// <summary>
    ///     Configures the context to use defaults optimized for Azure SQL, including retries on errors.
    /// </summary>
    /// <param name="enable">Whether the defaults should be enabled.</param>
    public virtual SqlServerDbContextOptionsBuilder UseAzureSqlDefaults(bool enable = true)
        => WithOption(e => e.WithAzureSql(enable));
}
