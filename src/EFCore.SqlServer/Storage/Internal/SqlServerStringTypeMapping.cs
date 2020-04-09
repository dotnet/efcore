// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerStringTypeMapping : StringTypeMapping
    {
        private const int UnicodeMax = 4000;
        private const int AnsiMax = 8000;

        private readonly SqlDbType? _sqlDbType;
        private readonly int _maxSpecificSize;
        private readonly int _maxSize;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerStringTypeMapping(
            [CanBeNull] string storeType = null,
            bool unicode = false,
            int? size = null,
            bool fixedLength = false,
            SqlDbType? sqlDbType = null,
            StoreTypePostfix? storeTypePostfix = null)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(string)),
                    storeType ?? GetStoreName(unicode, fixedLength),
                    storeTypePostfix ?? StoreTypePostfix.Size,
                    GetDbType(unicode, fixedLength),
                    unicode,
                    size,
                    fixedLength),
                sqlDbType)
        {
        }

        private static string GetStoreName(bool unicode, bool fixedLength) => unicode
            ? fixedLength ? "nchar" : "nvarchar"
            : fixedLength
                ? "char"
                : "varchar";

        private static DbType? GetDbType(bool unicode, bool fixedLength) => unicode
            ? (fixedLength
                ? System.Data.DbType.StringFixedLength
                : (DbType?)null)
            : (fixedLength
                ? System.Data.DbType.AnsiStringFixedLength
                : System.Data.DbType.AnsiString);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected SqlServerStringTypeMapping(RelationalTypeMappingParameters parameters, SqlDbType? sqlDbType)
            : base(parameters)
        {
            if (parameters.Unicode)
            {
                _maxSpecificSize = parameters.Size.HasValue && parameters.Size <= UnicodeMax ? parameters.Size.Value : UnicodeMax;
                _maxSize = UnicodeMax;
            }
            else
            {
                _maxSpecificSize = parameters.Size.HasValue && parameters.Size <= AnsiMax ? parameters.Size.Value : AnsiMax;
                _maxSize = AnsiMax;
            }

            _sqlDbType = sqlDbType;
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqlServerStringTypeMapping(parameters, _sqlDbType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            var value = parameter.Value;
            var length = (value as string)?.Length;

            if (_sqlDbType.HasValue
                && parameter is SqlParameter sqlParameter) // To avoid crashing wrapping providers
            {
                sqlParameter.SqlDbType = _sqlDbType.Value;
            }

            if ((value == null
                    || value == DBNull.Value)
                || (IsFixedLength
                    && length == _maxSpecificSize
                    && Size.HasValue))
            {
                // A fixed-length parameter where the value matches the length can remain a fixed-length parameter
                // because SQLClient will not do any padding or truncating.
                parameter.Size = _maxSpecificSize;
            }
            else
            {
                if (IsFixedLength)
                {
                    // Force the parameter type to be not fixed length to avoid SQLClient truncation and padding.
                    parameter.DbType = IsUnicode ? System.Data.DbType.String : System.Data.DbType.AnsiString;
                }

                // For strings and byte arrays, set the max length to the size facet if specified, or
                // 8000 bytes if no size facet specified, if the data will fit so as to avoid query cache
                // fragmentation by setting lots of different Size values otherwise set to the max bounded length
                // if the value will fit, otherwise set to -1 (unbounded) to avoid SQL client size inference.
                if (length != null
                    && length <= _maxSpecificSize)
                {
                    parameter.Size = _maxSpecificSize;
                }
                else if (length != null
                    && length <= _maxSize)
                {
                    parameter.Size = _maxSize;
                }
                else
                {
                    parameter.Size = -1;
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
            => EscapeLineBreaks(EscapeSqlLiteral((string)value));

        private static readonly char[] LineBreakChars = new char[] { '\r', '\n' };
        private string EscapeLineBreaks(string value)
        {
            var unicodePrefix = IsUnicode ? "N" : string.Empty;

            if (value == null
                || value.IndexOfAny(LineBreakChars) == -1)
            {
                return $"{unicodePrefix}'{value}'";
            }

            if (value.Length == 1)
            {
                return value[0] == '\n' ? "CHAR(10)" : "CHAR(13)";
            }

            return ($"CONCAT({unicodePrefix}'" + value
                    .Replace("\r", $"', CHAR(13), {unicodePrefix}'")
                    .Replace("\n", $"', CHAR(10), {unicodePrefix}'") + "')")
                    .Replace($"{unicodePrefix}'', ", string.Empty)
                    .Replace($", {unicodePrefix}''", string.Empty);
        }
    }
}
