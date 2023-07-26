// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     A transaction against the database.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are typically obtained from <see cref="DatabaseFacade.BeginTransaction" /> and it is not designed
///         to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public class RelationalTransaction : IDbContextTransaction, IInfrastructure<DbTransaction>
{
    private readonly DbTransaction _dbTransaction;
    private readonly bool _transactionOwned;
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    private bool _connectionClosed;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalTransaction" /> class.
    /// </summary>
    /// <param name="connection">The connection to the database.</param>
    /// <param name="transaction">The underlying <see cref="DbTransaction" />.</param>
    /// <param name="transactionId">The correlation ID for the transaction.</param>
    /// <param name="logger">The logger to write to.</param>
    /// <param name="transactionOwned">
    ///     A value indicating whether the transaction is owned by this class (i.e. if it can be disposed when this class is disposed).
    /// </param>
    /// <param name="sqlGenerationHelper">The SQL generation helper to use.</param>
    public RelationalTransaction(
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
        bool transactionOwned,
        ISqlGenerationHelper sqlGenerationHelper)
    {
        if (connection.DbConnection != transaction.Connection)
        {
            throw new InvalidOperationException(RelationalStrings.TransactionAssociatedWithDifferentConnection);
        }

        Connection = connection;
        TransactionId = transactionId;

        _dbTransaction = transaction;
        Logger = logger;
        _transactionOwned = transactionOwned;
        _sqlGenerationHelper = sqlGenerationHelper;
    }

    /// <summary>
    ///     The connection.
    /// </summary>
    protected virtual IRelationalConnection Connection { get; }

    /// <summary>
    ///     The logger.
    /// </summary>
    protected virtual IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> Logger { get; }

    /// <inheritdoc />
    public virtual Guid TransactionId { get; }

    /// <inheritdoc />
    public virtual void Commit()
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            var interceptionResult = Logger.TransactionCommitting(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime);

            if (!interceptionResult.IsSuppressed)
            {
                _dbTransaction.Commit();
            }

            Logger.TransactionCommitted(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime,
                stopwatch.Elapsed);
        }
        catch (Exception e)
        {
            Logger.TransactionError(
                Connection,
                _dbTransaction,
                TransactionId,
                "Commit",
                e,
                startTime,
                stopwatch.Elapsed);

            throw;
        }

        ClearTransaction();
    }

    /// <inheritdoc />
    public virtual void Rollback()
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            var interceptionResult = Logger.TransactionRollingBack(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime);

            if (!interceptionResult.IsSuppressed)
            {
                _dbTransaction.Rollback();
            }

            Logger.TransactionRolledBack(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime,
                stopwatch.Elapsed);
        }
        catch (Exception e)
        {
            Logger.TransactionError(
                Connection,
                _dbTransaction,
                TransactionId,
                "Rollback",
                e,
                startTime,
                stopwatch.Elapsed);

            throw;
        }

        ClearTransaction();
    }

    /// <inheritdoc />
    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            var interceptionResult = await Logger.TransactionCommittingAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!interceptionResult.IsSuppressed)
            {
                await _dbTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            await Logger.TransactionCommittedAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    stopwatch.Elapsed,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await Logger.TransactionErrorAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    "Commit",
                    e,
                    startTime,
                    stopwatch.Elapsed,
                    cancellationToken)
                .ConfigureAwait(false);

            throw;
        }

        await ClearTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            var interceptionResult = await Logger.TransactionRollingBackAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!interceptionResult.IsSuppressed)
            {
                await _dbTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }

            await Logger.TransactionRolledBackAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    startTime,
                    stopwatch.Elapsed,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await Logger.TransactionErrorAsync(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    "Rollback",
                    e,
                    startTime,
                    stopwatch.Elapsed,
                    cancellationToken)
                .ConfigureAwait(false);

            throw;
        }

        await ClearTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual void CreateSavepoint(string name)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            var interceptionResult = Logger.CreatingTransactionSavepoint(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime);

            if (!interceptionResult.IsSuppressed)
            {
                using var command = Connection.DbConnection.CreateCommand();
                command.Transaction = _dbTransaction;
                command.CommandText = _sqlGenerationHelper.GenerateCreateSavepointStatement(name);
                command.ExecuteNonQuery();
            }

            Logger.CreatedTransactionSavepoint(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime);
        }
        catch (Exception e)
        {
            Logger.TransactionError(
                Connection,
                _dbTransaction,
                TransactionId,
                "CreateSavepoint",
                e,
                startTime,
                stopwatch.Elapsed);

            throw;
        }
    }

    /// <inheritdoc />
    public virtual async Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            var interceptionResult = await Logger.CreatingTransactionSavepointAsync(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime,
                cancellationToken).ConfigureAwait(false);

            if (!interceptionResult.IsSuppressed)
            {
                var command = Connection.DbConnection.CreateCommand();
                await using var _ = command.ConfigureAwait(false);
                command.Transaction = _dbTransaction;
                command.CommandText = _sqlGenerationHelper.GenerateCreateSavepointStatement(name);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await Logger.CreatedTransactionSavepointAsync(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await Logger.TransactionErrorAsync(
                Connection,
                _dbTransaction,
                TransactionId,
                "CreateSavepoint",
                e,
                startTime,
                stopwatch.Elapsed,
                cancellationToken).ConfigureAwait(false);

            throw;
        }
    }

    /// <inheritdoc />
    public virtual void RollbackToSavepoint(string name)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            var interceptionResult = Logger.RollingBackToTransactionSavepoint(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime);

            if (!interceptionResult.IsSuppressed)
            {
                using var command = Connection.DbConnection.CreateCommand();
                command.Transaction = _dbTransaction;
                command.CommandText = _sqlGenerationHelper.GenerateRollbackToSavepointStatement(name);
                command.ExecuteNonQuery();
            }

            Logger.RolledBackToTransactionSavepoint(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime);
        }
        catch (Exception e)
        {
            Logger.TransactionError(
                Connection,
                _dbTransaction,
                TransactionId,
                "RollbackToSavepoint",
                e,
                startTime,
                stopwatch.Elapsed);

            throw;
        }
    }

    /// <inheritdoc />
    public virtual async Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            var interceptionResult = await Logger.RollingBackToTransactionSavepointAsync(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime,
                cancellationToken).ConfigureAwait(false);

            if (!interceptionResult.IsSuppressed)
            {
                var command = Connection.DbConnection.CreateCommand();
                await using var _ = command.ConfigureAwait(false);
                command.Transaction = _dbTransaction;
                command.CommandText = _sqlGenerationHelper.GenerateRollbackToSavepointStatement(name);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await Logger.RolledBackToTransactionSavepointAsync(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await Logger.TransactionErrorAsync(
                Connection,
                _dbTransaction,
                TransactionId,
                "RollbackToSavepoint",
                e,
                startTime,
                stopwatch.Elapsed,
                cancellationToken).ConfigureAwait(false);

            throw;
        }
    }

    /// <inheritdoc />
    public virtual void ReleaseSavepoint(string name)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            var interceptionResult = Logger.ReleasingTransactionSavepoint(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime);

            if (!interceptionResult.IsSuppressed)
            {
                using var command = Connection.DbConnection.CreateCommand();
                command.Transaction = _dbTransaction;
                command.CommandText = _sqlGenerationHelper.GenerateReleaseSavepointStatement(name);
                command.ExecuteNonQuery();
            }

            Logger.ReleasedTransactionSavepoint(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime);
        }
        catch (Exception e)
        {
            Logger.TransactionError(
                Connection,
                _dbTransaction,
                TransactionId,
                "ReleaseSavepoint",
                e,
                startTime,
                stopwatch.Elapsed);

            throw;
        }
    }

    /// <inheritdoc />
    public virtual async Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = SharedStopwatch.StartNew();

        try
        {
            var interceptionResult = await Logger.ReleasingTransactionSavepointAsync(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime,
                cancellationToken).ConfigureAwait(false);

            if (!interceptionResult.IsSuppressed)
            {
                var command = Connection.DbConnection.CreateCommand();
                await using var _ = command.ConfigureAwait(false);
                command.Transaction = _dbTransaction;
                command.CommandText = _sqlGenerationHelper.GenerateReleaseSavepointStatement(name);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await Logger.ReleasedTransactionSavepointAsync(
                Connection,
                _dbTransaction,
                TransactionId,
                startTime,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            await Logger.TransactionErrorAsync(
                Connection,
                _dbTransaction,
                TransactionId,
                "ReleaseSavepoint",
                e,
                startTime,
                stopwatch.Elapsed,
                cancellationToken).ConfigureAwait(false);

            throw;
        }
    }

    /// <inheritdoc />
    public virtual bool SupportsSavepoints
        => true;

    /// <inheritdoc />
    public virtual void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;

            if (_transactionOwned)
            {
                _dbTransaction.Dispose();

                Logger.TransactionDisposed(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    DateTimeOffset.UtcNow);
            }

            ClearTransaction();
        }
    }

    /// <inheritdoc />
    public virtual async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;

            if (_transactionOwned)
            {
                await _dbTransaction.DisposeAsync().ConfigureAwait(false);

                Logger.TransactionDisposed(
                    Connection,
                    _dbTransaction,
                    TransactionId,
                    DateTimeOffset.UtcNow);
            }

            await ClearTransactionAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Remove the underlying transaction from the connection
    /// </summary>
    protected virtual void ClearTransaction()
    {
        Check.DebugAssert(
            Connection.CurrentTransaction == null || Connection.CurrentTransaction == this,
            "Connection.CurrentTransaction is unexpected instance");

        Connection.UseTransaction(null);

        if (!_connectionClosed)
        {
            _connectionClosed = true;

            Connection.Close();
        }
    }

    /// <summary>
    ///     Remove the underlying transaction from the connection
    /// </summary>
    protected virtual async Task ClearTransactionAsync(CancellationToken cancellationToken = default)
    {
        Check.DebugAssert(
            Connection.CurrentTransaction == null || Connection.CurrentTransaction == this,
            "Connection.CurrentTransaction is unexpected instance");

        await Connection.UseTransactionAsync(null, cancellationToken).ConfigureAwait(false);

        if (!_connectionClosed)
        {
            _connectionClosed = true;

            await Connection.CloseAsync().ConfigureAwait(false);
        }
    }

    DbTransaction IInfrastructure<DbTransaction>.Instance
        => _dbTransaction;
}
