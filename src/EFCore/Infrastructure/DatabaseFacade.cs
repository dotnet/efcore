// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Provides access to database related information and operations for a context.
///     Instances of this class are typically obtained from <see cref="DbContext.Database" /> and it is not designed
///     to be directly constructed in your application code.
/// </summary>
public class DatabaseFacade : IInfrastructure<IServiceProvider>, IDatabaseFacadeDependenciesAccessor, IResettableService
{
    private readonly DbContext _context;
    private IDatabaseFacadeDependencies? _dependencies;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseFacade" /> class. Instances of this class are typically
    ///     obtained from <see cref="DbContext.Database" /> and it is not designed to be directly constructed
    ///     in your application code.
    /// </summary>
    /// <param name="context">The context this database API belongs to.</param>
    public DatabaseFacade(DbContext context)
    {
        _context = context;
    }

    private IDatabaseFacadeDependencies Dependencies
        => _dependencies ??= _context.GetService<IDatabaseFacadeDependencies>();

    /// <summary>
    ///     Ensures that the database for the context exists.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 If the database exists and has any tables, then no action is taken. Nothing is done to ensure
    ///                 the database schema is compatible with the Entity Framework model.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 If the database exists but does not have any tables, then the Entity Framework model is used to
    ///                 create the database schema.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 If the database does not exist, then the database is created and the Entity Framework model is used to
    ///                 create the database schema.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         It is common to use <see cref="EnsureCreated" /> immediately following <see cref="EnsureDeleted" /> when
    ///         testing or prototyping using Entity Framework. This ensures that the database is in a clean state before each
    ///         execution of the test/prototype. Note, however, that data in the database is not preserved.
    ///     </para>
    ///     <para>
    ///         Note that this API does **not** use migrations to create the database. In addition, the database that is
    ///         created cannot be later updated using migrations. If you are targeting a relational database and using migrations,
    ///         then you can use <see cref="M:Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.Migrate" />
    ///         to ensure the database is created using migrations and that all migrations have been applied.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-manage-schemas">Managing database schemas with EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-ensure-created">Database creation APIs</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <returns><see langword="true" /> if the database is created, <see langword="false" /> if it already existed.</returns>
    [RequiresDynamicCode(
        "Migrations operations require building the design-time model which is not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public virtual bool EnsureCreated()
        => Dependencies.DatabaseCreator.EnsureCreated();

    /// <summary>
    ///     Ensures that the database for the context exists.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 If the database exists and has any tables, then no action is taken. Nothing is done to ensure
    ///                 the database schema is compatible with the Entity Framework model.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 If the database exists but does not have any tables, then the Entity Framework model is used to
    ///                 create the database schema.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 If the database does not exist, then the database is created and the Entity Framework model is used to
    ///                 create the database schema.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         It is common to use <see cref="EnsureCreatedAsync" /> immediately following <see cref="EnsureDeletedAsync" /> when
    ///         testing or prototyping using Entity Framework. This ensures that the database is in a clean state before each
    ///         execution of the test/prototype. Note, however, that data in the database is not preserved.
    ///     </para>
    ///     <para>
    ///         Note that this API does **not** use migrations to create the database. In addition, the database that is
    ///         created cannot be later updated using migrations. If you are targeting a relational database and using migrations,
    ///         then you can use <see cref="M:Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.MigrateAsync" />
    ///         to ensure the database is created using migrations and that all migrations have been applied.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
    ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see>
    ///         for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-manage-schemas">Managing database schemas with EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-ensure-created">Database creation APIs</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains <see langword="true" /> if the database is created,
    ///     <see langword="false" /> if it already existed.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    [RequiresDynamicCode(
        "Migrations operations require building the design-time model which is not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        => Dependencies.DatabaseCreator.EnsureCreatedAsync(cancellationToken);

    /// <summary>
    ///     <para>
    ///         Ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
    ///         exist then the database is deleted.
    ///     </para>
    ///     <para>
    ///         Warning: The entire database is deleted, and no effort is made to remove just the database objects that are used by
    ///         the model for this context.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         It is common to use <see cref="EnsureCreated" /> immediately following <see cref="EnsureDeleted" /> when
    ///         testing or prototyping using Entity Framework. This ensures that the database is in a clean state before each
    ///         execution of the test/prototype. Note, however, that data in the database is not preserved.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-manage-schemas">Managing database schemas with EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-ensure-created">Database creation APIs</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <returns><see langword="true" /> if the database is deleted, <see langword="false" /> if it did not exist.</returns>
    [RequiresDynamicCode(
        "Migrations operations require building the design-time model which is not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public virtual bool EnsureDeleted()
        => Dependencies.DatabaseCreator.EnsureDeleted();

    /// <summary>
    ///     <para>
    ///         Asynchronously ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
    ///         exist then the database is deleted.
    ///     </para>
    ///     <para>
    ///         Warning: The entire database is deleted, and no effort is made to remove just the database objects that are used by
    ///         the model for this context.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         It is common to use <see cref="EnsureCreatedAsync" /> immediately following <see cref="EnsureDeletedAsync" /> when
    ///         testing or prototyping using Entity Framework. This ensures that the database is in a clean state before each
    ///         execution of the test/prototype. Note, however, that data in the database is not preserved.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
    ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see>
    ///         for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-manage-schemas">Managing database schemas with EF Core</see>
    ///         and <see href="https://aka.ms/efcore-docs-ensure-created">Database creation APIs</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains <see langword="true" /> if the database is deleted,
    ///     <see langword="false" /> if it did not exist.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    [RequiresDynamicCode(
        "Migrations operations require building the design-time model which is not supported with NativeAOT"
        + " Use a migration bundle or an alternate way of executing migration operations.")]
    public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        => Dependencies.DatabaseCreator.EnsureDeletedAsync(cancellationToken);

    /// <summary>
    ///     Determines whether or not the database is available and can be connected to.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Any exceptions thrown when attempting to connect are caught and not propagated to the application.
    ///     </para>
    ///     <para>
    ///         The configured connection string is used to create the connection in the normal way, so all
    ///         configured options such as timeouts are honored.
    ///     </para>
    ///     <para>
    ///         Note that being able to connect to the database does not mean that it is
    ///         up-to-date with regard to schema creation, etc.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connections">Database connections in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <returns><see langword="true" /> if the database is available; <see langword="false" /> otherwise.</returns>
    public virtual bool CanConnect()
        => Dependencies.DatabaseCreator.CanConnect();

    /// <summary>
    ///     Determines whether or not the database is available and can be connected to.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Any exceptions thrown when attempting to connect are caught and not propagated to the application.
    ///     </para>
    ///     <para>
    ///         The configured connection string is used to create the connection in the normal way, so all
    ///         configured options such as timeouts are honored.
    ///     </para>
    ///     <para>
    ///         Note that being able to connect to the database does not mean that it is
    ///         up-to-date with regard to schema creation, etc.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
    ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see>
    ///         for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-connections">Database connections in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns><see langword="true" /> if the database is available; <see langword="false" /> otherwise.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => Dependencies.DatabaseCreator.CanConnectAsync(cancellationToken);

    /// <summary>
    ///     Starts a new transaction.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <returns>
    ///     A <see cref="IDbContextTransaction" /> that represents the started transaction.
    /// </returns>
    public virtual IDbContextTransaction BeginTransaction()
        => Dependencies.TransactionManager.BeginTransaction();

    /// <summary>
    ///     Asynchronously starts a new transaction.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
    ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see>
    ///         for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous transaction initialization. The task result contains a <see cref="IDbContextTransaction" />
    ///     that represents the started transaction.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Dependencies.TransactionManager.BeginTransactionAsync(cancellationToken);

    /// <summary>
    ///     Applies the outstanding operations in the current transaction to the database.
    /// </summary>
    public virtual void CommitTransaction()
        => Dependencies.TransactionManager.CommitTransaction();

    /// <summary>
    ///     Applies the outstanding operations in the current transaction to the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
    ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see>
    ///         for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        => Dependencies.TransactionManager.CommitTransactionAsync(cancellationToken);

    /// <summary>
    ///     Discards the outstanding operations in the current transaction.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    public virtual void RollbackTransaction()
        => Dependencies.TransactionManager.RollbackTransaction();

    /// <summary>
    ///     Discards the outstanding operations in the current transaction.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    ///     </para>
    ///     <para>
    ///         Entity Framework Core does not support multiple parallel operations being run on the same DbContext instance. This
    ///         includes both parallel execution of async queries and any explicit concurrent use from multiple threads.
    ///         Therefore, always await async calls immediately, or use separate DbContext instances for operations that execute
    ///         in parallel. See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        => Dependencies.TransactionManager.RollbackTransactionAsync(cancellationToken);

    /// <summary>
    ///     Creates an instance of the configured <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-connection-resiliency">Connection resiliency and database retries</see>
    ///     for more information and examples.
    /// </remarks>
    /// <returns>An <see cref="IExecutionStrategy" /> instance.</returns>
    public virtual IExecutionStrategy CreateExecutionStrategy()
        => Dependencies.ExecutionStrategyFactory.Create();

    /// <summary>
    ///     Gets the current <see cref="IDbContextTransaction" /> being used by the context, or null
    ///     if no transaction is in use.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This property is null unless one of <see cref="BeginTransaction" />,
    ///         <see cref="M:Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.BeginTransaction" />, or
    ///         <see cref="O:Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.UseTransaction" />
    ///         has been called.
    ///         No attempt is made to obtain a transaction from the current DbConnection or similar.
    ///     </para>
    ///     <para>
    ///         For relational databases, the underlying <see cref="DbTransaction" /> can be obtained using
    ///         <see cref="M:Microsoft.EntityFrameworkCore.Storage.DbContextTransactionExtensions.GetDbTransaction" />
    ///         on the returned <see cref="IDbContextTransaction" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public virtual IDbContextTransaction? CurrentTransaction
        => Dependencies.TransactionManager.CurrentTransaction;

    /// <summary>
    ///     Gets or sets a value indicating whether or not a transaction will be created automatically by
    ///     <see cref="DbContext.SaveChanges()" /> if none of the 'BeginTransaction' or 'UseTransaction' methods have been called.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Setting this value to <see langword="false" /> will also disable the <see cref="IExecutionStrategy" /> for
    ///         <see cref="DbContext.SaveChanges()" />
    ///     </para>
    ///     <para>
    ///         The default value is <see langword="true" />, meaning that <see cref="DbContext.SaveChanges()" /> will always use a
    ///         transaction when saving changes.
    ///     </para>
    ///     <para>
    ///         Setting this value to <see langword="false" /> should only be done with caution, since the database could be left in an
    ///         inconsistent state if failure occurs.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    [Obsolete("Use " + nameof(AutoTransactionBehavior) + " instead")]
    public virtual bool AutoTransactionsEnabled
    {
        get => AutoTransactionBehavior is AutoTransactionBehavior.Always or AutoTransactionBehavior.WhenNeeded;
        set
        {
            if (value)
            {
                if (AutoTransactionBehavior == AutoTransactionBehavior.Never)
                {
                    AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;
                }
            }
            else
            {
                AutoTransactionBehavior = AutoTransactionBehavior.Never;
            }
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether or not a transaction will be created automatically by
    ///     <see cref="DbContext.SaveChanges()" /> if neither 'BeginTransaction' nor 'UseTransaction' has been called.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The default setting is <see cref="AutoTransactionBehavior.WhenNeeded" />.
    ///     </para>
    ///     <para>
    ///         Setting this to <see cref="AutoTransactionBehavior.Never" /> with caution, since the database could be left in
    ///         an inconsistent state if failure occurs.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public virtual AutoTransactionBehavior AutoTransactionBehavior { get; set; } = AutoTransactionBehavior.WhenNeeded;

    /// <summary>
    ///     Whether a transaction savepoint will be created automatically by <see cref="DbContext.SaveChanges()" /> if it is called
    ///     after a transaction has been manually started with <see cref="BeginTransaction" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The default value is <see langword="true" />, meaning that <see cref="DbContext.SaveChanges()" /> will create a
    ///         transaction savepoint within a manually-started transaction. Regardless of this property, savepoints are only created
    ///         if the data provider supports them; see <see cref="IDbContextTransaction.SupportsSavepoints" />.
    ///     </para>
    ///     <para>
    ///         Setting this value to <see langword="false" /> should only be done with caution since the database could be left in a
    ///         corrupted state if <see cref="DbContext.SaveChanges()" /> fails.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public virtual bool AutoSavepointsEnabled { get; set; } = true;

    /// <summary>
    ///     Returns the name of the database provider currently in use.
    ///     The name is typically the name of the provider assembly.
    ///     It is usually easier to use a sugar method such as
    ///     <see cref="M:Microsoft.EntityFrameworkCore.SqlServerDatabaseFacadeExtensions.IsSqlServer" />
    ///     instead of calling this method directly.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method can only be used after the <see cref="DbContext" /> has been configured because
    ///         it is only then that the provider is known. This means that this method cannot be used
    ///         in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
    ///         provider to use as part of configuring the context.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext">DbContext lifetime, configuration, and initialization</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    public virtual string? ProviderName
        // Needs to be lazy because used from OnModelCreating
        => _context.GetService<IEnumerable<IDatabaseProvider>>()
            .Select(p => p.Name)
            .FirstOrDefault();

    /// <summary>
    ///     <para>
    ///         Gets the scoped <see cref="IServiceProvider" /> being used to resolve services.
    ///     </para>
    ///     <para>
    ///         This property is intended for use by extension methods that need to make use of services
    ///         not directly exposed in the public API surface.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    IServiceProvider IInfrastructure<IServiceProvider>.Instance
        => ((IInfrastructure<IServiceProvider>)_context).Instance;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    IDatabaseFacadeDependencies IDatabaseFacadeDependenciesAccessor.Dependencies
        => Dependencies;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    DbContext IDatabaseFacadeDependenciesAccessor.Context
        => _context;

    /// <inheritdoc />
    void IResettableService.ResetState()
    {
        AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;
        AutoSavepointsEnabled = true;
    }

    Task IResettableService.ResetStateAsync(CancellationToken cancellationToken)
    {
        ((IResettableService)this).ResetState();

        return Task.CompletedTask;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
