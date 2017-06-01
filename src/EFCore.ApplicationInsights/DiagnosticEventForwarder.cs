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
            if (diagnosticListener.Name == DbLoggerCategory.Name)
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

                Debug.Assert(eventName.StartsWith(DbLoggerCategory.Name, StringComparison.Ordinal));
                Debug.Assert(eventData != null);

                switch (eventData)
                {
                    case CommandErrorEventData commandErrorData:
                    {
                        TrackCommandDependency(eventName, commandErrorData, false);

                        break;
                    }
                    case CommandExecutedEventData commandExecutedData:
                    {
                        TrackCommandDependency(eventName, commandExecutedData, true);

                        break;
                    }
                    case CommandEndEventData commandEndData:
                    {
                        TrackCommandDependency(eventName, commandEndData, true);

                        break;
                    }
                    case ConnectionErrorEventData connectionErrorData:
                    {
                        TrackConnectionDependency(eventName, connectionErrorData, false);

                        break;
                    }
                    case ConnectionEndEventData connectionEndData:
                    {
                        TrackConnectionDependency(eventName, connectionEndData, true);

                        break;
                    }
                    case TransactionErrorEventData transactionErrorData:
                    {
                        TrackTransactionDependency(eventName, transactionErrorData, false);

                        break;
                    }
                    case TransactionEndEventData transactionEndData:
                    {
                        TrackTransactionDependency(eventName, transactionEndData, true);

                        break;
                    }
                    case DataReaderDisposingEventData dataReaderDisposingData:
                    {
                        TrackReaderDependency(eventName, dataReaderDisposingData);

                        break;
                    }
                }
            }

            private void TrackCommandDependency(
                string eventName, CommandEndEventData commandEndEventData, bool success)
            {
                var command = commandEndEventData.Command;

                var dependencyTelemetry
                    = new DependencyTelemetry(
                        DependencyTypeName,
                        command.Connection.Database,
                        eventName,
                        command.CommandText,
                        commandEndEventData.StartTime,
                        commandEndEventData.Duration,
                        success: success,
                        resultCode: null);

                var properties = dependencyTelemetry.Properties;

                properties[nameof(commandEndEventData.ConnectionId)] = commandEndEventData.ConnectionId.ToString();
                properties[nameof(commandEndEventData.CommandId)] = commandEndEventData.CommandId.ToString();
                properties[nameof(commandEndEventData.IsAsync)] = commandEndEventData.IsAsync.ToString();
                properties[nameof(commandEndEventData.ExecuteMethod)] = commandEndEventData.ExecuteMethod.ToString();
                properties[nameof(command.CommandText)] = command.CommandText;

                _telemetryClient.TrackDependency(dependencyTelemetry);
            }

            private void TrackConnectionDependency(
                string eventName, ConnectionEndEventData connectionEndEventData, bool success)
            {
                var connection = connectionEndEventData.Connection;

                var dependencyTelemetry
                    = new DependencyTelemetry(
                        DependencyTypeName,
                        connectionEndEventData.Connection.Database,
                        eventName,
                        $"[{nameof(connection.Database)}='{connection.Database}',"
                        + $" {nameof(connection.DataSource)}='{connection.DataSource}',"
                        + $" {nameof(connection.ConnectionTimeout)}={connection.ConnectionTimeout}]",
                        connectionEndEventData.StartTime,
                        connectionEndEventData.Duration,
                        success: success,
                        resultCode: null);

                var properties = dependencyTelemetry.Properties;

                properties[nameof(connectionEndEventData.ConnectionId)] = connectionEndEventData.ConnectionId.ToString();
                properties[nameof(connectionEndEventData.IsAsync)] = connectionEndEventData.IsAsync.ToString();
                properties[nameof(connection.Database)] = connection.Database;
                properties[nameof(connection.DataSource)] = connection.DataSource;
                properties[nameof(connection.ConnectionTimeout)] = connection.ConnectionTimeout.ToString();

                _telemetryClient.TrackDependency(dependencyTelemetry);
            }

            private void TrackTransactionDependency(
                string eventName, TransactionEndEventData transactionEndEventData, bool success)
            {
                var transaction = transactionEndEventData.Transaction;

                var dependencyTelemetry
                    = new DependencyTelemetry(
                        DependencyTypeName,
                        transaction.Connection.Database,
                        eventName,
                        $"{nameof(transactionEndEventData.Transaction.IsolationLevel)}={transaction.IsolationLevel}",
                        transactionEndEventData.StartTime,
                        transactionEndEventData.Duration,
                        success: success,
                        resultCode: null);

                var properties = dependencyTelemetry.Properties;

                properties[nameof(transactionEndEventData.ConnectionId)] = transactionEndEventData.ConnectionId.ToString();
                properties[nameof(transactionEndEventData.TransactionId)] = transactionEndEventData.TransactionId.ToString();
                properties[nameof(transactionEndEventData.Transaction.IsolationLevel)] = transactionEndEventData.Transaction.IsolationLevel.ToString();

                _telemetryClient.TrackDependency(dependencyTelemetry);
            }

            private void TrackReaderDependency(string eventName, DataReaderDisposingEventData dataReaderDisposingEventData)
            {
                var command = dataReaderDisposingEventData.Command;

                var dependencyTelemetry
                    = new DependencyTelemetry(
                        DependencyTypeName,
                        command.Connection.Database,
                        eventName,
                        command.CommandText,
                        dataReaderDisposingEventData.StartTime,
                        dataReaderDisposingEventData.Duration,
                        success: true,
                        resultCode: null);

                var properties = dependencyTelemetry.Properties;

                properties[nameof(dataReaderDisposingEventData.ConnectionId)] = dataReaderDisposingEventData.ConnectionId.ToString();
                properties[nameof(dataReaderDisposingEventData.CommandId)] = dataReaderDisposingEventData.CommandId.ToString();
                properties[nameof(dataReaderDisposingEventData.RecordsAffected)] = dataReaderDisposingEventData.RecordsAffected.ToString();
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
