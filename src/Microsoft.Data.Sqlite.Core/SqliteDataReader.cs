// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Provides methods for reading the result of a command executed against a SQLite database.
    /// </summary>
    public class SqliteDataReader : DbDataReader
    {
        private readonly SqliteCommand _command;
        private readonly bool _closeConnection;
        private readonly Queue<(sqlite3_stmt stmt, bool)> _stmtQueue;
        private sqlite3_stmt _stmt;
        private SqliteDataRecord _record;
        private bool _hasRows;
        private bool _stepped;
        private bool _done;
        private bool _closed;

        internal SqliteDataReader(
            SqliteCommand command,
            Queue<(sqlite3_stmt, bool)> stmtQueue,
            int recordsAffected,
            bool closeConnection)
        {
            if (stmtQueue.Count != 0)
            {
                (_stmt, _hasRows) = stmtQueue.Dequeue();
                _record = new SqliteDataRecord(_stmt);
            }

            _command = command;
            _stmtQueue = stmtQueue;
            RecordsAffected = recordsAffected;
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
                : _record.FieldCount;

        /// <summary>
        ///     Gets a handle to underlying prepared statement.
        /// </summary>
        /// <value>A handle to underlying prepared statement.</value>
        /// <seealso href="http://sqlite.org/c3ref/stmt.html">Prepared Statement Object</seealso>
        public virtual sqlite3_stmt Handle
            => _stmt;

        /// <summary>
        ///     Gets a value indicating whether the data reader contains any rows.
        /// </summary>
        /// <value>A value indicating whether the data reader contains any rows.</value>
        public override bool HasRows
            => _hasRows;

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
        public override int RecordsAffected { get; }

        /// <summary>
        ///     Gets the value of the specified column.
        /// </summary>
        /// <param name="name">The name of the column. The value is case-sensitive.</param>
        /// <returns>The value.</returns>
        public override object this[string name]
            => _record[name];

        /// <summary>
        ///     Gets the value of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value.</returns>
        public override object this[int ordinal]
            => _record[ordinal];

        /// <summary>
        ///     Gets an enumerator that can be used to iterate through the rows in the data reader.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public override IEnumerator GetEnumerator()
            => new DbEnumerator(this, closeReader: false);

        /// <summary>
        ///     Advances to the next row in the result set.
        /// </summary>
        /// <returns>true if there are more rows; otherwise, false.</returns>
        public override bool Read()
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(Read)));
            }

            if (!_stepped)
            {
                _stepped = true;

                return _hasRows;
            }

            var rc = raw.sqlite3_step(_stmt);
            SqliteException.ThrowExceptionForRC(rc, _command.Connection.Handle);
            _record.Clear();

            _done = rc == raw.SQLITE_DONE;

            return !_done;
        }

        /// <summary>
        ///     Advances to the next result set for batched statements.
        /// </summary>
        /// <returns>true if there are more result sets; otherwise, false.</returns>
        public override bool NextResult()
        {
            if (_stmtQueue.Count == 0)
            {
                return false;
            }

            raw.sqlite3_reset(_stmt);

            (_stmt, _hasRows) = _stmtQueue.Dequeue();
            _record = new SqliteDataRecord(_stmt);
            _stepped = false;
            _done = false;

            return true;
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
        ///     true to release managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _command.DataReader = null;

            if (_stmt != null)
            {
                raw.sqlite3_reset(_stmt);
                _stmt = null;
                _record = null;
            }

            while (_stmtQueue.Count != 0)
            {
                raw.sqlite3_reset(_stmtQueue.Dequeue().stmt);
            }

            _closed = true;

            if (_closeConnection)
            {
                _command.Connection.Close();
            }
        }

        /// <summary>
        ///     Gets the name of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The name of the column.</returns>
        public override string GetName(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetName)));
            }

            return _record.GetName(ordinal);
        }

        /// <summary>
        ///     Gets the ordinal of the specified column.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The zero-based column ordinal.</returns>
        public override int GetOrdinal(string name)
            => _record.GetOrdinal(name);

        /// <summary>
        ///     Gets the declared data type name of the specified column. The storage class is returned for computed
        ///     columns.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The data type name of the column.</returns>
        /// <remarks>Due to SQLite's dynamic type system, this may not reflect the actual type of the value.</remarks>
        /// <seealso href="http://sqlite.org/datatype3.html">Datatypes In SQLite Version 3</seealso>
        public override string GetDataTypeName(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetDataTypeName)));
            }

            return _record.GetDataTypeName(ordinal);
        }

        /// <summary>
        ///     Gets the data type of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The data type of the column.</returns>
        public override Type GetFieldType(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetFieldType)));
            }

            return _record.GetFieldType(ordinal);
        }

        /// <summary>
        ///     Gets a value indicating whether the specified column is <see cref="DBNull" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>true if the specified column is <see cref="DBNull" />; otherwise, false.</returns>
        public override bool IsDBNull(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(IsDBNull)))
                : !_stepped || _done
                    ? throw new InvalidOperationException(Resources.NoData)
                    : _record.IsDBNull(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="bool" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override bool GetBoolean(int ordinal)
            => _record.GetBoolean(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="byte" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override byte GetByte(int ordinal)
            => _record.GetByte(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="char" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override char GetChar(int ordinal)
            => _record.GetChar(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="DateTime" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override DateTime GetDateTime(int ordinal)
            => _record.GetDateTime(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="DateTimeOffset" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public virtual DateTimeOffset GetDateTimeOffset(int ordinal)
            => _record.GetDateTimeOffset(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="TimeSpan" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public virtual TimeSpan GetTimeSpan(int ordinal)
            => _record.GetTimeSpan(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="decimal" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override decimal GetDecimal(int ordinal)
            => _record.GetDecimal(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="double" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override double GetDouble(int ordinal)
            => _record.GetDouble(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="float" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override float GetFloat(int ordinal)
            => _record.GetFloat(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="Guid" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override Guid GetGuid(int ordinal)
            => _record.GetGuid(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="short" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override short GetInt16(int ordinal)
            => _record.GetInt16(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="int" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override int GetInt32(int ordinal)
            => _record.GetInt32(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="long" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override long GetInt64(int ordinal)
            => _record.GetInt64(ordinal);

        /// <summary>
        ///     Gets the value of the specified column as a <see cref="string" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override string GetString(int ordinal)
            => _record.GetString(ordinal);

        /// <summary>
        ///     Reads a stream of bytes from the specified column. Not supported.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which the data is copied.</param>
        /// <param name="bufferOffset">The index to which the data will be copied.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
            => _record.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

        /// <summary>
        ///     Reads a stream of characters from the specified column. Not supported.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which the data is copied.</param>
        /// <param name="bufferOffset">The index to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of characters read.</returns>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
            => _record.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

        /// <summary>
        ///     Retrieves data as a Stream.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The returned object.</returns>
        public override Stream GetStream(int ordinal)
            => _record.GetStream(ordinal);

        /// <summary>
        ///     Gets the value of the specified column.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override T GetFieldValue<T>(int ordinal)
        {
            if (typeof(T) == typeof(DBNull)
                && (!_stepped || _done))
            {
                throw new InvalidOperationException(Resources.NoData);
            }

            return _record.GetFieldValue<T>(ordinal);
        }

        /// <summary>
        ///     Gets the value of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override object GetValue(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetValue)));
            }
            if (!_stepped || _done)
            {
                throw new InvalidOperationException(Resources.NoData);
            }

            return _record.GetValue(ordinal);
        }

        /// <summary>
        ///     Gets the column values of the current row.
        /// </summary>
        /// <param name="values">An array into which the values are copied.</param>
        /// <returns>The number of values copied into the array.</returns>
        public override int GetValues(object[] values)
            => _record.GetValues(values);

        /// <summary>
        ///     Returns a System.Data.DataTable that describes the column metadata of the System.Data.Common.DbDataReader.
        /// </summary>
        /// <returns>A System.Data.DataTable that describes the column metadata.</returns>
        public override DataTable GetSchemaTable()
        {
            var schemaTable = new DataTable("SchemaTable");

            var ColumnName = new DataColumn(SchemaTableColumn.ColumnName, typeof(string));
            var ColumnOrdinal = new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int));
            var ColumnSize = new DataColumn(SchemaTableColumn.ColumnSize, typeof(int));
            var NumericPrecision = new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short));
            var NumericScale = new DataColumn(SchemaTableColumn.NumericScale, typeof(short));

            var DataType = new DataColumn(SchemaTableColumn.DataType, typeof(Type));
            var DataTypeName = new DataColumn("DataTypeName", typeof(string));

            var IsLong = new DataColumn(SchemaTableColumn.IsLong, typeof(bool));
            var AllowDBNull = new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool));

            var IsUnique = new DataColumn(SchemaTableColumn.IsUnique, typeof(bool));
            var IsKey = new DataColumn(SchemaTableColumn.IsKey, typeof(bool));
            var IsAutoIncrement = new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));

            var BaseCatalogName = new DataColumn(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
            var BaseSchemaName = new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string));
            var BaseTableName = new DataColumn(SchemaTableColumn.BaseTableName, typeof(string));
            var BaseColumnName = new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string));

            var BaseServerName = new DataColumn(SchemaTableOptionalColumn.BaseServerName, typeof(string));
            var IsAliased = new DataColumn(SchemaTableColumn.IsAliased, typeof(bool));
            var IsExpression = new DataColumn(SchemaTableColumn.IsExpression, typeof(bool));

            var columns = schemaTable.Columns;

            columns.Add(ColumnName);
            columns.Add(ColumnOrdinal);
            columns.Add(ColumnSize);
            columns.Add(NumericPrecision);
            columns.Add(NumericScale);
            columns.Add(IsUnique);
            columns.Add(IsKey);
            columns.Add(BaseServerName);
            columns.Add(BaseCatalogName);
            columns.Add(BaseColumnName);
            columns.Add(BaseSchemaName);
            columns.Add(BaseTableName);
            columns.Add(DataType);
            columns.Add(DataTypeName);
            columns.Add(AllowDBNull);
            columns.Add(IsAliased);
            columns.Add(IsExpression);
            columns.Add(IsAutoIncrement);
            columns.Add(IsLong);

            for (var i = 0; i < FieldCount; i++)
            {
                var schemaRow = schemaTable.NewRow();
                schemaRow[ColumnName] = GetName(i);
                schemaRow[ColumnOrdinal] = i;
                schemaRow[ColumnSize] = DBNull.Value;
                schemaRow[NumericPrecision] = DBNull.Value;
                schemaRow[NumericScale] = DBNull.Value;
                schemaRow[BaseServerName] = _command.Connection.DataSource;
                var databaseName = raw.sqlite3_column_database_name(_stmt, i);
                schemaRow[BaseCatalogName] = databaseName;
                var columnName = raw.sqlite3_column_origin_name(_stmt, i);
                schemaRow[BaseColumnName] = columnName;
                schemaRow[BaseSchemaName] = DBNull.Value;
                var tableName = raw.sqlite3_column_table_name(_stmt, i);
                schemaRow[BaseTableName] = tableName;
                schemaRow[DataType] = GetFieldType(i);
                schemaRow[DataTypeName] = GetDataTypeName(i);
                schemaRow[IsAliased] = columnName != GetName(i);
                schemaRow[IsExpression] = columnName == null;
                schemaRow[IsLong] = DBNull.Value;

                if (!string.IsNullOrEmpty(tableName)
                    && !string.IsNullOrEmpty(columnName))
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

                        var cnt = (long)command.ExecuteScalar();
                        schemaRow[IsUnique] = cnt != 0;

                        command.Parameters.Clear();
                        var columnType = "typeof(\"" + columnName.Replace("\"", "\"\"") + "\")";
                        command.CommandText = new StringBuilder()
                            .AppendLine($"SELECT {columnType}")
                            .AppendLine($"FROM \"{tableName}\"")
                            .AppendLine($"WHERE {columnType} != 'null'")
                            .AppendLine($"GROUP BY {columnType}")
                            .AppendLine("ORDER BY count() DESC")
                            .AppendLine("LIMIT 1;").ToString();

                        var type = (string)command.ExecuteScalar();
                        schemaRow[DataType] = SqliteDataRecord.GetFieldType(type);
                    }

                    if (!string.IsNullOrEmpty(databaseName))
                    {
                        var rc = raw.sqlite3_table_column_metadata(_command.Connection.Handle, databaseName, tableName, columnName, out var dataType, out var collSeq, out var notNull, out var primaryKey, out var autoInc);
                        SqliteException.ThrowExceptionForRC(rc, _command.Connection.Handle);

                        schemaRow[IsKey] = primaryKey != 0;
                        schemaRow[AllowDBNull] = notNull == 0;
                        schemaRow[IsAutoIncrement] = autoInc != 0;
                    }
                }

                schemaTable.Rows.Add(schemaRow);
            }

            return schemaTable;
        }
    }
}
