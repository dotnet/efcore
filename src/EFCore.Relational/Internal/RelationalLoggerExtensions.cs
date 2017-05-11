// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Sql> diagnostics, 
            [NotNull] DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId, 
            Guid connectionId, 
            bool async, 
            DateTimeOffset startTime)
        {
            var eventId = RelationalEventId.CommandExecuting;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                var logData = CreateCommandLogData(diagnostics, command, TimeSpan.Zero);

                var message = RelationalStrings.RelationalLoggerExecutingCommand(
                    logData.Parameters
                        // Interpolation okay here because value is always a string.
                        .Select(p => $"{p.Name}={p.FormatParameter()}")
                        .Join(),
                    logData.CommandType,
                    logData.CommandTimeout,
                    Environment.NewLine,
                    logData.CommandText);

                diagnostics.Logger.LogDebug(eventId, message);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new CommandData(
                        command,
                        executeMethod,
                        commandId,
                        connectionId,
                        async,
                        startTime));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CommandExecuted(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Sql> diagnostics,
            [NotNull] DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            [CanBeNull] object methodResult,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var eventId = RelationalEventId.CommandExecuted;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Information))
            {
                var logData = CreateCommandLogData(diagnostics, command, duration);
                
                var message = RelationalStrings.RelationalLoggerExecutedCommand(
                    string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.Milliseconds),
                    logData.Parameters
                        // Interpolation okay here because value is always a string.
                        .Select(p => $"{p.Name}={p.FormatParameter()}")
                        .Join(),
                    logData.CommandType,
                    logData.CommandTimeout,
                    Environment.NewLine,
                    logData.CommandText);

                diagnostics.Logger.Log(LogLevel.Information, eventId, logData, null, (_, __) => message);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new CommandExecutedData(
                        command,
                        executeMethod,
                        commandId,
                        connectionId,
                        methodResult,
                        async,
                        startTime,
                        duration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CommandError(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Sql> diagnostics,
            [NotNull] DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            [NotNull] Exception exception,
            bool async,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var eventId = RelationalEventId.CommandError;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Error))
            {
                var logData = CreateCommandLogData(diagnostics, command, duration);

                var message = RelationalStrings.RelationalLoggerCommandFailed(
                    string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.Milliseconds),
                    logData.Parameters
                        // Interpolation okay here because value is always a string.
                        .Select(p => $"{p.Name}={p.FormatParameter()}")
                        .Join(),
                    logData.CommandType,
                    logData.CommandTimeout,
                    Environment.NewLine,
                    logData.CommandText);

                diagnostics.Logger.Log(LogLevel.Error, eventId, logData, null, (_, __) => message);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new CommandErrorData(
                        command,
                        executeMethod,
                        commandId,
                        connectionId,
                        exception,
                        async,
                        startTime,
                        duration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
#pragma warning disable 618
        private static DbCommandLogData CreateCommandLogData(
#pragma warning restore 618
            this IDiagnosticsLogger<LoggerCategory.Database.Sql> diagnostics,
            DbCommand command,
            TimeSpan duration)
        {
            var logParameterValues
                = command.Parameters.Count > 0
                  && diagnostics.Logger.ShouldLogSensitiveData(diagnostics);

#pragma warning disable 618
            var logData = new DbCommandLogData(
#pragma warning restore 618
                command.CommandText.TrimEnd(),
                command.CommandType,
                command.CommandTimeout,
                command.Parameters
                    .Cast<DbParameter>()
                    .Select(
                        p => new DbParameterLogData(
                            p.ParameterName,
                            logParameterValues ? p.Value : "?",
                            logParameterValues,
                            p.Direction,
                            p.DbType,
                            p.IsNullable,
                            p.Size,
                            p.Precision,
                            p.Scale))
                    .ToList(),
                duration.Milliseconds);

            return logData;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionOpening(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            bool async)
        {
            var eventId = RelationalEventId.ConnectionOpening;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.RelationalLoggerOpeningConnection(
                        connection.DbConnection.Database,
                        connection.DbConnection.DataSource));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new ConnectionData(
                        connection.DbConnection, 
                        connection.ConnectionId, 
                        async, 
                        startTime));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionOpened(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool async)
        {
            var eventId = RelationalEventId.ConnectionOpened;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.RelationalLoggerOpenedConnection(
                        connection.DbConnection.Database,
                        connection.DbConnection.DataSource));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new ConnectionEndData(
                        connection.DbConnection,
                        connection.ConnectionId,
                        async,
                        startTime,
                        duration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionClosing(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime)
        {
            var eventId = RelationalEventId.ConnectionClosing;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.RelationalLoggerClosingConnection(
                        connection.DbConnection.Database,
                        connection.DbConnection.DataSource));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new ConnectionData(
                        connection.DbConnection, 
                        connection.ConnectionId, 
                        false,
                        startTime));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionClosed(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var eventId = RelationalEventId.ConnectionClosed;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.RelationalLoggerClosedConnection(
                        connection.DbConnection.Database,
                        connection.DbConnection.DataSource));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new ConnectionEndData(
                        connection.DbConnection,
                        connection.ConnectionId,
                        false,
                        startTime,
                        duration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionError(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool async)
        {
            var eventId = RelationalEventId.ConnectionError;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Error))
            {
                diagnostics.Logger.LogError(
                    eventId,
                    exception,
                    RelationalStrings.RelationalLoggerConnectionError(
                        connection.DbConnection.Database,
                        connection.DbConnection.DataSource));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new ConnectionErrorData(
                        connection.DbConnection,
                        connection.ConnectionId,
                        exception,
                        async,
                        startTime,
                        duration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TransactionStarted(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startDate)
        {
            var eventId = RelationalEventId.TransactionStarted;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.RelationalLoggerBeginningTransaction(transaction.IsolationLevel.ToString("G")));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new TransactionData(
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
        public static void TransactionUsed(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startDate)
        {
            var eventId = RelationalEventId.TransactionUsed;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.RelationalLoggerUsingTransaction(transaction.IsolationLevel.ToString("G")));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new TransactionData(
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
        public static void TransactionCommitted(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var eventId = RelationalEventId.TransactionCommitted;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.RelationalLoggerCommittingTransaction);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new TransactionEndData(
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var eventId = RelationalEventId.TransactionRolledBack;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.RelationalLoggerRollingbackTransaction);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new TransactionEndData(
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            DateTimeOffset startDate)
        {
            var eventId = RelationalEventId.TransactionDisposed;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.RelationalLoggerDisposingTransaction);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new TransactionData(
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            Guid transactionId,
            [NotNull] string action,
            [NotNull] Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var eventId = RelationalEventId.TransactionError;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Error))
            {
                diagnostics.Logger.LogError(
                    eventId,
                    exception,
                    RelationalStrings.RelationalLoggerTransactionError);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new TransactionErrorData(
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            DateTimeOffset startDate)
        {
            var eventId = RelationalEventId.AmbientTransactionWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalStrings.AmbientTransaction);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new ConnectionData(
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.DataReader> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [NotNull] DbDataReader dataReader,
            Guid commandId,
            int recordsAffected,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var eventId = RelationalEventId.DataReaderDisposing;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.DisposingDataReader);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new DataReaderDisposingData(
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
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] IRelationalConnection connection)
        {
            var eventId = RelationalEventId.MigrateUsingConnection;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                var dbConnection = connection.DbConnection;
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.UsingConnection(dbConnection.Database, dbConnection.DataSource));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new MigratorConnectionData(
                        migrator,
                        connection.DbConnection,
                        connection.ConnectionId));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationReverting(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration)
        {
            var eventId = RelationalEventId.MigrationReverting;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Information))
            {
                diagnostics.Logger.LogInformation(
                    eventId,
                    RelationalStrings.RevertingMigration(migration.GetId()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new MigrationData(
                        migrator,
                        migration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationApplying(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration)
        {
            var eventId = RelationalEventId.MigrationApplying;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Information))
            {
                diagnostics.Logger.LogInformation(
                    eventId,
                    RelationalStrings.ApplyingMigration(migration.GetId()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new MigrationData(
                        migrator,
                        migration));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationGeneratingDownScript(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration,
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent)
        {
            var eventId = RelationalEventId.MigrationGeneratingDownScript;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.GeneratingDown(migration.GetId()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new MigrationScriptingData(
                        migrator,
                        migration,
                        fromMigration,
                        toMigration,
                        idempotent));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationGeneratingUpScript(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration,
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent)
        {
            var eventId = RelationalEventId.MigrationGeneratingUpScript;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    RelationalStrings.GeneratingUp(migration.GetId()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new MigrationScriptingData(
                        migrator,
                        migration,
                        fromMigration,
                        toMigration,
                        idempotent));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryClientEvaluationWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel,
            [NotNull] object expression)
        {
            var eventId = RelationalEventId.QueryClientEvaluationWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalStrings.ClientEvalWarning(expression.ToString()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        QueryModel = queryModel,
                        Expression = expression
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryPossibleUnintendedUseOfEqualsWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Query> diagnostics,
            [NotNull] MethodCallExpression methodCallExpression,
            [NotNull] Expression argument)
        {
            var eventId = RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalStrings.PossibleUnintendedUseOfEquals(
                        methodCallExpression.Object.ToString(),
                        argument));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        MethodCallExpression = methodCallExpression,
                        Argument = argument
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ModelValidationKeyDefaultValueWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var eventId = RelationalEventId.ModelValidationKeyDefaultValueWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    RelationalStrings.KeyHasDefaultValue(property.Name, property.DeclaringEntityType.DisplayName()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        Property = property
                    });
            }
        }
    }
}
