// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

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
            DbCommand command, string executeMethod,
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

        public static void WriteConnectionOpened(this DiagnosticSource diagnosticSource, DbConnection connection)
        {
            if (diagnosticSource.IsEnabled(ConnectionOpened))
            {
                diagnosticSource.Write(ConnectionOpened, connection);
            }
        }

        public static void WriteConnectionClosed(this DiagnosticSource diagnosticSource, DbConnection connection)
        {
            if (diagnosticSource.IsEnabled(ConnectionClosed))
            {
                diagnosticSource.Write(ConnectionClosed, connection);
            }
        }

        public static void WriteTransactionStarted(this DiagnosticSource diagnosticSource, DbTransaction transaction)
        {
            if (diagnosticSource.IsEnabled(TransactionStarted))
            {
                diagnosticSource.Write(TransactionStarted, transaction);
            }
        }

        public static void WriteTransactionCommit(this DiagnosticSource diagnosticSource, DbTransaction transaction)
        {
            if (diagnosticSource.IsEnabled(TransactionCommitted))
            {
                diagnosticSource.Write(TransactionCommitted, transaction);
            }
        }

        public static void WriteTransactionRollback(this DiagnosticSource diagnosticSource, DbTransaction transaction)
        {
            if (diagnosticSource.IsEnabled(TransactionRolledback))
            {
                diagnosticSource.Write(TransactionRolledback, transaction);
            }
        }

        public static void WriteTransactionDisposed(this DiagnosticSource diagnosticSource, DbTransaction transaction)
        {
            if (diagnosticSource.IsEnabled(TransactionDisposed))
            {
                diagnosticSource.Write(TransactionDisposed, transaction);
            }
        }

        public static void WriteDataReaderDisposing(this DiagnosticSource diagnosticSource, DbDataReader dataReader)
        {
            if (diagnosticSource.IsEnabled(DataReaderDisposing))
            {
                diagnosticSource.Write(DataReaderDisposing, dataReader);
            }
        }
    }
}
