// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Reads result sets from a relational database.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalDataReader : IDisposable, IAsyncDisposable
    {
        private IRelationalConnection _relationalConnection = default!;
        private DbCommand _command = default!;
        private DbDataReader _reader = default!;
        private Guid _commandId;
        private IRelationalCommandDiagnosticsLogger? _logger;
        private DateTimeOffset _startTime;
        private readonly Stopwatch _stopwatch = new();

        private int _readCount;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalDataReader" /> class.
        /// </summary>
        /// <param name="relationalConnection"> The relational connection. </param>
        /// <param name="command"> The command that was executed. </param>
        /// <param name="reader"> The underlying reader for the result set. </param>
        /// <param name="commandId"> A correlation ID that identifies the <see cref="DbCommand" /> instance being used. </param>
        /// <param name="logger"> The diagnostic source. </param>
        public virtual void Initialize(
            IRelationalConnection relationalConnection,
            DbCommand command,
            DbDataReader reader,
            Guid commandId,
            IRelationalCommandDiagnosticsLogger? logger)
        {
            Check.NotNull(command, nameof(command));
            Check.NotNull(reader, nameof(reader));

            _relationalConnection = relationalConnection;
            _command = command;
            _reader = reader;
            _commandId = commandId;
            _logger = logger;
            _disposed = false;
            _startTime = DateTimeOffset.UtcNow;
            _stopwatch.Restart();
        }

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
        ///     Calls <see cref="DbDataReader.Read()" /> on the underlying <see cref="System.Data.Common.DbDataReader" />.
        /// </summary>
        /// <returns> <see langword="true" /> if there are more rows; otherwise <see langword="false" />. </returns>
        public virtual bool Read()
        {
            _readCount++;

            return _reader.Read();
        }

        /// <summary>
        ///     Calls <see cref="DbDataReader.ReadAsync(CancellationToken)" /> on the underlying
        ///     <see cref="System.Data.Common.DbDataReader" />.
        /// </summary>
        /// <returns> <see langword="true" /> if there are more rows; otherwise <see langword="false" />. </returns>
        public virtual Task<bool> ReadAsync(CancellationToken cancellationToken = default)
        {
            _readCount++;

            return _reader.ReadAsync(cancellationToken);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (!_disposed)
            {
                var interceptionResult = default(InterceptionResult);
                try
                {
                    _reader.Close(); // can throw

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

                    if (!interceptionResult.IsSuppressed)
                    {
                        _reader.Dispose();
                        _command.Parameters.Clear();
                        _command.Dispose();
                        _relationalConnection.Close();
                    }
                }
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                var interceptionResult = default(InterceptionResult);
                try
                {
                    await _reader.CloseAsync().ConfigureAwait(false); // can throw

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

                    if (!interceptionResult.IsSuppressed)
                    {
                        await _reader.DisposeAsync().ConfigureAwait(false);
                        _command.Parameters.Clear();
                        await _command.DisposeAsync().ConfigureAwait(false);
                        await _relationalConnection.CloseAsync().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
