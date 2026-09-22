// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents a parameter and its value in a <see cref="SqliteCommand" />.
    /// </summary>
    /// <remarks>Due to SQLite's dynamic type system, parameter values are not converted.</remarks>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
    public class SqliteParameter : DbParameter
    {
        private string _parameterName = string.Empty;
        private object? _value;
        private int? _size;
        private SqliteType? _sqliteType;
        private string _sourceColumn = string.Empty;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteParameter" /> class.
        /// </summary>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public SqliteParameter()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteParameter" /> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter. Can be null.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
        public SqliteParameter(string? name, object? value)
        {
            ParameterName = name;
            Value = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteParameter" /> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public SqliteParameter(string? name, SqliteType type)
        {
            ParameterName = name;
            SqliteType = type;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteParameter" /> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="size">The maximum size, in bytes, of the parameter.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public SqliteParameter(string? name, SqliteType type, int size)
            : this(name, type)
            => Size = size;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteParameter" /> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="size">The maximum size, in bytes, of the parameter.</param>
        /// <param name="sourceColumn">The source column used for loading the value. Can be null.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public SqliteParameter(string? name, SqliteType type, int size, string? sourceColumn)
            : this(name, type, size)
            => SourceColumn = sourceColumn;

        /// <summary>
        ///     Gets or sets the type of the parameter.
        /// </summary>
        /// <value>The type of the parameter.</value>
        /// <remarks>Due to SQLite's dynamic type system, parameter values are not converted.</remarks>
        public override DbType DbType { get; set; } = DbType.String;

        /// <summary>
        ///     Gets or sets the SQLite type of the parameter.
        /// </summary>
        /// <value>The SQLite type of the parameter.</value>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public virtual SqliteType SqliteType
        {
            get => _sqliteType ?? SqliteValueBinder.GetSqliteType(_value);
            set => _sqliteType = value;
        }

        /// <summary>
        ///     Gets or sets the direction of the parameter. Only <see cref="ParameterDirection.Input" /> is supported.
        /// </summary>
        /// <value>The direction of the parameter.</value>
        public override ParameterDirection Direction
        {
            get => ParameterDirection.Input;
            set
            {
                if (value != ParameterDirection.Input)
                {
                    throw new ArgumentException(Resources.InvalidParameterDirection(value));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the parameter is nullable.
        /// </summary>
        /// <value>A value indicating whether the parameter is nullable.</value>
        public override bool IsNullable { get; set; }

        /// <summary>
        ///     Gets or sets the name of the parameter.
        /// </summary>
        /// <value>The name of the parameter.</value>
        [AllowNull]
        public override string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value ?? string.Empty;
        }

        /// <summary>
        ///     Gets or sets the maximum size, in bytes, of the parameter.
        /// </summary>
        /// <value>The maximum size, in bytes, of the parameter.</value>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public override int Size
        {
            get
                => _size
                    ?? (_value is string stringValue
                        ? stringValue.Length
                        : _value is byte[] byteArray
                            ? byteArray.Length
                            : 0);

            set
            {
                if (value < -1)
                {
                    // NB: Message is provided by the framework
                    throw new ArgumentOutOfRangeException(nameof(value), value, message: null);
                }

                _size = value;
            }
        }

        /// <summary>
        ///     Gets or sets the source column used for loading the value.
        /// </summary>
        /// <value>The source column used for loading the value.</value>
        [AllowNull]
        public override string SourceColumn
        {
            get => _sourceColumn;
            set => _sourceColumn = value ?? string.Empty;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the source column is nullable.
        /// </summary>
        /// <value>A value indicating whether the source column is nullable.</value>
        public override bool SourceColumnNullMapping { get; set; }

        /// <summary>
        ///     Gets or sets the value of the parameter.
        /// </summary>
        /// <value>The value of the parameter.</value>
        /// <remarks>Due to SQLite's dynamic type system, parameter values are not converted.</remarks>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
        public override object? Value
        {
            get => _value;
            set { _value = value; }
        }

        /// <summary>
        ///     Resets the <see cref="DbType" /> property to its original value.
        /// </summary>
        public override void ResetDbType()
            => ResetSqliteType();

        /// <summary>
        ///     Resets the <see cref="SqliteType" /> property to its original value.
        /// </summary>
        public virtual void ResetSqliteType()
        {
            DbType = DbType.String;
            _sqliteType = null;
        }

        internal bool Bind(sqlite3_stmt stmt, sqlite3 handle)
        {
            if (string.IsNullOrEmpty(ParameterName))
            {
                throw new InvalidOperationException(Resources.RequiresSet(nameof(ParameterName)));
            }

            var index = sqlite3_bind_parameter_index(stmt, ParameterName);
            if (index == 0
                && (index = FindPrefixedParameter(stmt)) == 0)
            {
                return false;
            }

            if (_value == null)
            {
                throw new InvalidOperationException(Resources.RequiresSet(nameof(Value)));
            }

            new SqliteParameterBinder(stmt, handle, index, _value, _size, _sqliteType).Bind();

            return true;
        }

        private static readonly char[] _parameterPrefixes = ['@', '$', ':'];

        private int FindPrefixedParameter(sqlite3_stmt stmt)
        {
            var index = 0;
            int nextIndex;

            foreach (var prefix in _parameterPrefixes)
            {
                if (ParameterName[0] == prefix)
                {
                    // If name already has a prefix characters, the first call to sqlite3_bind_parameter_index
                    // would have worked if the parameter name was in the statement
                    return 0;
                }

                nextIndex = sqlite3_bind_parameter_index(stmt, prefix + ParameterName);

                if (nextIndex == 0)
                {
                    continue;
                }

                if (index != 0)
                {
                    throw new InvalidOperationException(Resources.AmbiguousParameterName(ParameterName));
                }

                index = nextIndex;
            }

            return index;
        }
    }
}
