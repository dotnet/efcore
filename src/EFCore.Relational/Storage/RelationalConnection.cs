// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents a connection with a relational database.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public abstract class RelationalConnection : IRelationalConnection, ITransactionEnlistmentManager
{
    private string? _connectionString;
    private bool _connectionOwned;
    private int _openedCount;
    private bool _openedInternally;
    private int? _commandTimeout;
    private readonly int? _defaultCommandTimeout;
    private readonly ConcurrentStack<Transaction> _ambientTransactions = new();
    private DbConnection? _connection;
    private readonly IRelationalCommandBuilder _relationalCommandBuilder;
    private IRelationalCommand? _cachedRelationalCommand;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalConnection" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    protected RelationalConnection(RelationalConnectionDependencies dependencies)
    {
        Context = dependencies.CurrentContext.Context;
        _relationalCommandBuilder = dependencies.RelationalCommandBuilderFactory.Create();

        Dependencies = dependencies;

        var relationalOptions = RelationalOptionsExtension.Extract(dependencies.ContextOptions);

        _defaultCommandTimeout = _commandTimeout = relationalOptions.CommandTimeout;

        _connectionString = string.IsNullOrWhiteSpace(relationalOptions.ConnectionString)
            ? null
            : dependencies.ConnectionStringResolver.ResolveConnectionString(relationalOptions.ConnectionString);

        if (relationalOptions.Connection != null)
        {
            _connection = relationalOptions.Connection;
            _connectionOwned = relationalOptions.IsConnectionOwned;

            Check.DebugAssert(_connectionString == null, "ConnectionString is not null");
        }
        else
        {
            _connectionOwned = true;
        }
    }

    /// <summary>
    ///     The unique identifier for this connection.
    /// </summary>
    public virtual Guid ConnectionId { get; } = Guid.NewGuid();

    /// <summary>
    ///     The <see cref="DbContext" /> currently in use.
    /// </summary>
    public virtual DbContext Context { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConnectionDependencies Dependencies { get; }

    /// <summary>
    ///     Creates a <see cref="DbConnection" /> to the database.
    /// </summary>
    /// <returns>The connection.</returns>
    protected abstract DbConnection CreateDbConnection();

    /// <summary>
    ///     Gets or sets the connection string for the database.
    /// </summary>
    public virtual string? ConnectionString
    {
        get => _connectionString ?? _connection?.ConnectionString;
        set
        {
            if (_connection != null && _connection.ConnectionString != value)
            {
                // Let ADO.NET throw if this is not valid for the state of the connection.
                _connection.ConnectionString = value;
            }

            _connectionString = value;
        }
    }

    /// <summary>
    ///     Returns the configured connection string only if it has been set or a valid <see cref="DbConnection" /> exists.
    /// </summary>
    /// <returns>The connection string.</returns>
    /// <exception cref="InvalidOperationException">when connection string cannot be obtained.</exception>
    protected virtual string GetValidatedConnectionString()
        => ConnectionString!;

    /// <summary>
    ///     Gets or sets the underlying <see cref="System.Data.Common.DbConnection" /> used to connect to the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The connection can only be changed when the existing connection, if any, is not open.
    ///     </para>
    ///     <para>
    ///         Note that a connection set must be disposed by application code since it was not created by Entity Framework.
    ///     </para>
    /// </remarks>
    [AllowNull]
    public virtual DbConnection DbConnection
    {
        get
        {
            if (_connection != null)
            {
                return _connection;
            }

            var logger = Dependencies.ConnectionLogger;
            var startTime = DateTimeOffset.UtcNow;

            if (logger.ShouldLogConnectionCreate(startTime))
            {
                var stopwatch = SharedStopwatch.StartNew();

                var interceptionResult = logger.ConnectionCreating(this, startTime);

                _connection = interceptionResult.HasResult
                    ? interceptionResult.Result
                    : CreateDbConnection();

                _connection = logger.ConnectionCreated(this, startTime, stopwatch.Elapsed);
            }
            else
            {
                _connection = CreateDbConnection();
            }

            return _connection;
        }
        set => SetDbConnection(value, contextOwnsConnection: false);
    }

    /// <inheritdoc />
    public virtual void SetDbConnection(DbConnection? value, bool contextOwnsConnection)
    {
        if (!ReferenceEquals(_connection, value))
        {
            if (_connectionOwned && _openedCount > 0)
            {
                throw new InvalidOperationException(RelationalStrings.CannotChangeWhenOpen);
            }

            Dispose();

            _connection = value;
            _connectionString = null;
            _connectionOwned = contextOwnsConnection;
        }
    }

    /// <summary>
    ///     The current ambient transaction. Defaults to <see cref="Transaction.Current" />.
    /// </summary>
    public virtual Transaction? CurrentAmbientTransaction
        => Transaction.Current;

    /// <summary>
    ///     Gets the current transaction.
    /// </summary>
    public virtual IDbContextTransaction? CurrentTransaction { get; protected set; }

    /// <summary>
    ///     The currently enlisted transaction.
    /// </summary>
    public virtual Transaction? EnlistedTransaction
    {
        get
        {
            if (_enlistedTransaction != null)
            {
                try
                {
                    if (_enlistedTransaction.TransactionInformation.Status != TransactionStatus.Active)
                    {
                        _enlistedTransaction = null;
                    }
                }
                catch (ObjectDisposedException)
                {
                    _enlistedTransaction = null;
                }
            }

            return _enlistedTransaction;
        }
        protected set => _enlistedTransaction = value;
    }

    /// <summary>
    ///     Specifies an existing <see cref="Transaction" /> to be used for database operations.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    public virtual void EnlistTransaction(Transaction? transaction)
    {
        if (!SupportsAmbientTransactions)
        {
            Dependencies.TransactionLogger.AmbientTransactionWarning(this, DateTimeOffset.UtcNow);
            return;
        }

        if (transaction != null)
        {
            Dependencies.TransactionLogger.ExplicitTransactionEnlisted(this, transaction);
        }

        ConnectionEnlistTransaction(transaction);

        EnlistedTransaction = transaction;
    }

    /// <summary>
    ///     Template method that by default calls <see cref="System.Data.Common.DbConnection.EnlistTransaction" /> but can be overridden
    ///     by providers to make a different call instead.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    protected virtual void ConnectionEnlistTransaction(Transaction? transaction)
        => DbConnection.EnlistTransaction(transaction);

    /// <summary>
    ///     Indicates whether the store connection supports ambient transactions
    /// </summary>
    protected virtual bool SupportsAmbientTransactions
        => false;

    /// <summary>
    ///     Gets the timeout for executing a command against the database.
    /// </summary>
    public virtual int? CommandTimeout
    {
        get => _commandTimeout;
        set
        {
            if (value is < 0)
            {
                throw new ArgumentException(RelationalStrings.InvalidCommandTimeout(value));
            }

            _commandTimeout = value;
        }
    }

    /// <summary>
    ///     Rents a relational command that can be executed with this connection.
    /// </summary>
    /// <returns>A relational command that can be executed with this connection.</returns>
    public virtual IRelationalCommand RentCommand()
    {
        var command = _cachedRelationalCommand;

        if (command is null)
        {
            return _relationalCommandBuilder.Build();
        }

        _cachedRelationalCommand = null;
        return command;
    }

    /// <summary>
    ///     Returns a relational command to this connection, so that it can be reused in the future.
    /// </summary>
    public virtual void ReturnCommand(IRelationalCommand command)
        => _cachedRelationalCommand ??= command;

    /// <summary>
    ///     Begins a new transaction.
    /// </summary>
    /// <returns>The newly created transaction.</returns>
    public virtual IDbContextTransaction BeginTransaction()
        => BeginTransaction(IsolationLevel.Unspecified);

    /// <summary>
    ///     Asynchronously begins a new transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the newly created transaction.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => await BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken).ConfigureAwait(false);

    /// <summary>
    ///     Begins a new transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <returns>The newly created transaction.</returns>
    // ReSharper disable once RedundantNameQualifier
    public virtual IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        Open();

        EnsureNoTransactions();

        var transactionId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        var interceptionResult = Dependencies.TransactionLogger.TransactionStarting(
            this,
            isolationLevel,
            transactionId,
            startTime);

        var dbTransaction = interceptionResult.HasResult
            ? interceptionResult.Result
            : ConnectionBeginTransaction(isolationLevel);

        dbTransaction = Dependencies.TransactionLogger.TransactionStarted(
            this,
            dbTransaction,
            transactionId,
            startTime,
            stopwatch.Elapsed);

        return CreateRelationalTransaction(dbTransaction, transactionId, true);
    }

    /// <summary>
    ///     Template method that by default calls <see cref="System.Data.Common.DbConnection.BeginDbTransaction" /> but can be overridden
    ///     by providers to make a different call instead.
    /// </summary>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <returns>The newly created transaction.</returns>
    protected virtual DbTransaction ConnectionBeginTransaction(IsolationLevel isolationLevel)
        => DbConnection.BeginTransaction(isolationLevel);

    /// <summary>
    ///     Asynchronously begins a new transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the newly created transaction.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<IDbContextTransaction> BeginTransactionAsync(
        // ReSharper disable once RedundantNameQualifier
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        await OpenAsync(cancellationToken).ConfigureAwait(false);

        EnsureNoTransactions();

        var transactionId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        var interceptionResult = await Dependencies.TransactionLogger.TransactionStartingAsync(
                this,
                isolationLevel,
                transactionId,
                startTime,
                cancellationToken)
            .ConfigureAwait(false);

        var dbTransaction = interceptionResult.HasResult
            ? interceptionResult.Result
            : await ConnectionBeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);

        dbTransaction = await Dependencies.TransactionLogger.TransactionStartedAsync(
                this,
                dbTransaction,
                transactionId,
                startTime,
                stopwatch.Elapsed,
                cancellationToken)
            .ConfigureAwait(false);

        return CreateRelationalTransaction(dbTransaction, transactionId, true);
    }

    /// <summary>
    ///     Template method that by default calls <see cref="System.Data.Common.DbConnection.BeginDbTransactionAsync" /> but can be
    ///     overridden by providers to make a different call instead.
    /// </summary>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The newly created transaction.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected virtual ValueTask<DbTransaction> ConnectionBeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
        => DbConnection.BeginTransactionAsync(isolationLevel, cancellationToken);

    private void EnsureNoTransactions()
    {
        if (CurrentTransaction != null)
        {
            throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
        }

        EnsureNoAmbientOrEnlistedTransactions();
    }

    private void EnsureNoAmbientOrEnlistedTransactions()
    {
        if (CurrentAmbientTransaction != null)
        {
            throw new InvalidOperationException(RelationalStrings.ConflictingAmbientTransaction);
        }

        if (EnlistedTransaction != null)
        {
            throw new InvalidOperationException(RelationalStrings.ConflictingEnlistedTransaction);
        }
    }

    private IDbContextTransaction CreateRelationalTransaction(DbTransaction transaction, Guid transactionId, bool transactionOwned)
        => CurrentTransaction
            = Dependencies.RelationalTransactionFactory.Create(
                this,
                transaction,
                transactionId,
                Dependencies.TransactionLogger,
                transactionOwned: transactionOwned);

    /// <summary>
    ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    /// <returns>An instance of <see cref="IDbTransaction" /> that wraps the provided transaction.</returns>
    public virtual IDbContextTransaction? UseTransaction(DbTransaction? transaction)
        => UseTransaction(transaction, Guid.NewGuid());

    /// <summary>
    ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    /// <param name="transactionId">The unique identifier for the transaction.</param>
    /// <returns>
    ///     An instance of <see cref="IDbContextTransaction" /> that wraps the provided transaction, or <see langword="null" />
    ///     if <paramref name="transaction" /> is <see langword="null" />.
    /// </returns>
    [return: NotNullIfNotNull("transaction")]
    public virtual IDbContextTransaction? UseTransaction(DbTransaction? transaction, Guid transactionId)
    {
        if (ShouldUseTransaction(transaction))
        {
            Open();

            transaction = Dependencies.TransactionLogger.TransactionUsed(
                this,
                transaction,
                transactionId,
                DateTimeOffset.UtcNow);

            CurrentTransaction = CreateRelationalTransaction(transaction, transactionId, transactionOwned: false);
        }

        return CurrentTransaction;
    }

    /// <summary>
    ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="IDbTransaction" /> that wraps the provided transaction.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task<IDbContextTransaction?> UseTransactionAsync(
        DbTransaction? transaction,
        CancellationToken cancellationToken = default)
        => UseTransactionAsync(transaction, Guid.NewGuid(), cancellationToken);

    /// <summary>
    ///     Specifies an existing <see cref="DbTransaction" /> to be used for database operations.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    /// <param name="transactionId">The unique identifier for the transaction.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="IDbTransaction" /> that wraps the provided transaction.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<IDbContextTransaction?> UseTransactionAsync(
        DbTransaction? transaction,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        if (ShouldUseTransaction(transaction))
        {
            await OpenAsync(cancellationToken).ConfigureAwait(false);

            transaction = await Dependencies.TransactionLogger.TransactionUsedAsync(
                    this,
                    transaction,
                    transactionId,
                    DateTimeOffset.UtcNow,
                    cancellationToken)
                .ConfigureAwait(false);

            CurrentTransaction = CreateRelationalTransaction(transaction, transactionId, transactionOwned: false);
        }

        return CurrentTransaction;
    }

    private bool ShouldUseTransaction([NotNullWhen(true)] DbTransaction? transaction)
    {
        if (transaction == null)
        {
            if (CurrentTransaction != null)
            {
                CurrentTransaction = null;
            }

            return false;
        }

        EnsureNoAmbientOrEnlistedTransactions();

        return CurrentTransaction == null || transaction != CurrentTransaction.GetDbTransaction();
    }

    /// <summary>
    ///     Commits all changes made to the database in the current transaction.
    /// </summary>
    public virtual void CommitTransaction()
    {
        if (CurrentTransaction == null)
        {
            throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
        }

        CurrentTransaction.Commit();
    }

    /// <summary>
    ///     Commits all changes made to the database in the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
        {
            throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
        }

        return CurrentTransaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    ///     Discards all changes made to the database in the current transaction.
    /// </summary>
    public virtual void RollbackTransaction()
    {
        if (CurrentTransaction == null)
        {
            throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
        }

        CurrentTransaction.Rollback();
    }

    /// <summary>
    ///     Discards all changes made to the database in the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
        {
            throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
        }

        return CurrentTransaction.RollbackAsync(cancellationToken);
    }

    /// <summary>
    ///     Opens the connection to the database.
    /// </summary>
    /// <param name="errorsExpected">
    ///     Indicates if the connection errors are expected and should be logged as debug message.
    /// </param>
    /// <returns><see langword="true" /> if the underlying connection was actually opened; <see langword="false" /> otherwise.</returns>
    public virtual bool Open(bool errorsExpected = false)
    {
        if (DbConnection.State == ConnectionState.Broken)
        {
            CloseDbConnection();
        }

        var wasOpened = false;
        if (DbConnection.State != ConnectionState.Open)
        {
            CurrentTransaction?.Dispose();
            ClearTransactions(clearAmbient: false);
            OpenInternal(errorsExpected);
            wasOpened = true;
        }

        _openedCount++;

        HandleAmbientTransactions();

        return wasOpened;
    }

    /// <summary>
    ///     Asynchronously opens the connection to the database.
    /// </summary>
    /// <param name="errorsExpected">Indicate if the connection errors are expected and should be logged as debug message.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation, with a value of <see langword="true" /> if the connection
    ///     was actually opened.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public virtual async Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
    {
        if (DbConnection.State == ConnectionState.Broken)
        {
            await CloseDbConnectionAsync().ConfigureAwait(false);
        }

        var wasOpened = false;
        if (DbConnection.State != ConnectionState.Open)
        {
            if (CurrentTransaction != null)
            {
                await CurrentTransaction.DisposeAsync().ConfigureAwait(false);
            }

            ClearTransactions(clearAmbient: false);
            await OpenInternalAsync(errorsExpected, cancellationToken).ConfigureAwait(false);

            wasOpened = true;
        }

        _openedCount++;

        HandleAmbientTransactions();

        return wasOpened;
    }

    private void ClearTransactions(bool clearAmbient)
    {
        CurrentTransaction = null;
        EnlistedTransaction = null;
        if (clearAmbient && _ambientTransactions.Count > 0)
        {
            while (_ambientTransactions.Any(t => t != null))
            {
                _ambientTransactions.TryPop(out var ambientTransaction);
                if (ambientTransaction != null)
                {
                    ambientTransaction.TransactionCompleted -= HandleTransactionCompleted;
                }
            }
        }

        if (_openedCount < 0)
        {
            _openedCount = 0;
        }
    }

    private void OpenInternal(bool errorsExpected)
    {
        var logger = Dependencies.ConnectionLogger;
        var startTime = DateTimeOffset.UtcNow;

        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            if (logger.ShouldLogConnectionOpen(startTime))
            {
                var interceptionResult = logger.ConnectionOpening(this, startTime);

                if (!interceptionResult.IsSuppressed)
                {
                    OpenDbConnection(errorsExpected);
                }

                logger.ConnectionOpened(this, startTime, stopwatch.Elapsed);
            }
            else
            {
                OpenDbConnection(errorsExpected);
            }
        }
        catch (Exception e)
        {
            logger.ConnectionError(this, e, startTime, stopwatch.Elapsed, errorsExpected);

            throw;
        }

        if (_openedCount == 0)
        {
            _openedInternally = true;
        }
    }

    /// <summary>
    ///     Template method that by default calls <see cref="System.Data.Common.DbConnection.Open" /> but can be overridden
    ///     by providers to make a different call instead.
    /// </summary>
    /// <param name="errorsExpected">Indicates if the connection errors are expected and should be logged as debug message.</param>
    protected virtual void OpenDbConnection(bool errorsExpected)
        => DbConnection.Open();

    private async Task OpenInternalAsync(bool errorsExpected, CancellationToken cancellationToken)
    {
        var logger = Dependencies.ConnectionLogger;
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            if (logger.ShouldLogConnectionOpen(startTime))
            {
                var interceptionResult
                    = await logger.ConnectionOpeningAsync(this, startTime, cancellationToken).ConfigureAwait(false);

                if (!interceptionResult.IsSuppressed)
                {
                    await OpenDbConnectionAsync(errorsExpected, cancellationToken).ConfigureAwait(false);
                }

                await logger.ConnectionOpenedAsync(this, startTime, stopwatch.Elapsed, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await OpenDbConnectionAsync(errorsExpected, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            await logger.ConnectionErrorAsync(
                    this,
                    e,
                    startTime,
                    stopwatch.Elapsed,
                    errorsExpected,
                    cancellationToken)
                .ConfigureAwait(false);

            throw;
        }

        if (_openedCount == 0)
        {
            _openedInternally = true;
        }
    }

    /// <summary>
    ///     Template method that by default calls <see cref="O:System.Data.Common.DbConnection.OpenAsync" /> but can be overridden
    ///     by providers to make a different call instead.
    /// </summary>
    /// <param name="errorsExpected">Indicates if the connection errors are expected and should be logged as debug message.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    protected virtual Task OpenDbConnectionAsync(bool errorsExpected, CancellationToken cancellationToken)
        => DbConnection.OpenAsync(cancellationToken);

    private void HandleAmbientTransactions()
    {
        var current = CurrentAmbientTransaction;

        if (current == null)
        {
            var rootTransaction = _ambientTransactions.Count > 0 && _ambientTransactions.TryPeek(out var transaction)
                ? transaction
                : null;

            if (rootTransaction != null)
            {
                throw new InvalidOperationException(RelationalStrings.PendingAmbientTransaction);
            }

            return;
        }

        if (!SupportsAmbientTransactions)
        {
            Dependencies.TransactionLogger.AmbientTransactionWarning(this, DateTimeOffset.UtcNow);
            return;
        }

        if (_ambientTransactions.Contains(current))
        {
            return;
        }

        Dependencies.TransactionLogger.AmbientTransactionEnlisted(this, current);
        current.TransactionCompleted += HandleTransactionCompleted;

        ConnectionEnlistTransaction(current);
        _ambientTransactions.Push(current);
    }

    private void HandleTransactionCompleted(object? sender, TransactionEventArgs e)
    {
        // This could be invoked on a different thread at arbitrary time after the transaction completes
        _ambientTransactions.TryPeek(out var ambientTransaction);
        if (e.Transaction != ambientTransaction)
        {
            throw new InvalidOperationException(RelationalStrings.NestedAmbientTransactionError);
        }

        if (ambientTransaction != null)
        {
            ambientTransaction.TransactionCompleted -= HandleTransactionCompleted;
        }

        _ambientTransactions.TryPop(out var _);
    }

    /// <summary>
    ///     Closes the connection to the database.
    /// </summary>
    /// <returns><see langword="true" /> if the underlying connection was actually closed; <see langword="false" /> otherwise.</returns>
    public virtual bool Close()
    {
        var wasClosed = false;

        if (ShouldClose())
        {
            CurrentTransaction?.Dispose();
            ClearTransactions(clearAmbient: false);

            if (DbConnection.State != ConnectionState.Closed)
            {
                var logger = Dependencies.ConnectionLogger;
                var startTime = DateTimeOffset.UtcNow;
                var stopwatch = SharedStopwatch.StartNew();

                try
                {
                    if (logger.ShouldLogConnectionClose(startTime))
                    {
                        var interceptionResult = logger.ConnectionClosing(this, startTime);

                        if (!interceptionResult.IsSuppressed)
                        {
                            CloseDbConnection();
                        }

                        logger.ConnectionClosed(this, startTime, stopwatch.Elapsed);
                    }
                    else
                    {
                        CloseDbConnection();
                    }

                    wasClosed = true;
                }
                catch (Exception e)
                {
                    logger.ConnectionError(this, e, startTime, stopwatch.Elapsed, false);

                    throw;
                }
            }

            _openedInternally = false;
        }

        return wasClosed;
    }

    /// <summary>
    ///     Template method that by default calls <see cref="System.Data.Common.DbConnection.Close" /> but can be overridden
    ///     by providers to make a different call instead.
    /// </summary>
    protected virtual void CloseDbConnection()
        => DbConnection.Close();

    /// <summary>
    ///     Closes the connection to the database.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation, with a value of <see langword="true" /> if the connection
    ///     was actually closed.
    /// </returns>
    public virtual async Task<bool> CloseAsync()
    {
        var wasClosed = false;

        if (ShouldClose())
        {
            if (CurrentTransaction != null)
            {
                await CurrentTransaction.DisposeAsync().ConfigureAwait(false);
            }

            ClearTransactions(clearAmbient: false);

            var stopwatch = SharedStopwatch.StartNew();

            if (DbConnection.State != ConnectionState.Closed)
            {
                var logger = Dependencies.ConnectionLogger;
                var startTime = DateTimeOffset.UtcNow;

                try
                {
                    if (logger.ShouldLogConnectionClose(startTime))
                    {
                        var interceptionResult = await logger.ConnectionClosingAsync(this, startTime)
                            .ConfigureAwait(false);

                        if (!interceptionResult.IsSuppressed)
                        {
                            await CloseDbConnectionAsync().ConfigureAwait(false);
                        }

                        await logger.ConnectionClosedAsync(this, startTime, stopwatch.Elapsed).ConfigureAwait(false);
                    }
                    else
                    {
                        await CloseDbConnectionAsync().ConfigureAwait(false);
                    }

                    wasClosed = true;
                }
                catch (Exception e)
                {
                    await logger.ConnectionErrorAsync(
                            this,
                            e,
                            startTime,
                            stopwatch.Elapsed,
                            false)
                        .ConfigureAwait(false);

                    throw;
                }
            }

            _openedInternally = false;
        }

        return wasClosed;
    }

    /// <summary>
    ///     Template method that by default calls <see cref="System.Data.Common.DbConnection.CloseAsync" /> but can be overridden
    ///     by providers to make a different call instead.
    /// </summary>
    protected virtual Task CloseDbConnectionAsync()
        => DbConnection.CloseAsync();

    private bool ShouldClose()
        => (_openedCount == 0
                || _openedCount > 0
                && --_openedCount == 0)
            && _openedInternally;

    void IResettableService.ResetState()
        => ResetState(disposeDbConnection: false);

    Task IResettableService.ResetStateAsync(CancellationToken cancellationToken)
        => ResetStateAsync(disposeDbConnection: false).AsTask();

    private Transaction? _enlistedTransaction;

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual void Dispose()
        => ResetState(disposeDbConnection: true);

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual ValueTask DisposeAsync()
        => ResetStateAsync(disposeDbConnection: true);

    /// <summary>
    ///     Resets the connection state. Called by <see cref="Dispose" /> and <see cref="IResettableService.ResetState" />.
    /// </summary>
    /// <param name="disposeDbConnection">
    ///     Whether the underlying <see cref="DbConnection" /> should be disposed, assuming it is owned by this
    ///     <see cref="RelationalConnection" />. If <see langword="false" />, the existing <see cref="DbConnection" /> may get reused.
    /// </param>
    protected virtual void ResetState(bool disposeDbConnection)
    {
        CurrentTransaction?.Dispose();
        ClearTransactions(clearAmbient: true);

        _commandTimeout = _defaultCommandTimeout;

        _openedCount = 0;
        _openedInternally = false;

        if (_connectionOwned
            && _connection is not null)
        {
            CloseDbConnection();

            if (disposeDbConnection)
            {
                DisposeDbConnection();
                _connection = null;
                _openedCount = 0;
                _openedInternally = false;
            }
        }
    }

    /// <summary>
    ///     Resets the connection state. Called by <see cref="DisposeAsync" /> and <see cref="IResettableService.ResetStateAsync" />.
    /// </summary>
    /// <param name="disposeDbConnection">
    ///     Whether the underlying <see cref="DbConnection" /> should be disposed, assuming it is owned by this
    ///     <see cref="RelationalConnection" />. If <see langword="false" />, the existing <see cref="DbConnection" /> may get reused.
    /// </param>
    protected virtual async ValueTask ResetStateAsync(bool disposeDbConnection)
    {
        if (CurrentTransaction != null)
        {
            await CurrentTransaction.DisposeAsync().ConfigureAwait(false);
        }

        ClearTransactions(clearAmbient: true);

        _commandTimeout = _defaultCommandTimeout;

        if (_connectionOwned
            && _connection is not null)
        {
            await CloseDbConnectionAsync().ConfigureAwait(continueOnCapturedContext: false);

            if (disposeDbConnection)
            {
                await DisposeDbConnectionAsync().ConfigureAwait(false);
                _connection = null;
                _openedCount = 0;
                _openedInternally = false;
            }
        }
    }

    /// <summary>
    ///     Template method that by default calls <see cref="Component.Dispose()" /> but can be overridden by
    ///     providers to make a different call instead.
    /// </summary>
    protected virtual void DisposeDbConnection()
    {
        var logger = Dependencies.ConnectionLogger;
        var startTime = DateTimeOffset.UtcNow;

        if (logger.ShouldLogConnectionDispose(startTime))
        {
            var stopwatch = SharedStopwatch.StartNew();

            var interceptionResult = logger.ConnectionDisposing(this, startTime);

            if (!interceptionResult.IsSuppressed)
            {
                DbConnection.Dispose();
            }

            logger.ConnectionDisposed(this, startTime, stopwatch.Elapsed);
        }
        else
        {
            DbConnection.Dispose();
        }
    }

    /// <summary>
    ///     Template method that by default calls <see cref="System.Data.Common.DbConnection.DisposeAsync" /> but can be overridden by
    ///     providers to make a different call instead.
    /// </summary>
    protected virtual async ValueTask DisposeDbConnectionAsync()
    {
        var logger = Dependencies.ConnectionLogger;
        var startTime = DateTimeOffset.UtcNow;

        if (logger.ShouldLogConnectionDispose(startTime))
        {
            var stopwatch = SharedStopwatch.StartNew();

            var interceptionResult = await logger.ConnectionDisposingAsync(this, startTime).ConfigureAwait(false);

            if (!interceptionResult.IsSuppressed)
            {
                await DbConnection.DisposeAsync().ConfigureAwait(false);
            }

            await logger.ConnectionDisposedAsync(this, startTime, stopwatch.Elapsed).ConfigureAwait(false);
        }
        else
        {
            await DbConnection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
