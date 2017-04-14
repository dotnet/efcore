// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
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
            Guid connectionId,
            [NotNull] DbCommand command,
            [NotNull] string executeMethod,
            Guid instanceId,
            long startTimestamp,
            bool async)
        {
            var eventId = RelationalEventId.CommandExecuting;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                var logData = CreateCommandLogData(diagnostics, command, 0, 0);

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
                    new RelationalDiagnosticSourceBeforeMessage
                    {
                        ConnectionId = connectionId,
                        Command = command,
                        ExecuteMethod = executeMethod,
                        InstanceId = instanceId,
                        Timestamp = startTimestamp,
                        IsAsync = async
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CommandExecuted(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Sql> diagnostics,
            Guid connectionId,
            [NotNull] DbCommand command,
            [NotNull] string executeMethod,
            [CanBeNull] object methodResult,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
            bool async = false)
        {
            var eventId = RelationalEventId.CommandExecuted;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Information))
            {
                var logData = CreateCommandLogData(diagnostics, command, startTimestamp, currentTimestamp);

                var elapsedMilliseconds = DeriveTimespan(startTimestamp, currentTimestamp);

                var message = RelationalStrings.RelationalLoggerExecutedCommand(
                    string.Format(CultureInfo.InvariantCulture, "{0:N0}", elapsedMilliseconds),
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
                    new RelationalDiagnosticSourceAfterMessage
                    {
                        ConnectionId = connectionId,
                        Command = command,
                        ExecuteMethod = executeMethod,
                        Result = methodResult,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        IsAsync = async
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CommandError(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Sql> diagnostics,
            Guid connectionId,
            [NotNull] DbCommand command,
            [NotNull] string executeMethod,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
            [NotNull] Exception exception,
            bool async)
        {
            var eventId = RelationalEventId.CommandError;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Error))
            {
                var logData = CreateCommandLogData(diagnostics, command, startTimestamp, currentTimestamp);

                var elapsedMilliseconds = DeriveTimespan(startTimestamp, currentTimestamp);

                var message = RelationalStrings.RelationalLoggerCommandFailed(
                    string.Format(CultureInfo.InvariantCulture, "{0:N0}", elapsedMilliseconds),
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
                    new RelationalDiagnosticSourceAfterMessage
                    {
                        ConnectionId = connectionId,
                        Command = command,
                        ExecuteMethod = executeMethod,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        Exception = exception,
                        IsAsync = async
                    });
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
            long startTimestamp,
            long currentTimestamp)
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
                DeriveTimespan(startTimestamp, currentTimestamp));

            return logData;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionOpening(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            Guid instanceId,
            long startTimestamp,
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        InstanceId = instanceId,
                        Timestamp = startTimestamp,
                        IsAsync = async
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionOpened(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        IsAsync = async
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionClosing(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            Guid instanceId,
            long startTimestamp,
            bool async)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        InstanceId = instanceId,
                        Timestamp = startTimestamp,
                        IsAsync = async
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConnectionClosed(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Connection> diagnostics,
            [NotNull] IRelationalConnection connection,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
            bool async)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        IsAsync = async
                    });
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
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        Exception = exception,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        IsAsync = async
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TransactionStarted(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        Transaction = transaction
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TransactionUsed(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        Transaction = transaction
                    });
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
            long startTimestamp,
            long currentTimestamp)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        Transaction = transaction,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp
                    });
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
            long startTimestamp,
            long currentTimestamp)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        Transaction = transaction,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void TransactionDisposed(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        Transaction = transaction
                    });
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
            [NotNull] string action,
            [NotNull] Exception exception,
            long startTimestamp,
            long currentTimestamp)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        Transaction = transaction,
                        Exception = exception,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void AmbientTransactionWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.Transaction> diagnostics,
            [NotNull] IRelationalConnection connection)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DataReaderDisposing(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Database.DataReader> diagnostics,
            [NotNull] IRelationalConnection connection,
            [NotNull] DbDataReader dataReader,
            int recordsAffected,
            long startTimestamp,
            long currentTimestamp)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        DataReader = dataReader,
                        RecordsAffected = recordsAffected,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrateUsingConnection(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] IRelationalConnection connection,
            [CanBeNull] string targetMigration)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Migrator = migrator,
                        Connection = connection.DbConnection,
                        ConnectionId = connection.ConnectionId,
                        TargetMigration = targetMigration
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationReverting(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration,
            [CanBeNull] string targetMigration)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Migrator = migrator,
                        Migration = migration,
                        TargetMigration = targetMigration
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MigrationApplying(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Migrations> diagnostics,
            [NotNull] IMigrator migrator,
            [NotNull] Migration migration,
            [CanBeNull] string targetMigration)
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Migrator = migrator,
                        Migration = migration,
                        TargetMigration = targetMigration
                    });
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Migrator = migrator,
                        Migration = migration,
                        FromMigration= fromMigration,
                        ToMigration = toMigration,
                        Idempotent = idempotent
                    });
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Migrator = migrator,
                        Migration = migration,
                        FromMigration = fromMigration,
                        ToMigration = toMigration,
                        Idempotent = idempotent
                    });
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
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
                diagnostics.DiagnosticSource.Write(eventId.Name,
                    new
                    {
                        Property = property
                    });
            }
        }

        private static readonly double _timestampToMilliseconds 
            = (double)TimeSpan.TicksPerSecond / (Stopwatch.Frequency * TimeSpan.TicksPerMillisecond);

        private static long DeriveTimespan(long startTimestamp, long currentTimestamp)
            => (long)((currentTimestamp - startTimestamp) * _timestampToMilliseconds);
    }
}
