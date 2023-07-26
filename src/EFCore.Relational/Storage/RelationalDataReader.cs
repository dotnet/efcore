// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Reads result sets from a relational database.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class RelationalDataReader : IDisposable, IAsyncDisposable
{
    private IRelationalConnection _relationalConnection = default!;
    private DbCommand _command = default!;
    private DbDataReader _reader = default!;
    private Guid _commandId;
    private IRelationalCommandDiagnosticsLogger? _logger;
    private DateTimeOffset _startTime;
    private SharedStopwatch _stopwatch;

    private int _readCount;

    private bool _closed;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalDataReader" /> class.
    /// </summary>
    /// <param name="relationalConnection">The relational connection.</param>
    /// <param name="command">The command that was executed.</param>
    /// <param name="reader">The underlying reader for the result set.</param>
    /// <param name="commandId">A correlation ID that identifies the <see cref="DbCommand" /> instance being used.</param>
    /// <param name="logger">The diagnostic source.</param>
    public virtual void Initialize(
        IRelationalConnection relationalConnection,
        DbCommand command,
        DbDataReader reader,
        Guid commandId,
        IRelationalCommandDiagnosticsLogger? logger)
    {
        _relationalConnection = relationalConnection;
        _command = command;
        _reader = reader;
        _commandId = commandId;
        _logger = logger;
        _readCount = 0;
        _closed = false;
        _disposed = false;
        _startTime = DateTimeOffset.UtcNow;
        _stopwatch = SharedStopwatch.StartNew();
    }

    /// <summary>
    ///     Gets the underlying relational connection being used.
    /// </summary>
    public virtual IRelationalConnection RelationalConnection
        => _relationalConnection;

    /// <summary>
    ///     Gets the underlying reader for the result set.
    /// </summary>
    public virtual DbDataReader DbDataReader
        => _reader;

    /// <summary>
    ///     Gets the underlying command for the result set.
    /// </summary>
    public virtual DbCommand DbCommand
        => _command;

    /// <summary>
    ///     A correlation ID that identifies the <see cref="DbCommand" /> instance being used.
    /// </summary>
    public virtual Guid CommandId
        => _commandId;

    /// <summary>
    ///     Calls <see cref="System.Data.Common.DbDataReader.Read" /> on the underlying <see cref="System.Data.Common.DbDataReader" />.
    /// </summary>
    /// <returns><see langword="true" /> if there are more rows; otherwise <see langword="false" />.</returns>
    public virtual bool Read()
    {
        _readCount++;

        return _reader.Read();
    }

    /// <summary>
    ///     Calls <see cref="System.Data.Common.DbDataReader.ReadAsync(CancellationToken)" /> on the underlying
    ///     <see cref="System.Data.Common.DbDataReader" />.
    /// </summary>
    /// <returns><see langword="true" /> if there are more rows; otherwise <see langword="false" />.</returns>
    public virtual Task<bool> ReadAsync(CancellationToken cancellationToken = default)
    {
        _readCount++;

        return _reader.ReadAsync(cancellationToken);
    }

    /// <summary>
    ///     Closes the reader.
    /// </summary>
    public virtual void Close()
    {
        if (_closed)
        {
            return;
        }

        var closeInterceptionResult = default(InterceptionResult);
        try
        {
            if (_logger?.ShouldLogDataReaderClose(DateTimeOffset.UtcNow) == true)
            {
                closeInterceptionResult = _logger.DataReaderClosing(
                    _relationalConnection,
                    _command,
                    _reader,
                    _commandId,
                    _reader.RecordsAffected,
                    _readCount,
                    _startTime); // can throw
            }
        }
        finally
        {
            _closed = true;

            if (!closeInterceptionResult.IsSuppressed)
            {
                _reader.Close(); // can throw
            }
        }
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var interceptionResult = default(InterceptionResult);
        try
        {
            Close();

            if (_logger?.ShouldLogDataReaderDispose(DateTimeOffset.UtcNow) == true)
            {
                interceptionResult = _logger.DataReaderDisposing(
                    _relationalConnection,
                    _command,
                    _reader,
                    _commandId,
                    _reader.RecordsAffected,
                    _readCount,
                    _startTime,
                    _stopwatch.Elapsed); // can throw
            }
        }
        finally
        {
            _disposed = true;

            try
            {
                if (!interceptionResult.IsSuppressed)
                {
                    _reader.Dispose();
                    _command.Parameters.Clear();
                    _command.Dispose();
                    _relationalConnection.Close();
                }
            }
            finally
            {
                _reader = null!;
                _command = null!;
                _relationalConnection = null!;
                _logger = null;
            }
        }
    }

    /// <summary>
    ///     Closes the reader.
    /// </summary>
    public virtual async ValueTask CloseAsync()
    {
        if (_closed)
        {
            return;
        }

        var closeInterceptionResult = default(InterceptionResult);
        try
        {
            if (_logger?.ShouldLogDataReaderClose(DateTimeOffset.UtcNow) == true)
            {
                closeInterceptionResult = await _logger.DataReaderClosingAsync(
                    _relationalConnection,
                    _command,
                    _reader,
                    _commandId,
                    _reader.RecordsAffected,
                    _readCount,
                    _startTime).ConfigureAwait(false); // can throw
            }
        }
        finally
        {
            _closed = true;

            if (!closeInterceptionResult.IsSuppressed)
            {
                await _reader.CloseAsync().ConfigureAwait(false); // can throw
            }
        }
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        var interceptionResult = default(InterceptionResult);
        try
        {
            // Skip extra async call for most cases
            if (_logger?.ShouldLogDataReaderClose(DateTimeOffset.UtcNow) == true)
            {
                await CloseAsync().ConfigureAwait(false);
            }
            else
            {
                await _reader.CloseAsync().ConfigureAwait(false); // can throw
            }

            if (_logger?.ShouldLogDataReaderDispose(DateTimeOffset.UtcNow) == true)
            {
                interceptionResult = _logger.DataReaderDisposing(
                    _relationalConnection,
                    _command,
                    _reader,
                    _commandId,
                    _reader.RecordsAffected,
                    _readCount,
                    _startTime,
                    _stopwatch.Elapsed); // can throw
            }
        }
        finally
        {
            _disposed = true;

            try
            {
                if (!interceptionResult.IsSuppressed)
                {
                    await _reader.DisposeAsync().ConfigureAwait(false);
                    _command.Parameters.Clear();
                    await _command.DisposeAsync().ConfigureAwait(false);
                    await _relationalConnection.CloseAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                _reader = null!;
                _command = null!;
                _relationalConnection = null!;
                _logger = null;
            }
        }
    }
}
