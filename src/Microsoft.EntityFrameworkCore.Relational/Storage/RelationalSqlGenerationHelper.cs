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
    public class RelationalSqlGenerationHelper : ISqlGenerationHelper
    {
        private const string DecimalFormatConst = "0.0###########################";
        private const string DecimalFormatStringConst = "{0:" + DecimalFormatConst + "}";
        private const string DateTimeFormatConst = @"yyyy-MM-dd HH\:mm\:ss.fffffff";
        private const string DateTimeFormatStringConst = "TIMESTAMP '{0:" + DateTimeFormatConst + "}'";
        private const string DateTimeOffsetFormatConst = @"yyyy-MM-dd HH\:mm\:ss.fffffffzzz";
        private const string DateTimeOffsetFormatStringConst = "TIMESTAMP '{0:" + DateTimeOffsetFormatConst + "}'";

        protected virtual string BoolTrueLiteral => "CAST(1 AS BIT)";
        protected virtual string BoolFalseLiteral => "CAST(0 AS BIT)";
        protected virtual string FloatingPointFormatString => "{0}E0";
        protected virtual string DecimalFormat => DecimalFormatConst;
        protected virtual string DecimalFormatString => DecimalFormatStringConst;
        protected virtual string DateTimeFormat => DateTimeFormatConst;
        protected virtual string DateTimeFormatString => DateTimeFormatStringConst;
        protected virtual string DateTimeOffsetFormat => DateTimeOffsetFormatConst;
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

        public virtual string StatementTerminator => ";";

        public virtual string BatchTerminator => string.Empty;

        public virtual string GenerateParameterName(string name)
            => "@" + name;

        public virtual void GenerateParameterName(StringBuilder builder, string name)
            => builder.Append("@").Append(name);

        public virtual string GenerateLiteral(object value, RelationalTypeMapping typeMapping = null)
        {
            if (value != null)
            {
                var s = value as string;
                return s != null ? GenerateLiteralValue(s, typeMapping) : GenerateLiteralValue((dynamic)value);
            }
            return "NULL";
        }

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

        public virtual string EscapeLiteral(string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");

        public virtual void EscapeLiteral(StringBuilder builder, string literal)
        {
            Check.NotNull(literal, nameof(literal));

            var initialLength = builder.Length;
            builder.Append(literal);
            builder.Replace("'", "''", initialLength, literal.Length);
        }

        public virtual string EscapeIdentifier(string identifier)
            => Check.NotEmpty(identifier, nameof(identifier)).Replace("\"", "\"\"");

        public virtual void EscapeIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            var initialLength = builder.Length;
            builder.Append(identifier);
            builder.Replace("\"", "\"\"", initialLength, identifier.Length);
        }

        public virtual string DelimitIdentifier(string identifier)
            => $"\"{EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier)))}\"";

        public virtual void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            builder.Append('"');
            EscapeIdentifier(builder, identifier);
            builder.Append('"');
        }

        public virtual string DelimitIdentifier(string name, string schema)
            => (!string.IsNullOrEmpty(schema)
                ? DelimitIdentifier(schema) + "."
                : string.Empty)
               + DelimitIdentifier(Check.NotEmpty(name, nameof(name)));

        public virtual void DelimitIdentifier(StringBuilder builder, string name, string schema)
        {
            if (!string.IsNullOrEmpty(schema))
            {
                DelimitIdentifier(builder, schema);
                builder.Append(".");
            }

            DelimitIdentifier(builder, name);
        }

        protected virtual string GenerateLiteralValue(int value)
            => value.ToString();

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, int value)
            => builder.Append(value);

        protected virtual string GenerateLiteralValue(short value)
            => value.ToString();

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, short value)
            => builder.Append(value);

        protected virtual string GenerateLiteralValue(long value)
            => value.ToString();

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, long value)
            => builder.Append(value);

        protected virtual string GenerateLiteralValue(byte value)
            => value.ToString();

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, byte value)
            => builder.Append(value);

        protected virtual string GenerateLiteralValue(decimal value)
            => value.ToString(DecimalFormat, CultureInfo.InvariantCulture);

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, decimal value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, DecimalFormatString, value);

        protected virtual string GenerateLiteralValue(double value)
            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormatString, value);

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, double value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, FloatingPointFormatString, value);

        protected virtual string GenerateLiteralValue(float value)
            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormatString, value);

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, float value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, FloatingPointFormatString, value);

        protected virtual string GenerateLiteralValue(bool value)
            => value ? BoolTrueLiteral : BoolFalseLiteral;

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, bool value)
            => builder.Append(value ? BoolTrueLiteral : BoolFalseLiteral);

        protected virtual string GenerateLiteralValue(char value)
            => $"'{value}'";

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, char value)
            => builder.Append("'").Append(value).Append("'");

        protected virtual string GenerateLiteralValue([NotNull] string value, [CanBeNull] RelationalTypeMapping typeMapping)
            => $"'{EscapeLiteral(Check.NotNull(value, nameof(value)))}'";

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, [NotNull] string value, [CanBeNull] RelationalTypeMapping typeMapping)
        {
            builder.Append("'");
            EscapeLiteral(builder, value);
            builder.Append("'");
        }

        protected virtual string GenerateLiteralValue([NotNull] object value)
            => string.Format(CultureInfo.InvariantCulture, "{0}", value);

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, [NotNull] object value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", value);

        protected virtual string GenerateLiteralValue([NotNull] byte[] value)
        {
            var stringBuilder = new StringBuilder();
            GenerateLiteralValue(stringBuilder, value);
            return stringBuilder.ToString();
        }

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

        protected virtual string GenerateLiteralValue(DbType value)
            => _dbTypeNameMapping[value];

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, DbType value)
            => builder.Append(_dbTypeNameMapping[value]);

        protected virtual string GenerateLiteralValue([NotNull] Enum value)
            => string.Format(CultureInfo.InvariantCulture, "{0:d}", Check.NotNull(value, nameof(value)));

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, [NotNull] Enum value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, "{0:d}", Check.NotNull(value, nameof(value)));

        protected virtual string GenerateLiteralValue(Guid value)
            => $"'{value}'";

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, Guid value)
            => builder.Append("'").Append(value).Append("'");

        protected virtual string GenerateLiteralValue(DateTime value)
            => $"TIMESTAMP '{value.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}'";

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, DateTime value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, DateTimeFormatString, value);

        protected virtual string GenerateLiteralValue(DateTimeOffset value)
            => $"TIMESTAMP '{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'";

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, DateTimeOffset value)
            => builder.AppendFormat(CultureInfo.InvariantCulture, DateTimeOffsetFormatString, value);

        protected virtual string GenerateLiteralValue(TimeSpan value)
            => $"'{value}'";

        protected virtual void GenerateLiteralValue([NotNull] StringBuilder builder, TimeSpan value)
            => builder.Append("'").Append(value).Append("'");
    }
}
