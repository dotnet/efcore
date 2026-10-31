// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal;

internal readonly struct RelationalCommandErrorReporter
{
    private readonly IRelationalConnection _connection;
    private readonly DbCommand _command;
    private readonly string _logCommandText;
    private readonly DbContext? _context;
    private readonly Guid _commandId;
    private readonly Guid _connectionId;
    private readonly IRelationalCommandDiagnosticsLogger _logger;
    private readonly IExceptionDetector _exceptionDetector;
    private readonly DateTimeOffset _startTime;
    private readonly SharedStopwatch _stopwatch;
    private readonly CommandSource _commandSource;

    public static bool IsEnabled(IRelationalCommandDiagnosticsLogger? logger)
    {
        if (logger == null)
        {
            return false;
        }

        var errorDefinition = RelationalResources.LogCommandFailed(logger);
        var canceledDefinition = RelationalResources.LogCommandCanceled(logger);
        return logger.ShouldLog(errorDefinition)
            || logger.ShouldLog(canceledDefinition)
            || logger.NeedsEventData<IDbCommandInterceptor>(errorDefinition, out _, out _, out _)
            || logger.NeedsEventData<IDbCommandInterceptor>(canceledDefinition, out _, out _, out _);
    }

    public RelationalCommandErrorReporter(
        IRelationalConnection connection,
        DbCommand command,
        string logCommandText,
        DbContext? context,
        Guid commandId,
        Guid connectionId,
        IRelationalCommandDiagnosticsLogger logger,
        IExceptionDetector exceptionDetector,
        DateTimeOffset startTime,
        SharedStopwatch stopwatch,
        CommandSource commandSource)
    {
        _connection = connection;
        _command = command;
        _logCommandText = logCommandText;
        _context = context;
        _commandId = commandId;
        _connectionId = connectionId;
        _logger = logger;
        _exceptionDetector = exceptionDetector;
        _startTime = startTime;
        _stopwatch = stopwatch;
        _commandSource = commandSource;
    }

    public void Report(Exception exception)
    {
        if (_exceptionDetector.IsCancellation(exception))
        {
            _logger.CommandCanceled(
                _connection,
                _command,
                _logCommandText,
                _context,
                DbCommandMethod.ExecuteReader,
                _commandId,
                _connectionId,
                _startTime,
                _stopwatch.Elapsed,
                _commandSource);
        }
        else
        {
            _logger.CommandError(
                _connection,
                _command,
                _logCommandText,
                _context,
                DbCommandMethod.ExecuteReader,
                _commandId,
                _connectionId,
                exception,
                _startTime,
                _stopwatch.Elapsed,
                _commandSource);
        }
    }

    public Task ReportAsync(Exception exception, CancellationToken cancellationToken)
    {
        if (_exceptionDetector.IsCancellation(exception, cancellationToken))
        {
            return _logger.CommandCanceledAsync(
                _connection,
                _command,
                _logCommandText,
                _context,
                DbCommandMethod.ExecuteReader,
                _commandId,
                _connectionId,
                _startTime,
                _stopwatch.Elapsed,
                _commandSource,
                cancellationToken);
        }

        return _logger.CommandErrorAsync(
            _connection,
            _command,
            _logCommandText,
            _context,
            DbCommandMethod.ExecuteReader,
            _commandId,
            _connectionId,
            exception,
            _startTime,
            _stopwatch.Elapsed,
            _commandSource,
            cancellationToken);
    }
}
