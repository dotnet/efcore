// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerTypeMapping : RelationalTypeMapping
    {
        private const string DateTimeFormatConst = "yyyy-MM-ddTHH:mm:ss.fffK";
        private const string DateTimeFormatStringConst = "'{0:" + DateTimeFormatConst + "}'";
        private const string DateTimeOffsetFormatConst = "yyyy-MM-ddTHH:mm:ss.fffzzz";
        private const string DateTimeOffsetFormatStringConst = "'{0:" + DateTimeOffsetFormatConst + "}'";

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlServerTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        public SqlServerTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType)
            : base(storeType, clrType)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlServerTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        public SqlServerTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] DbType? dbType)
            : base(storeType, clrType, dbType)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlServerTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <param name="hasNonDefaultUnicode"> A value indicating whether the Unicode setting has been manually configured to a non-default value. </param>
        /// <param name="hasNonDefaultSize"> A value indicating whether the size setting has been manually configured to a non-default value. </param>
        public SqlServerTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] DbType? dbType,
            bool unicode,
            int? size,
            bool hasNonDefaultUnicode = false,
            bool hasNonDefaultSize = false)
            : base(storeType, clrType, dbType, unicode, size, hasNonDefaultUnicode, hasNonDefaultSize)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public override RelationalTypeMapping CreateCopy([NotNull] string storeType, int? size)
            => new SqlServerTypeMapping(
                storeType,
                ClrType,
                DbType,
                IsUnicode,
                size,
                HasNonDefaultUnicode,
                hasNonDefaultSize: size != Size);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string DateTimeFormat => DateTimeFormatConst;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string DateTimeFormatString => DateTimeFormatStringConst;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string DateTimeOffsetFormat => DateTimeOffsetFormatConst;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string DateTimeOffsetFormatString => DateTimeOffsetFormatStringConst;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void GenerateLiteralValue(StringBuilder builder, byte[] value)
        {
            Check.NotNull(value, nameof(value));

            builder.Append("0x");

            foreach (var @byte in value)
            {
                builder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GenerateLiteralValue(string value)
            => IsUnicode
                ? $"N'{EscapeLiteral(Check.NotNull(value, nameof(value)))}'" // Interpolation okay; strings
                : $"'{EscapeLiteral(Check.NotNull(value, nameof(value)))}'";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void GenerateLiteralValue(StringBuilder builder, string value)
        {
            builder.Append(IsUnicode ? "N'" : "'");
            EscapeLiteral(builder, value);
            builder.Append("'");
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GenerateLiteralValue(DateTime value)
            => $"'{value.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}'"; // Interpolation okay; strings

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GenerateLiteralValue(DateTimeOffset value)
            => $"'{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'"; // Interpolation okay; strings
    }

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerMaxLengthMapping : SqlServerTypeMapping
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerMaxLengthMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            DbType? dbType = null)
            : this(storeType, clrType, dbType, unicode: false, size: null)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerMaxLengthMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            DbType? dbType,
            bool unicode,
            int? size,
            bool hasNonDefaultUnicode = false,
            bool hasNonDefaultSize = false)
            : base(storeType, clrType, dbType, unicode, CalculateSize(unicode, size), hasNonDefaultUnicode, hasNonDefaultSize)
        {
        }

        private static int CalculateSize(bool unicode, int? size)
            => unicode
                ? size.HasValue && size < 4000 ? size.Value : 4000
                : size.HasValue && size < 8000 ? size.Value : 8000;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override RelationalTypeMapping CreateCopy(string storeType, int? size)
            => new SqlServerMaxLengthMapping(
                storeType,
                ClrType,
                DbType,
                IsUnicode,
                size,
                HasNonDefaultUnicode,
                hasNonDefaultSize: size != Size);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            // For strings and byte arrays, set the max length to the size facet if specififed, or
            // 8000 bytes if no size facet specified, if the data will fit so as to avoid query cache
            // fragmentation by setting lots of differet Size values otherwise always set to 
            // -1 (unbounded) to avoid SQL client size inference.

            var value = parameter.Value;
            var length = (value as string)?.Length ?? (value as byte[])?.Length;

            parameter.Size = value == null || value == DBNull.Value || (length != null && length <= Size.Value)
                ? Size.Value
                : -1;
        }
    }
}
