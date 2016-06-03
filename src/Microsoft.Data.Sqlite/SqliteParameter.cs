// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.Data.Sqlite.Interop;
using Microsoft.Data.Sqlite.Utilities;

using static Microsoft.Data.Sqlite.Interop.Constants;

namespace Microsoft.Data.Sqlite
{
    // TODO: Truncate to specified size
    // TODO: Infer type and size from value
    /// <summary>
    /// Represents a parameter and its value in a <see cref="SqliteCommand" />.
    /// </summary>
    /// <remarks>Due to SQLite's dynamic type system, parameter values are not converted.</remarks>
    /// <seealso href="http://sqlite.org/datatype3.html">Datatypes In SQLite Version 3</seealso>
    public class SqliteParameter : DbParameter
    {
        private string _parameterName = string.Empty;
        private object _value;
        private Action<Sqlite3StmtHandle, int> _bindAction;
        private bool _bindActionValid;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteParameter" /> class.
        /// </summary>
        public SqliteParameter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteParameter" /> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter. Can be null.</param>
        public SqliteParameter(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _parameterName = name;
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteParameter" /> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        public SqliteParameter(string name, SqliteType type)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _parameterName = name;
            SqliteType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteParameter" /> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="size">The maximum size, in bytes, of the parameter.</param>
        public SqliteParameter(string name, SqliteType type, int size)
            : this(name, type)
        {
            Size = size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteParameter" /> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="size">The maximum size, in bytes, of the parameter.</param>
        /// <param name="sourceColumn">The source column used for loading the value. Can be null.</param>
        public SqliteParameter(string name, SqliteType type, int size, string sourceColumn)
            : this(name, type, size)
        {
            SourceColumn = sourceColumn;
        }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        /// <remarks>Due to SQLite's dynamic type system, parameter values are not converted.</remarks>
        /// <seealso href="http://sqlite.org/datatype3.html">Datatypes In SQLite Version 3</seealso>
        public override DbType DbType { get; set; } = DbType.String;

        /// <summary>
        /// Gets or sets the SQLite type of the parameter.
        /// </summary>
        /// <remarks>Due to SQLite's dynamic type system, parameter values are not converted.</remarks>
        /// <seealso href="http://sqlite.org/datatype3.html">Datatypes In SQLite Version 3</seealso>
        public virtual SqliteType SqliteType { get; set; } = SqliteType.Text;

        /// <summary>
        /// Gets or sets direction of the parameter. Only <see cref="ParameterDirection.Input" /> is supported.
        /// </summary>
        public override ParameterDirection Direction
        {
            get { return ParameterDirection.Input; }
            set
            {
                if (value != ParameterDirection.Input)
                {
                    throw new ArgumentException(Strings.InvalidParameterDirection(value));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is nullable.
        /// </summary>
        public override bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of the parameter.
        /// </summary>
        public override int Size { get; set; }

        /// <summary>
        /// Gets or sets the source column used for loading the value.
        /// </summary>
        public override string SourceColumn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the source column is nullable.
        /// </summary>
        public override bool SourceColumnNullMapping { get; set; }

#if NET451
        /// <summary>
        /// Gets or sets the version to use when loading the value.
        /// </summary>
        public override DataRowVersion SourceVersion { get; set; }
#endif

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <remarks>Due to SQLite's dynamic type system, parameter values are not converted.</remarks>
        /// <seealso href="http://sqlite.org/datatype3.html">Datatypes In SQLite Version 3</seealso>
        public override object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _bindActionValid = false;
            }
        }

        /// <summary>
        /// Resets the <see cref="DbType" /> property to its original value.
        /// </summary>
        public override void ResetDbType()
            => ResetSqliteType();

        /// <summary>
        /// Resets the <see cref="SqliteType" /> property to its original value.
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
                throw new InvalidOperationException(Strings.RequiresSet("ParameterName"));
            }

            var index = NativeMethods.sqlite3_bind_parameter_index(stmt, _parameterName);
            if (index == 0 &&
                (index = FindPrefixedParameter(stmt)) == 0)
            {
                return false;
            }

            if (_value == null)
            {
                throw new InvalidOperationException(Strings.RequiresSet("Value"));
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
                    _bindAction = (s, i) => BindDouble(s, i, value);
                }
                else if (type == typeof(float))
                {
                    var value = (double)(float)_value;
                    _bindAction = (s, i) => BindDouble(s, i, value);
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
                    throw new InvalidOperationException(Strings.UnknownDataType(type));
                }

                _bindActionValid = true;
            }

            _bindAction(stmt, index);

            return true;
        }

        private static void BindBlob(Sqlite3StmtHandle stmt, int index, byte[] value)
            => NativeMethods.sqlite3_bind_blob(stmt, index, value, value.Length, SQLITE_TRANSIENT);

        private static void BindText(Sqlite3StmtHandle stmt, int index, string value)
            => NativeMethods.sqlite3_bind_text(stmt, index, value, SQLITE_TRANSIENT);

        private static void BindDouble(Sqlite3StmtHandle stmt, int index, double value)
        {
            if (double.IsNaN(value))
            {
                throw new InvalidOperationException(Strings.CannotStoreNaN);
            }

            NativeMethods.sqlite3_bind_double(stmt, index, value);
        }

        private readonly static char[] _parameterPrefixes = { '@', '$', ':' };

        private int FindPrefixedParameter(Sqlite3StmtHandle stmt)
        {
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
                    throw new InvalidOperationException(Strings.AmbiguousParameterName(_parameterName));
                }

                index = nextIndex;
            }

            return index;
        }
    }
}
