// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     <para>
///         This class contains static methods used by EF Core internals and relational database providers to
///         write information to an <see cref="ILogger" /> and a <see cref="DiagnosticListener" /> for
///         well-known events.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public static class RelationalLoggerExtensions
{
    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionStarting" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    public static InterceptionResult<DbTransaction> TransactionStarting(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        IsolationLevel isolationLevel,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogBeginningTransaction(diagnostics);

        LogTransactionStarting(diagnostics, isolationLevel, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionStarting(
                diagnostics,
                connection,
                isolationLevel,
                transactionId,
                false,
                startTime,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionStarting(connection.DbConnection, eventData, default);
            }
        }

        return default;
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionStarting" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        IsolationLevel isolationLevel,
        Guid transactionId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogBeginningTransaction(diagnostics);

        LogTransactionStarting(diagnostics, isolationLevel, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionStarting(
                diagnostics,
                connection,
                isolationLevel,
                transactionId,
                true,
                startTime,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionStartingAsync(connection.DbConnection, eventData, default, cancellationToken);
            }
        }

        return default;
    }

    private static TransactionStartingEventData BroadcastTransactionStarting(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        IsolationLevel isolationLevel,
        Guid transactionId,
        bool async,
        DateTimeOffset startTime,
        EventDefinition<string> definition,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionStartingEventData(
            definition,
            TransactionStarting,
            connection.Context,
            isolationLevel,
            transactionId,
            connection.ConnectionId,
            async,
            startTime);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogTransactionStarting(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IsolationLevel isolationLevel,
        EventDefinition<string> definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, isolationLevel.ToString("G"));
        }
    }

    private static string TransactionStarting(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (TransactionStartingEventData)payload;
        return d.GenerateMessage(
            p.IsolationLevel.ToString("G"));
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionStarted" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The amount of time before the connection was opened.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    public static DbTransaction TransactionStarted(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        TimeSpan duration)
    {
        var definition = RelationalResources.LogBeganTransaction(diagnostics);

        LogTransactionStarted(diagnostics, transaction, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionStarted(
                diagnostics,
                connection,
                transaction,
                transactionId,
                false,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionStarted(connection.DbConnection, eventData, transaction);
            }
        }

        return transaction;
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionStarted" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The amount of time before the connection was opened.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<DbTransaction> TransactionStartedAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogBeganTransaction(diagnostics);

        LogTransactionStarted(diagnostics, transaction, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionStarted(
                diagnostics,
                connection,
                transaction,
                transactionId,
                true,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionStartedAsync(connection.DbConnection, eventData, transaction, cancellationToken);
            }
        }

        return ValueTask.FromResult(transaction);
    }

    private static TransactionEndEventData BroadcastTransactionStarted(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        bool async,
        DateTimeOffset startTime,
        TimeSpan duration,
        EventDefinition<string> definition,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEndEventData(
            definition,
            TransactionStarted,
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime,
            duration);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogTransactionStarted(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        DbTransaction transaction,
        EventDefinition<string> definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, transaction.IsolationLevel.ToString("G"));
        }
    }

    private static string TransactionStarted(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (TransactionEndEventData)payload;
        return d.GenerateMessage(p.Transaction.IsolationLevel.ToString("G"));
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionUsed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    public static DbTransaction TransactionUsed(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogUsingTransaction(diagnostics);

        LogTransactionUsed(diagnostics, transaction, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionUsed(
                diagnostics,
                connection,
                transaction,
                transactionId,
                false,
                startTime,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionUsed(connection.DbConnection, eventData, transaction);
            }
        }

        return transaction;
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionUsed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<DbTransaction> TransactionUsedAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogUsingTransaction(diagnostics);

        LogTransactionUsed(diagnostics, transaction, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionUsed(
                diagnostics,
                connection,
                transaction,
                transactionId,
                true,
                startTime,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionUsedAsync(connection.DbConnection, eventData, transaction, cancellationToken);
            }
        }

        return ValueTask.FromResult(transaction);
    }

    private static TransactionEventData BroadcastTransactionUsed(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        bool async,
        DateTimeOffset startTime,
        EventDefinition<string> definition,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEventData(
            definition,
            TransactionUsed,
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime);

        if (diagnosticSourceEnabled)
        {
            diagnostics.DiagnosticSource.Write(definition.EventId.Name!, eventData);
        }

        if (simpleLogEnabled)
        {
            diagnostics.DbContextLogger.Log(eventData);
        }

        return eventData;
    }

    private static void LogTransactionUsed(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        DbTransaction transaction,
        EventDefinition<string> definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, transaction.IsolationLevel.ToString("G"));
        }
    }

    private static string TransactionUsed(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string?>)definition;
        var p = (TransactionEventData)payload;
        return d.GenerateMessage(p.Transaction.IsolationLevel.ToString("G"));
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionCommitting" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    public static InterceptionResult TransactionCommitting(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogCommittingTransaction(diagnostics);

        LogTransactionCommitting(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionCommitting(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionCommitting(transaction, eventData, default);
            }
        }

        return default;
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionCommitting" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<InterceptionResult> TransactionCommittingAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogCommittingTransaction(diagnostics);

        LogTransactionCommitting(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionCommitting(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionCommittingAsync(transaction, eventData, default, cancellationToken);
            }
        }

        return default;
    }

    private static TransactionEventData BroadcastTransactionCommitting(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogTransactionCommitting(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionCommitted" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The elapsed time from when the operation was started.</param>
    public static void TransactionCommitted(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        TimeSpan duration)
    {
        var definition = RelationalResources.LogCommittedTransaction(diagnostics);

        LogTransactionCommitted(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionCommitted(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                duration,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            interceptor?.TransactionCommitted(transaction, eventData);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionCommitted" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The elapsed time from when the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task TransactionCommittedAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogCommittedTransaction(diagnostics);

        LogTransactionCommitted(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionCommitted(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                duration,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionCommittedAsync(transaction, eventData, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    private static TransactionEndEventData BroadcastTransactionCommitted(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        TimeSpan duration,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEndEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime,
            duration);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogTransactionCommitted(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionRolledBack" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The elapsed time from when the operation was started.</param>
    public static void TransactionRolledBack(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        TimeSpan duration)
    {
        var definition = RelationalResources.LogRolledBackTransaction(diagnostics);

        LogTransactionRolledBack(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionRolledBack(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                duration,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            interceptor?.TransactionRolledBack(transaction, eventData);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionRolledBack" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The elapsed time from when the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task TransactionRolledBackAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogRolledBackTransaction(diagnostics);

        LogTransactionRolledBack(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionRolledBack(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                duration,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionRolledBackAsync(transaction, eventData, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    private static TransactionEndEventData BroadcastTransactionRolledBack(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        TimeSpan duration,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEndEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime,
            duration);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogTransactionRolledBack(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionRollingBack" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    public static InterceptionResult TransactionRollingBack(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogRollingBackTransaction(diagnostics);

        LogTransactionRollingBack(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionRollingBack(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionRollingBack(transaction, eventData, default);
            }
        }

        return default;
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionRollingBack" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<InterceptionResult> TransactionRollingBackAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogRollingBackTransaction(diagnostics);

        LogTransactionRollingBack(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionRollingBack(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionRollingBackAsync(transaction, eventData, default, cancellationToken);
            }
        }

        return default;
    }

    private static TransactionEventData BroadcastTransactionRollingBack(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogTransactionRollingBack(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.CreatingTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    public static InterceptionResult CreatingTransactionSavepoint(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogCreatingTransactionSavepoint(diagnostics);

        LogCreatingTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastCreatingTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.CreatingSavepoint(transaction, eventData, default);
            }
        }

        return default;
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.CreatingTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<InterceptionResult> CreatingTransactionSavepointAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogCreatingTransactionSavepoint(diagnostics);

        LogCreatingTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastCreatingTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.CreatingSavepointAsync(transaction, eventData, default, cancellationToken);
            }
        }

        return default;
    }

    private static TransactionEventData BroadcastCreatingTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogCreatingTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.CreatedTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    public static void CreatedTransactionSavepoint(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogCreatedTransactionSavepoint(diagnostics);

        LogCreatedTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastCreatedTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            interceptor?.CreatedSavepoint(transaction, eventData);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.CreatedTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task CreatedTransactionSavepointAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogCreatedTransactionSavepoint(diagnostics);

        LogCreatedTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastCreatedTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.CreatedSavepointAsync(transaction, eventData, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    private static TransactionEventData BroadcastCreatedTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogCreatedTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.RollingBackToTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    public static InterceptionResult RollingBackToTransactionSavepoint(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogRollingBackToTransactionSavepoint(diagnostics);

        LogRollingBackToTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastRollingBackToTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.RollingBackToSavepoint(transaction, eventData, default);
            }
        }

        return default;
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.RollingBackToTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<InterceptionResult> RollingBackToTransactionSavepointAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogRollingBackToTransactionSavepoint(diagnostics);

        LogRollingBackToTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastRollingBackToTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.RollingBackToSavepointAsync(transaction, eventData, default, cancellationToken);
            }
        }

        return default;
    }

    private static TransactionEventData BroadcastRollingBackToTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogRollingBackToTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.RolledBackToTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    public static void RolledBackToTransactionSavepoint(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogRolledBackToTransactionSavepoint(diagnostics);

        LogRolledBackToTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastRolledBackToTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            interceptor?.RolledBackToSavepoint(transaction, eventData);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.CreatedTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task RolledBackToTransactionSavepointAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogRolledBackToTransactionSavepoint(diagnostics);

        LogCreatedTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastRolledBackToTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.RolledBackToSavepointAsync(transaction, eventData, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    private static TransactionEventData BroadcastRolledBackToTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogRolledBackToTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.RollingBackToTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <returns>The result of execution, which may have been modified by an interceptor.</returns>
    public static InterceptionResult ReleasingTransactionSavepoint(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogReleasingTransactionSavepoint(diagnostics);

        LogReleasingTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastReleasingTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.ReleasingSavepoint(transaction, eventData, default);
            }
        }

        return default;
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ReleasingTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static ValueTask<InterceptionResult> ReleasingTransactionSavepointAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogReleasingTransactionSavepoint(diagnostics);

        LogReleasingTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastReleasingTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.ReleasingSavepointAsync(transaction, eventData, default, cancellationToken);
            }
        }

        return default;
    }

    private static TransactionEventData BroadcastReleasingTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogReleasingTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ReleasedTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    public static void ReleasedTransactionSavepoint(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogReleasedTransactionSavepoint(diagnostics);

        LogReleasedTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastReleasedTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            interceptor?.ReleasedSavepoint(transaction, eventData);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ReleasedTransactionSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task ReleasedTransactionSavepointAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogReleasedTransactionSavepoint(diagnostics);

        LogReleasedTransactionSavepoint(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastReleasedTransactionSavepoint(
                diagnostics,
                connection,
                transaction,
                transactionId,
                startTime,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.ReleasedSavepointAsync(transaction, eventData, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    private static TransactionEventData BroadcastReleasedTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            startTime);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogReleasedTransactionSavepoint(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionDisposed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    public static void TransactionDisposed(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogDisposingTransaction(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TransactionEventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage(),
                transaction,
                connection.Context,
                transactionId,
                connection.ConnectionId,
                false,
                startTime);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionError" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entityType">The entity type.</param>
    public static void TriggerOnNonRootTphEntity(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IEntityType entityType)
    {
        var definition = RelationalResources.LogTriggerOnNonRootTphEntity(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, entityType.DisplayName(), entityType.GetRootType().DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EntityTypeEventData(
                definition,
                TriggerOnNonRootTphEntity,
                entityType);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string TriggerOnNonRootTphEntity(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var e = (EntityTypeEventData)payload;
        return d.GenerateMessage(e.EntityType.DisplayName(), e.EntityType.GetRootType().DisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionError" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="action">The action being taken.</param>
    /// <param name="exception">The exception that represents the error.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The elapsed time from when the operation was started.</param>
    public static void TransactionError(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        string action,
        Exception exception,
        DateTimeOffset startTime,
        TimeSpan duration)
    {
        var definition = RelationalResources.LogTransactionError(diagnostics);

        LogTransactionError(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionError(
                diagnostics,
                connection,
                transaction,
                transactionId,
                action,
                exception,
                startTime,
                duration,
                definition,
                false,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            interceptor?.TransactionFailed(transaction, eventData);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.TransactionError" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="transactionId">The correlation ID associated with the <see cref="DbTransaction" />.</param>
    /// <param name="action">The action being taken.</param>
    /// <param name="exception">The exception that represents the error.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    /// <param name="duration">The elapsed time from when the operation was started.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the async operation.</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task TransactionErrorAsync(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        string action,
        Exception exception,
        DateTimeOffset startTime,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogTransactionError(diagnostics);

        LogTransactionError(diagnostics, definition);

        if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastTransactionError(
                diagnostics,
                connection,
                transaction,
                transactionId,
                action,
                exception,
                startTime,
                duration,
                definition,
                true,
                diagnosticSourceEnabled,
                simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.TransactionFailedAsync(transaction, eventData, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    private static TransactionErrorEventData BroadcastTransactionError(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        string action,
        Exception exception,
        DateTimeOffset startTime,
        TimeSpan duration,
        EventDefinition definition,
        bool async,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled)
    {
        var eventData = new TransactionErrorEventData(
            definition,
            (d, _) => ((EventDefinition)d).GenerateMessage(),
            transaction,
            connection.Context,
            transactionId,
            connection.ConnectionId,
            async,
            action,
            exception,
            startTime,
            duration);

        diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;
    }

    private static void LogTransactionError(
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        EventDefinition definition)
    {
        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.AmbientTransactionWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="startTime">The time that the operation was started.</param>
    public static void AmbientTransactionWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        DateTimeOffset startTime)
    {
        var definition = RelationalResources.LogAmbientTransaction(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ConnectionEventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage(),
                connection.DbConnection,
                connection.Context,
                connection.ConnectionId,
                false,
                startTime);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.AmbientTransactionEnlisted" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    public static void AmbientTransactionEnlisted(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        Transaction transaction)
    {
        var definition = RelationalResources.LogAmbientTransactionEnlisted(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, transaction.IsolationLevel.ToString("G"));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TransactionEnlistedEventData(
                definition,
                AmbientTransactionEnlisted,
                transaction,
                connection.DbConnection,
                connection.ConnectionId);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string AmbientTransactionEnlisted(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (TransactionEnlistedEventData)payload;
        return d.GenerateMessage(p.Transaction.IsolationLevel.ToString("G"));
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ExplicitTransactionEnlisted" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="transaction">The transaction.</param>
    public static void ExplicitTransactionEnlisted(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
        IRelationalConnection connection,
        Transaction transaction)
    {
        var definition = RelationalResources.LogExplicitTransactionEnlisted(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, transaction.IsolationLevel.ToString("G"));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TransactionEnlistedEventData(
                definition,
                ExplicitTransactionEnlisted,
                transaction,
                connection.DbConnection,
                connection.ConnectionId);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExplicitTransactionEnlisted(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (TransactionEnlistedEventData)payload;
        return d.GenerateMessage(p.Transaction.IsolationLevel.ToString("G"));
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.MigrateUsingConnection" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="migrator">The migrator.</param>
    /// <param name="connection">The connection.</param>
    public static void MigrateUsingConnection(
        this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
        IMigrator migrator,
        IRelationalConnection connection)
    {
        var definition = RelationalResources.LogMigrating(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var dbConnection = connection.DbConnection;

            definition.Log(diagnostics, dbConnection.Database, dbConnection.DataSource);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new MigratorConnectionEventData(
                definition,
                MigrateUsingConnection,
                migrator,
                connection.DbConnection,
                connection.ConnectionId);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MigrateUsingConnection(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (MigratorConnectionEventData)payload;
        return d.GenerateMessage(
            p.Connection.Database,
            p.Connection.DataSource);
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.MigrationReverting" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="migrator">The migrator.</param>
    /// <param name="migration">The migration.</param>
    public static void MigrationReverting(
        this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
        IMigrator migrator,
        Migration migration)
    {
        var definition = RelationalResources.LogRevertingMigration(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, migration.GetId());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new MigrationEventData(
                definition,
                MigrationReverting,
                migrator,
                migration);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MigrationReverting(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (MigrationEventData)payload;
        return d.GenerateMessage(p.Migration.GetId());
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.MigrationApplying" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="migrator">The migrator.</param>
    /// <param name="migration">The migration.</param>
    public static void MigrationApplying(
        this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
        IMigrator migrator,
        Migration migration)
    {
        var definition = RelationalResources.LogApplyingMigration(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, migration.GetId());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new MigrationEventData(
                definition,
                MigrationApplying,
                migrator,
                migration);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MigrationApplying(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (MigrationEventData)payload;
        return d.GenerateMessage(p.Migration.GetId());
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.MigrationGeneratingDownScript" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="migrator">The migrator.</param>
    /// <param name="migration">The migration.</param>
    /// <param name="fromMigration">The starting migration name.</param>
    /// <param name="toMigration">The ending migration name.</param>
    /// <param name="idempotent">Indicates whether or not an idempotent script is being generated.</param>
    public static void MigrationGeneratingDownScript(
        this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
        IMigrator migrator,
        Migration migration,
        string? fromMigration,
        string? toMigration,
        bool idempotent)
    {
        var definition = RelationalResources.LogGeneratingDown(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, migration.GetId());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new MigrationScriptingEventData(
                definition,
                MigrationGeneratingDownScript,
                migrator,
                migration,
                fromMigration,
                toMigration,
                idempotent);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MigrationGeneratingDownScript(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (MigrationScriptingEventData)payload;
        return d.GenerateMessage(p.Migration.GetId());
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.MigrationGeneratingUpScript" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="migrator">The migrator.</param>
    /// <param name="migration">The migration.</param>
    /// <param name="fromMigration">The starting migration name.</param>
    /// <param name="toMigration">The ending migration name.</param>
    /// <param name="idempotent">Indicates whether or not an idempotent script is being generated.</param>
    public static void MigrationGeneratingUpScript(
        this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
        IMigrator migrator,
        Migration migration,
        string? fromMigration,
        string? toMigration,
        bool idempotent)
    {
        var definition = RelationalResources.LogGeneratingUp(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, migration.GetId());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new MigrationScriptingEventData(
                definition,
                MigrationGeneratingUpScript,
                migrator,
                migration,
                fromMigration,
                toMigration,
                idempotent);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MigrationGeneratingUpScript(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (MigrationScriptingEventData)payload;
        return d.GenerateMessage(p.Migration.GetId());
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.MigrationsNotApplied" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="migrator">The migrator.</param>
    public static void MigrationsNotApplied(
        this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
        IMigrator migrator)
    {
        var definition = RelationalResources.LogNoMigrationsApplied(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new MigratorEventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage(),
                migrator);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.MigrationsNotFound" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="migrator">The migrator.</param>
    /// <param name="migrationsAssembly">The assembly in which migrations are stored.</param>
    public static void MigrationsNotFound(
        this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
        IMigrator migrator,
        IMigrationsAssembly migrationsAssembly)
    {
        var definition = RelationalResources.LogNoMigrationsFound(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, migrationsAssembly.Assembly.GetName().Name!);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new MigrationAssemblyEventData(
                definition,
                MigrationsNotFound,
                migrator,
                migrationsAssembly);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MigrationsNotFound(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (MigrationAssemblyEventData)payload;
        return d.GenerateMessage(p.MigrationsAssembly.Assembly.GetName().Name!);
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.MigrationAttributeMissingWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="migrationType">Info for the migration type.</param>
    public static void MigrationAttributeMissingWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
        TypeInfo migrationType)
    {
        var definition = RelationalResources.LogMigrationAttributeMissingWarning(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, migrationType.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new MigrationTypeEventData(
                definition,
                MigrationAttributeMissingWarning,
                migrationType);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string MigrationAttributeMissingWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (MigrationTypeEventData)payload;
        return d.GenerateMessage(p.MigrationType.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="left">The left SQL expression of the Equals.</param>
    /// <param name="right">The right SQL expression of the Equals.</param>
    public static void QueryPossibleUnintendedUseOfEqualsWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        SqlExpression left,
        SqlExpression right)
    {
        var definition = RelationalResources.LogPossibleUnintendedUseOfEquals(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, left.Print(), right.Print());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new TwoSqlExpressionsEventData(
                definition,
                QueryPossibleUnintendedUseOfEqualsWarning,
                left,
                right);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string QueryPossibleUnintendedUseOfEqualsWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (TwoSqlExpressionsEventData)payload;
        return d.GenerateMessage(p.Left.Print(), p.Right.Print());
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ExecuteDeleteFailed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="contextType">The <see cref="DbContext" /> type being used.</param>
    /// <param name="exception">The exception that caused this failure.</param>
    public static void ExecuteDeleteFailed(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        Type contextType,
        Exception exception)
    {
        var definition = RelationalResources.LogExceptionDuringExecuteDelete(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                contextType, Environment.NewLine, exception,
                exception);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextTypeErrorEventData(
                definition,
                ExecuteDeleteFailed,
                contextType,
                exception);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExecuteDeleteFailed(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<Type, string, Exception>)definition;
        var p = (DbContextTypeErrorEventData)payload;
        return d.GenerateMessage(p.ContextType, Environment.NewLine, p.Exception);
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ExecuteUpdateFailed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="contextType">The <see cref="DbContext" /> type being used.</param>
    /// <param name="exception">The exception that caused this failure.</param>
    public static void ExecuteUpdateFailed(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        Type contextType,
        Exception exception)
    {
        var definition = RelationalResources.LogExceptionDuringExecuteUpdate(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                contextType, Environment.NewLine, exception,
                exception);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextTypeErrorEventData(
                definition,
                ExecuteUpdateFailed,
                contextType,
                exception);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExecuteUpdateFailed(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<Type, string, Exception>)definition;
        var p = (DbContextTypeErrorEventData)payload;
        return d.GenerateMessage(p.ContextType, Environment.NewLine, p.Exception);
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.NonQueryOperationFailed" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="contextType">The <see cref="DbContext" /> type being used.</param>
    /// <param name="exception">The exception that caused this failure.</param>
    public static void NonQueryOperationFailed(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        Type contextType,
        Exception exception)
    {
        var definition = RelationalResources.LogExceptionDuringNonQueryOperation(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                contextType, Environment.NewLine, exception,
                exception);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextTypeErrorEventData(
                definition,
                NonQueryOperationFailed,
                contextType,
                exception);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string NonQueryOperationFailed(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<Type, string, Exception>)definition;
        var p = (DbContextTypeErrorEventData)payload;
        return d.GenerateMessage(p.ContextType, Environment.NewLine, p.Exception);
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.MultipleCollectionIncludeWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    public static void MultipleCollectionIncludeWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics)
    {
        var definition = RelationalResources.LogMultipleCollectionIncludeWarning(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.ModelValidationKeyDefaultValueWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="property">The property.</param>
    public static void ModelValidationKeyDefaultValueWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IProperty property)
    {
        var definition = RelationalResources.LogKeyHasDefaultValue(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, property.Name, property.DeclaringType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyEventData(
                definition,
                ModelValidationKeyDefaultValueWarning,
                property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ModelValidationKeyDefaultValueWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyEventData)payload;
        return d.GenerateMessage(
            p.Property.Name,
            p.Property.DeclaringType.DisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.BoolWithDefaultWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="property">The property.</param>
    public static void BoolWithDefaultWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IProperty property)
    {
        var definition = RelationalResources.LogBoolWithDefaultWarning(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var defaultValue = property.ClrType.GetDefaultValue();
            definition.Log(
                diagnostics,
                property.ClrType.ShortDisplayName(),
                property.Name,
                property.DeclaringType.DisplayName(),
                defaultValue == null ? "null" : defaultValue.ToString()!,
                property.ClrType.ShortDisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyEventData(
                definition,
                BoolWithDefaultWarning,
                property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string BoolWithDefaultWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, string, string>)definition;
        var p = (PropertyEventData)payload;
        var defaultValue = p.Property.ClrType.GetDefaultValue();
        return d.GenerateMessage(
            p.Property.ClrType.ShortDisplayName(),
            p.Property.Name,
            p.Property.DeclaringType.DisplayName(),
            defaultValue == null ? "null" : defaultValue.ToString()!,
            p.Property.ClrType.ShortDisplayName());
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.BatchReadyForExecution" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entries">The entries for entities in the batch.</param>
    /// <param name="commandCount">The number of commands.</param>
    public static void BatchReadyForExecution(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        IEnumerable<IUpdateEntry> entries,
        int commandCount)
    {
        var definition = RelationalResources.LogBatchReadyForExecution(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, commandCount);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new BatchEventData(
                definition,
                BatchReadyForExecution,
                entries,
                commandCount);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string BatchReadyForExecution(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<int>)definition;
        var p = (BatchEventData)payload;
        return d.GenerateMessage(p.CommandCount);
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.BatchSmallerThanMinBatchSize" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entries">The entries for entities in the batch.</param>
    /// <param name="commandCount">The number of commands.</param>
    /// <param name="minBatchSize">The minimum batch size.</param>
    public static void BatchSmallerThanMinBatchSize(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        IEnumerable<IUpdateEntry> entries,
        int commandCount,
        int minBatchSize)
    {
        var definition = RelationalResources.LogBatchSmallerThanMinBatchSize(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, commandCount, minBatchSize);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new MinBatchSizeEventData(
                definition,
                BatchSmallerThanMinBatchSize,
                entries,
                commandCount,
                minBatchSize);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string BatchSmallerThanMinBatchSize(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<int, int>)definition;
        var p = (MinBatchSizeEventData)payload;
        return d.GenerateMessage(p.CommandCount, p.MinBatchSize);
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.AllIndexPropertiesNotToMappedToAnyTable" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entityType">The entity type on which the index is defined.</param>
    /// <param name="index">The index on the entity type.</param>
    public static void AllIndexPropertiesNotToMappedToAnyTable(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IEntityType entityType,
        IIndex index)
    {
        if (index.Name == null)
        {
            var definition = RelationalResources.LogUnnamedIndexAllPropertiesNotToMappedToAnyTable(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    entityType.DisplayName(),
                    index.Properties.Format());
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new IndexEventData(
                    definition,
                    UnnamedIndexAllPropertiesNotToMappedToAnyTable,
                    entityType,
                    null,
                    index.Properties.Select(p => p.Name).ToList());

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }
        else
        {
            var definition = RelationalResources.LogNamedIndexAllPropertiesNotToMappedToAnyTable(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    index.Name,
                    entityType.DisplayName(),
                    index.Properties.Format());
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new IndexEventData(
                    definition,
                    NamedIndexAllPropertiesNotToMappedToAnyTable,
                    entityType,
                    index.Name,
                    index.Properties.Select(p => p.Name).ToList());

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }
    }

    private static string UnnamedIndexAllPropertiesNotToMappedToAnyTable(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (IndexEventData)payload;
        return d.GenerateMessage(
            p.EntityType.DisplayName(),
            p.PropertyNames.Format());
    }

    private static string NamedIndexAllPropertiesNotToMappedToAnyTable(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string?, string, string>)definition;
        var p = (IndexEventData)payload;
        return d.GenerateMessage(
            p.Name,
            p.EntityType.DisplayName(),
            p.PropertyNames.Format());
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.IndexPropertiesBothMappedAndNotMappedToTable" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entityType">The entity type on which the index is defined.</param>
    /// <param name="index">The index on the entity type.</param>
    /// <param name="unmappedPropertyName">The name of the property which is not mapped.</param>
    public static void IndexPropertiesBothMappedAndNotMappedToTable(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IEntityType entityType,
        IIndex index,
        string unmappedPropertyName)
    {
        if (index.Name == null)
        {
            var definition = RelationalResources.LogUnnamedIndexPropertiesBothMappedAndNotMappedToTable(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    entityType.DisplayName(),
                    index.Properties.Format(),
                    unmappedPropertyName);
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new IndexWithPropertyEventData(
                    definition,
                    UnnamedIndexPropertiesBothMappedAndNotMappedToTable,
                    entityType,
                    null,
                    index.Properties.Select(p => p.Name).ToList(),
                    unmappedPropertyName);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }
        else
        {
            var definition = RelationalResources.LogNamedIndexPropertiesBothMappedAndNotMappedToTable(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    index.Name,
                    entityType.DisplayName(),
                    index.Properties.Format(),
                    unmappedPropertyName);
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new IndexWithPropertyEventData(
                    definition,
                    NamedIndexPropertiesBothMappedAndNotMappedToTable,
                    entityType,
                    index.Name,
                    index.Properties.Select(p => p.Name).ToList(),
                    unmappedPropertyName);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }
    }

    private static string UnnamedIndexPropertiesBothMappedAndNotMappedToTable(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string>)definition;
        var p = (IndexWithPropertyEventData)payload;
        return d.GenerateMessage(
            p.EntityType.DisplayName(),
            p.PropertyNames.Format(),
            p.PropertyName);
    }

    private static string NamedIndexPropertiesBothMappedAndNotMappedToTable(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string?, string, string, string>)definition;
        var p = (IndexWithPropertyEventData)payload;
        return d.GenerateMessage(
            p.Name,
            p.EntityType.DisplayName(),
            p.PropertyNames.Format(),
            p.PropertyName);
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.IndexPropertiesMappedToNonOverlappingTables" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entityType">The entity type on which the index is defined.</param>
    /// <param name="index">The index on the entity type.</param>
    /// <param name="property1Name">The first property name which is invalid.</param>
    /// <param name="tablesMappedToProperty1">The tables mapped to the first property.</param>
    /// <param name="property2Name">The second property name which is invalid.</param>
    /// <param name="tablesMappedToProperty2">The tables mapped to the second property.</param>
    public static void IndexPropertiesMappedToNonOverlappingTables(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IEntityType entityType,
        IIndex index,
        string property1Name,
        List<(string Table, string? Schema)> tablesMappedToProperty1,
        string property2Name,
        List<(string Table, string? Schema)> tablesMappedToProperty2)
    {
        if (index.Name == null)
        {
            var definition = RelationalResources.LogUnnamedIndexPropertiesMappedToNonOverlappingTables(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    entityType.DisplayName(),
                    index.Properties.Format(),
                    property1Name,
                    tablesMappedToProperty1.FormatTables(),
                    property2Name,
                    tablesMappedToProperty2.FormatTables());
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new IndexWithPropertiesEventData(
                    definition,
                    UnnamedIndexPropertiesMappedToNonOverlappingTables,
                    entityType,
                    null,
                    index.Properties.Select(p => p.Name).ToList(),
                    property1Name,
                    tablesMappedToProperty1,
                    property2Name,
                    tablesMappedToProperty2);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }
        else
        {
            var definition = RelationalResources.LogNamedIndexPropertiesMappedToNonOverlappingTables(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    l => l.Log(
                        definition.Level,
                        definition.EventId,
                        definition.MessageFormat,
                        index.Name,
                        entityType.DisplayName(),
                        index.Properties.Format(),
                        property1Name,
                        tablesMappedToProperty1.FormatTables(),
                        property2Name,
                        tablesMappedToProperty2.FormatTables()));
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new IndexWithPropertiesEventData(
                    definition,
                    NamedIndexPropertiesMappedToNonOverlappingTables,
                    entityType,
                    index.Name,
                    index.Properties.Select(p => p.Name).ToList(),
                    property1Name,
                    tablesMappedToProperty1,
                    property2Name,
                    tablesMappedToProperty2);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }
    }

    private static string UnnamedIndexPropertiesMappedToNonOverlappingTables(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, string, string, string>)definition;
        var p = (IndexWithPropertiesEventData)payload;
        return d.GenerateMessage(
            p.EntityType.DisplayName(),
            p.PropertyNames.Format(),
            p.Property1Name,
            p.TablesMappedToProperty1.FormatTables(),
            p.Property2Name,
            p.TablesMappedToProperty2.FormatTables());
    }

    private static string NamedIndexPropertiesMappedToNonOverlappingTables(EventDefinitionBase definition, EventData payload)
    {
        var d = (FallbackEventDefinition)definition;
        var p = (IndexWithPropertiesEventData)payload;
        return d.GenerateMessage(
            l => l.Log(
                d.Level,
                d.EventId,
                d.MessageFormat,
                p.Name,
                p.EntityType.DisplayName(),
                p.PropertyNames.Format(),
                p.Property1Name,
                p.TablesMappedToProperty1.FormatTables(),
                p.Property2Name,
                p.TablesMappedToProperty2.FormatTables()));
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.KeyPropertiesNotMappedToTable" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="key">The foreign key.</param>
    public static void KeyPropertiesNotMappedToTable(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IKey key)
    {
        var definition = RelationalResources.LogKeyPropertiesNotMappedToTable(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                key.Properties.Format(),
                key.DeclaringEntityType.DisplayName(),
                key.DeclaringEntityType.GetSchemaQualifiedTableName()!);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new KeyEventData(
                definition,
                KeyPropertiesNotMappedToTable,
                key);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string KeyPropertiesNotMappedToTable(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string>)definition;
        var p = (KeyEventData)payload;
        return d.GenerateMessage(
            p.Key.Properties.Format(),
            p.Key.DeclaringEntityType.DisplayName(),
            p.Key.DeclaringEntityType.GetSchemaQualifiedTableName()!);
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="foreignKey">The foreign key.</param>
    public static void ForeignKeyPropertiesMappedToUnrelatedTables(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IForeignKey foreignKey)
    {
        var definition = RelationalResources.LogForeignKeyPropertiesMappedToUnrelatedTables(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                l => l.Log(
                    definition.Level,
                    definition.EventId,
                    definition.MessageFormat,
                    foreignKey.Properties.Format(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName(),
                    foreignKey.Properties.Format(),
                    foreignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                    foreignKey.PrincipalKey.Properties.Format(),
                    foreignKey.PrincipalEntityType.GetSchemaQualifiedTableName()));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ForeignKeyEventData(
                definition,
                ForeignKeyPropertiesMappedToUnrelatedTables,
                foreignKey);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ForeignKeyPropertiesMappedToUnrelatedTables(EventDefinitionBase definition, EventData payload)
    {
        var d = (FallbackEventDefinition)definition;
        var p = (ForeignKeyEventData)payload;
        return d.GenerateMessage(
            l => l.Log(
                d.Level,
                d.EventId,
                d.MessageFormat,
                p.ForeignKey.Properties.Format(),
                p.ForeignKey.DeclaringEntityType.DisplayName(),
                p.ForeignKey.PrincipalEntityType.DisplayName(),
                p.ForeignKey.Properties.Format(),
                p.ForeignKey.DeclaringEntityType.GetSchemaQualifiedTableName(),
                p.ForeignKey.PrincipalKey.Properties.Format(),
                p.ForeignKey.PrincipalEntityType.GetSchemaQualifiedTableName()));
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.ForeignKeyTpcPrincipalWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="foreignKey">The foreign key.</param>
    public static void ForeignKeyTpcPrincipalWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IForeignKey foreignKey)
    {
        var definition = RelationalResources.LogForeignKeyTpcPrincipal(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                l => l.Log(
                    definition.Level,
                    definition.EventId,
                    definition.MessageFormat,
                    foreignKey.Properties.Format(),
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.GetSchemaQualifiedTableName()!,
                    foreignKey.DeclaringEntityType.DisplayName(),
                    foreignKey.PrincipalEntityType.DisplayName()));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ForeignKeyEventData(
                definition,
                ForeignKeyTPCPrincipal,
                foreignKey);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ForeignKeyTPCPrincipal(EventDefinitionBase definition, EventData payload)
    {
        var d = (FallbackEventDefinition)definition;
        var p = (ForeignKeyEventData)payload;
        return d.GenerateMessage(
            l => l.Log(
                d.Level,
                d.EventId,
                d.MessageFormat,
                p.ForeignKey.Properties.Format(),
                p.ForeignKey.DeclaringEntityType.DisplayName(),
                p.ForeignKey.PrincipalEntityType.DisplayName(),
                p.ForeignKey.PrincipalEntityType.GetSchemaQualifiedTableName()!,
                p.ForeignKey.PrincipalEntityType.DisplayName(),
                p.ForeignKey.DeclaringEntityType.DisplayName(),
                p.ForeignKey.PrincipalEntityType.DisplayName()));
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.TpcStoreGeneratedIdentityWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="property">The entity type on which the index is defined.</param>
    public static void TpcStoreGeneratedIdentityWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IProperty property)
    {
        var definition = RelationalResources.LogTpcStoreGeneratedIdentity(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics,
                property.DeclaringType.DisplayName(),
                property.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new PropertyEventData(
                definition,
                TpcStoreGeneratedIdentity,
                property);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string TpcStoreGeneratedIdentity(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (PropertyEventData)payload;
        return d.GenerateMessage(
            p.Property.DeclaringType.DisplayName(),
            p.Property.Name);
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.OptionalDependentWithoutIdentifyingPropertyWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entityType">The entity type.</param>
    public static void OptionalDependentWithoutIdentifyingPropertyWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IEntityType entityType)
    {
        var definition = RelationalResources.LogOptionalDependentWithoutIdentifyingProperty(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, entityType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EntityTypeEventData(
                definition,
                OptionalDependentWithoutIdentifyingPropertyWarning,
                entityType);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string OptionalDependentWithoutIdentifyingPropertyWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (EntityTypeEventData)payload;
        return d.GenerateMessage(p.EntityType.DisplayName());
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.StoredProcedureConcurrencyTokenNotMapped" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entityType">The entity type that the stored procedure is mapped to.</param>
    /// <param name="concurrencyProperty">The property which represents the concurrency token.</param>
    /// <param name="storedProcedureName">The stored procedure name.</param>
    public static void StoredProcedureConcurrencyTokenNotMapped(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        IEntityType entityType,
        IProperty concurrencyProperty,
        string storedProcedureName)
    {
        var definition = RelationalResources.LogStoredProcedureConcurrencyTokenNotMapped(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, entityType.DisplayName(), storedProcedureName, concurrencyProperty.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new StoredProcedurePropertyEventData(
                definition,
                StoredProcedureConcurrencyTokenNotMapped,
                entityType,
                concurrencyProperty,
                storedProcedureName);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string StoredProcedureConcurrencyTokenNotMapped(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string>)definition;
        var p = (StoredProcedurePropertyEventData)payload;
        return d.GenerateMessage(p.EntityType.DisplayName(), p.StoredProcedureName, p.Property.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.BatchExecutorFailedToRollbackToSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="contextType">The <see cref="DbContext" /> type being used.</param>
    /// <param name="exception">The exception that caused this failure.</param>
    public static void BatchExecutorFailedToRollbackToSavepoint(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        Type contextType,
        Exception exception)
    {
        var definition = RelationalResources.LogBatchExecutorFailedToRollbackToSavepoint(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextTypeErrorEventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage(),
                contextType,
                exception);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.BatchExecutorFailedToReleaseSavepoint" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="contextType">The <see cref="DbContext" /> type being used.</param>
    /// <param name="exception">The exception that caused this failure.</param>
    public static void BatchExecutorFailedToReleaseSavepoint(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        Type contextType,
        Exception exception)
    {
        var definition = RelationalResources.LogBatchExecutorFailedToReleaseSavepoint(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new DbContextTypeErrorEventData(
                definition,
                (d, _) => ((EventDefinition)d).GenerateMessage(),
                contextType,
                exception);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.OptionalDependentWithAllNullPropertiesWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entry">The entry.</param>
    public static void OptionalDependentWithAllNullPropertiesWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        IUpdateEntry entry)
    {
        var definition = RelationalResources.LogOptionalDependentWithAllNullProperties(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, entry.EntityType.DisplayName());
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new UpdateEntryEventData(
                definition,
                OptionalDependentWithAllNullPropertiesWarning,
                entry);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string OptionalDependentWithAllNullPropertiesWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string>)definition;
        var p = (UpdateEntryEventData)payload;
        return d.GenerateMessage(p.EntityEntry.EntityType.DisplayName());
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.OptionalDependentWithAllNullPropertiesWarning" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="entry">The entry.</param>
    public static void OptionalDependentWithAllNullPropertiesWarningSensitive(
        this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
        IUpdateEntry entry)
    {
        var definition = RelationalResources.LogOptionalDependentWithAllNullPropertiesSensitive(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(
                diagnostics, entry.EntityType.DisplayName(),
                entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new UpdateEntryEventData(
                definition,
                OptionalDependentWithAllNullPropertiesWarningSensitive,
                entry
            );

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string OptionalDependentWithAllNullPropertiesWarningSensitive(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (UpdateEntryEventData)payload;
        return d.GenerateMessage(
            p.EntityEntry.EntityType.DisplayName(),
            p.EntityEntry.BuildCurrentValuesString(p.EntityEntry.EntityType.FindPrimaryKey()!.Properties));
    }

    /// <summary>
    ///     Logs the <see cref="RelationalEventId.DuplicateColumnOrders" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    /// <param name="storeObject">The table.</param>
    /// <param name="columns">The columns.</param>
    public static void DuplicateColumnOrders(
        this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
        StoreObjectIdentifier storeObject,
        IReadOnlyList<string> columns)
    {
        var definition = RelationalResources.LogDuplicateColumnOrders(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, storeObject.DisplayName(), string.Join(", ", columns.Select(c => "'" + c + "'")));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new ColumnsEventData(
                definition,
                DuplicateColumnOrders,
                storeObject,
                columns);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string DuplicateColumnOrders(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (ColumnsEventData)payload;

        return d.GenerateMessage(p.StoreObject.DisplayName(), string.Join(", ", p.Columns.Select(c => "'" + c + "'")));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ColumnOrderIgnoredWarning(
        this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
        ColumnOperation operation)
    {
        var definition = RelationalResources.LogColumnOrderIgnoredWarning(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics, (operation.Table, operation.Schema).FormatTable(), operation.Name);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new MigrationColumnOperationEventData(
                definition,
                ColumnOrderIgnoredWarning,
                operation);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ColumnOrderIgnoredWarning(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (MigrationColumnOperationEventData)payload;
        return d.GenerateMessage((p.ColumnOperation.Table, p.ColumnOperation.Schema).FormatTable(), p.ColumnOperation.Name);
    }

    /// <summary>
    ///     Logs for the <see cref="RelationalEventId.UnexpectedTrailingResultSetWhenSaving" /> event.
    /// </summary>
    /// <param name="diagnostics">The diagnostics logger to use.</param>
    public static void UnexpectedTrailingResultSetWhenSaving(this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics)
    {
        var definition = RelationalResources.LogUnexpectedTrailingResultSetWhenSaving(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EventData(
                definition,
                static (definition, _) =>
                {
                    var d = (EventDefinition)definition;
                    return d.GenerateMessage();
                });

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }
}
