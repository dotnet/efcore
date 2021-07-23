// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     An <see cref="IDiagnosticsLogger{ConnectionCategory}" /> with some extra functionality suited for high-performance logging.
    /// </summary>
    public interface IRelationalCommandDiagnosticsLogger : IDiagnosticsLogger<DbLoggerCategory.Database.Command>
    {
        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandCreating" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="commandMethod"> The type of method that will be called on this command. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <returns> An intercepted result. </returns>
        InterceptionResult<DbCommand> CommandCreating(
            IRelationalConnection connection,
            DbCommandMethod commandMethod,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandCreated" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="commandMethod"> The type of method that will be called on this command. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command creation. </param>
        /// <returns> An intercepted result. </returns>
        DbCommand CommandCreated(
            IRelationalConnection connection,
            DbCommand command,
            DbCommandMethod commandMethod,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            TimeSpan duration);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <returns> An intercepted result. </returns>
        InterceptionResult<DbDataReader> CommandReaderExecuting(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <returns> An intercepted result. </returns>
        InterceptionResult<object> CommandScalarExecuting(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <returns> An intercepted result. </returns>
        InterceptionResult<int> CommandNonQueryExecuting(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> An intercepted result. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        ValueTask<InterceptionResult<DbDataReader>> CommandReaderExecutingAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> An intercepted result. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        ValueTask<InterceptionResult<object>> CommandScalarExecutingAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> An intercepted result. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        ValueTask<InterceptionResult<int>> CommandNonQueryExecutingAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        DbDataReader CommandReaderExecuted(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DbDataReader methodResult,
            DateTimeOffset startTime,
            TimeSpan duration);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        object? CommandScalarExecuted(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            object? methodResult,
            DateTimeOffset startTime,
            TimeSpan duration);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        int CommandNonQueryExecuted(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            int methodResult,
            DateTimeOffset startTime,
            TimeSpan duration);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        ValueTask<DbDataReader> CommandReaderExecutedAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DbDataReader methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        ValueTask<object?> CommandScalarExecutedAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            object? methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        ValueTask<int> CommandNonQueryExecutedAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            int methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandError" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="executeMethod"> Represents the method that will be called to execute the command. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="exception"> The exception that caused this failure. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The amount of time that passed until the exception was raised. </param>
        void CommandError(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandError" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="executeMethod"> Represents the method that will be called to execute the command. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="exception"> The exception that caused this failure. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The amount of time that passed until the exception was raised. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        Task CommandErrorAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.DataReaderDisposing" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="dataReader"> The data reader. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="recordsAffected"> The number of records in the database that were affected. </param>
        /// <param name="readCount"> The number of records that were read. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The elapsed time from when the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        InterceptionResult DataReaderDisposing(
            IRelationalConnection connection,
            DbCommand command,
            DbDataReader dataReader,
            Guid commandId,
            int recordsAffected,
            int readCount,
            DateTimeOffset startTime,
            TimeSpan duration);

        /// <summary>
        ///     Whether <see cref="RelationalEventId.CommandCreating" /> or <see cref="RelationalEventId.CommandCreated" /> need
        ///     to be logged.
        /// </summary>
        bool ShouldLogCommandCreate(DateTimeOffset now);

        /// <summary>
        ///     Whether <see cref="RelationalEventId.CommandExecuting" /> or <see cref="RelationalEventId.CommandExecuted" /> need
        ///     to be logged.
        /// </summary>
        bool ShouldLogCommandExecute(DateTimeOffset now);

        /// <summary>
        ///     Whether <see cref="RelationalEventId.DataReaderDisposing" /> needs to be logged.
        /// </summary>
        bool ShouldLogDataReaderDispose(DateTimeOffset now);

        private bool ShouldLogParameterValues(DbCommand command)
            => command.Parameters.Count > 0 && ShouldLogSensitiveData();
    }
}
