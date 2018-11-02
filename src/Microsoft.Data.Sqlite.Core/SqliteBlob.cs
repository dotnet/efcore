// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using SQLitePCL;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Provides methods to access the contents of a BLOB.
    /// </summary>
    public class SqliteBlob : Stream
    {
        private const string MainDatabaseName = "main";

        private readonly sqlite3_blob _blob;
        private readonly sqlite3 _db;
        private readonly bool _writable;
        private readonly long _length;
        private bool _disposed = false;
        private int _position = 0;

        /// <summary>
        /// Gets the length of the BLOB in bytes.
        /// </summary>
        /// <value>Length of the BLOB in bytes.</value>
        public override long Length => _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteBlob"/> class.
        /// </summary>
        /// <param name="connection">Sqlite connection to be used.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="rowid">The row index.</param>
        /// <param name="writable">Flag indicating whether the blob can be written to.</param>
        /// <returns>Object to access the BLOB.</returns>
        public SqliteBlob(SqliteConnection connection, string tableName, string columnName, long rowid, bool writable) :
            this(connection, MainDatabaseName, tableName, columnName, rowid, writable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteBlob"/> class.
        /// </summary>
        /// <param name="connection">Sqlite connection to be used.</param>
        /// <param name="databaseName">The name of the database ('main' is default table).</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="rowid">The row index.</param>
        /// <param name="writable">Flag indicating whether the blob can be written to.</param>
        /// <returns>Object to access the BLOB.</returns>
        public SqliteBlob(SqliteConnection connection, string databaseName, string tableName, string columnName, long rowid, bool writable)
        {
            _db = connection.Handle;
            _writable = writable;
            var rc = raw.sqlite3_blob_open(_db, databaseName, tableName, columnName, rowid, writable ? 1 : 0, out _blob);
            SqliteException.ThrowExceptionForRC(rc, _db);
            _length = raw.sqlite3_blob_bytes(_blob);
        }

        private SqliteBlob() => throw new NotSupportedException();

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value>true if the stream supports reading; otherwise, false.</value>
        public override bool CanRead => true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value>true if the stream supports writing; otherwise, false.</value>
        public override bool CanWrite => _writable;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value>true if the stream supports seeking; otherwise, false.</value>
        public override bool CanSeek { get; }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <value>The current position within the stream.</value>
        public override long Position
        {
            get => _position;
            set => _position = (int)value;
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position
        /// within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array
        /// with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
            }

            if (_position + count > Length)
            {
                count = (int)(Length - _position);
                if (count == 0)
                {
                    return 0;
                }
            }

            var rc = raw.sqlite3_blob_read(_blob, buffer, offset, count, _position);
            SqliteException.ThrowExceptionForRC(rc, _db);
            _position += count;
            return count;
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position
        /// within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_writable)
            {
                throw new InvalidOperationException("Blob is not openned as writable!");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("The sum of offset and count is larger than the buffer length.");
            }

            var rc = raw.sqlite3_blob_write(_blob, buffer, count, _position);
            SqliteException.ThrowExceptionForRC(rc, _db);
            _position += count;
        }

        /// <summary>
        ///  Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = (int)offset;
                    break;
                case SeekOrigin.Current:
                    _position += (int)offset;
                    break;
                case SeekOrigin.End:
                    _position = (int)(Length - offset);
                    break;
                default:
                    throw new ArgumentException("Invalid value: " + origin, nameof(origin));
            }

            return _position;
        }

        /// <summary>
        ///     Releases any resources used by the BLOB and closes it.
        /// </summary>
        /// <param name="disposing">
        ///     true to release managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                raw.sqlite3_blob_close(_blob);
                _disposed = true;
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SqliteBlob"/> class.
        /// </summary>
        ~SqliteBlob()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// Is a noop in this case.
        /// </summary>
        public override void Flush() { }

        /// <summary>
        /// Sets the length of the current stream. This is not supported by sqlite blobs.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value) => throw new NotSupportedException("Blob cannot be resized.");
    }
}