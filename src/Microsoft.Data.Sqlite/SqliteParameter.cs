// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using Microsoft.Data.Sqlite.Interop;
using Microsoft.Data.Sqlite.Utilities;

namespace Microsoft.Data.Sqlite
{
    // TODO: Truncate to specified size
    // TODO: Convert to specified type
    // TODO: Infer type and size from value
    /// <summary>
    /// Represents a parameter and its value in a SQL statement to be executed against a SQLite database.
    /// </summary>
    public class SqliteParameter : DbParameter
    {
        private string _parameterName = string.Empty;
        private object _value;
        private Action<Sqlite3StmtHandle, int> _bindAction;
        private bool _bindActionValid;

        public SqliteParameter()
        {
        }

        public SqliteParameter(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _parameterName = name;
            Value = value;
        }

        public SqliteParameter(string name, SqliteType type)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _parameterName = name;
            SqliteType = type;
        }

        public SqliteParameter(string name, SqliteType type, int size)
            : this(name, type)
        {
            Size = size;
        }

        public SqliteParameter(string name, SqliteType type, int size, string sourceColumn)
            : this(name, type, size)
        {
            SourceColumn = sourceColumn;
        }

        public override DbType DbType { get; set; } = DbType.String;
        /// <summary>
        /// Represents the type affinity for this parameter.
        /// </summary>
        public virtual SqliteType SqliteType { get; set; } = SqliteType.Text;

        public override ParameterDirection Direction
        {
            get { return ParameterDirection.Input; }
            set
            {
                if (value != ParameterDirection.Input)
                {
                    throw new ArgumentException(Strings.FormatInvalidParameterDirection(value));
                }
            }
        }

        public override bool IsNullable { get; set; }

        public override string ParameterName
        {
            get { return _parameterName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }

                _parameterName = value;
            }
        }

        public override int Size { get; set; }
        public override string SourceColumn { get; set; } = string.Empty;
        public override bool SourceColumnNullMapping { get; set; }

#if NET451
        public override DataRowVersion SourceVersion { get; set; }
#endif

        public override object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _bindActionValid = false;
            }
        }

        public override void ResetDbType() => ResetSqliteType();

        /// <summary>
        /// Sets the parameter type to <see cref="SqliteType">SqliteType.Text</see>.
        /// </summary>
        public virtual void ResetSqliteType()
        {
            DbType = DbType.String;
            SqliteType = SqliteType.Text;
        }

        internal bool Bind(Sqlite3StmtHandle stmt)
        {
            if (_parameterName.Length == 0)
            {
                throw new InvalidOperationException(Strings.FormatRequiresSet("ParameterName"));
            }

            var index = NativeMethods.sqlite3_bind_parameter_index(stmt, _parameterName);
            if (index == 0 &&
                (index = FindPrefixedParameter(stmt)) == 0)
            {
                return false;
            }

            if (_value == null)
            {
                throw new InvalidOperationException(Strings.FormatRequiresSet("Value"));
            }

            if (!_bindActionValid)
            {
                var type = Value.GetType().UnwrapNullableType().UnwrapEnumType();
                if (type == typeof(bool))
                {
                    var value = (bool)_value ? 1L : 0;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_int64(s, i, value);
                }
                else if (type == typeof(byte))
                {
                    var value = (long)(byte)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_int64(s, i, value);
                }
                else if (type == typeof(byte[]))
                {
                    var value = (byte[])_value;
                    _bindAction = (s, i) => BindBlob(s, i, value);
                }
                else if (type == typeof(char))
                {
                    var value = (long)(char)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_int64(s, i, value);
                }
                else if (type == typeof(DateTime))
                {
                    var value = ((DateTime)_value).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF");
                    _bindAction = (s, i) => BindText(s, i, value);
                }
                else if (type == typeof(DateTimeOffset))
                {
                    var value = ((DateTimeOffset)_value).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz");
                    _bindAction = (s, i) => BindText(s, i, value);
                }
                else if (type == typeof(DBNull))
                {
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_null(s, i);
                }
                else if (type == typeof(decimal))
                {
                    var value = ((decimal)_value).ToString(CultureInfo.InvariantCulture);
                    _bindAction = (s, i) => BindText(s, i, value);
                }
                else if (type == typeof(double))
                {
                    var value = (double)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_double(s, i, value);
                }
                else if (type == typeof(float))
                {
                    var value = (double)(float)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_double(s, i, value);
                }
                else if (type == typeof(Guid))
                {
                    var value = ((Guid)_value).ToByteArray();
                    _bindAction = (s, i) => BindBlob(s, i, value);
                }
                else if (type == typeof(int))
                {
                    var value = (long)(int)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_int64(s, i, value);
                }
                else if (type == typeof(long))
                {
                    var value = (long)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_int64(s, i, value);
                }
                else if (type == typeof(sbyte))
                {
                    var value = (long)(sbyte)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_int64(s, i, value);
                }
                else if (type == typeof(short))
                {
                    var value = (long)(short)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_int64(s, i, value);
                }
                else if (type == typeof(string))
                {
                    var value = (string)_value;
                    _bindAction = (s, i) => BindText(s, i, value);
                }
                else if (type == typeof(TimeSpan))
                {
                    var value = ((TimeSpan)_value).ToString("c");
                    _bindAction = (s, i) => BindText(s, i, value);
                }
                else if (type == typeof(uint))
                {
                    var value = (long)(uint)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_int64(s, i, value);
                }
                else if (type == typeof(ulong))
                {
                    var value = (long)(ulong)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_int64(s, i, value);
                }
                else if (type == typeof(ushort))
                {
                    var value = (long)(ushort)_value;
                    _bindAction = (s, i) => NativeMethods.sqlite3_bind_int64(s, i, value);
                }
                else
                {
                    throw new InvalidOperationException(Strings.FormatUnknownDataType(type));
                }

                _bindActionValid = true;
            }

            _bindAction(stmt, index);

            return true;
        }

        private static void BindBlob(Sqlite3StmtHandle stmt, int index, byte[] value) =>
            NativeMethods.sqlite3_bind_blob(stmt, index, value, value.Length, Constants.SQLITE_TRANSIENT);

        private static void BindText(Sqlite3StmtHandle stmt, int index, string value) =>
            NativeMethods.sqlite3_bind_text(stmt, index, value, Constants.SQLITE_TRANSIENT);

        private readonly static char[] _parameterPrefixes = { '@', '$', ':' };

        private int FindPrefixedParameter(Sqlite3StmtHandle stmt)
        {
            var count = NativeMethods.sqlite3_bind_parameter_count(stmt);
            var index = 0;
            int nextIndex;

            foreach (var prefix in _parameterPrefixes)
            {
                if (_parameterName[0] == prefix)
                {
                    // If name already has a prefix characters, the first call to sqlite3_bind_parameter_index
                    // would have worked if the parameter name was in the statement
                    return 0;
                }

                nextIndex = NativeMethods.sqlite3_bind_parameter_index(stmt, prefix + _parameterName);

                if (nextIndex == 0)
                {
                    continue;
                }

                if (index != 0)
                {
                    throw new InvalidOperationException(Strings.FormatAmbiguousParameterName(_parameterName));
                }

                index = nextIndex;
            }

            return index;
        }
    }
}
