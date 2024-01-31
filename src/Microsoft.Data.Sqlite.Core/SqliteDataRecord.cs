// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    
    internal class SqliteDataRecord : SqliteValueReader, IDisposable
    {
        internal class RowIdInfo
        {
            public int Ordinal { get; set; }
            public string TableName { get; set; }

            public RowIdInfo(int ordinal, string tableName)
            {
                Ordinal = ordinal;
                TableName = tableName;
            }
        }

        private readonly SqliteConnection _connection;
        private readonly Action<int> _addChanges;
        private byte[][]? _blobCache;
        private int?[]? _typeCache;
        private Dictionary<string, int>? _columnNameOrdinalCache;
        private string[]? _columnNameCache;
        private bool _stepped;
        readonly Dictionary<string, RowIdInfo> RowIds = new Dictionary<string, RowIdInfo>();

        private bool _alreadyThrown;
        private bool _alreadyAddedChanges;

        public SqliteDataRecord(sqlite3_stmt stmt, bool hasRows, SqliteConnection connection, Action<int> addChanges)
        {
            Handle = stmt;
            HasRows = hasRows;
            _connection = connection;
            _addChanges = addChanges;
        }

        public virtual object this[string name]
            => GetValue(GetOrdinal(name));

        public virtual object this[int ordinal]
            => GetValue(ordinal);

        public override int FieldCount
            => sqlite3_column_count(Handle);

        public sqlite3_stmt Handle { get; }

        public bool HasRows { get; }

        public override bool IsDBNull(int ordinal)
            => !_stepped || sqlite3_data_count(Handle) == 0
                ? throw new InvalidOperationException(Resources.NoData)
                : base.IsDBNull(ordinal);

        public override object GetValue(int ordinal)
            => !_stepped || sqlite3_data_count(Handle) == 0
                ? throw new InvalidOperationException(Resources.NoData)
                : base.GetValue(ordinal)!;

        protected override double GetDoubleCore(int ordinal)
            => sqlite3_column_double(Handle, ordinal);

        protected override long GetInt64Core(int ordinal)
            => sqlite3_column_int64(Handle, ordinal);

        protected override string GetStringCore(int ordinal)
            => sqlite3_column_text(Handle, ordinal).utf8_to_string();

        public override T GetFieldValue<T>(int ordinal)
        {
            if (typeof(T) == typeof(Stream))
            {
                return (T)(object)GetStream(ordinal);
            }

            if (typeof(T) == typeof(TextReader))
            {
                return (T)(object)GetTextReader(ordinal);
            }

            return base.GetFieldValue<T>(ordinal)!;
        }

        protected override byte[] GetBlob(int ordinal)
            => base.GetBlob(ordinal)!;

        protected override byte[] GetBlobCore(int ordinal)
            => sqlite3_column_blob(Handle, ordinal).ToArray();

        protected override int GetSqliteType(int ordinal)
        {
            var type = sqlite3_column_type(Handle, ordinal);
            if (type == SQLITE_NULL
                && (ordinal < 0 || ordinal >= FieldCount))
            {
                // NB: Message is provided by the framework
                throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, message: null);
            }

            return type;
        }

        protected override T GetNull<T>(int ordinal)
            => typeof(T) == typeof(DBNull) || typeof(T) == typeof(object)
                ? (T)(object)DBNull.Value
                : throw new InvalidOperationException(GetOnNullErrorMsg(ordinal));

        public virtual string GetName(int ordinal)
        {
            var name = _columnNameCache?[ordinal] ?? sqlite3_column_name(Handle, ordinal).utf8_to_string();
            if (name == null
                && (ordinal < 0 || ordinal >= FieldCount))
            {
                // NB: Message is provided by the framework
                throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, message: null);
            }

            _columnNameCache ??= new string[FieldCount];
            _columnNameCache[ordinal] = name!;

            return name!;
        }

        public virtual int GetOrdinal(string name)
        {
            if (_columnNameOrdinalCache == null)
            {
                _columnNameOrdinalCache = new Dictionary<string, int>();
                for (var i = 0; i < FieldCount; i++)
                {
                    _columnNameOrdinalCache[GetName(i)] = i;
                }
            }

            if (_columnNameOrdinalCache.TryGetValue(name, out var ordinal))
            {
                return ordinal;
            }

            KeyValuePair<string, int>? match = null;
            foreach (var item in _columnNameOrdinalCache)
            {
                if (string.Equals(name, item.Key, StringComparison.OrdinalIgnoreCase))
                {
                    if (match != null)
                    {
                        throw new InvalidOperationException(
                            Resources.AmbiguousColumnName(name, match.Value.Key, item.Key));
                    }

                    match = item;
                }
            }

            if (match != null)
            {
                _columnNameOrdinalCache.Add(name, match.Value.Value);

                return match.Value.Value;
            }

            // NB: Message is provided by framework
            throw new ArgumentOutOfRangeException(nameof(name), name, message: null);
        }

        public virtual string GetDataTypeName(int ordinal)
        {
            var typeName = sqlite3_column_decltype(Handle, ordinal).utf8_to_string();
            if (typeName != null)
            {
                var i = typeName.IndexOf('(');

                return i == -1
                    ? typeName
                    : typeName.Substring(0, i);
            }

            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case SQLITE_INTEGER:
                    return "INTEGER";

                case SQLITE_FLOAT:
                    return "REAL";

                case SQLITE_TEXT:
                    return "TEXT";

                default:
                    Debug.Assert(sqliteType is SQLITE_BLOB or SQLITE_NULL, "Unexpected column type: " + sqliteType);
                    return "BLOB";
            }
        }

