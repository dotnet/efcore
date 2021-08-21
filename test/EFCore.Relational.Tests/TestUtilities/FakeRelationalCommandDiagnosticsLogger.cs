// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

#nullable enable

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class FakeRelationalCommandDiagnosticsLogger
        : FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>, IRelationalCommandDiagnosticsLogger
    {
        public InterceptionResult<DbCommand> CommandCreating(
            IRelationalConnection connection,
            DbCommandMethod commandMethod,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CommandSource commandSource)
            => default;

        public DbCommand CommandCreated(
            IRelationalConnection connection,
            DbCommand command,
            DbCommandMethod commandMethod,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            TimeSpan duration,
            CommandSource commandSource)
            => command;

        public InterceptionResult<DbDataReader> CommandReaderExecuting(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CommandSource commandSource)
            => default;

        public InterceptionResult<object> CommandScalarExecuting(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CommandSource commandSource)
            => default;

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="System.Data.Common.DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="System.Data.Common.DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="commandSource"> Source of the command. </param>
        /// <returns> An intercepted result. </returns>
        public InterceptionResult<int> CommandNonQueryExecuting(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CommandSource commandSource)
            => default;

        public ValueTask<InterceptionResult<DbDataReader>> CommandReaderExecutingAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CommandSource commandSource,
            CancellationToken cancellationToken = default)
            => default;

        public ValueTask<InterceptionResult<object>> CommandScalarExecutingAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CommandSource commandSource,
            CancellationToken cancellationToken = default)
            => default;

        public ValueTask<InterceptionResult<int>> CommandNonQueryExecutingAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CommandSource commandSource,
            CancellationToken cancellationToken = default)
            => default;

        public DbDataReader CommandReaderExecuted(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DbDataReader methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CommandSource commandSource)
            => methodResult;

        public object? CommandScalarExecuted(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            object? methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CommandSource commandSource)
            => methodResult;

        public int CommandNonQueryExecuted(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            int methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CommandSource commandSource)
            => methodResult;

        public ValueTask<DbDataReader> CommandReaderExecutedAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            DbDataReader methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CommandSource commandSource,
            CancellationToken cancellationToken = default)
            => new(methodResult);

        public ValueTask<object?> CommandScalarExecutedAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            object? methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CommandSource commandSource,
            CancellationToken cancellationToken = default)
            => new(methodResult);

        public ValueTask<int> CommandNonQueryExecutedAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            Guid commandId,
            Guid connectionId,
            int methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CommandSource commandSource,
            CancellationToken cancellationToken = default)
            => new(methodResult);

        public void CommandError(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            CommandSource commandSource)
        {
        }

        public Task CommandErrorAsync(
            IRelationalConnection connection,
            DbCommand command,
            DbContext? context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            CommandSource commandSource,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public InterceptionResult DataReaderDisposing(
            IRelationalConnection connection,
            DbCommand command,
            DbDataReader dataReader,
            Guid commandId,
            int recordsAffected,
            int readCount,
            DateTimeOffset startTime,
            TimeSpan duration)
            => default;

        public bool ShouldLogCommandCreate(DateTimeOffset now)
            => true;

        public bool ShouldLogCommandExecute(DateTimeOffset now)
            => true;

        public bool ShouldLogDataReaderDispose(DateTimeOffset now)
            => true;
    }
}
