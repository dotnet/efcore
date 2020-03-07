// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Provides methods to access the contents of a blob.
    /// </summary>
    public class SqliteBlob : Stream
    {
        private sqlite3_blob _blob;
        private readonly sqlite3 _db;
        private long _position;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteBlob" /> class.
        /// </summary>
        /// <param name="connection">An open connection to the database.</param>
        /// <param name="tableName">The name of table containing the blob.</param>
        /// <param name="columnName">The name of the column containing the blob.</param>
        /// <param name="rowid">The rowid of the row containing the blob.</param>
        /// <param name="readOnly">A value indicating whether the blob is read-only.</param>
        public SqliteBlob(
            SqliteConnection connection,
            string tableName,
            string columnName,
            long rowid,
            bool readOnly = false)
            : this(connection, SqliteConnection.MainDatabaseName, tableName, columnName, rowid, readOnly)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteBlob" /> class.
        /// </summary>
        /// <param name="connection">An open connection to the database.</param>
        /// <param name="databaseName">The name of the attached database containing the blob.</param>
        /// <param name="tableName">The name of table containing the blob.</param>
        /// <param name="columnName">The name of the column containing the blob.</param>
        /// <param name="rowid">The rowid of the row containing the blob.</param>
        /// <param name="readOnly">A value indicating whether the blob is read-only.</param>
        public SqliteBlob(
            SqliteConnection connection,
            string databaseName,
            string tableName,
            string columnName,
            long rowid,
            bool readOnly = false)
        {
            if (connection?.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.SqlBlobRequiresOpenConnection);
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            _db = connection.Handle;
            CanWrite = !readOnly;
            var rc = sqlite3_blob_open(
                _db,
                databaseName,
                tableName,
                columnName,
                rowid,
                readOnly ? 0 : 1,
                out _blob);
            SqliteException.ThrowExceptionForRC(rc, _db);
            Length = sqlite3_blob_bytes(_blob);
        }

        /// <summary>
        ///     Gets a value indicating whether the current stream supports reading.
        ///     Always true.
        /// </summary>
        /// <value>true if the stream supports reading; otherwise, false.</value>
        public override bool CanRead => true;

        /// <summary>
        ///     Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value>true if the stream supports writing; otherwise, false.</value>
        public override bool CanWrite { get; }

        /// <summary>
        ///     Gets a value indicating whether the current stream supports seeking.
        ///     Always true.
        /// </summary>
        /// <value>true if the stream supports seeking; otherwise, false.</value>
        public override bool CanSeek => true;

        /// <summary>
        ///     Gets the length in bytes of the stream.
        /// </summary>
        /// <value>A long value representing the length of the stream in bytes.</value>
        public override long Length { get; }

        /// <summary>
        ///     Gets or sets the position within the current stream.
        /// </summary>
        /// <value>The current position within the stream.</value>
        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0)
                {
                    // NB: Message is provided by the framework
                    throw new ArgumentOutOfRangeException(nameof(value), value, message: null);
                }

                _position = value;
            }
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current stream and advances the position
        ///     within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">
        ///     An array of bytes. When this method returns, the buffer contains the specified byte array
        ///     with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.
        /// </param>
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
                // NB: Message is provided by the framework
                throw new ArgumentOutOfRangeException(nameof(offset), offset, message: null);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, message: null);
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException(Resources.InvalidOffsetAndCount);
            }

            if (_blob == null)
            {
                throw new ObjectDisposedException(objectName: null);
            }

            var position = _position;
            if (position > Length)
            {
                position = Length;
            }

            if (position + count > Length)
            {
                count = (int)(Length - position);
            }

            var rc = sqlite3_blob_read(_blob, buffer.AsSpan(offset, count), (int)position);
            SqliteException.ThrowExceptionForRC(rc, _db);
            _position += count;
            return count;
        }

        /// <summary>
        ///     Writes a sequence of bytes to the current stream and advances the current position
        ///     within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
            {
                throw new NotSupportedException(Resources.WriteNotSupported);
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                // NB: Message is provided by the framework
                throw new ArgumentOutOfRangeException(nameof(offset), offset, message: null);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, message: null);
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException(Resources.InvalidOffsetAndCount);
            }

            if (_blob == null)
            {
                throw new ObjectDisposedException(objectName: null);
            }

            var position = _position;
            if (position > Length)
            {
                position = Length;
            }

            if (position + count > Length)
            {
                throw new NotSupportedException(Resources.ResizeNotSupported);
            }

            var rc = sqlite3_blob_write(_blob, buffer.AsSpan(offset, count), (int)position);
            SqliteException.ThrowExceptionForRC(rc, _db);
            _position += count;
        }

        /// <summary>
        ///     Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long position;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    position = _position + offset;
                    break;
                case SeekOrigin.End:
                    position = Length + offset;
                    break;
                default:
                    throw new ArgumentException(Resources.InvalidEnumValue(typeof(SeekOrigin), origin), nameof(origin));
            }

            if (position < 0)
            {
                throw new IOException(Resources.SeekBeforeBegin);
            }

            return _position = position;
        }

        /// <summary>
        ///     Releases any resources used by the blob and closes it.
        /// </summary>
        /// <param name="disposing">
        ///     true to release managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (_blob != null)
            {
                _blob.Dispose();
                _blob = null;
            }
        }

        /// <summary>
        ///     Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        ///     Does nothing.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        ///     Sets the length of the current stream. This is not supported by sqlite blobs.
        ///     Not supported.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="NotSupportedException">Always.</exception>
        public override void SetLength(long value)
            => throw new NotSupportedException(Resources.ResizeNotSupported);
    }
}
