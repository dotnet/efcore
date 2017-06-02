// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using Remotion.Linq;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class RelationalLoggerExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CommandExecuting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime)
        {
            var definition = RelationalStrings.LogRelationalLoggerExecutingCommand;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    command.Parameters.FormatParameters(ShouldLogParameterValues(diagnostics, command)),
                    command.CommandType,
                    command.CommandTimeout,
                    Environment.NewLine,
                    command.CommandText.TrimEnd());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CommandEventData(
                        definition,
                        CommandExecuting,
                        command,
                        executeMethod,
                        commandId,
                        connectionId,
                        async,
                        ShouldLogParameterValues(diagnostics, command),
                        startTime));
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CommandExecuted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            [CanBeNull] object methodResult,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalStrings.LogRelationalLoggerExecutedCommand;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.Milliseconds),
                    command.Parameters.FormatParameters(ShouldLogParameterValues(diagnostics, command)),
                    command.CommandType,
                    command.CommandTimeout,
                    Environment.NewLine,
                    command.CommandText.TrimEnd());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CommandExecutedEventData(
                        definition,
                        CommandExecuted,
                        command,
                        executeMethod,
                        commandId,
                        connectionId,
                        methodResult,
                        async,
                        ShouldLogParameterValues(diagnostics, command),
                        startTime,
                        duration));
            }
        }

        private static string CommandExecuted(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, CommandType, int, string, string>)definition;
            var p = (CommandExecutedEventData)payload;
            return d.GenerateMessage(
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", p.Duration.Milliseconds),
                p.Command.Parameters.FormatParameters(p.LogParameterValues),
                p.Command.CommandType,
                p.Command.CommandTimeout,
                Environment.NewLine,
                p.Command.CommandText.TrimEnd());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CommandError(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            [NotNull] Exception exception,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalStrings.LogRelationalLoggerCommandFailed;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.Milliseconds),
                    command.Parameters.FormatParameters(ShouldLogParameterValues(diagnostics, command)),
                    command.CommandType,
                    command.CommandTimeout,
                    Environment.NewLine,
                    command.CommandText.TrimEnd(),
                    exception);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CommandErrorEventData(
                        definition,
                        CommandError,
                        command,
                        executeMethod,
                        commandId,
                        connectionId,
                        exception,
                        async,
                        ShouldLogParameterValues(diagnostics, command),
                        startTime,
                        duration));
            }
        }

        private static string CommandError(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, CommandType, int, string, string>)definition;
            var p = (CommandErrorEventData)payload;
            return d.GenerateMessage(
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", p.Duration.Milliseconds),
                p.Command.Parameters.FormatParameters(p.LogParameterValues),
                p.Command.CommandType,
                p.Command.CommandTimeout,
                Environment.NewLine,
                p.Command.CommandText.TrimEnd(),
                p.Exception);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionOpening(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            bool async)
        {
            var definition = RelationalStrings.LogRelationalLoggerOpeningConnection;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    connection.DbConnection.Database,
                    connection.DbConnection.DataSource);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ConnectionEventData(
                        definition,
                        ConnectionOpening,
                        connection.DbConnection,
                        connection.ConnectionId,
                        async,
                        startTime));
            }
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionOpened(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool async)
        {
            var definition = RelationalStrings.LogRelationalLoggerOpenedConnection;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    connection.DbConnection.Database,
                    connection.DbConnection.DataSource);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ConnectionEndEventData(
                        definition,
                        ConnectionOpened,
                        connection.DbConnection,
                        connection.ConnectionId,
                        async,
                        startTime,
                        duration));
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionClosing(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime)
        {
            var definition = RelationalStrings.LogRelationalLoggerClosingConnection;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    connection.DbConnection.Database,
                    connection.DbConnection.DataSource);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ConnectionEventData(
                        definition,
                        ConnectionClosing,
                        connection.DbConnection,
                        connection.ConnectionId,
                        false,
                        startTime));
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionClosed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalStrings.LogRelationalLoggerClosedConnection;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    connection.DbConnection.Database,
                    connection.DbConnection.DataSource);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ConnectionEndEventData(
                        definition,
                        ConnectionClosed,
                        connection.DbConnection,
                        connection.ConnectionId,
                        false,
                        startTime,
                        duration));
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionError(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool async)
        {
            var definition = RelationalStrings.LogRelationalLoggerConnectionError;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    connection.DbConnection.Database,
                    connection.DbConnection.DataSource,
                    exception);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ConnectionErrorEventData(
                        definition,
                        ConnectionError,
                        connection.DbConnection,
                        connection.ConnectionId,
                        exception,
                        async,
                        startTime,
                        duration));
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TransactionStarted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startDate)
        {
            var definition = RelationalStrings.LogRelationalLoggerBeginningTransaction;

            definition.Log(
                diagnostics,
                transaction.IsolationLevel.ToString("G"));

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TransactionEventData(
                        definition,
                        TransactionStarted,
                        transaction,
                        transactionId,
                        connection.ConnectionId,
                        startDate));
            }
        }

        private static string TransactionStarted(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (TransactionEventData)payload;
            return d.GenerateMessage(
                p.Transaction.IsolationLevel.ToString("G"));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TransactionUsed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startDate)
        {
            var definition = RelationalStrings.LogRelationalLoggerUsingTransaction;

            definition.Log(
                diagnostics,
                transaction.IsolationLevel.ToString("G"));

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TransactionEventData(
                        definition,
                        TransactionUsed,
                        transaction,
                        transactionId,
                        connection.ConnectionId,
                        startDate));
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TransactionCommitted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalStrings.LogRelationalLoggerCommittingTransaction;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TransactionEndEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        transaction,
                        transactionId,
                        connection.ConnectionId,
                        startTime,
                        duration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TransactionRolledBack(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalStrings.LogRelationalLoggerRollingbackTransaction;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TransactionEndEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        transaction,
                        transactionId,
                        connection.ConnectionId,
                        startTime,
                        duration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TransactionDisposed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startDate)
        {
            var definition = RelationalStrings.LogRelationalLoggerDisposingTransaction;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TransactionEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        transaction,
                        transactionId,
                        connection.ConnectionId,
                        startDate));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
            var definition = RelationalStrings.LogRelationalLoggerTransactionError;

            definition.Log(diagnostics, exception);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TransactionErrorEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        transaction,
                        connection.ConnectionId,
                        transactionId,
                        action,
                        exception,
                        startTime,
                        duration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void AmbientTransactionWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startDate)
        {
            var definition = RelationalStrings.LogAmbientTransaction;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ConnectionEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        connection.DbConnection,
                        connection.ConnectionId,
                        false,
                        startDate));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DataReaderDisposing(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [NotNull] DbDataReader dataReader,
            Guid commandId,
            int recordsAffected,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalStrings.LogDisposingDataReader;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DataReaderDisposingEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        command,
                        dataReader,
                        commandId,
                        connection.ConnectionId,
                        recordsAffected,
                        startTime,
                        duration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrateUsingConnection(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] IRelationalConnection connection)
        {
            var definition = RelationalStrings.LogMigrating;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                var dbConnection = connection.DbConnection;

                definition.Log(
                    diagnostics,
                    dbConnection.Database,
                    dbConnection.DataSource);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new MigratorConnectionEventData(
                        definition,
                        MigrateUsingConnection,
                        migrator,
                        connection.DbConnection,
                        connection.ConnectionId));
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationReverting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration)
        {
            var definition = RelationalStrings.LogRevertingMigration;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    migration.GetId());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new MigrationEventData(
                        definition,
                        MigrationReverting,
                        migrator,
                        migration));
            }
        }

        private static string MigrationReverting(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (MigrationEventData)payload;
            return d.GenerateMessage(p.Migration.GetId());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationApplying(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration)
        {
            var definition = RelationalStrings.LogApplyingMigration;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    migration.GetId());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new MigrationEventData(
                        definition,
                        MigrationApplying,
                        migrator,
                        migration));
            }
        }

        private static string MigrationApplying(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (MigrationEventData)payload;
            return d.GenerateMessage(p.Migration.GetId());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationGeneratingDownScript(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration,
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent)
        {
            var definition = RelationalStrings.LogGeneratingDown;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    migration.GetId());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new MigrationScriptingEventData(
                        definition,
                        MigrationGeneratingDownScript,
                        migrator,
                        migration,
                        fromMigration,
                        toMigration,
                        idempotent));
            }
        }

        private static string MigrationGeneratingDownScript(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (MigrationScriptingEventData)payload;
            return d.GenerateMessage(p.Migration.GetId());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationGeneratingUpScript(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration,
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent)
        {
            var definition = RelationalStrings.LogGeneratingUp;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    migration.GetId());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new MigrationScriptingEventData(
                        definition,
                        MigrationGeneratingUpScript,
                        migrator,
                        migration,
                        fromMigration,
                        toMigration,
                        idempotent));
            }
        }

        private static string MigrationGeneratingUpScript(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (MigrationScriptingEventData)payload;
            return d.GenerateMessage(p.Migration.GetId());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationsNotApplied(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator)
        {
            var definition = RelationalStrings.LogNoMigrationsApplied;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new MigratorEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        migrator));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationsNotFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] IMigrationsAssembly migrationsAssembly)
        {
            var definition = RelationalStrings.LogNoMigrationsFound;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    migrationsAssembly.Assembly.GetName().Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new MigrationAssemblyEventData(
                        definition,
                        MigrationsNotFound,
                        migrator,
                        migrationsAssembly));
            }
        }

        private static string MigrationsNotFound(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (MigrationAssemblyEventData)payload;
            return d.GenerateMessage(p.MigrationsAssembly.Assembly.GetName().Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryClientEvaluationWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel,
            [NotNull] object expression)
        {
            var definition = RelationalStrings.LogClientEvalWarning;

            definition.Log(diagnostics, expression);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryModelExpressionEventData(
                        definition,
                        QueryClientEvaluationWarning,
                        queryModel,
                        expression));
            }
        }

        private static string QueryClientEvaluationWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<object>)definition;
            var p = (QueryModelExpressionEventData)payload;
            return d.GenerateMessage(p.Expression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryPossibleUnintendedUseOfEqualsWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Expression argument)
        {
            var definition = RelationalStrings.LogPossibleUnintendedUseOfEquals;

            definition.Log(
                diagnostics,
                methodCallExpression.Object,
                argument);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new BinaryExpressionEventData(
                        definition,
                        QueryPossibleUnintendedUseOfEqualsWarning,
                        methodCallExpression.Object,
                        argument));
            }
        }

        private static string QueryPossibleUnintendedUseOfEqualsWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<object, object>)definition;
            var p = (BinaryExpressionEventData)payload;
            return d.GenerateMessage(p.Left, p.Right);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ModelValidationKeyDefaultValueWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = RelationalStrings.LogKeyHasDefaultValue;

            // Checking for enabled here to avoid string formatting if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    property.Name,
                    property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyEventData(
                        definition,
                        ModelValidationKeyDefaultValueWarning,
                        property));
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void BoolWithDefaultWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = RelationalStrings.LogBoolWithDefaultWarning;

            // Checking for enabled here to avoid building strings if not needed.
            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    property.Name,
                    property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyEventData(
                        definition,
                        BoolWithDefaultWarning,
                        property));
            }
        }

        private static string BoolWithDefaultWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(p.Property.Name, p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MissingTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalStrings.LogMissingTable;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SequenceNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics)
        {
            var definition = RelationalStrings.LogSequencesRequireName;

            definition.Log(diagnostics);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(definition.EventId.Name, null);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnsNotMappedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [NotNull] IList<string> unmappedColumnNames)
        {
            var definition = RelationalStrings.LogUnableToScaffoldIndexMissingProperty;

            definition.Log(
                diagnostics,
                indexName,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames));

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        IndexName = indexName,
                        UnmappedColumnNames = unmappedColumnNames
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesMissingTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName)
        {
            var definition = RelationalStrings.LogForeignKeyScaffoldErrorPrincipalTableNotFound;

            definition.Log(diagnostics, foreignKeyName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnsNotMappedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [NotNull] IList<string> unmappedColumnNames)
        {
            var definition = RelationalStrings.LogForeignKeyScaffoldErrorPropertyNotFound;

            definition.Log(
                diagnostics,
                foreignKeyName,
                string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, unmappedColumnNames));

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        UnmappedColumnNames = unmappedColumnNames
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SequenceFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string sequenceName,
            [CanBeNull] string sequenceTypeName,
            bool? cyclic,
            int? increment,
            long? start,
            long? min,
            long? max)
        {
            var definition = RelationalStrings.LogFoundSequence;

            Debug.Assert(LogLevel.Debug == definition.Level);

            if (diagnostics.GetLogBehavior(definition.EventId, definition.Level) != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    l => l.LogDebug(
                        definition.EventId,
                        null,
                        definition.MessageFormat,
                        sequenceName,
                        sequenceTypeName,
                        cyclic,
                        increment,
                        start,
                        min,
                        max));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        SequenceName = sequenceName,
                        SequenceTypeName = sequenceTypeName,
                        Cyclic = cyclic,
                        Increment = increment,
                        Start = start,
                        Min = min,
                        Max = max
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TableFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalStrings.LogFoundTable;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TableSkipped(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalStrings.LogTableNotInSelectionSet;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnSkipped(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string columnName)
        {
            var definition = RelationalStrings.LogColumnNotInSelectionSet;

            definition.Log(diagnostics, columnName, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName,
                        ColumnName = columnName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string indexName,
            bool? unique,
            [CanBeNull] string columnName,
            int? ordinal)
        {
            var definition = RelationalStrings.LogFoundIndexColumn;

            definition.Log(diagnostics, indexName, tableName, columnName, ordinal);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName,
                        IndexName = indexName,
                        Unique = unique,
                        ColumnName = columnName,
                        Ordinal = ordinal
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ColumnNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalStrings.LogColumnNameEmptyOnTable;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnSkipped(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName,
            [CanBeNull] string indexName,
            [CanBeNull] string columnName)
        {
            var definition = RelationalStrings.LogIndexColumnNotInSelectionSet;

            definition.Log(diagnostics, columnName, indexName, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName,
                        IndexName = indexName,
                        ColumnName = columnName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalStrings.LogIndexNameEmpty;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexTableMissingWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [CanBeNull] string tableName)
        {
            var definition = RelationalStrings.LogUnableToFindTableForIndex;

            definition.Log(diagnostics, indexName, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        IndexName = indexName,
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexColumnNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [CanBeNull] string tableName)
        {
            var definition = RelationalStrings.LogColumnNameEmptyOnIndex;

            definition.Log(diagnostics, indexName, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        IndexName = indexName,
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string tableName)
        {
            var definition = RelationalStrings.LogForeignKeyNameEmpty;

            definition.Log(diagnostics, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnMissingWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string columnName,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName)
        {
            var definition = RelationalStrings.LogForeignKeyColumnNotInSelectionSet;

            definition.Log(diagnostics, columnName, foreignKeyName, tableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ColumnName = columnName,
                        ForeignKeyName = foreignKeyName,
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyReferencesMissingPrincipalTableWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName,
            [CanBeNull] string principalTableName)
        {
            var definition = RelationalStrings.LogPrincipalTableNotInSelectionSet;

            definition.Log(diagnostics, foreignKeyName, tableName, principalTableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        TableName = tableName,
                        PrincipalTableName = principalTableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyColumnNotNamedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName)
        {
            var definition = RelationalStrings.LogColumnNameEmptyOnForeignKey;

            definition.Log(diagnostics, tableName, foreignKeyName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        TableName = tableName
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IndexFound(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string indexName,
            [CanBeNull] string tableName,
            bool? unique)
        {
            var definition = RelationalStrings.LogFoundIndex;

            definition.Log(diagnostics, indexName, tableName, unique);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        IndexName = indexName,
                        TableName = tableName,
                        Unique = unique
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyPrincipalColumnMissingWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Scaffolding> diagnostics,
            [CanBeNull] string foreignKeyName,
            [CanBeNull] string tableName,
            [CanBeNull] string principalColumnName,
            [CanBeNull] string principalTableName)
        {
            var definition = RelationalStrings.LogPrincipalColumnNotFound;

            definition.Log(diagnostics, foreignKeyName, tableName, principalColumnName, principalTableName);

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new
                    {
                        ForeignKeyName = foreignKeyName,
                        TableName = tableName,
                        PrincipalColumnName = principalColumnName,
                        PrincipalTableName = principalTableName
                    });
            }
        }
    }
}
