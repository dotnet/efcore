// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Data.Sqlite.Properties;
using Microsoft.Data.Sqlite.Utilities;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Provides methods for reading the result of a command executed against a SQLite database.
    /// </summary>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
    public class SqliteDataReader : DbDataReader
    {
        private readonly SqliteCommand _command;
        private readonly bool _closeConnection;
        private TimeSpan _totalElapsedTime;
        private IEnumerator<sqlite3_stmt>? _stmtEnumerator;
        private SqliteDataRecord? _record;
        private bool _closed;
        private int _recordsAffected = -1;

        internal SqliteDataReader(
            SqliteCommand command,
            IEnumerable<sqlite3_stmt> stmts,
            bool closeConnection)
        {
            _command = command;
            _stmtEnumerator = stmts.GetEnumerator();
            _closeConnection = closeConnection;
        }

        /// <summary>
        ///     Gets the depth of nesting for the current row. Always zero.
        /// </summary>
        /// <value>The depth of nesting for the current row.</value>
        public override int Depth
            => 0;

        /// <summary>
        ///     Gets the number of columns in the current row.
        /// </summary>
        /// <value>The number of columns in the current row.</value>
        public override int FieldCount
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(FieldCount)))
                : (_record?.FieldCount ?? 0);

        /// <summary>
        ///     Gets a handle to underlying prepared statement.
        /// </summary>
        /// <value>A handle to underlying prepared statement.</value>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/interop">Interoperability</seealso>
        public virtual sqlite3_stmt? Handle
            => _record?.Handle;

        /// <summary>
        ///     Gets a value indicating whether the data reader contains any rows.
        /// </summary>
        /// <value>A value indicating whether the data reader contains any rows.</value>
        public override bool HasRows
            => _record?.HasRows ?? false;

        /// <summary>
        ///     Gets a value indicating whether the data reader is closed.
        /// </summary>
        /// <value>A value indicating whether the data reader is closed.</value>
        public override bool IsClosed
            => _closed;

        /// <summary>
        ///     Gets the number of rows inserted, updated, or deleted. -1 for SELECT statements.
        /// </summary>
        /// <value>The number of rows inserted, updated, or deleted.</value>
        public override int RecordsAffected
            => _recordsAffected;

        /// <summary>
        ///     Gets the value of the specified column.
        /// </summary>
        /// <param name="name">The name of the column. The value is case-sensitive.</param>
        /// <returns>The value.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
        public override object this[string name]
            => _record == null
                ? throw new InvalidOperationException(Resources.NoData)
                : _record[name];

        /// <summary>
        ///     Gets the value of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
        public override object this[int ordinal]
            => _record == null
                ? throw new InvalidOperationException(Resources.NoData)
                : _record[ordinal];

        /// <summary>
        ///     Gets an enumerator that can be used to iterate through the rows in the data reader.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public override IEnumerator GetEnumerator()
            => new DbEnumerator(this, closeReader: false);

        /// <summary>
        ///     Advances to the next row in the result set.
        /// </summary>
        /// <returns><see langword="true" /> if there are more rows; otherwise, <see langword="false" />.</returns>
        public override bool Read()
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(Read)))
                : (_record?.Read() ?? false);

        /// <summary>
        ///     Advances to the next result set for batched statements.
        /// </summary>
        /// <returns><see langword="true" /> if there are more result sets; otherwise, <see langword="false" />.</returns>
        /// <exception cref="SqliteException">A SQLite error occurs during execution.</exception>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/batching">Batching</seealso>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/database-errors">Database Errors</seealso>
        public override bool NextResult()
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(NextResult)));
            }

            if (_record != null)
            {
                _record.Dispose();
                _record = null;
            }

            sqlite3_stmt stmt;
            int rc;

            while (_stmtEnumerator!.MoveNext())
            {
                try
                {
                    stmt = _stmtEnumerator.Current;

                    var timer = SharedStopwatch.StartNew();

                    while (IsBusy(rc = sqlite3_step(stmt)))
                    {
                        if (_command.CommandTimeout != 0
                            && (_totalElapsedTime + timer.Elapsed).TotalMilliseconds >= _command.CommandTimeout * 1000L)
                        {
                            break;
                        }

                        sqlite3_reset(stmt);

                        // TODO: Consider having an async path that uses Task.Delay()
                        Thread.Sleep(150);
                    }

                    _totalElapsedTime += timer.Elapsed;

                    SqliteException.ThrowExceptionForRC(rc, _command.Connection!.Handle);

                    // It's a SELECT statement
                    if (sqlite3_column_count(stmt) != 0)
                    {
                        _record = new SqliteDataRecord(stmt, rc != SQLITE_DONE, _command.Connection, AddChanges);

                        return true;
                    }

                    while (rc != SQLITE_DONE)
                    {
                        rc = sqlite3_step(stmt);
                        SqliteException.ThrowExceptionForRC(rc, _command.Connection.Handle);
                    }

                    sqlite3_reset(stmt);

                    var changes = sqlite3_changes(_command.Connection.Handle);
                    AddChanges(changes);
                }
                catch
                {
                    sqlite3_reset(_stmtEnumerator.Current);
                    _stmtEnumerator.Dispose();
                    _stmtEnumerator = null;
                    Dispose();

                    throw;
                }
            }

            return false;
        }

        private static bool IsBusy(int rc)
            => rc is SQLITE_LOCKED or SQLITE_BUSY or SQLITE_LOCKED_SHAREDCACHE;

        private void AddChanges(int changes)
        {
            if (_recordsAffected == -1)
            {
                _recordsAffected = changes;
            }
            else
            {
                _recordsAffected += changes;
            }
        }

        /// <summary>
        ///     Closes the data reader.
        /// </summary>
        public override void Close()
            => Dispose(true);

        /// <summary>
        ///     Releases any resources used by the data reader and closes it.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing || _closed)
            {
                return;
            }

            _command.DataReader = null;

            _record?.Dispose();
            _record = null;

            if (_stmtEnumerator != null)
            {
                try
                {
                    while (NextResult())
                    {
                    }
                }
                catch
                {
                }
            }

            _stmtEnumerator?.Dispose();

            _closed = true;

            if (_closeConnection)
            {
                _command.Connection!.Close();
            }
        }

        /// <summary>
        ///     Gets the name of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The name of the column.</returns>
        public override string GetName(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetName)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetName(ordinal);

        /// <summary>
        ///     Gets the ordinal of the specified column.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The zero-based column ordinal.</returns>
        public override int GetOrdinal(string name)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetOrdinal)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetOrdinal(name);

        /// <summary>
        ///     Gets the declared data type name of the specified column. The storage class is returned for computed
        ///     columns.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The data type name of the column.</returns>
        /// <remarks>Due to SQLite's dynamic type system, this may not reflect the actual type of the value.</remarks>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
        public override string GetDataTypeName(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetDataTypeName)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetDataTypeName(ordinal);

        /// <summary>
        ///     Gets the data type of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The data type of the column.</returns>
