// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Provides services to help with generation of SQL commands.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalSqlGenerationHelper : ISqlGenerationHelper
    {
        private const string DecimalFormatConst = "0.0###########################";
        private const string DecimalFormatStringConst = "{0:" + DecimalFormatConst + "}";
        private const string DateTimeFormatConst = @"yyyy-MM-dd HH\:mm\:ss.fffffff";
        private const string DateTimeFormatStringConst = "TIMESTAMP '{0:" + DateTimeFormatConst + "}'";
        private const string DateTimeOffsetFormatConst = @"yyyy-MM-dd HH\:mm\:ss.fffffffzzz";
        private const string DateTimeOffsetFormatStringConst = "TIMESTAMP '{0:" + DateTimeOffsetFormatConst + "}'";

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
            { DbType.Byte, "tinyint" },
            { DbType.Decimal, "decimal" },
            { DbType.Double, "float" },
            { DbType.Int16, "smallint" },
            { DbType.Int32, "int" },
            { DbType.Int64, "bigint" },
            { DbType.String, "nvarchar" },
            { DbType.Date, "date" }
        };

        /// <summary>
        ///     The terminator to be used for SQL statements.
        /// </summary>
        public virtual string StatementTerminator => ";";

        /// <summary>
        ///     The terminator to be used for batches of SQL statements.
        /// </summary>
        public virtual string BatchTerminator => string.Empty;

        /// <summary>
        ///     Generates a valid parameter name for the given candidate name.
        /// </summary>
        /// <param name="name">The candidate name for the parameter.</param>
        /// <returns>
        ///     A valid name based on the candidate name.
        /// </returns>
        public virtual string GenerateParameterName(string name)
            => "@" + name;

        /// <summary>
        ///     Writes a valid parameter name for the given candidate name.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="name">The candidate name for the parameter.</param>
        public virtual void GenerateParameterName(StringBuilder builder, string name)
            => builder.Append("@").Append(name);

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <param name="typeMapping">An optional type mapping that is used for this value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string GenerateLiteral(object value, RelationalTypeMapping typeMapping = null)
        {
            if (value != null)
            {
                var s = value as string;
                return s != null ? GenerateLiteralValue(s, typeMapping) : GenerateLiteralValue((dynamic)value);
            }
            return "NULL";
        }

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        /// <param name="typeMapping">An optional type mapping that is used for this value.</param>
        public virtual void GenerateLiteral(StringBuilder builder, object value, RelationalTypeMapping typeMapping = null)
        {
            if (value != null)
            {
                var s = value as string;
                if (s != null)
                {
                    GenerateLiteralValue(builder, s, typeMapping);
                }
                else
                {
                    GenerateLiteralValue(builder, (dynamic)value);
                }
            }

            builder.Append("NULL");
        }

        /// <summary>
        ///     Generates the escaped SQL representation of a literal value.
        /// </summary>
        /// <param name="literal">The value to be escaped.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string EscapeLiteral(string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");

        /// <summary>
        ///     Writes the escaped SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="literal">The value to be escaped.</param>
        public virtual void EscapeLiteral(StringBuilder builder, string literal)
        {
            Check.NotNull(literal, nameof(literal));

            var initialLength = builder.Length;
            builder.Append(literal);
            builder.Replace("'", "''", initialLength, literal.Length);
        }

        /// <summary>
        ///     Generates the escaped SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to be escaped.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string EscapeIdentifier(string identifier)
            => Check.NotEmpty(identifier, nameof(identifier)).Replace("\"", "\"\"");

        /// <summary>
        ///     Writes the escaped SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="identifier">The identifier to be escaped.</param>
        public virtual void EscapeIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            var initialLength = builder.Length;
            builder.Append(identifier);
            builder.Replace("\"", "\"\"", initialLength, identifier.Length);
        }

        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to delimit.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string DelimitIdentifier(string identifier)
            => $"\"{EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier)))}\""; // Interpolation okay; strings

        /// <summary>
        ///     Writes the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="identifier">The identifier to delimit.</param>
        public virtual void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            builder.Append('"');
            EscapeIdentifier(builder, identifier);
            builder.Append('"');
        }

        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="name">The identifier to delimit.</param>
        /// <param name="schema">The schema of the identifier.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string DelimitIdentifier(string name, string schema)
            => (!string.IsNullOrEmpty(schema)
                   ? DelimitIdentifier(schema) + "."
                   : string.Empty)
               + DelimitIdentifier(Check.NotEmpty(name, nameof(name)));

        /// <summary>
        ///     Writes the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="name">The identifier to delimit.</param>
        /// <param name="schema">The schema of the identifier.</param>
        public virtual void DelimitIdentifier(StringBuilder builder, string name, string schema)
        {
            if (!string.IsNullOrEmpty(schema))
            {
                DelimitIdentifier(builder, schema);
                builder.Append(".");
            }

            DelimitIdentifier(builder, name);
        }

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(int value)
            => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, int value)
            => builder.Append(value.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(short value)
            => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, short value)
            => builder.Append(value.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(long value)
            => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, long value)
            => builder.Append(value.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(byte value)
            => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, byte value)
            => builder.Append(value.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(decimal value)
            => value.ToString(DecimalFormat, CultureInfo.InvariantCulture);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, decimal value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, DecimalFormatString, value);

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(double value)
            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormatString, value);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, double value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, FloatingPointFormatString, value);

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(float value)
            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormatString, value);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, float value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, FloatingPointFormatString, value);

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(bool value)
            => value ? "1" : "0";

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, bool value)
            => builder.Append(value ? "1" : "0");

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(char value)
            => string.Format(CultureInfo.InvariantCulture, "'{0}'", value);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, char value)
            => builder.Append("'").Append(value.ToString()).Append("'");

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <param name="typeMapping">An optional type mapping that is used for this value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue([NotNull] string value, [CanBeNull] RelationalTypeMapping typeMapping)
            => $"'{EscapeLiteral(Check.NotNull(value, nameof(value)))}'"; // Interpolation okay; strings

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="typeMapping">An optional type mapping that is used for this value.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, [NotNull] string value, [CanBeNull] RelationalTypeMapping typeMapping)
        {
            builder.Append("'");
            EscapeLiteral(builder, value);
            builder.Append("'");
        }

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue([NotNull] object value)
            => string.Format(CultureInfo.InvariantCulture, "{0}", value);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, [NotNull] object value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", value);

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue([NotNull] byte[] value)
        {
            var stringBuilder = new StringBuilder();
            GenerateLiteralValue(stringBuilder, value);
            return stringBuilder.ToString();
        }

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, [NotNull] byte[] value)
        {
            Check.NotNull(value, nameof(value));

            builder.Append("X'");

            foreach (var @byte in value)
            {
                builder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            builder.Append("'");
        }

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(DbType value)
            => _dbTypeNameMapping[value];

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, DbType value)
            => builder.Append(_dbTypeNameMapping[value]);

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue([NotNull] Enum value)
            => string.Format(CultureInfo.InvariantCulture, "{0:d}", Check.NotNull(value, nameof(value)));

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, [NotNull] Enum value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, "{0:d}", Check.NotNull(value, nameof(value)));

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(Guid value)
            => string.Format(CultureInfo.InvariantCulture, "'{0}'", value);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, Guid value)
            => builder.Append("'").Append(value).Append("'");

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(DateTime value)
            => $"TIMESTAMP '{value.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}'"; // Interpolation okay; strings

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, DateTime value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, DateTimeFormatString, value);

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(DateTimeOffset value)
            => $"TIMESTAMP '{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'"; // Interpolation okay; strings

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, DateTimeOffset value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, DateTimeOffsetFormatString, value);

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns> The generated string. </returns>
        protected virtual string GenerateLiteralValue(TimeSpan value)
            => string.Format(CultureInfo.InvariantCulture, "'{0}'", value);

        /// <summary>
        ///     Writes the SQL representation of a literal value.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="value">The literal value.</param>
        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, TimeSpan value)
            => builder.Append("'").Append(value).Append("'");
    }
}
