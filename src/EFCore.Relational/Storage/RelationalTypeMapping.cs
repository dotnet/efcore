// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalTypeMapping
    {
        private const string DecimalFormatConst = "0.0###########################";
        private const string DecimalFormatStringConst = "{0:" + DecimalFormatConst + "}";
        private const string DateTimeFormatConst = @"yyyy-MM-dd HH\:mm\:ss.fffffff";
        private const string DateTimeFormatStringConst = "TIMESTAMP '{0:" + DateTimeFormatConst + "}'";
        private const string DateTimeOffsetFormatConst = @"yyyy-MM-dd HH\:mm\:ss.fffffffzzz";
        private const string DateTimeOffsetFormatStringConst = "TIMESTAMP '{0:" + DateTimeOffsetFormatConst + "}'";

        /// <summary>
        ///     Gets the mapping to be used when the only piece of information is that there is a null value.
        /// </summary>
        public static readonly RelationalTypeMapping NullMapping = new RelationalTypeMapping("NULL");

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        public RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType)
            : this(storeType, clrType, dbType: null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        public RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] DbType? dbType)
            : this(storeType, clrType, dbType, unicode: false, size: null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <param name="hasNonDefaultUnicode"> A value indicating whether the Unicode setting has been manually configured to a non-default value. </param>
        /// <param name="hasNonDefaultSize"> A value indicating whether the size setting has been manually configured to a non-default value. </param>
        public RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] DbType? dbType,
            bool unicode,
            int? size,
            bool hasNonDefaultUnicode = false,
            bool hasNonDefaultSize = false)
            : this(storeType)
        {
            Check.NotNull(clrType, nameof(clrType));

            ClrType = clrType;
            DbType = dbType;
            IsUnicode = unicode;
            Size = size;
            HasNonDefaultUnicode = hasNonDefaultUnicode;
            HasNonDefaultSize = hasNonDefaultSize;
        }

        private RelationalTypeMapping([NotNull] string storeType)
        {
            Check.NotEmpty(storeType, nameof(storeType));

            StoreType = storeType;
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public virtual RelationalTypeMapping CreateCopy([NotNull] string storeType, int? size)
            => new RelationalTypeMapping(
                storeType,
                ClrType,
                DbType,
                IsUnicode,
                size,
                HasNonDefaultUnicode,
                hasNonDefaultSize: size != Size);

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public virtual RelationalTypeMapping CreateCopy([NotNull] string storeType, int? size)
            => new RelationalTypeMapping(
                storeType,
                ClrType,
                DbType,
                IsUnicode,
                size,
                HasNonDefaultUnicode,
                hasNonDefaultSize: size != Size);

        /// <summary>
        ///     Gets the floating point format.
        /// </summary>
        protected virtual string FloatingPointFormatString => "{0}E0";

        /// <summary>
        ///     Gets the decimal format.
        /// </summary>
        protected virtual string DecimalFormat => DecimalFormatConst;

        /// <summary>
        ///     Gets the decimal format.
        /// </summary>
        protected virtual string DecimalFormatString => DecimalFormatStringConst;

        /// <summary>
        ///     Gets the date time format.
        /// </summary>
        protected virtual string DateTimeFormat => DateTimeFormatConst;

        /// <summary>
        ///     Gets the date time format.
        /// </summary>
        protected virtual string DateTimeFormatString => DateTimeFormatStringConst;

        /// <summary>
        ///     Gets the date time offset format.
        /// </summary>
        protected virtual string DateTimeOffsetFormat => DateTimeOffsetFormatConst;

        /// <summary>
        ///     Gets the date time offset format.
        /// </summary>
        protected virtual string DateTimeOffsetFormatString => DateTimeOffsetFormatStringConst;

        private readonly Dictionary<DbType, string> _dbTypeNameMapping = new Dictionary<DbType, string>
        {
            { System.Data.DbType.Byte, "tinyint" },
            { System.Data.DbType.Decimal, "decimal" },
            { System.Data.DbType.Double, "float" },
            { System.Data.DbType.Int16, "smallint" },
            { System.Data.DbType.Int32, "int" },
            { System.Data.DbType.Int64, "bigint" },
            { System.Data.DbType.String, "nvarchar" },
            { System.Data.DbType.Date, "date" }
        };

        /// <summary>
        ///     Gets the name of the database type.
        /// </summary>
        public virtual string StoreType { get; }

        /// <summary>
        ///     Gets the .NET type.
        /// </summary>
        public virtual Type ClrType { get; }

        /// <summary>
        ///     Gets the <see cref="System.Data.DbType" /> to be used.
        /// </summary>
        public virtual DbType? DbType { get; }

        /// <summary>
        ///     Gets a value indicating whether the type should handle Unicode data or not.
        /// </summary>
        public virtual bool IsUnicode { get; }

        /// <summary>
        ///     Gets the size of data the property is configured to store, or null if no size is configured.
        /// </summary>
        public virtual int? Size { get; }

        /// <summary>
        ///     Gets a value indicating whether the Unicode setting has been manually configured to a non-default value.
        /// </summary>
        public virtual bool HasNonDefaultUnicode { get; }

        /// <summary>
        ///     Gets a value indicating whether the size setting has been manually configured to a non-default value.
        /// </summary>
        public virtual bool HasNonDefaultSize { get; }

        /// <summary>
        ///     Creates a <see cref="DbParameter" /> with the appropriate type information configured.
        /// </summary>
        /// <param name="command"> The command the parameter should be created on. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="value"> The value to be assigned to the parameter. </param>
        /// <param name="nullable"> A value indicating whether the parameter should be a nullable type. </param>
        /// <returns> The newly created parameter. </returns>
        public virtual DbParameter CreateParameter(
            [NotNull] DbCommand command,
            [NotNull] string name,
            [CanBeNull] object value,
            bool? nullable = null)
        {
            Check.NotNull(command, nameof(command));

            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            if (nullable.HasValue)
            {
                parameter.IsNullable = nullable.Value;
            }

            if (DbType.HasValue)
            {
                parameter.DbType = DbType.Value;
            }

            if (Size.HasValue)
            {
                parameter.Size = Size.Value;
            }

            ConfigureParameter(parameter);

            return parameter;
        }

        /// <summary>
        ///     Configures type information of a <see cref="DbParameter" />.
        /// </summary>
        /// <param name="parameter"> The parameter to be configured. </param>
        protected virtual void ConfigureParameter([NotNull] DbParameter parameter)
        {
        }

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string GenerateSqlLiteral([CanBeNull]object value)
        {
            if (value == null)
            {
                return "NULL";
            }

            return "Unknown type " + value.GetType() +
                " with value " + string.Format(CultureInfo.InvariantCulture, "{0}", value);
//LAJLAJ            return GenerateSqlLiteralValue((dynamic)value);
        }

        /// <summary>
        ///     Generates the escaped SQL representation of a literal value.
        /// </summary>
        /// <param name="literal">The value to be escaped.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string EscapeSqlLiteral([NotNull]string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");

//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(int value)
//LAJLAJ            => value.ToString(CultureInfo.InvariantCulture);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(short value)
//LAJLAJ            => value.ToString(CultureInfo.InvariantCulture);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(long value)
//LAJLAJ            => value.ToString(CultureInfo.InvariantCulture);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(byte value)
//LAJLAJ            => value.ToString(CultureInfo.InvariantCulture);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(decimal value)
//LAJLAJ            => value.ToString(DecimalFormat, CultureInfo.InvariantCulture);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(double value)
//LAJLAJ            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormatString, value);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(float value)
//LAJLAJ            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormatString, value);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(bool value)
//LAJLAJ            => value ? "1" : "0";
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(char value)
//LAJLAJ            => string.Format(CultureInfo.InvariantCulture, "'{0}'", value);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue([NotNull] string value)
//LAJLAJ            => $"'{EscapeSqlLiteral(Check.NotNull(value, nameof(value)))}'"; // Interpolation okay; strings
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue([NotNull] object value)
//LAJLAJ            => string.Format(CultureInfo.InvariantCulture, "{0}", value);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue([NotNull] byte[] value)
//LAJLAJ        {
//LAJLAJ            var stringBuilder = new StringBuilder();
//LAJLAJ            stringBuilder.Append("X'");
//LAJLAJ            
//LAJLAJ            foreach (var @byte in value)
//LAJLAJ            {
//LAJLAJ                stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
//LAJLAJ            }
//LAJLAJ
//LAJLAJ            stringBuilder.Append("'");
//LAJLAJ            return stringBuilder.ToString();
//LAJLAJ        }
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(DbType value)
//LAJLAJ            => _dbTypeNameMapping[value];
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue([NotNull] Enum value)
//LAJLAJ            => string.Format(CultureInfo.InvariantCulture, "{0:d}", Check.NotNull(value, nameof(value)));
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(Guid value)
//LAJLAJ            => string.Format(CultureInfo.InvariantCulture, "'{0}'", value);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(DateTime value)
//LAJLAJ            => $"TIMESTAMP '{value.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}'"; // Interpolation okay; strings
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(DateTimeOffset value)
//LAJLAJ            => $"TIMESTAMP '{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'"; // Interpolation okay; strings
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns> The generated string. </returns>
//LAJLAJ        protected virtual string GenerateSqlLiteralValue(TimeSpan value)
//LAJLAJ            => string.Format(CultureInfo.InvariantCulture, "'{0}'", value);
    }
}
