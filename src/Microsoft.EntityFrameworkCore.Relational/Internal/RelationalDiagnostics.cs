// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Internal
{
    internal static class RelationalDiagnostics
    {
        private const string NamePrefix = "Microsoft.EntityFrameworkCore.";

        public const string BeforeExecuteCommand = NamePrefix + nameof(BeforeExecuteCommand);
        public const string AfterExecuteCommand = NamePrefix + nameof(AfterExecuteCommand);
        public const string CommandExecutionError = NamePrefix + nameof(CommandExecutionError);

        public const string ConnectionOpening = NamePrefix + nameof(ConnectionOpening);
        public const string ConnectionOpened = NamePrefix + nameof(ConnectionOpened);
        public const string ConnectionClosing = NamePrefix + nameof(ConnectionClosing);
        public const string ConnectionClosed = NamePrefix + nameof(ConnectionClosed);
        public const string ConnectionError = NamePrefix + nameof(ConnectionClosed);

        public const string TransactionStarted = NamePrefix + nameof(TransactionStarted);
        public const string TransactionCommitted = NamePrefix + nameof(TransactionCommitted);
        public const string TransactionRolledback = NamePrefix + nameof(TransactionRolledback);
        public const string TransactionDisposed = NamePrefix + nameof(TransactionDisposed);
        public const string TransactionError = NamePrefix + nameof(TransactionError);

        public const string DataReaderDisposing = NamePrefix + nameof(DataReaderDisposing);

        public static void WriteCommandBefore(
            this DiagnosticSource diagnosticSource,
            Guid connectionId,
            DbCommand command,
            string executeMethod,
            Guid instanceId,
            long startTimestamp,
            bool async)
        {
            if (diagnosticSource.IsEnabled(BeforeExecuteCommand))
            {
                diagnosticSource.Write(
                    BeforeExecuteCommand,
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

        public static void WriteCommandAfter(
            this DiagnosticSource diagnosticSource,
            Guid connectionId,
            DbCommand command,
            string executeMethod,
            object methodResult,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
            bool async = false)
        {
            if (diagnosticSource.IsEnabled(AfterExecuteCommand))
            {
                diagnosticSource.Write(
                    AfterExecuteCommand,
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

        public static void WriteCommandError(
            this DiagnosticSource diagnosticSource,
            Guid connectionId,
            DbCommand command,
            string executeMethod,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
            Exception exception,
            bool async)
        {
            if (diagnosticSource.IsEnabled(CommandExecutionError))
            {
                diagnosticSource.Write(
                    CommandExecutionError,
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

        public static void WriteConnectionOpening(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            Guid instanceId,
            long startTimestamp,
            bool async)
        {
            if (diagnosticSource.IsEnabled(ConnectionOpening))
            {
                diagnosticSource.Write(ConnectionOpening,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        InstanceId = instanceId,
                        Timestamp = startTimestamp,
                        IsAsync = async
                    });
            }
        }

        public static void WriteConnectionOpened(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
            bool async)
        {
            if (diagnosticSource.IsEnabled(ConnectionOpened))
            {
                diagnosticSource.Write(ConnectionOpened,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        IsAsync = async
                    });
            }
        }

        public static void WriteConnectionClosing(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            Guid instanceId,
            long startTimestamp,
            bool async)
        {
            if (diagnosticSource.IsEnabled(ConnectionClosing))
            {
                diagnosticSource.Write(ConnectionClosing,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        InstanceId = instanceId,
                        Timestamp = startTimestamp,
                        IsAsync = async
                    });
            }
        }

        public static void WriteConnectionClosed(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
            bool async)
        {
            if (diagnosticSource.IsEnabled(ConnectionClosed))
            {
                diagnosticSource.Write(ConnectionClosed,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        IsAsync = async
                    });
            }
        }

        public static void WriteConnectionError(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            Exception exception,
            Guid instanceId,
            long startTimestamp,
            long currentTimestamp,
            bool async)
        {
            if (diagnosticSource.IsEnabled(ConnectionError))
            {
                diagnosticSource.Write(ConnectionError,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        Exception = exception,
                        InstanceId = instanceId,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp,
                        IsAsync = async
                    });
            }
        }

        public static void WriteTransactionStarted(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            DbTransaction transaction)
        {
            if (diagnosticSource.IsEnabled(TransactionStarted))
            {
                diagnosticSource.Write(TransactionStarted,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        Transaction = transaction
                    });
            }
        }

        public static void WriteTransactionCommit(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            DbTransaction transaction,
            long startTimestamp,
            long currentTimestamp)
        {
            if (diagnosticSource.IsEnabled(TransactionCommitted))
            {
                diagnosticSource.Write(TransactionCommitted,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        Transaction = transaction,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp
                    });
            }
        }

        public static void WriteTransactionRollback(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            DbTransaction transaction,
            long startTimestamp,
            long currentTimestamp)
        {
            if (diagnosticSource.IsEnabled(TransactionRolledback))
            {
                diagnosticSource.Write(TransactionRolledback,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        Transaction = transaction,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp
                    });
            }
        }

        public static void WriteTransactionDisposed(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            DbTransaction transaction)
        {
            if (diagnosticSource.IsEnabled(TransactionDisposed))
            {
                diagnosticSource.Write(TransactionDisposed,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        Transaction = transaction
                    });
            }
        }

        public static void WriteTransactionError(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            DbTransaction transaction,
            string action,
            Exception exception,
            long startTimestamp,
            long currentTimestamp)
        {
            if (diagnosticSource.IsEnabled(TransactionError))
            {
                diagnosticSource.Write(TransactionError,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        Transaction = transaction,
                        Exception = exception,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp
                    });
            }
        }

        public static void WriteDataReaderDisposing(
            this DiagnosticSource diagnosticSource,
            DbConnection connection,
            Guid connectionId,
            DbDataReader dataReader,
            int recordsAffected,
            long startTimestamp,
            long currentTimestamp)
        {
            if (diagnosticSource.IsEnabled(DataReaderDisposing))
            {
                diagnosticSource.Write(DataReaderDisposing,
                    new
                    {
                        Connection = connection,
                        ConnectionId = connectionId,
                        DataReader = dataReader,
                        RecordsAffected = recordsAffected,
                        Timestamp = currentTimestamp,
                        Duration = currentTimestamp - startTimestamp
                    });
            }
        }
    }
}
