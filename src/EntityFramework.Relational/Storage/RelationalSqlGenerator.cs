// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalSqlGenerator : ISqlGenerator
    {
        protected virtual string FloatingPointFormat => "{0}E0";
        protected virtual string DateTimeFormat => @"yyyy-MM-dd HH\:mm\:ss.fffffff";
        protected virtual string DateTimeOffsetFormat => @"yyyy-MM-dd HH\:mm\:ss.fffffffzzz";

        private readonly Dictionary<DbType, string> _dbTypeNameMapping = new Dictionary<DbType, string>
        {
            { DbType.Byte, "tinyint" },
            { DbType.Decimal, "decimal" },
            { DbType.Double, "float" },
            { DbType.Int16, "smallint" },
            { DbType.Int32, "int" },
            { DbType.Int64, "bigint" },
            { DbType.String, "nvarchar" }
        };

        public virtual string BatchCommandSeparator => ";";

        public virtual string BatchSeparator => string.Empty;

        public virtual string GenerateParameterName(string name)
            => $"@{Check.NotEmpty(name, nameof(name))}";

        public virtual string GenerateLiteral(object value)
            => value != null
                ? GenerateLiteralValue((dynamic)value)
                : "NULL";

        public virtual string EscapeLiteral(string literal)
            => Check.NotNull(literal, nameof(literal)).Replace("'", "''");

        public virtual string EscapeIdentifier(string identifier)
            => Check.NotEmpty(identifier, nameof(identifier)).Replace("\"", "\"\"");

        public virtual string DelimitIdentifier(string identifier)
            => $"\"{EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier)))}\"";

        public virtual string DelimitIdentifier(string name, string schema)
            => (!string.IsNullOrEmpty(schema)
                ? DelimitIdentifier(schema) + "."
                : string.Empty)
               + DelimitIdentifier(Check.NotEmpty(name, nameof(name)));

        protected virtual string GenerateLiteralValue(int value)
            => value.ToString();

        protected virtual string GenerateLiteralValue(short value)
            => value.ToString();

        protected virtual string GenerateLiteralValue(long value)
            => value.ToString();

        protected virtual string GenerateLiteralValue(byte value)
            => value.ToString();

        protected virtual string GenerateLiteralValue(decimal value)
            => string.Format(value.ToString(CultureInfo.InvariantCulture));

        protected virtual string GenerateLiteralValue(double value)
            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormat, value);

        protected virtual string GenerateLiteralValue(float value)
            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormat, value);

        protected virtual string GenerateLiteralValue(bool value)
            => value ? "1" : "0";

        protected virtual string GenerateLiteralValue(char value)
            => $"'{value}'";

        protected virtual string GenerateLiteralValue([NotNull] string value)
            => $"'{EscapeLiteral(Check.NotNull(value, nameof(value)))}'";

        protected virtual string GenerateLiteralValue([NotNull] object value)
            => string.Format(CultureInfo.InvariantCulture, "{0}", value);

        protected virtual string GenerateLiteralValue([NotNull] byte[] value)
        {
            Check.NotNull(value, nameof(value));

            var stringBuilder = new StringBuilder("X'");

            foreach (var @byte in value)
            {
                stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            stringBuilder.Append("'");

            return stringBuilder.ToString();
        }

        protected virtual string GenerateLiteralValue(DbType value)
            => _dbTypeNameMapping[value];

        protected virtual string GenerateLiteralValue([NotNull] Enum value)
            => string.Format(CultureInfo.InvariantCulture, "{0:d}", Check.NotNull(value, nameof(value)));

        protected virtual string GenerateLiteralValue(Guid value)
            => $"'{value}'";

        protected virtual string GenerateLiteralValue(DateTime value)
            => $"TIMESTAMP '{value.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}'";

        protected virtual string GenerateLiteralValue(DateTimeOffset value)
            => $"TIMESTAMP '{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'";

        protected virtual string GenerateLiteralValue(TimeSpan value)
            => $"'{value}'";
    }
}
