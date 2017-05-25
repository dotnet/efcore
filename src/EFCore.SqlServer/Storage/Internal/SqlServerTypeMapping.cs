// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override RelationalTypeMapping CreateCopy(string storeType, int? size)
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

//LAJLAJ        /// <summary>
//LAJLAJ        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
//LAJLAJ        ///     directly from your code. This API may change or be removed in future releases.
//LAJLAJ        /// </summary>
//LAJLAJ        protected override string GenerateSqlLiteralValue(byte[] value)
//LAJLAJ        {
//LAJLAJ            Check.NotNull(value, nameof(value));
//LAJLAJ
//LAJLAJ            var builder = new StringBuilder();
//LAJLAJ            builder.Append("0x");
//LAJLAJ
//LAJLAJ            foreach (var @byte in value)
//LAJLAJ            {
//LAJLAJ                builder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
//LAJLAJ            }
//LAJLAJ
//LAJLAJ            return builder.ToString();
//LAJLAJ        }
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
//LAJLAJ        ///     directly from your code. This API may change or be removed in future releases.
//LAJLAJ        /// </summary>
//LAJLAJ        protected override string GenerateSqlLiteralValue(string value)
//LAJLAJ            => IsUnicode
//LAJLAJ                ? $"N'{EscapeSqlLiteral(Check.NotNull(value, nameof(value)))}'" // Interpolation okay; strings
//LAJLAJ                : $"'{EscapeSqlLiteral(Check.NotNull(value, nameof(value)))}'";
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
//LAJLAJ        ///     directly from your code. This API may change or be removed in future releases.
//LAJLAJ        /// </summary>
//LAJLAJ        protected override string GenerateSqlLiteralValue(DateTime value)
//LAJLAJ            => $"'{value.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}'"; // Interpolation okay; strings
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
//LAJLAJ        ///     directly from your code. This API may change or be removed in future releases.
//LAJLAJ        /// </summary>
//LAJLAJ        protected override string GenerateSqlLiteralValue(DateTimeOffset value)
//LAJLAJ            => $"'{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'"; // Interpolation okay; strings
    }
}
