// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

#pragma warning disable EF1001

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalConnectionDiagnosticsLogger
        : DiagnosticsLogger<DbLoggerCategory.Database.Connection>, IRelationalConnectionDiagnosticsLogger
    {
        private DateTimeOffset _suppressOpenExpiration;
        private DateTimeOffset _suppressCloseExpiration;

        private readonly TimeSpan _loggingCacheTime;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RelationalConnectionDiagnosticsLogger(
            ILoggerFactory loggerFactory,
            ILoggingOptions loggingOptions,
            DiagnosticSource diagnosticSource,
            LoggingDefinitions loggingDefinitions,
            IDbContextLogger contextLogger,
            IDbContextOptions contextOptions,
            IInterceptors? interceptors = null)
            : base(loggerFactory, loggingOptions, diagnosticSource, loggingDefinitions, contextLogger, interceptors)
        {
            _loggingCacheTime = contextOptions.FindExtension<CoreOptionsExtension>()?.LoggingCacheTime ??
                                      CoreOptionsExtension.DefaultLoggingCacheTime;
        }

        #region ConnectionOpening

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InterceptionResult ConnectionOpening(
            IRelationalConnection connection,
            DateTimeOffset startTime)
        {
            _suppressOpenExpiration = startTime + _loggingCacheTime;

            var definition = RelationalResources.LogOpeningConnection(this);

            if (ShouldLog(definition))
            {
                _suppressOpenExpiration = default;

                definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
            }

            if (NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                _suppressOpenExpiration = default;

                var eventData = BroadcastConnectionOpening(
                    connection,
                    startTime,
                    definition,
                    async: false,
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ValueTask<InterceptionResult> ConnectionOpeningAsync(
            IRelationalConnection connection,
            DateTimeOffset startTime,
            CancellationToken cancellationToken)
        {
            _suppressOpenExpiration = startTime + _loggingCacheTime;

            var definition = RelationalResources.LogOpeningConnection(this);

            if (ShouldLog(definition))
            {
                _suppressOpenExpiration = default;

                definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
            }

            if (NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                _suppressOpenExpiration = default;

                var eventData = BroadcastConnectionOpening(
                    connection,
                    startTime,
                    definition,
                    async: true,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                if (interceptor != null)
                {
                    return interceptor.ConnectionOpeningAsync(connection.DbConnection, eventData, default, cancellationToken);
                }
            }

            return default;
        }

        private ConnectionEventData BroadcastConnectionOpening(
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

            DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;

            static string ConnectionOpening(EventDefinitionBase definition, EventData payload)
            {
                var d = (EventDefinition<string, string>)definition;
                var p = (ConnectionEventData)payload;
                return d.GenerateMessage(
                    p.Connection.Database,
                    p.Connection.DataSource);
            }
        }

        #endregion ConnectionOpening

        #region ConnectionOpened

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ConnectionOpened(
            IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogOpenedConnection(this);

            if (ShouldLog(definition))
            {
                _suppressOpenExpiration = default;

                definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
            }

            if (NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                _suppressOpenExpiration = default;

                var eventData = BroadcastConnectionOpened(
                    connection,
                    async: false,
                    startTime,
                    duration,
                    definition,
                    diagnosticSourceEnabled,
                    simpleLogEnabled);

                interceptor?.ConnectionOpened(connection.DbConnection, eventData);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task ConnectionOpenedAsync(
            IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration,
            CancellationToken cancellationToken = default)
        {
            var definition = RelationalResources.LogOpenedConnection(this);

            if (ShouldLog(definition))
            {
                _suppressOpenExpiration = default;

                definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
            }

            if (NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                _suppressOpenExpiration = default;

                var eventData = BroadcastConnectionOpened(
                    connection,
                    async: true,
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

        private ConnectionEndEventData BroadcastConnectionOpened(
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

            DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;

            static string ConnectionOpened(EventDefinitionBase definition, EventData payload)
            {
                var d = (EventDefinition<string, string>)definition;
                var p = (ConnectionEndEventData)payload;
                return d.GenerateMessage(
                    p.Connection.Database,
                    p.Connection.DataSource);
            }
        }

        #endregion ConnectionOpened

        #region ConnectionClosing

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InterceptionResult ConnectionClosing(
            IRelationalConnection connection,
            DateTimeOffset startTime)
        {
            _suppressCloseExpiration = startTime + _loggingCacheTime;

            var definition = RelationalResources.LogClosingConnection(this);

            if (ShouldLog(definition))
            {
                _suppressCloseExpiration = default;

                definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
            }

            if (NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                _suppressCloseExpiration = default;

                var eventData = BroadcastConnectionClosing(
                    connection,
                    startTime,
                    async: false,
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ValueTask<InterceptionResult> ConnectionClosingAsync(
            IRelationalConnection connection,
            DateTimeOffset startTime)
        {
            _suppressCloseExpiration = startTime + _loggingCacheTime;

            var definition = RelationalResources.LogClosingConnection(this);

            if (ShouldLog(definition))
            {
                _suppressCloseExpiration = default;

                definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
            }

            if (NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                _suppressCloseExpiration = default;

                var eventData = BroadcastConnectionClosing(
                    connection,
                    startTime,
                    async: true,
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

        private ConnectionEventData BroadcastConnectionClosing(
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

            DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;

            static string ConnectionClosing(EventDefinitionBase definition, EventData payload)
            {
                var d = (EventDefinition<string, string>)definition;
                var p = (ConnectionEventData)payload;
                return d.GenerateMessage(
                    p.Connection.Database,
                    p.Connection.DataSource);
            }
        }

        #endregion ConnectionClosing

        #region ConnectionClosed

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ConnectionClosed(
            IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogClosedConnection(this);

            if (ShouldLog(definition))
            {
                _suppressCloseExpiration = default;

                definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
            }

            if (NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                _suppressCloseExpiration = default;

                var eventData = BroadcastCollectionClosed(
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task ConnectionClosedAsync(
            IRelationalConnection connection,
            DateTimeOffset startTime,
            TimeSpan duration)
        {
            var definition = RelationalResources.LogClosedConnection(this);

            if (ShouldLog(definition))
            {
                _suppressCloseExpiration = default;

                definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
            }

            if (NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                _suppressCloseExpiration = default;

                var eventData = BroadcastCollectionClosed(
                    connection,
                    startTime,
                    duration,
                    async: true,
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

        private ConnectionEndEventData BroadcastCollectionClosed(
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

            DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;

            static string ConnectionClosed(EventDefinitionBase definition, EventData payload)
            {
                var d = (EventDefinition<string, string>)definition;
                var p = (ConnectionEndEventData)payload;
                return d.GenerateMessage(
                    p.Connection.Database,
                    p.Connection.DataSource);
            }
        }

        #endregion ConnectionClosed

        #region ConnectionError

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ConnectionError(
            IRelationalConnection connection,
            Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool logErrorAsDebug)
        {
            var definition = logErrorAsDebug
                ? RelationalResources.LogConnectionErrorAsDebug(this)
                : RelationalResources.LogConnectionError(this);

            LogConnectionError(connection, definition);

            if (NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastConnectionError(
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Task ConnectionErrorAsync(
            IRelationalConnection connection,
            Exception exception,
            DateTimeOffset startTime,
            TimeSpan duration,
            bool logErrorAsDebug,
            CancellationToken cancellationToken = default)
        {
            var definition = logErrorAsDebug
                ? RelationalResources.LogConnectionErrorAsDebug(this)
                : RelationalResources.LogConnectionError(this);

            LogConnectionError(connection, definition);

            if (NeedsEventData<IDbConnectionInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = BroadcastConnectionError(
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

        private ConnectionErrorEventData BroadcastConnectionError(
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

            DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            return eventData;

            static string ConnectionError(EventDefinitionBase definition, EventData payload)
            {
                var d = (EventDefinition<string, string>)definition;
                var p = (ConnectionErrorEventData)payload;
                return d.GenerateMessage(
                    p.Connection.Database,
                    p.Connection.DataSource);
            }
        }

        private void LogConnectionError(
            IRelationalConnection connection,
            EventDefinition<string, string> definition)
        {
            if (ShouldLog(definition))
            {
                definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
            }
        }

        #endregion ConnectionError

        #region ShouldLog checks

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool ShouldLogConnectionOpen(DateTimeOffset now)
            => now > _suppressOpenExpiration;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool ShouldLogConnectionClose(DateTimeOffset now)
            => now > _suppressCloseExpiration;

        #endregion ShouldLog checks
    }
}
