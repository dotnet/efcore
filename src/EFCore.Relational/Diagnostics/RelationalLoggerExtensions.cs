// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using IsolationLevel = System.Data.IsolationLevel;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
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
    public static class RelationalLoggerExtensions
    {
        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandCreating" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="commandMethod"> The type of method that will be called on this command. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <returns> An intercepted result. </returns>
        public static InterceptionResult<DbCommand> CommandCreating(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            DbCommandMethod commandMethod,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime)
        {
            var definition = RelationalResources.LogCommandCreating(diagnostics);

            LogCommandCreating(diagnostics, definition, commandMethod);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandCreating(
                    diagnostics,
                    connection.DbConnection,
                    context,
                    commandMethod,
                    commandId,
                    connectionId,
                    false,
                    startTime,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.CommandCreating(eventData, default);
                }
            }

            return default;
        }

        private static CommandCorrelatedEventData BroadcastCommandCreating(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            DbConnection connection,
            DbContext context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime,
            EventDefinition<string> definition,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            var eventData = new CommandCorrelatedEventData(
                definition,
                CommandCreating,
                connection,
                context,
                executeMethod,
                commandId,
                connectionId,
                async,
                startTime);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;
        }

        private static void LogCommandCreating(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            EventDefinition<string> definition,
            DbCommandMethod commandMethod)
        {
            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, commandMethod.ToString());
            }
        }

        private static string CommandCreating(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (CommandCorrelatedEventData)payload;
            return d.GenerateMessage(p.ExecuteMethod.ToString());
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandCreated" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="commandMethod"> The type of method that will be called on this command. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command creation. </param>
        /// <returns> An intercepted result. </returns>
        public static DbCommand CommandCreated(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            DbCommandMethod commandMethod,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogCommandCreated(diagnostics);

            LogCommandCreated(diagnostics, definition, commandMethod, duration);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandCreated(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    commandMethod,
                    commandId,
                    connectionId,
                    false,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.CommandCreated(eventData, command);
                }
            }

            return command;
        }

        private static CommandEndEventData BroadcastCommandCreated(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            DbConnection connection,
            DbCommand command,
            DbContext context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration,
            EventDefinition<string, int> definition,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            var eventData = new CommandEndEventData(
                definition,
                CommandCreated,
                connection,
                command,
                context,
                executeMethod,
                commandId,
                connectionId,
                async,
                false,
                startTime,
                duration);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;
        }

        private static void LogCommandCreated(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            EventDefinition<string, int> definition,
            DbCommandMethod commandMethod,
            TimeSpan duration)
        {
            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, commandMethod.ToString(), (int)duration.TotalMilliseconds);
            }
        }

        private static string CommandCreated(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, int>)definition;
            var p = (CommandEndEventData)payload;
            return d.GenerateMessage(p.ExecuteMethod.ToString(), (int)p.Duration.TotalMilliseconds);
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <returns> An intercepted result. </returns>
        public static InterceptionResult<DbDataReader> CommandReaderExecuting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime)
        {
            var definition = RelationalResources.LogExecutingCommand(diagnostics);

            LogCommandExecuting(diagnostics, command, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuting(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connectionId,
                    false,
                    startTime,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ReaderExecuting(command, eventData, default);
                }
            }

            return default;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <returns> An intercepted result. </returns>
        public static InterceptionResult<object> CommandScalarExecuting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime)
        {
            var definition = RelationalResources.LogExecutingCommand(diagnostics);

            LogCommandExecuting(diagnostics, command, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuting(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteScalar,
                    commandId,
                    connectionId,
                    false,
                    startTime,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ScalarExecuting(command, eventData, default);
                }
            }

            return default;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <returns> An intercepted result. </returns>
        public static InterceptionResult<int> CommandNonQueryExecuting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime)
        {
            var definition = RelationalResources.LogExecutingCommand(diagnostics);

            LogCommandExecuting(diagnostics, command, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuting(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteNonQuery,
                    commandId,
                    connectionId,
                    false,
                    startTime,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.NonQueryExecuting(command, eventData, default);
                }
            }

            return default;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> An intercepted result. </returns>
        public static ValueTask<InterceptionResult<DbDataReader>> CommandReaderExecutingAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogExecutingCommand(diagnostics);

            LogCommandExecuting(diagnostics, command, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuting(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connectionId,
                    true,
                    startTime,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ReaderExecutingAsync(command, eventData, default, cancellationToken);
                }
            }

            return default;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> An intercepted result. </returns>
        public static ValueTask<InterceptionResult<object>> CommandScalarExecutingAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogExecutingCommand(diagnostics);

            LogCommandExecuting(diagnostics, command, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuting(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteScalar,
                    commandId,
                    connectionId,
                    true,
                    startTime,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ScalarExecutingAsync(command, eventData, default, cancellationToken);
                }
            }

            return default;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuting" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> An intercepted result. </returns>
        public static ValueTask<InterceptionResult<int>> CommandNonQueryExecutingAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            DateTimeOffset startTime,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogExecutingCommand(diagnostics);

            LogCommandExecuting(diagnostics, command, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuting(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteNonQuery,
                    commandId,
                    connectionId,
                    true,
                    startTime,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.NonQueryExecutingAsync(command, eventData, default, cancellationToken);
                }
            }

            return default;
        }

        private static CommandEventData BroadcastCommandExecuting(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            DbConnection connection,
            DbCommand command,
            DbContext context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime,
            EventDefinition<string, CommandType, int, string, string> definition,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            var eventData = new CommandEventData(
                definition,
                CommandExecuting,
                connection,
                command,
                context,
                executeMethod,
                commandId,
                connectionId,
                async,
                ShouldLogParameterValues(diagnostics, command),
                startTime);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;
        }

        private static void LogCommandExecuting(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            DbCommand command,
            EventDefinition<string, CommandType, int, string, string> definition)
        {
            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    command.Parameters.FormatParameters(ShouldLogParameterValues(diagnostics, command)),
                    command.CommandType,
                    command.CommandTimeout,
                    Environment.NewLine,
                    command.CommandText.TrimEnd());
            }
        }

        private static string CommandExecuting(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, CommandType, int, string, string>)definition;
            var p = (CommandEventData)payload;
            return d.GenerateMessage(
                p.Command.Parameters.FormatParameters(p.LogParameterValues),
                p.Command.CommandType,
                p.Command.CommandTimeout,
                Environment.NewLine,
                p.Command.CommandText.TrimEnd());
        }

        private static bool ShouldLogParameterValues(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            DbCommand command)
            => command.Parameters.Count > 0
                && diagnostics.ShouldLogSensitiveData();

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static DbDataReader CommandReaderExecuted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            [CanBeNull] DbDataReader methodResult,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogExecutedCommand(diagnostics);

            LogCommandExecuted(diagnostics, command, duration, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuted(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connectionId,
                    methodResult,
                    false,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ReaderExecuted(command, eventData, methodResult);
                }
            }

            return methodResult;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static object CommandScalarExecuted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            [CanBeNull] object methodResult,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogExecutedCommand(diagnostics);

            LogCommandExecuted(diagnostics, command, duration, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuted(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteScalar,
                    commandId,
                    connectionId,
                    methodResult,
                    false,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ScalarExecuted(command, eventData, methodResult);
                }
            }

            return methodResult;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static int CommandNonQueryExecuted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            int methodResult,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogExecutedCommand(diagnostics);

            LogCommandExecuted(diagnostics, command, duration, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuted(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteNonQuery,
                    commandId,
                    connectionId,
                    methodResult,
                    false,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.NonQueryExecuted(command, eventData, methodResult);
                }
            }

            return methodResult;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static ValueTask<DbDataReader> CommandReaderExecutedAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            [CanBeNull] DbDataReader methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogExecutedCommand(diagnostics);

            LogCommandExecuted(diagnostics, command, duration, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuted(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteReader,
                    commandId,
                    connectionId,
                    methodResult,
                    true,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ReaderExecutedAsync(command, eventData, methodResult, cancellationToken);
                }
            }

            return new ValueTask<DbDataReader>(methodResult);
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static ValueTask<object> CommandScalarExecutedAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            [CanBeNull] object methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogExecutedCommand(diagnostics);

            LogCommandExecuted(diagnostics, command, duration, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuted(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteScalar,
                    commandId,
                    connectionId,
                    methodResult,
                    true,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ScalarExecutedAsync(command, eventData, methodResult, cancellationToken);
                }
            }

            return new ValueTask<object>(methodResult);
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandExecuted" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="methodResult"> The return value from the underlying method execution. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The duration of the command execution, not including consuming results. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static ValueTask<int> CommandNonQueryExecutedAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            Guid commandId,
            Guid connectionId,
            int methodResult,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogExecutedCommand(diagnostics);

            LogCommandExecuted(diagnostics, command, duration, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandExecuted(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    DbCommandMethod.ExecuteNonQuery,
                    commandId,
                    connectionId,
                    methodResult,
                    true,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.NonQueryExecutedAsync(command, eventData, methodResult, cancellationToken);
                }
            }

            return new ValueTask<int>(methodResult);
        }

        private static CommandExecutedEventData BroadcastCommandExecuted(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            DbConnection connection,
            DbCommand command,
            DbContext context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            object methodResult,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration,
            EventDefinition<string, string, CommandType, int, string, string> definition,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            var eventData = new CommandExecutedEventData(
                definition,
                CommandExecuted,
                connection,
                command,
                context,
                executeMethod,
                commandId,
                connectionId,
                methodResult,
                async,
                ShouldLogParameterValues(diagnostics, command),
                startTime,
                duration);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;
        }

        private static void LogCommandExecuted(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            DbCommand command,
            TimeSpan duration,
            EventDefinition<string, string, CommandType, int, string, string> definition)
        {
            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.TotalMilliseconds),
                    command.Parameters.FormatParameters(ShouldLogParameterValues(diagnostics, command)),
                    command.CommandType,
                    command.CommandTimeout,
                    Environment.NewLine,
                    command.CommandText.TrimEnd());
            }
        }

        private static string CommandExecuted(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, CommandType, int, string, string>)definition;
            var p = (CommandExecutedEventData)payload;
            return d.GenerateMessage(
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", p.Duration.TotalMilliseconds),
                p.Command.Parameters.FormatParameters(p.LogParameterValues),
                p.Command.CommandType,
                p.Command.CommandTimeout,
                Environment.NewLine,
                p.Command.CommandText.TrimEnd());
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandError" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="executeMethod"> Represents the method that will be called to execute the command. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="exception"> The exception that caused this failure. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The amount of time that passed until the exception was raised. </param>
        public static void CommandError(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            [NotNull] Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogCommandFailed(diagnostics);

            LogCommandError(diagnostics, command, duration, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandError(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    executeMethod,
                    commandId,
                    connectionId,
                    exception,
                    false,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                interceptor?.CommandFailed(command, eventData);
            }
        }

        private static void LogCommandError(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            DbCommand command,
            TimeSpan duration,
            EventDefinition<string, string, CommandType, int, string, string> definition)
        {
            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.TotalMilliseconds),
                    command.Parameters.FormatParameters(ShouldLogParameterValues(diagnostics, command)),
                    command.CommandType,
                    command.CommandTimeout,
                    Environment.NewLine,
                    command.CommandText.TrimEnd());
            }
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.CommandError" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="context"> The <see cref="DbContext" /> currently being used, to null if not known. </param>
        /// <param name="executeMethod"> Represents the method that will be called to execute the command. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="connectionId"> The correlation ID associated with the <see cref="DbConnection" /> being used. </param>
        /// <param name="exception"> The exception that caused this failure. </param>
        /// <param name="startTime"> The time that execution began. </param>
        /// <param name="duration"> The amount of time that passed until the exception was raised. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static Task CommandErrorAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [CanBeNull] DbContext context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            [NotNull] Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogCommandFailed(diagnostics);

            LogCommandError(diagnostics, command, duration, definition);

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCommandError(
                    diagnostics,
                    connection.DbConnection,
                    command,
                    context,
                    executeMethod,
                    commandId,
                    connectionId,
                    exception,
                    true,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.CommandFailedAsync(command, eventData, cancellationToken);
                }
            }

            return Task.CompletedTask;
        }

        private static CommandErrorEventData BroadcastCommandError(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            DbConnection connection,
            DbCommand command,
            DbContext context,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            Exception exception,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration,
            EventDefinition<string, string, CommandType, int, string, string> definition,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            var eventData = new CommandErrorEventData(
                definition,
                CommandError,
                connection,
                command,
                context,
                executeMethod,
                commandId,
                connectionId,
                exception,
                async,
                ShouldLogParameterValues(diagnostics, command),
                startTime,
                duration);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;
        }

        private static string CommandError(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, CommandType, int, string, string>)definition;
            var p = (CommandErrorEventData)payload;
            return d.GenerateMessage(
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", p.Duration.TotalMilliseconds),
                p.Command.Parameters.FormatParameters(p.LogParameterValues),
                p.Command.CommandType,
                p.Command.CommandTimeout,
                Environment.NewLine,
                p.Command.CommandText.TrimEnd());
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ConnectionOpening" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static InterceptionResult ConnectionOpening(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime)
        {
            var definition = RelationalResources.LogOpeningConnection(diagnostics);

            LogConnectionOpening(diagnostics, connection, definition);

            if (diagnostics.NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastConnectionOpening(
                    diagnostics,
                    connection,
                    startTime,
                    definition,
                    false,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ConnectionOpening(connection.DbConnection, eventData, default);
                }
            }

            return default;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ConnectionOpening" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static ValueTask<InterceptionResult> ConnectionOpeningAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            CancellationToken cancellationToken)
        {
            var definition = RelationalResources.LogOpeningConnection(diagnostics);

            LogConnectionOpening(diagnostics, connection, definition);

            if (diagnostics.NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastConnectionOpening(
                    diagnostics,
                    connection,
                    startTime,
                    definition,
                    true,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ConnectionOpeningAsync(connection.DbConnection, eventData, default, cancellationToken);
                }
            }

            return default;
        }

        private static void LogConnectionOpening(
            IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            IRelationalConnection connection,
            EventDefinition<string, string> definition)
        {
            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    connection.DbConnection.Database, connection.DbConnection.DataSource);
            }
        }

        private static ConnectionEventData BroadcastConnectionOpening(
            IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            IRelationalConnection connection,
            DateTimeOffset startTime,
            EventDefinition<string, string> definition,
            bool async,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            var eventData = new ConnectionEventData(
                definition,
                ConnectionOpening,
                connection.DbConnection,
                connection.Context,
                connection.ConnectionId,
                async,
                startTime);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;
        }

        private static string ConnectionOpening(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (ConnectionEventData)payload;
            return d.GenerateMessage(
                p.Connection.Database,
                p.Connection.DataSource);
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ConnectionOpened" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The amount of time before the connection was opened. </param>
        public static void ConnectionOpened(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogOpenedConnection(diagnostics);

            LogConnectionOpened(diagnostics, connection, definition);

            if (diagnostics.NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastConnectionOpened(
                    diagnostics,
                    connection,
                    false,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                interceptor?.ConnectionOpened(connection.DbConnection, eventData);
            }
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ConnectionOpened" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The amount of time before the connection was opened. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static Task ConnectionOpenedAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogOpenedConnection(diagnostics);

            LogConnectionOpened(diagnostics, connection, definition);

            if (diagnostics.NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastConnectionOpened(
                    diagnostics,
                    connection,
                    true,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ConnectionOpenedAsync(connection.DbConnection, eventData, cancellationToken);
                }
            }

            return Task.CompletedTask;
        }

        private static ConnectionEndEventData BroadcastConnectionOpened(
            IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            IRelationalConnection connection,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration,
            EventDefinition<string, string> definition,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            var eventData = new ConnectionEndEventData(
                definition,
                ConnectionOpened,
                connection.DbConnection,
                connection.Context,
                connection.ConnectionId,
                async,
                startTime,
                duration);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;
        }

        private static void LogConnectionOpened(
            IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            IRelationalConnection connection,
            EventDefinition<string, string> definition)
        {
            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    connection.DbConnection.Database, connection.DbConnection.DataSource);
            }
        }

        private static string ConnectionOpened(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (ConnectionEndEventData)payload;
            return d.GenerateMessage(
                p.Connection.Database,
                p.Connection.DataSource);
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ConnectionClosing" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static InterceptionResult ConnectionClosing(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime)
        {
            var definition = RelationalResources.LogClosingConnection(diagnostics);

            LogConnectionClosing(diagnostics, connection, definition);

            if (diagnostics.NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastConnectionClosing(
                    diagnostics,
                    connection,
                    startTime,
                    false,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ConnectionClosing(connection.DbConnection, eventData, default);
                }
            }

            return default;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ConnectionClosing" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static ValueTask<InterceptionResult> ConnectionClosingAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime)
        {
            var definition = RelationalResources.LogClosingConnection(diagnostics);

            LogConnectionClosing(diagnostics, connection, definition);

            if (diagnostics.NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastConnectionClosing(
                    diagnostics,
                    connection,
                    startTime,
                    true,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ConnectionClosingAsync(connection.DbConnection, eventData, default);
                }
            }

            return default;
        }

        private static ConnectionEventData BroadcastConnectionClosing(
            IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            IRelationalConnection connection,
            DateTimeOffset startTime,
            bool async,
            EventDefinition<string, string> definition,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            var eventData = new ConnectionEventData(
                definition,
                ConnectionClosing,
                connection.DbConnection,
                connection.Context,
                connection.ConnectionId,
                async,
                startTime);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;
        }

        private static void LogConnectionClosing(
            IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            IRelationalConnection connection,
            EventDefinition<string, string> definition)
        {
            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    connection.DbConnection.Database, connection.DbConnection.DataSource);
            }
        }

        private static string ConnectionClosing(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (ConnectionEventData)payload;
            return d.GenerateMessage(
                p.Connection.Database,
                p.Connection.DataSource);
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ConnectionClosed" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The amount of time before the connection was closed. </param>
        public static void ConnectionClosed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogClosedConnection(diagnostics);

            LogConnectionClosed(diagnostics, connection, definition);

            if (diagnostics.NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCollectionClosed(
                    diagnostics,
                    connection,
                    startTime,
                    duration,
                    false,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                interceptor?.ConnectionClosed(connection.DbConnection, eventData);
            }
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ConnectionClosed" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The amount of time before the connection was closed. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static Task ConnectionClosedAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogClosedConnection(diagnostics);

            LogConnectionClosed(diagnostics, connection, definition);

            if (diagnostics.NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastCollectionClosed(
                    diagnostics,
                    connection,
                    startTime,
                    duration,
                    true,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ConnectionClosedAsync(connection.DbConnection, eventData);
                }
            }

            return Task.CompletedTask;
        }

        private static ConnectionEndEventData BroadcastCollectionClosed(
            IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool async,
            EventDefinition<string, string> definition,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            var eventData = new ConnectionEndEventData(
                definition,
                ConnectionClosed,
                connection.DbConnection,
                connection.Context,
                connection.ConnectionId,
                async,
                startTime,
                duration);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;
        }

        private static void LogConnectionClosed(
            IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            IRelationalConnection connection,
            EventDefinition<string, string> definition)
        {
            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    connection.DbConnection.Database, connection.DbConnection.DataSource);
            }
        }

        private static string ConnectionClosed(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (ConnectionEndEventData)payload;
            return d.GenerateMessage(
                p.Connection.Database,
                p.Connection.DataSource);
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ConnectionError" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="exception"> The exception representing the error. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The elapsed time before the operation failed. </param>
        /// <param name="logErrorAsDebug"> A flag indicating the exception is being handled and so it should be logged at Debug level. </param>
        public static void ConnectionError(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool logErrorAsDebug)
        {
            var definition = logErrorAsDebug
                ? RelationalResources.LogConnectionErrorAsDebug(diagnostics)
                : RelationalResources.LogConnectionError(diagnostics);

            LogConnectionError(diagnostics, connection, definition);

            if (diagnostics.NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastConnectionError(
                    diagnostics,
                    connection,
                    exception,
                    startTime,
                    duration,
                    false,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                interceptor?.ConnectionFailed(connection.DbConnection, eventData);
            }
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ConnectionError" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="exception"> The exception representing the error. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The elapsed time before the operation failed. </param>
        /// <param name="logErrorAsDebug"> A flag indicating the exception is being handled and so it should be logged at Debug level. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static Task ConnectionErrorAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool logErrorAsDebug,
            CancellationToken cancellationToken = default)
        {
            var definition = logErrorAsDebug
                ? RelationalResources.LogConnectionErrorAsDebug(diagnostics)
                : RelationalResources.LogConnectionError(diagnostics);

            LogConnectionError(diagnostics, connection, definition);

            if (diagnostics.NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastConnectionError(
                    diagnostics,
                    connection,
                    exception,
                    startTime,
                    duration,
                    true,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ConnectionFailedAsync(connection.DbConnection, eventData, cancellationToken);
                }
            }

            return Task.CompletedTask;
        }

        private static ConnectionErrorEventData BroadcastConnectionError(
            IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            IRelationalConnection connection,
            Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool async,
            EventDefinition<string, string> definition,
            bool diagnosticSourceEnabled,
            bool simpleLogEnabled)
        {
            var eventData = new ConnectionErrorEventData(
                definition,
                ConnectionError,
                connection.DbConnection,
                connection.Context,
                connection.ConnectionId,
                exception,
                async,
                startTime,
                duration);

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;
        }

        private static void LogConnectionError(
            IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            IRelationalConnection connection,
            EventDefinition<string, string> definition)
        {
            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(
                    diagnostics,
                    connection.DbConnection.Database, connection.DbConnection.DataSource);
            }
        }

        private static string ConnectionError(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (ConnectionErrorEventData)payload;
            return d.GenerateMessage(
                p.Connection.Database,
                p.Connection.DataSource);
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.TransactionStarting" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="isolationLevel"> The transaction isolation level. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static InterceptionResult<DbTransaction> TransactionStarting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="isolationLevel"> The transaction isolation level. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The amount of time before the connection was opened. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static DbTransaction TransactionStarted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The amount of time before the connection was opened. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static ValueTask<DbTransaction> TransactionStartedAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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

            return new ValueTask<DbTransaction>(transaction);
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static DbTransaction TransactionUsed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startTime)
        {
            var definition = RelationalResources.LogUsingTransaction(diagnostics);

            LogTransactionUsed(diagnostics, transaction, definition);

            if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcasstTransactionUsed(
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static ValueTask<DbTransaction> TransactionUsedAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startTime,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogUsingTransaction(diagnostics);

            LogTransactionUsed(diagnostics, transaction, definition);

            if (diagnostics.NeedsEventData<IDbTransactionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcasstTransactionUsed(
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

            return new ValueTask<DbTransaction>(transaction);
        }

        private static TransactionEventData BroadcasstTransactionUsed(
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
                diagnostics.DiagnosticSource.Write(definition.EventId.Name, eventData);
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
            var d = (EventDefinition<string>)definition;
            var p = (TransactionEventData)payload;
            return d.GenerateMessage(
                p.Transaction.IsolationLevel.ToString("G"));
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.TransactionCommitting" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static InterceptionResult TransactionCommitting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static ValueTask<InterceptionResult> TransactionCommittingAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The elapsed time from when the operation was started. </param>
        public static void TransactionCommitted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The elapsed time from when the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static Task TransactionCommittedAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The elapsed time from when the operation was started. </param>
        public static void TransactionRolledBack(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The elapsed time from when the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static Task TransactionRolledBackAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static InterceptionResult TransactionRollingBack(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static ValueTask<InterceptionResult> TransactionRollingBackAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static InterceptionResult CreatingTransactionSavepoint(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static ValueTask<InterceptionResult> CreatingTransactionSavepointAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        public static void CreatedTransactionSavepoint(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static Task CreatedTransactionSavepointAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static InterceptionResult RollingBackToTransactionSavepoint(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static ValueTask<InterceptionResult> RollingBackToTransactionSavepointAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        public static void RolledBackToTransactionSavepoint(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static Task RolledBackToTransactionSavepointAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static InterceptionResult ReleasingTransactionSavepoint(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static ValueTask<InterceptionResult> ReleasingTransactionSavepointAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        public static void ReleasedTransactionSavepoint(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static Task ReleasedTransactionSavepointAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        public static void TransactionDisposed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
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
                    (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="action"> The action being taken. </param>
        /// <param name="exception"> The exception that represents the error. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The elapsed time from when the operation was started. </param>
        public static void TransactionError(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            [NotNull] string action,
            [NotNull] Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogTransactionError(diagnostics);

            LogTransactionError(diagnostics, exception, definition);

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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="transactionId"> The correlation ID associated with the <see cref="DbTransaction" />. </param>
        /// <param name="action"> The action being taken. </param>
        /// <param name="exception"> The exception that represents the error. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The elapsed time from when the operation was started. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the async operation. </returns>
        public static Task TransactionErrorAsync(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            [NotNull] string action,
            [NotNull] Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogTransactionError(diagnostics);

            LogTransactionError(diagnostics, exception, definition);

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
                (d, p) => ((EventDefinition)d).GenerateMessage(),
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
            Exception exception,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        public static void AmbientTransactionWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
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
                    (d, p) => ((EventDefinition)d).GenerateMessage(),
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        public static void AmbientTransactionEnlisted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] Transaction transaction)
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="transaction"> The transaction. </param>
        public static void ExplicitTransactionEnlisted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] Transaction transaction)
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
        ///     Logs for the <see cref="RelationalEventId.DataReaderDisposing" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The database command object. </param>
        /// <param name="dataReader"> The data reader. </param>
        /// <param name="commandId"> The correlation ID associated with the given <see cref="DbCommand" />. </param>
        /// <param name="recordsAffected"> The number of records in the database that were affected. </param>
        /// <param name="readCount"> The number of records that were read. </param>
        /// <param name="startTime"> The time that the operation was started. </param>
        /// <param name="duration"> The elapsed time from when the operation was started. </param>
        /// <returns> The result of execution, which may have been modified by an interceptor. </returns>
        public static InterceptionResult DataReaderDisposing(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [NotNull] DbDataReader dataReader,
            Guid commandId,
            int recordsAffected,
            int readCount,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogDisposingDataReader(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics);
            }

            if (diagnostics.NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new DataReaderDisposingEventData(
                    definition,
                    (d, p) => ((EventDefinition)d).GenerateMessage(),
                    command,
                    dataReader,
                    connection.Context,
                    commandId,
                    connection.ConnectionId,
                    recordsAffected,
                    readCount,
                    startTime,
                    duration);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.DataReaderDisposing(command, eventData, default);
                }
            }

            return default;
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.MigrateUsingConnection" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="migrator"> The migrator. </param>
        /// <param name="connection"> The connection. </param>
        public static void MigrateUsingConnection(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] IRelationalConnection connection)
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="migrator"> The migrator. </param>
        /// <param name="migration"> The migration. </param>
        public static void MigrationReverting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration)
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="migrator"> The migrator. </param>
        /// <param name="migration"> The migration. </param>
        public static void MigrationApplying(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration)
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="migrator"> The migrator. </param>
        /// <param name="migration"> The migration. </param>
        /// <param name="fromMigration"> The starting migration name. </param>
        /// <param name="toMigration"> The ending migration name. </param>
        /// <param name="idempotent"> Indicates whether or not an idempotent script is being generated. </param>
        public static void MigrationGeneratingDownScript(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration,
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="migrator"> The migrator. </param>
        /// <param name="migration"> The migration. </param>
        /// <param name="fromMigration"> The starting migration name. </param>
        /// <param name="toMigration"> The ending migration name. </param>
        /// <param name="idempotent"> Indicates whether or not an idempotent script is being generated. </param>
        public static void MigrationGeneratingUpScript(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration,
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="migrator"> The migrator. </param>
        public static void MigrationsNotApplied(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator)
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
                    (d, p) => ((EventDefinition)d).GenerateMessage(),
                    migrator);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.MigrationsNotFound" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="migrator"> The migrator. </param>
        /// <param name="migrationsAssembly"> The assembly in which migrations are stored. </param>
        public static void MigrationsNotFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] IMigrationsAssembly migrationsAssembly)
        {
            var definition = RelationalResources.LogNoMigrationsFound(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, migrationsAssembly.Assembly.GetName().Name);
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
            return d.GenerateMessage(p.MigrationsAssembly.Assembly.GetName().Name);
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.MigrationAttributeMissingWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="migrationType"> Info for the migration type. </param>
        public static void MigrationAttributeMissingWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] TypeInfo migrationType)
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="left"> The left SQL expression of the Equals. </param>
        /// <param name="right"> The right SQL expression of the Equals. </param>
        public static void QueryPossibleUnintendedUseOfEqualsWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right)
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
        ///     Logs for the <see cref="RelationalEventId.QueryPossibleExceptionWithAggregateOperatorWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        [Obsolete]
        public static void QueryPossibleExceptionWithAggregateOperatorWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics)
        {
            var definition = RelationalResources.LogQueryPossibleExceptionWithAggregateOperatorWarning(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics);
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new EventData(
                    definition,
                    (d, p) => ((EventDefinition)d).GenerateMessage());

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.MultipleCollectionIncludeWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        public static void MultipleCollectionIncludeWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics)
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
                    (d, p) => ((EventDefinition)d).GenerateMessage());

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.ModelValidationKeyDefaultValueWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="property"> The property. </param>
        public static void ModelValidationKeyDefaultValueWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = RelationalResources.LogKeyHasDefaultValue(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, property.Name, property.DeclaringEntityType.DisplayName());
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
                p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.BoolWithDefaultWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="property"> The property. </param>
        public static void BoolWithDefaultWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = RelationalResources.LogBoolWithDefaultWarning(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics, property.Name, property.DeclaringEntityType.DisplayName());
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
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(p.Property.Name, p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.BatchReadyForExecution" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="entries"> The entries for entities in the batch. </param>
        /// <param name="commandCount"> The number of commands. </param>
        public static void BatchReadyForExecution(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] IEnumerable<IUpdateEntry> entries,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="entries"> The entries for entities in the batch. </param>
        /// <param name="commandCount"> The number of commands. </param>
        /// <param name="minBatchSize"> The minimum batch size. </param>
        public static void BatchSmallerThanMinBatchSize(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] IEnumerable<IUpdateEntry> entries,
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="entityType"> The entity type on which the index is defined. </param>
        /// <param name="index"> The index on the entity type. </param>
        public static void AllIndexPropertiesNotToMappedToAnyTable(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IEntityType entityType,
            [NotNull] IIndex index)
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
            var d = (EventDefinition<string, string, string>)definition;
            var p = (IndexEventData)payload;
            return d.GenerateMessage(
                p.Name,
                p.EntityType.DisplayName(),
                p.PropertyNames.Format());
        }

        /// <summary>
        ///     Logs the <see cref="RelationalEventId.IndexPropertiesBothMappedAndNotMappedToTable" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="entityType"> The entity type on which the index is defined. </param>
        /// <param name="index"> The index on the entity type. </param>
        /// <param name="unmappedPropertyName"> The name of the property which is not mapped. </param>
        public static void IndexPropertiesBothMappedAndNotMappedToTable(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IEntityType entityType,
            [NotNull] IIndex index,
            [NotNull] string unmappedPropertyName)
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
            var d = (EventDefinition<string, string, string, string>)definition;
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
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="entityType"> The entity type on which the index is defined. </param>
        /// <param name="index"> The index on the entity type. </param>
        /// <param name="property1Name"> The first property name which is invalid. </param>
        /// <param name="tablesMappedToProperty1"> The tables mapped to the first property. </param>
        /// <param name="property2Name"> The second property name which is invalid. </param>
        /// <param name="tablesMappedToProperty2"> The tables mapped to the second property. </param>
        public static void IndexPropertiesMappedToNonOverlappingTables(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IEntityType entityType,
            [NotNull] IIndex index,
            [NotNull] string property1Name,
            [NotNull] List<(string Table, string Schema)> tablesMappedToProperty1,
            [NotNull] string property2Name,
            [NotNull] List<(string Table, string Schema)> tablesMappedToProperty2)
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
        ///     Logs the <see cref="RelationalEventId.IndexPropertiesMappedToNonOverlappingTables" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="foreignKey"> The foreign key. </param>
        public static void ForeignKeyPropertiesMappedToUnrelatedTables(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IForeignKey foreignKey)
        {
            var definition = RelationalResources.LogForeignKeyPropertiesMappedToUnrelatedTables(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                definition.Log(diagnostics,
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
        ///     Logs for the <see cref="RelationalEventId.BatchExecutorFailedToRollbackToSavepoint" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="contextType"> The <see cref="DbContext" /> type being used. </param>
        /// <param name="exception"> The exception that caused this failure. </param>
        public static void BatchExecutorFailedToRollbackToSavepoint(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] Type contextType,
            [NotNull] Exception exception)
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
                    (d, p) => ((EventDefinition)d).GenerateMessage(),
                    contextType,
                    exception);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        /// <summary>
        ///     Logs for the <see cref="RelationalEventId.BatchExecutorFailedToReleaseSavepoint" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="contextType"> The <see cref="DbContext" /> type being used. </param>
        /// <param name="exception"> The exception that caused this failure. </param>
        public static void BatchExecutorFailedToReleaseSavepoint(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] Type contextType,
            [NotNull] Exception exception)
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
                    (d, p) => ((EventDefinition)d).GenerateMessage(),
                    contextType,
                    exception);

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }
    }
}