#if NET8_0_OR_GREATER
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)]
#endif
        public override Type GetFieldType(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetFieldType)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetFieldType(ordinal);

        /// <summary>
        ///     Gets a value indicating whether the specified column is <see cref="DBNull" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns><see langword="true" /> if the specified column is <see cref="DBNull" />; otherwise, <see langword="false" />.</returns>
        public override bool IsDBNull(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(IsDBNull)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.IsDBNull(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="bool" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override bool GetBoolean(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetBoolean)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetBoolean(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="byte" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override byte GetByte(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetByte)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetByte(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="char" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override char GetChar(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetChar)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetChar(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="DateTime" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override DateTime GetDateTime(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetDateTime)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetDateTime(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="DateTimeOffset" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public virtual DateTimeOffset GetDateTimeOffset(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetDateTimeOffset)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetDateTimeOffset(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="TimeSpan" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public virtual TimeSpan GetTimeSpan(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetTimeSpan)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetTimeSpan(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="decimal" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override decimal GetDecimal(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetDecimal)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetDecimal(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="double" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override double GetDouble(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetDouble)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetDouble(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="float" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override float GetFloat(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetFloat)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetFloat(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="Guid" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override Guid GetGuid(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetGuid)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetGuid(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="short" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override short GetInt16(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetInt16)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetInt16(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="int" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override int GetInt32(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetInt32)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetInt32(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="long" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override long GetInt64(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetInt64)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetInt64(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="string" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override string GetString(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetString)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetString(ordinal);

        /// <summary>
        ///     Reads a stream of bytes from the specified column. Not supported.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which the data is copied.</param>
        /// <param name="bufferOffset">The index to which the data will be copied.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetBytes)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

        /// <summary>
        ///     Reads a stream of characters from the specified column. Not supported.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which the data is copied.</param>
        /// <param name="bufferOffset">The index to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of characters read.</returns>
        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetChars)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

        /// <summary>
        ///     Retrieves data as a Stream. If the reader includes rowid (or any of its aliases), a
        ///     <see cref="SqliteBlob" /> is returned. Otherwise, the all of the data is read into memory and a
        ///     <see cref="MemoryStream" /> is returned.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The returned object.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/blob-io">BLOB I/O</seealso>
        public override Stream GetStream(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetStream)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetStream(ordinal);

        /// <summary>
        ///     Retrieves data as a <see cref="TextReader" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The returned object.</returns>
        public override TextReader GetTextReader(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetTextReader)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetTextReader(ordinal);

        /// <summary>
        ///     Gets the value of the specified column.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
        public override T GetFieldValue<T>(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetFieldValue)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetFieldValue<T>(ordinal);

        /// <summary>
        ///     Gets the value of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
        public override object GetValue(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetValue)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetValue(ordinal);

        /// <summary>
        ///     Gets the column values of the current row.
        /// </summary>
        /// <param name="values">An array into which the values are copied.</param>
        /// <returns>The number of values copied into the array.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
        public override int GetValues(object?[] values)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetValues)))
                : _record == null
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.GetValues(values);

        /// <summary>
        ///     Returns a System.Data.DataTable that describes the column metadata of the System.Data.Common.DbDataReader.
        /// </summary>
        /// <returns>A System.Data.DataTable that describes the column metadata.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/metadata">Metadata</seealso>
        public override DataTable GetSchemaTable()
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetSchemaTable)));
            }

            if (_record == null)
            {
                throw new InvalidOperationException(Resources.NoData);
            }

            var schemaTable = new DataTable("SchemaTable");

            var columnNameColumn = new DataColumn(SchemaTableColumn.ColumnName, typeof(string));
            var columnOrdinalColumn = new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int));
            var columnSizeColumn = new DataColumn(SchemaTableColumn.ColumnSize, typeof(int));
            var numericPrecisionColumn = new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short));
            var numericScaleColumn = new DataColumn(SchemaTableColumn.NumericScale, typeof(short));
            var dataTypeColumn = CreateDataTypeColumn();
            var dataTypeNameColumn = new DataColumn("DataTypeName", typeof(string));

            var isLongColumn = new DataColumn(SchemaTableColumn.IsLong, typeof(bool));
            var allowDBNullColumn = new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool));

            var isUniqueColumn = new DataColumn(SchemaTableColumn.IsUnique, typeof(bool));
            var isKeyColumn = new DataColumn(SchemaTableColumn.IsKey, typeof(bool));
            var isAutoIncrementColumn = new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));

            var baseCatalogNameColumn = new DataColumn(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
            var baseSchemaNameColumn = new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string));
            var baseTableNameColumn = new DataColumn(SchemaTableColumn.BaseTableName, typeof(string));
            var baseColumnNameColumn = new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string));

            var baseServerNameColumn = new DataColumn(SchemaTableOptionalColumn.BaseServerName, typeof(string));
            var isAliasedColumn = new DataColumn(SchemaTableColumn.IsAliased, typeof(bool));
            var isExpressionColumn = new DataColumn(SchemaTableColumn.IsExpression, typeof(bool));

            var columns = schemaTable.Columns;

            columns.Add(columnNameColumn);
            columns.Add(columnOrdinalColumn);
            columns.Add(columnSizeColumn);
            columns.Add(numericPrecisionColumn);
            columns.Add(numericScaleColumn);
            columns.Add(isUniqueColumn);
            columns.Add(isKeyColumn);
            columns.Add(baseServerNameColumn);
            columns.Add(baseCatalogNameColumn);
            columns.Add(baseColumnNameColumn);
            columns.Add(baseSchemaNameColumn);
            columns.Add(baseTableNameColumn);
            columns.Add(dataTypeColumn);
            columns.Add(dataTypeNameColumn);
            columns.Add(allowDBNullColumn);
            columns.Add(isAliasedColumn);
            columns.Add(isExpressionColumn);
            columns.Add(isAutoIncrementColumn);
            columns.Add(isLongColumn);

            for (var i = 0; i < FieldCount; i++)
            {
                var schemaRow = schemaTable.NewRow();
                schemaRow[columnNameColumn] = GetName(i);
                schemaRow[columnOrdinalColumn] = i;
                schemaRow[columnSizeColumn] = -1;
                schemaRow[numericPrecisionColumn] = DBNull.Value;
                schemaRow[numericScaleColumn] = DBNull.Value;
                schemaRow[baseServerNameColumn] = _command.Connection!.DataSource;
                var databaseName = sqlite3_column_database_name(_record.Handle, i).utf8_to_string();
                schemaRow[baseCatalogNameColumn] = databaseName;
                var columnName = sqlite3_column_origin_name(_record.Handle, i).utf8_to_string();
                schemaRow[baseColumnNameColumn] = columnName;
                schemaRow[baseSchemaNameColumn] = DBNull.Value;
                var tableName = sqlite3_column_table_name(_record.Handle, i).utf8_to_string();
                schemaRow[baseTableNameColumn] = tableName;
                schemaRow[dataTypeColumn] = GetFieldType(i);
                var dataTypeName = GetDataTypeName(i);
                schemaRow[dataTypeNameColumn] = dataTypeName;
                var isAliased = columnName != GetName(i);
                schemaRow[isAliasedColumn] = isAliased;
                schemaRow[isExpressionColumn] = columnName == null;
                schemaRow[isLongColumn] = DBNull.Value;

                var eponymousVirtualTable = false;
                if (tableName != null
                    && columnName != null)
                {
                    using (var command = _command.Connection.CreateCommand())
                    {
                        command.CommandText = new StringBuilder()
                            .AppendLine("SELECT COUNT(*)")
                            .AppendLine("FROM pragma_index_list($table) i, pragma_index_info(i.name) c")
                            .AppendLine("WHERE \"unique\" = 1 AND c.name = $column AND")
                            .AppendLine("NOT EXISTS (SELECT * FROM pragma_index_info(i.name) c2 WHERE c2.name != c.name);").ToString();
                        command.Parameters.AddWithValue("$table", tableName);
                        command.Parameters.AddWithValue("$column", columnName);

                        var cnt = (long)command.ExecuteScalar()!;
                        schemaRow[isUniqueColumn] = !isAliased && cnt != 0;

                        command.Parameters.Clear();
                        var columnType = "typeof(\"" + columnName.Replace("\"", "\"\"") + "\")";
                        command.CommandText = new StringBuilder()
                            .AppendLine($"SELECT {columnType}")
                            .AppendLine($"FROM \"{tableName.Replace("\"", "\"\"")}\"")
                            .AppendLine($"WHERE {columnType} != 'null'")
                            .AppendLine($"GROUP BY {columnType}")
                            .AppendLine("ORDER BY count() DESC")
                            .AppendLine("LIMIT 1;").ToString();

                        var type = (string?)command.ExecuteScalar();
                        schemaRow[dataTypeColumn] =
                            (type != null)
                                ? SqliteDataRecord.GetFieldType(type)
                                : SqliteDataRecord.GetFieldTypeFromSqliteType(
                                    SqliteDataRecord.Sqlite3AffinityType(dataTypeName));

                        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE name = $name AND type IN ('table', 'view')";
                        command.Parameters.AddWithValue("$name", tableName);

                        eponymousVirtualTable = (long)command.ExecuteScalar()! == 0L;
                    }

                    if (databaseName != null
                        && !eponymousVirtualTable)
                    {
                        var rc = sqlite3_table_column_metadata(
                            _command.Connection.Handle, databaseName, tableName, columnName, out var dataType, out var collSeq,
                            out var notNull, out var primaryKey, out var autoInc);
                        SqliteException.ThrowExceptionForRC(rc, _command.Connection.Handle);

                        schemaRow[isKeyColumn] = primaryKey != 0;
                        schemaRow[allowDBNullColumn] = isAliased || notNull == 0;
                        schemaRow[isAutoIncrementColumn] = autoInc != 0;
                    }
                }

                schemaTable.Rows.Add(schemaRow);
            }

            return schemaTable;

#if NET6_0_OR_GREATER
            [UnconditionalSuppressMessage("Trimming", "IL2111:Method with parameters or return value with `DynamicallyAccessedMembersAttribute`"
                + " is accessed via reflection. Trimmer can't guarantee availability of the requirements of the method.",
                Justification = "This is about System.Type.TypeInitializer.get. It is accessed via reflection"
                + " as the type parameter in DataColumn is annotated with DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties" +
                " However, reflection is only used for nullable columns.")]
#endif
            static DataColumn CreateDataTypeColumn()
                => new(SchemaTableColumn.DataType, typeof(Type))
                {
                    AllowDBNull = false
                };
        }
    }
}