#if NET6_0_OR_GREATER
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)]
#endif
        public virtual Type GetFieldType(int ordinal)
        {
            var sqliteType = GetSqliteType(ordinal);
            if (sqliteType == SQLITE_NULL)
            {
                sqliteType = _typeCache?[ordinal] ?? Sqlite3AffinityType(GetDataTypeName(ordinal));
            }
            else
            {
                _typeCache ??= new int?[FieldCount];
                _typeCache[ordinal] = sqliteType;
            }

            return GetFieldTypeFromSqliteType(sqliteType);
        }

#if NET6_0_OR_GREATER
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)]
#endif
        internal static Type GetFieldTypeFromSqliteType(int sqliteType)
        {
            switch (sqliteType)
            {
                case SQLITE_INTEGER:
                    return typeof(long);

                case SQLITE_FLOAT:
                    return typeof(double);

                case SQLITE_TEXT:
                    return typeof(string);

                default:
                    Debug.Assert(sqliteType is SQLITE_BLOB or SQLITE_NULL, "Unexpected column type: " + sqliteType);
                    return typeof(byte[]);
            }
        }

#if NET6_0_OR_GREATER
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields)]
#endif
        public static Type GetFieldType(string type)
        {
            switch (type)
            {
                case "integer":
                    return typeof(long);

                case "real":
                    return typeof(double);

                case "text":
                    return typeof(string);

                default:
                    Debug.Assert(type is "blob" or null, "Unexpected column type: " + type);
                    return typeof(byte[]);
            }
        }

        public virtual long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
        {
            using var stream = GetStream(ordinal);

            if (buffer == null)
            {
                return stream.Length;
            }

            stream.Position = dataOffset;

            return stream.Read(buffer, bufferOffset, length);
        }

        public virtual long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
        {
            using var reader = new StreamReader(GetStream(ordinal), Encoding.UTF8);

            if (buffer == null)
            {
                // TODO: Consider using a stackalloc buffer and reading blocks instead
                var charCount = 0;
                while (reader.Read() != -1)
                {
                    charCount++;
                }

                return charCount;
            }

            for (var position = 0; position < dataOffset; position++)
            {
                if (reader.Read() == -1)
                {
                    // NB: Message is provided by the framework
                    throw new ArgumentOutOfRangeException(nameof(dataOffset), dataOffset, message: null);
                }
            }

            return reader.Read(buffer, bufferOffset, length);
        }

        public virtual Stream GetStream(int ordinal)
        {
            if (ordinal < 0
                || ordinal >= FieldCount)
            {
                throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, message: null);
            }

            var blobDatabaseName = sqlite3_column_database_name(Handle, ordinal).utf8_to_string();
            var blobTableName = sqlite3_column_table_name(Handle, ordinal).utf8_to_string();

            RowIdInfo? rowIdForOrdinal = null;
            string rowidkey = $"{blobDatabaseName}_{blobTableName}";
            if (!RowIds.TryGetValue(rowidkey, out rowIdForOrdinal))
            {
                var pkColumns = -1L;
                for (var i = 0; i < FieldCount; i++)
                {
                    if (i == ordinal)
                    {
                        continue;
                    }

                    var databaseName = sqlite3_column_database_name(Handle, i).utf8_to_string();
                    if (databaseName != blobDatabaseName)
                    {
                        continue;
                    }

                    var tableName = sqlite3_column_table_name(Handle, i).utf8_to_string();
                    if (tableName != blobTableName)
                    {
                        continue;
                    }

                    var columnName = sqlite3_column_origin_name(Handle, i).utf8_to_string();
                    if (columnName == "rowid")
                    {
                        rowIdForOrdinal = new RowIdInfo(i, tableName);
                        RowIds.Add(rowidkey, rowIdForOrdinal);
                        break;
                    }

                    var rc = sqlite3_table_column_metadata(
                        _connection.Handle,
                        databaseName,
                        tableName,
                        columnName,
                        out var dataType,
                        out var collSeq,
                        out var notNull,
                        out var primaryKey,
                        out var autoInc);
                    SqliteException.ThrowExceptionForRC(rc, _connection.Handle);
                    if (string.Equals(dataType, "INTEGER", StringComparison.OrdinalIgnoreCase)
                        && primaryKey != 0)
                    {
                        if (pkColumns < 0L)
                        {
                            using (var command = _connection.CreateCommand())
                            {
                                command.CommandText = "SELECT COUNT(*) FROM pragma_table_info($table) WHERE pk != 0;";
                                command.Parameters.AddWithValue("$table", tableName);

                                pkColumns = (long)command.ExecuteScalar()!;
                            }
                        }

                        if (pkColumns == 1L)
                        {
                            rowIdForOrdinal = new RowIdInfo(i, tableName);
                            RowIds.Add(rowidkey, rowIdForOrdinal);
                            break;
                        }
                    }
                }

                //Debug.Assert(rowIdForOrdinal!=null);
                //debug assertion no more needed:
                //rowIdForOrdinal == null => matching rowid not found, MemoryStream returned
                //rowIdForOrdinal != null => matching rowid found, SqliteBlob returned
            }

            if (rowIdForOrdinal == null)
            {
                return new MemoryStream(GetCachedBlob(ordinal), false);
            }

            var blobColumnName = sqlite3_column_origin_name(Handle, ordinal).utf8_to_string();
            var rowid = GetInt64(rowIdForOrdinal.Ordinal);

            return new SqliteBlob(_connection, blobDatabaseName, blobTableName, blobColumnName, rowid, readOnly: true);
        }

        public virtual TextReader GetTextReader(int ordinal)
            => IsDBNull(ordinal)
                ? new StringReader(string.Empty)
                : new StreamReader(GetStream(ordinal), Encoding.UTF8);

        public bool Read()
        {
            if (!_stepped)
            {
                _stepped = true;

                return HasRows;
            }

            if (sqlite3_data_count(Handle) == 0)
            {
                return false;
            }

            int rc;
            try
            {
                rc = sqlite3_step(Handle);
                SqliteException.ThrowExceptionForRC(rc, _connection.Handle);
            }
            catch
            {
                _alreadyThrown = true;

                throw;
            }

            if (_blobCache != null)
            {
                Array.Clear(_blobCache, 0, _blobCache.Length);
            }

            if (rc != SQLITE_DONE)
            {
                return true;
            }
            
            AddChanges();
            _alreadyAddedChanges = true;

            return false;
        }

        public void Dispose()
        {
            var rc = sqlite3_reset(Handle);
            if (!_alreadyThrown)
            {
                SqliteException.ThrowExceptionForRC(rc, _connection.Handle);
            }

            if (!_alreadyAddedChanges)
            {
                AddChanges();
            }
        }

        private void AddChanges()
        {
            if (sqlite3_stmt_readonly(Handle) != 0)
            {
                return;
            }

            var changes = sqlite3_changes(_connection.Handle);
            _addChanges(changes);
        }

        private byte[] GetCachedBlob(int ordinal)
        {
            if (ordinal < 0
                || ordinal >= FieldCount)
            {
                // NB: Message is provided by the framework
                throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, message: null);
            }

            var blob = _blobCache?[ordinal];
            if (blob == null)
            {
                blob = GetBlob(ordinal);
                _blobCache ??= new byte[FieldCount][];
                _blobCache[ordinal] = blob;
            }

            return blob;
        }

        internal static int Sqlite3AffinityType(string dataTypeName)
        {
            if (dataTypeName == null)
            {
                // if no type is specified then the column has affinity BLOB
                return SQLITE_BLOB;
            }

            var typeRules = new Func<string, int?>[]
            {
                name => Contains(name, "INT") ? SQLITE_INTEGER : (int?)null,
                name => Contains(name, "CHAR")
                    || Contains(name, "CLOB")
                    || Contains(name, "TEXT")
                        ? SQLITE_TEXT
                        : (int?)null,
                name => Contains(name, "BLOB") ? SQLITE_BLOB : (int?)null,
                name => Contains(name, "REAL")
                    || Contains(name, "FLOA")
                    || Contains(name, "DOUB")
                        ? SQLITE_FLOAT
                        : (int?)null
            };

            return typeRules.Select(r => r(dataTypeName)).FirstOrDefault(r => r != null) ?? SQLITE_TEXT; // code NUMERICAL affinity as TEXT
        }

        private static bool Contains(string haystack, string needle)
            => haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
