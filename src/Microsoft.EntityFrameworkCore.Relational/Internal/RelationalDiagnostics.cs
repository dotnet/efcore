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

        public const string ConnectionOpened = NamePrefix + nameof(ConnectionOpened);
        public const string ConnectionClosed = NamePrefix + nameof(ConnectionClosed);

        public const string TransactionStarted = NamePrefix + nameof(TransactionStarted);
        public const string TransactionCommitted = NamePrefix + nameof(TransactionCommitted);
        public const string TransactionRolledback = NamePrefix + nameof(TransactionRolledback);
        public const string TransactionDisposed = NamePrefix + nameof(TransactionDisposed);

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

        public static void WriteConnectionOpened(this DiagnosticSource diagnosticSource, DbConnection connection, Guid connectionId)
        {
            if (diagnosticSource.IsEnabled(ConnectionOpened))
            {
                diagnosticSource.Write(ConnectionOpened, new
                {
                    Connection = connection,
                    ConnectionId = connectionId
                });
            }
        }

        public static void WriteConnectionClosed(this DiagnosticSource diagnosticSource, DbConnection connection, Guid connectionId)
        {
            if (diagnosticSource.IsEnabled(ConnectionClosed))
            {
                diagnosticSource.Write(ConnectionClosed, new
                {
                    Connection = connection,
                    ConnectionId = connectionId
                });
            }
        }

        public static void WriteTransactionStarted(this DiagnosticSource diagnosticSource, DbTransaction transaction, Guid connectionId)
        {
            if (diagnosticSource.IsEnabled(TransactionStarted))
            {
                diagnosticSource.Write(TransactionStarted, new
                {
                    Transaction = transaction,
                    ConnectionId = connectionId
                });
            }
        }

        public static void WriteTransactionCommit(this DiagnosticSource diagnosticSource, DbTransaction transaction, Guid connectionId)
        {
            if (diagnosticSource.IsEnabled(TransactionCommitted))
            {
                diagnosticSource.Write(TransactionCommitted, new
                {
                    Transaction = transaction,
                    ConnectionId = connectionId
                });
            }
        }

        public static void WriteTransactionRollback(this DiagnosticSource diagnosticSource, DbTransaction transaction, Guid connectionId)
        {
            if (diagnosticSource.IsEnabled(TransactionRolledback))
            {
                diagnosticSource.Write(TransactionRolledback, new
                {
                    Transaction = transaction,
                    ConnectionId = connectionId
                });
            }
        }

        public static void WriteTransactionDisposed(this DiagnosticSource diagnosticSource, DbTransaction transaction, Guid connectionId)
        {
            if (diagnosticSource.IsEnabled(TransactionDisposed))
            {
                diagnosticSource.Write(TransactionDisposed, new
                {
                    Transaction = transaction,
                    ConnectionId = connectionId
                });
            }
        }

        public static void WriteDataReaderDisposing(this DiagnosticSource diagnosticSource, DbDataReader dataReader, Guid connectionId)
        {
            if (diagnosticSource.IsEnabled(DataReaderDisposing))
            {
                diagnosticSource.Write(DataReaderDisposing, new
                {
                    DataReader = dataReader,
                    ConnectionId = connectionId
                });
            }
        }
    }
}
