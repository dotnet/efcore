// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
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
    public class RelationalDataReader : IDisposable
    {
        private readonly IRelationalConnection _connection;
        private readonly DbCommand _command;
        private readonly DbDataReader _reader;
        private readonly Guid _commandId;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _logger;
        private readonly DateTimeOffset _startTime;
        private readonly Stopwatch _stopwatch;

        private int _readCount;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalDataReader" /> class.
        /// </summary>
        /// <param name="connection"> The connection. </param>
        /// <param name="command"> The command that was executed. </param>
        /// <param name="reader"> The underlying reader for the result set. </param>
        /// <param name="commandId"> A correlation ID that identifies the <see cref="DbCommand" /> instance being used. </param>
        /// <param name="logger"> The diagnostic source. </param>
        public RelationalDataReader(
            [CanBeNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [NotNull] DbDataReader reader,
            Guid commandId,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
        {
            Check.NotNull(command, nameof(command));
            Check.NotNull(reader, nameof(reader));
            Check.NotNull(logger, nameof(logger));

            _connection = connection;
            _command = command;
            _reader = reader;
            _commandId = commandId;
            _logger = logger;
            _startTime = DateTimeOffset.UtcNow;
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected RelationalDataReader([NotNull] DbDataReader reader)
        {
            // For testing

            Check.NotNull(reader, nameof(reader));

            _reader = reader;
        }

        /// <summary>
        ///     Gets the underlying reader for the result set.
        /// </summary>
        public virtual DbDataReader DbDataReader => _reader;

        /// <summary>
        ///     Calls Read on the underlying DbDataReader.
        /// </summary>
        /// <returns>true if there are more rows; otherwise false.</returns>
        public virtual bool Read()
        {
            _readCount++;

            return _reader.Read();
        }

        /// <summary>
        ///     Calls Read on the underlying DbDataReader.
        /// </summary>
        /// <returns>true if there are more rows; otherwise false.</returns>
        public virtual Task<bool> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
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
                _logger.DataReaderDisposing(
                    _connection,
                    _command,
                    _reader,
                    _commandId,
                    _reader.RecordsAffected,
                    _readCount,
                    _startTime,
                    _stopwatch.Elapsed);

                _reader.Dispose();
                if (!AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue9277", out var isEnabled)
                    || !isEnabled)
                {
                    _command.Parameters.Clear();
                }
                _command.Dispose();
                _connection?.Close();

                _disposed = true;
            }
        }
    }
}
