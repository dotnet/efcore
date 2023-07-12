// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.Internal;

#pragma warning disable EF1001

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationalCommandDiagnosticsLogger
    : DiagnosticsLogger<DbLoggerCategory.Database.Command>, IRelationalCommandDiagnosticsLogger
{
    private DateTimeOffset _suppressCommandCreateExpiration;
    private DateTimeOffset _suppressCommandExecuteExpiration;
    private DateTimeOffset _suppressDataReaderClosingExpiration;
    private DateTimeOffset _suppressDataReaderDisposingExpiration;

    private readonly TimeSpan _loggingCacheTime;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationalCommandDiagnosticsLogger(
        ILoggerFactory loggerFactory,
        ILoggingOptions loggingOptions,
        DiagnosticSource diagnosticSource,
        LoggingDefinitions loggingDefinitions,
        IDbContextLogger contextLogger,
        IDbContextOptions contextOptions,
        IInterceptors? interceptors = null)
        : base(loggerFactory, loggingOptions, diagnosticSource, loggingDefinitions, contextLogger, interceptors)
    {
        var coreOptionsExtension =
            contextOptions.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        _loggingCacheTime = coreOptionsExtension.LoggingCacheTime;
    }

    #region CommandCreating

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InterceptionResult<DbCommand> CommandCreating(
        IRelationalConnection connection,
        DbCommandMethod commandMethod,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        CommandSource commandSource)
    {
        _suppressCommandCreateExpiration = startTime + _loggingCacheTime;

        var definition = RelationalResources.LogCommandCreating(this);

        if (ShouldLog(definition))
        {
            _suppressCommandCreateExpiration = default;

            definition.Log(this, commandMethod.ToString());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandCreateExpiration = default;

            var eventData = BroadcastCommandCreating(
                connection.DbConnection,
                context,
                commandMethod,
                commandId,
                connectionId,
                async: false,
                startTime,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.CommandCreating(eventData, default);
            }
        }

        return default;
    }

    private CommandCorrelatedEventData BroadcastCommandCreating(
        DbConnection connection,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        bool async,
        DateTimeOffset startTime,
        EventDefinition<string> definition,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled,
        CommandSource commandSource)
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
            startTime,
            commandSource);

        DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;

        static string CommandCreating(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (CommandCorrelatedEventData)payload;
            return d.GenerateMessage(p.ExecuteMethod.ToString());
        }
    }

    #endregion CommandCreating

    #region CommandCreated

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbCommand CommandCreated(
        IRelationalConnection connection,
        DbCommand command,
        DbCommandMethod commandMethod,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource)
    {
        var definition = RelationalResources.LogCommandCreated(this);

        if (ShouldLog(definition))
        {
            _suppressCommandCreateExpiration = default;

            definition.Log(this, commandMethod.ToString(), (int)duration.TotalMilliseconds);
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandCreateExpiration = default;

            var eventData = BroadcastCommandCreated(
                connection.DbConnection,
                command,
                context,
                commandMethod,
                commandId,
                connectionId,
                async: false,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.CommandCreated(eventData, command);
            }
        }

        return command;
    }

    private CommandEndEventData BroadcastCommandCreated(
        DbConnection connection,
        DbCommand command,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        bool async,
        DateTimeOffset startTime,
        TimeSpan duration,
        EventDefinition<string, int> definition,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled,
        CommandSource commandSource)
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
            duration,
            commandSource);

        DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;

        static string CommandCreated(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, int>)definition;
            var p = (CommandEndEventData)payload;
            return d.GenerateMessage(p.ExecuteMethod.ToString(), (int)p.Duration.TotalMilliseconds);
        }
    }

    #endregion CommandCreated

    #region CommandInitialized

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbCommand CommandInitialized(
        IRelationalConnection connection,
        DbCommand command,
        DbCommandMethod commandMethod,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource)
    {
        var definition = RelationalResources.LogCommandInitialized(this);

        if (ShouldLog(definition))
        {
            _suppressCommandCreateExpiration = default;

            definition.Log(this, commandMethod.ToString(), (int)duration.TotalMilliseconds);
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandCreateExpiration = default;

            var eventData = BroadcastCommandInitialized(
                connection.DbConnection,
                command,
                context,
                commandMethod,
                commandId,
                connectionId,
                async: false,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.CommandInitialized(eventData, command);
            }
        }

        return command;
    }

    private CommandEndEventData BroadcastCommandInitialized(
        DbConnection connection,
        DbCommand command,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        bool async,
        DateTimeOffset startTime,
        TimeSpan duration,
        EventDefinition<string, int> definition,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled,
        CommandSource commandSource)
    {
        var eventData = new CommandEndEventData(
            definition,
            CommandInitialized,
            connection,
            command,
            context,
            executeMethod,
            commandId,
            connectionId,
            async,
            false,
            startTime,
            duration,
            commandSource);

        DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;

        static string CommandInitialized(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, int>)definition;
            var p = (CommandEndEventData)payload;
            return d.GenerateMessage(p.ExecuteMethod.ToString(), (int)p.Duration.TotalMilliseconds);
        }
    }

    #endregion CommandInitialized

    #region CommandExecuting

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InterceptionResult<DbDataReader> CommandReaderExecuting(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        CommandSource commandSource)
    {
        _suppressCommandExecuteExpiration = startTime + _loggingCacheTime;

        var definition = RelationalResources.LogExecutingCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuting(
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
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.ReaderExecuting(command, eventData, default);
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
    public virtual InterceptionResult<object> CommandScalarExecuting(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        CommandSource commandSource)
    {
        _suppressCommandExecuteExpiration = startTime + _loggingCacheTime;

        var definition = RelationalResources.LogExecutingCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuting(
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
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.ScalarExecuting(command, eventData, default);
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
    public virtual InterceptionResult<int> CommandNonQueryExecuting(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        CommandSource commandSource)
    {
        _suppressCommandExecuteExpiration = startTime + _loggingCacheTime;

        var definition = RelationalResources.LogExecutingCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuting(
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
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.NonQueryExecuting(command, eventData, default);
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
    public virtual ValueTask<InterceptionResult<DbDataReader>> CommandReaderExecutingAsync(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        CommandSource commandSource,
        CancellationToken cancellationToken = default)
    {
        _suppressCommandExecuteExpiration = startTime + _loggingCacheTime;

        var definition = RelationalResources.LogExecutingCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuting(
                connection.DbConnection,
                command,
                context,
                DbCommandMethod.ExecuteReader,
                commandId,
                connectionId,
                async: true,
                startTime,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.ReaderExecutingAsync(command, eventData, default, cancellationToken);
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
    public virtual ValueTask<InterceptionResult<object>> CommandScalarExecutingAsync(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        CommandSource commandSource,
        CancellationToken cancellationToken = default)
    {
        _suppressCommandExecuteExpiration = startTime + _loggingCacheTime;

        var definition = RelationalResources.LogExecutingCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuting(
                connection.DbConnection,
                command,
                context,
                DbCommandMethod.ExecuteScalar,
                commandId,
                connectionId,
                async: true,
                startTime,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.ScalarExecutingAsync(command, eventData, default, cancellationToken);
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
    public virtual ValueTask<InterceptionResult<int>> CommandNonQueryExecutingAsync(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        CommandSource commandSource,
        CancellationToken cancellationToken = default)
    {
        _suppressCommandExecuteExpiration = startTime + _loggingCacheTime;

        var definition = RelationalResources.LogExecutingCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuting(
                connection.DbConnection,
                command,
                context,
                DbCommandMethod.ExecuteNonQuery,
                commandId,
                connectionId,
                async: true,
                startTime,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.NonQueryExecutingAsync(command, eventData, default, cancellationToken);
            }
        }

        return default;
    }

    private CommandEventData BroadcastCommandExecuting(
        DbConnection connection,
        DbCommand command,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        bool async,
        DateTimeOffset startTime,
        EventDefinition<string, CommandType, int, string, string> definition,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled,
        CommandSource commandSource)
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
            ShouldLogParameterValues(command),
            startTime,
            commandSource);

        DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;

        static string CommandExecuting(EventDefinitionBase definition, EventData payload)
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
    }

    #endregion CommandExecuting

    #region CommandExecuted

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual DbDataReader CommandReaderExecuted(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DbDataReader methodResult,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource)
    {
        var definition = RelationalResources.LogExecutedCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.TotalMilliseconds),
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuted(
                connection.DbConnection,
                command,
                context,
                DbCommandMethod.ExecuteReader,
                commandId,
                connectionId,
                methodResult,
                async: false,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.ReaderExecuted(command, eventData, methodResult);
            }
        }

        return methodResult;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? CommandScalarExecuted(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        object? methodResult,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource)
    {
        var definition = RelationalResources.LogExecutedCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.TotalMilliseconds),
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuted(
                connection.DbConnection,
                command,
                context,
                DbCommandMethod.ExecuteScalar,
                commandId,
                connectionId,
                methodResult,
                async: false,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.ScalarExecuted(command, eventData, methodResult);
            }
        }

        return methodResult;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int CommandNonQueryExecuted(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        int methodResult,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource)
    {
        var definition = RelationalResources.LogExecutedCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.TotalMilliseconds),
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuted(
                connection.DbConnection,
                command,
                context,
                DbCommandMethod.ExecuteNonQuery,
                commandId,
                connectionId,
                methodResult,
                async: false,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.NonQueryExecuted(command, eventData, methodResult);
            }
        }

        return methodResult;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueTask<DbDataReader> CommandReaderExecutedAsync(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        DbDataReader methodResult,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogExecutedCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.TotalMilliseconds),
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuted(
                connection.DbConnection,
                command,
                context,
                DbCommandMethod.ExecuteReader,
                commandId,
                connectionId,
                methodResult,
                async: true,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.ReaderExecutedAsync(command, eventData, methodResult, cancellationToken);
            }
        }

        return ValueTask.FromResult(methodResult);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueTask<object?> CommandScalarExecutedAsync(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        object? methodResult,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogExecutedCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.TotalMilliseconds),
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuted(
                connection.DbConnection,
                command,
                context,
                DbCommandMethod.ExecuteScalar,
                commandId,
                connectionId,
                methodResult,
                async: true,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.ScalarExecutedAsync(command, eventData, methodResult, cancellationToken);
            }
        }

        return ValueTask.FromResult(methodResult);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueTask<int> CommandNonQueryExecutedAsync(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        int methodResult,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogExecutedCommand(this);

        if (ShouldLog(definition))
        {
            _suppressCommandExecuteExpiration = default;

            definition.Log(
                this,
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.TotalMilliseconds),
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressCommandExecuteExpiration = default;

            var eventData = BroadcastCommandExecuted(
                connection.DbConnection,
                command,
                context,
                DbCommandMethod.ExecuteNonQuery,
                commandId,
                connectionId,
                methodResult,
                async: true,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.NonQueryExecutedAsync(command, eventData, methodResult, cancellationToken);
            }
        }

        return ValueTask.FromResult(methodResult);
    }

    private CommandExecutedEventData BroadcastCommandExecuted(
        DbConnection connection,
        DbCommand command,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        object? methodResult,
        bool async,
        DateTimeOffset startTime,
        TimeSpan duration,
        EventDefinition<string, string, CommandType, int, string, string> definition,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled,
        CommandSource commandSource)
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
            ShouldLogParameterValues(command),
            startTime,
            duration,
            commandSource);

        DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;

        static string CommandExecuted(EventDefinitionBase definition, EventData payload)
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
    }

    #endregion CommandExecuted

    #region CommandError

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void CommandError(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        Exception exception,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource)
    {
        var definition = RelationalResources.LogCommandFailed(this);

        LogCommandError(command, duration, definition);

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastCommandError(
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
                simpleLogEnabled,
                commandSource);

            interceptor?.CommandFailed(command, eventData);
        }
    }

    private void LogCommandError(
        DbCommand command,
        TimeSpan duration,
        EventDefinition<string, string, CommandType, int, string, string> definition)
    {
        if (ShouldLog(definition))
        {
            definition.Log(
                this,
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.TotalMilliseconds),
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task CommandErrorAsync(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        Exception exception,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogCommandFailed(this);

        LogCommandError(command, duration, definition);

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastCommandError(
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
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.CommandFailedAsync(command, eventData, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    private CommandErrorEventData BroadcastCommandError(
        DbConnection connection,
        DbCommand command,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        Exception exception,
        bool async,
        DateTimeOffset startTime,
        TimeSpan duration,
        EventDefinition<string, string, CommandType, int, string, string> definition,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled,
        CommandSource commandSource)
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
            ShouldLogParameterValues(command),
            startTime,
            duration,
            commandSource);

        DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;

        static string CommandError(EventDefinitionBase definition, EventData payload)
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
    }

    #endregion CommandError

    #region CommandCanceled

    /// <inheritdoc />
    public virtual void CommandCanceled(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource)
    {
        var definition = RelationalResources.LogCommandCanceled(this);

        LogCommandCanceled(command, duration, definition);

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastCommandCanceled(
                connection.DbConnection,
                command,
                context,
                executeMethod,
                commandId,
                connectionId,
                false,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            interceptor?.CommandCanceled(command, eventData);
        }
    }

    /// <inheritdoc />
    public virtual Task CommandCanceledAsync(
        IRelationalConnection connection,
        DbCommand command,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        DateTimeOffset startTime,
        TimeSpan duration,
        CommandSource commandSource,
        CancellationToken cancellationToken = default)
    {
        var definition = RelationalResources.LogCommandCanceled(this);

        LogCommandCanceled(command, duration, definition);

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = BroadcastCommandCanceled(
                connection.DbConnection,
                command,
                context,
                executeMethod,
                commandId,
                connectionId,
                true,
                startTime,
                duration,
                definition,
                diagnosticSourceEnabled,
                simpleLogEnabled,
                commandSource);

            if (interceptor != null)
            {
                return interceptor.CommandCanceledAsync(command, eventData, cancellationToken);
            }
        }

        return Task.CompletedTask;
    }

    private void LogCommandCanceled(
        DbCommand command,
        TimeSpan duration,
        EventDefinition<string, string, CommandType, int, string, string> definition)
    {
        if (ShouldLog(definition))
        {
            definition.Log(
                this,
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", duration.TotalMilliseconds),
                command.Parameters.FormatParameters(ShouldLogParameterValues(command)),
                command.CommandType,
                command.CommandTimeout,
                Environment.NewLine,
                command.CommandText.TrimEnd());
        }
    }

    private CommandEndEventData BroadcastCommandCanceled(
        DbConnection connection,
        DbCommand command,
        DbContext? context,
        DbCommandMethod executeMethod,
        Guid commandId,
        Guid connectionId,
        bool async,
        DateTimeOffset startTime,
        TimeSpan duration,
        EventDefinition<string, string, CommandType, int, string, string> definition,
        bool diagnosticSourceEnabled,
        bool simpleLogEnabled,
        CommandSource commandSource)
    {
        var eventData = new CommandEndEventData(
            definition,
            CommandCanceled,
            connection,
            command,
            context,
            executeMethod,
            commandId,
            connectionId,
            async,
            ShouldLogParameterValues(command),
            startTime,
            duration,
            commandSource);

        DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

        return eventData;

        static string CommandCanceled(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, CommandType, int, string, string>)definition;
            var p = (CommandEndEventData)payload;
            return d.GenerateMessage(
                string.Format(CultureInfo.InvariantCulture, "{0:N0}", p.Duration.TotalMilliseconds),
                p.Command.Parameters.FormatParameters(p.LogParameterValues),
                p.Command.CommandType,
                p.Command.CommandTimeout,
                Environment.NewLine,
                p.Command.CommandText.TrimEnd());
        }
    }

    #endregion CommandCanceled

    #region DataReader

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InterceptionResult DataReaderClosing(
        IRelationalConnection connection,
        DbCommand command,
        DbDataReader dataReader,
        Guid commandId,
        int recordsAffected,
        int readCount,
        DateTimeOffset startTime)
    {
        _suppressDataReaderClosingExpiration = startTime + _loggingCacheTime;

        var definition = RelationalResources.LogClosingDataReader(this);

        if (ShouldLog(definition))
        {
            _suppressDataReaderClosingExpiration = default;

            definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressDataReaderClosingExpiration = default;

            var eventData = new DataReaderClosingEventData(
                definition,
                CreateDataReaderClosingString,
                command,
                dataReader,
                connection.Context,
                commandId,
                connection.ConnectionId,
                async: false,
                recordsAffected,
                readCount,
                startTime);

            DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.DataReaderClosing(command, eventData, default);
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
    public virtual ValueTask<InterceptionResult> DataReaderClosingAsync(
        IRelationalConnection connection,
        DbCommand command,
        DbDataReader dataReader,
        Guid commandId,
        int recordsAffected,
        int readCount,
        DateTimeOffset startTime)
    {
        _suppressDataReaderClosingExpiration = startTime + _loggingCacheTime;

        var definition = RelationalResources.LogClosingDataReader(this);

        if (ShouldLog(definition))
        {
            _suppressDataReaderClosingExpiration = default;

            definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource);
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressDataReaderClosingExpiration = default;

            var eventData = new DataReaderClosingEventData(
                definition,
                CreateDataReaderClosingString,
                command,
                dataReader,
                connection.Context,
                commandId,
                connection.ConnectionId,
                async: true,
                recordsAffected,
                readCount,
                startTime);

            DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.DataReaderClosingAsync(command, eventData, default);
            }
        }

        return default;
    }

    private static string CreateDataReaderClosingString(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string>)definition;
        var p = (DataReaderClosingEventData)payload;
        return d.GenerateMessage(
            p.Command.Connection?.Database ?? "<Unknown>",
            p.Command.Connection?.DataSource ?? "<Unknown>");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InterceptionResult DataReaderDisposing(
        IRelationalConnection connection,
        DbCommand command,
        DbDataReader dataReader,
        Guid commandId,
        int recordsAffected,
        int readCount,
        DateTimeOffset startTime,
        TimeSpan duration)
    {
        _suppressDataReaderDisposingExpiration = startTime + _loggingCacheTime;

        var definition = RelationalResources.LogDisposingDataReader(this);

        if (ShouldLog(definition))
        {
            _suppressDataReaderDisposingExpiration = default;

            definition.Log(this, connection.DbConnection.Database, connection.DbConnection.DataSource, (int)duration.TotalMilliseconds);
        }

        if (NeedsEventData<IDbCommandInterceptor>(
                definition, out var interceptor, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            _suppressDataReaderDisposingExpiration = default;

            var eventData = new DataReaderDisposingEventData(
                definition,
                CreateDataReaderDisposingString,
                command,
                dataReader,
                connection.Context,
                commandId,
                connection.ConnectionId,
                recordsAffected,
                readCount,
                startTime,
                duration);

            DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);

            if (interceptor != null)
            {
                return interceptor.DataReaderDisposing(command, eventData, default);
            }
        }

        return default;
    }

    private static string CreateDataReaderDisposingString(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, int>)definition;
        var p = (DataReaderDisposingEventData)payload;
        return d.GenerateMessage(
            p.Command.Connection?.Database ?? "<Unknown>",
            p.Command.Connection?.DataSource ?? "<Unknown>",
            (int)p.Duration.TotalMilliseconds);
    }

    #endregion DataReader

    #region ShouldLog checks

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ShouldLogCommandCreate(DateTimeOffset now)
        => now > _suppressCommandCreateExpiration;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ShouldLogCommandExecute(DateTimeOffset now)
        => now > _suppressCommandExecuteExpiration;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ShouldLogDataReaderClose(DateTimeOffset now)
        => now > _suppressDataReaderClosingExpiration;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ShouldLogDataReaderDispose(DateTimeOffset now)
        => now > _suppressDataReaderDisposingExpiration;

    private bool ShouldLogParameterValues(DbCommand command)
        => command.Parameters.Count > 0 && ShouldLogSensitiveData();

    #endregion ShouldLog checks
}
