// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ApplicationInsights
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DiagnosticEventForwarder : IObserver<DiagnosticListener>, IDisposable
    {
        private const string DependencyTypeName = "SQL";

        private readonly object _sync = new object();

        private readonly TelemetryClient _telemetryClient;

        private IDisposable _efSubscription;
        private IDisposable _listenerSubscription;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DiagnosticEventForwarder([NotNull] TelemetryClient telemetryClient)
        {
            Check.NotNull(telemetryClient, nameof(telemetryClient));

            _telemetryClient = telemetryClient;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Dispose()
        {
            lock (_sync)
            {
                _efSubscription?.Dispose();
                _efSubscription = null;
                _listenerSubscription?.Dispose();
                _listenerSubscription = null;
            }
        }

        void IObserver<DiagnosticListener>.OnNext(DiagnosticListener diagnosticListener)
        {
            if (diagnosticListener.Name == DbLoggerCategory.Root)
            {
                lock (_sync)
                {
                    _efSubscription = diagnosticListener.Subscribe(new EventObserver(_telemetryClient));
                }
            }
        }

        void IObserver<DiagnosticListener>.OnCompleted()
        {
        }

        void IObserver<DiagnosticListener>.OnError(Exception exception)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Start()
        {
            lock (_sync)
            {
                if (_listenerSubscription == null)
                {
                    _listenerSubscription = DiagnosticListener.AllListeners.Subscribe(this);
                }
            }
        }

        private sealed class EventObserver : IObserver<KeyValuePair<string, object>>
        {
            private readonly TelemetryClient _telemetryClient;

            public EventObserver(TelemetryClient telemetryClient) => _telemetryClient = telemetryClient;

            public void OnNext(KeyValuePair<string, object> value)
            {
                if (!_telemetryClient.IsEnabled())
                {
                    return;
                }

                var eventName = value.Key;
                var eventData = value.Value;

                Debug.Assert(eventName.StartsWith(DbLoggerCategory.Root, StringComparison.Ordinal));
                Debug.Assert(eventData != null);

                switch (eventData)
                {
                    case CommandErrorData commandErrorData:
                    {
                        TrackCommandDependency(eventName, commandErrorData, false);

                        break;
                    }
                    case CommandExecutedData commandExecutedData:
                    {
                        TrackCommandDependency(eventName, commandExecutedData, true);

                        break;
                    }
                    case CommandEndData commandEndData:
                    {
                        TrackCommandDependency(eventName, commandEndData, true);

                        break;
                    }
                    case ConnectionErrorData connectionErrorData:
                    {
                        TrackConnectionDependency(eventName, connectionErrorData, false);

                        break;
                    }
                    case ConnectionEndData connectionEndData:
                    {
                        TrackConnectionDependency(eventName, connectionEndData, true);

                        break;
                    }
                    case TransactionErrorData transactionErrorData:
                    {
                        TrackTransactionDependency(eventName, transactionErrorData, false);

                        break;
                    }
                    case TransactionEndData transactionEndData:
                    {
                        TrackTransactionDependency(eventName, transactionEndData, true);

                        break;
                    }
                    case DataReaderDisposingData dataReaderDisposingData:
                    {
                        TrackReaderDependency(eventName, dataReaderDisposingData);

                        break;
                    }
                }
            }

            private void TrackCommandDependency(
                string eventName, CommandEndData commandEndData, bool success)
            {
                var command = commandEndData.Command;

                var dependencyTelemetry
                    = new DependencyTelemetry(
                        DependencyTypeName,
                        command.Connection.Database,
                        eventName,
                        command.CommandText,
                        commandEndData.StartTime,
                        commandEndData.Duration,
                        success: success,
                        resultCode: null);

                var properties = dependencyTelemetry.Properties;

                properties[nameof(commandEndData.ConnectionId)] = commandEndData.ConnectionId.ToString();
                properties[nameof(commandEndData.CommandId)] = commandEndData.CommandId.ToString();
                properties[nameof(commandEndData.IsAsync)] = commandEndData.IsAsync.ToString();
                properties[nameof(commandEndData.ExecuteMethod)] = commandEndData.ExecuteMethod.ToString();
                properties[nameof(command.CommandText)] = command.CommandText;

                _telemetryClient.TrackDependency(dependencyTelemetry);
            }

            private void TrackConnectionDependency(
                string eventName, ConnectionEndData connectionEndData, bool success)
            {
                var connection = connectionEndData.Connection;

                var dependencyTelemetry
                    = new DependencyTelemetry(
                        DependencyTypeName,
                        connectionEndData.Connection.Database,
                        eventName,
                        $"[{nameof(connection.Database)}='{connection.Database}',"
                        + $" {nameof(connection.DataSource)}='{connection.DataSource}',"
                        + $" {nameof(connection.ConnectionTimeout)}={connection.ConnectionTimeout}]",
                        connectionEndData.StartTime,
                        connectionEndData.Duration,
                        success: success,
                        resultCode: null);

                var properties = dependencyTelemetry.Properties;

                properties[nameof(connectionEndData.ConnectionId)] = connectionEndData.ConnectionId.ToString();
                properties[nameof(connectionEndData.IsAsync)] = connectionEndData.IsAsync.ToString();
                properties[nameof(connection.Database)] = connection.Database;
                properties[nameof(connection.DataSource)] = connection.DataSource;
                properties[nameof(connection.ConnectionTimeout)] = connection.ConnectionTimeout.ToString();

                _telemetryClient.TrackDependency(dependencyTelemetry);
            }

            private void TrackTransactionDependency(
                string eventName, TransactionEndData transactionEndData, bool success)
            {
                var transaction = transactionEndData.Transaction;

                var dependencyTelemetry
                    = new DependencyTelemetry(
                        DependencyTypeName,
                        transaction.Connection.Database,
                        eventName,
                        $"{nameof(transactionEndData.Transaction.IsolationLevel)}={transaction.IsolationLevel}",
                        transactionEndData.StartTime,
                        transactionEndData.Duration,
                        success: success,
                        resultCode: null);

                var properties = dependencyTelemetry.Properties;

                properties[nameof(transactionEndData.ConnectionId)] = transactionEndData.ConnectionId.ToString();
                properties[nameof(transactionEndData.TransactionId)] = transactionEndData.TransactionId.ToString();
                properties[nameof(transactionEndData.Transaction.IsolationLevel)] = transactionEndData.Transaction.IsolationLevel.ToString();

                _telemetryClient.TrackDependency(dependencyTelemetry);
            }

            private void TrackReaderDependency(string eventName, DataReaderDisposingData dataReaderDisposingData)
            {
                var command = dataReaderDisposingData.Command;

                var dependencyTelemetry
                    = new DependencyTelemetry(
                        DependencyTypeName,
                        command.Connection.Database,
                        eventName,
                        command.CommandText,
                        dataReaderDisposingData.StartTime,
                        dataReaderDisposingData.Duration,
                        success: true,
                        resultCode: null);

                var properties = dependencyTelemetry.Properties;

                properties[nameof(dataReaderDisposingData.ConnectionId)] = dataReaderDisposingData.ConnectionId.ToString();
                properties[nameof(dataReaderDisposingData.CommandId)] = dataReaderDisposingData.CommandId.ToString();
                properties[nameof(dataReaderDisposingData.RecordsAffected)] = dataReaderDisposingData.RecordsAffected.ToString();
                properties[nameof(command.CommandText)] = command.CommandText;

                _telemetryClient.TrackDependency(dependencyTelemetry);
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }
        }
    }
}
