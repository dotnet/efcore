// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Globalization;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
    {
        private const string DateTimeOffsetFormatConst = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlServerDateTimeOffsetTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        public SqlServerDateTimeOffsetTypeMapping([NotNull] string storeType)
            : this(storeType, dbType: System.Data.DbType.DateTimeOffset, unicode: false, size: null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTimeTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <param name="hasNonDefaultUnicode"> A value indicating whether the Unicode setting has been manually configured to a non-default value. </param>
        /// <param name="hasNonDefaultSize"> A value indicating whether the size setting has been manually configured to a non-default value. </param>
        public SqlServerDateTimeOffsetTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType,
            bool unicode,
            int? size,
            bool hasNonDefaultUnicode = false,
            bool hasNonDefaultSize = false)
            : base(storeType, dbType, unicode, size, hasNonDefaultUnicode, hasNonDefaultSize)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public override RelationalTypeMapping CreateCopy([NotNull] string storeType, int? size)
            => new SqlServerDateTimeOffsetTypeMapping(
                storeType,
                DbType,
                IsUnicode,
                size,
                HasNonDefaultUnicode,
                hasNonDefaultSize: size != Size);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string DateTimeOffsetFormat => DateTimeOffsetFormatConst;

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        protected override string GenerateNonNullSqlLiteral([NotNull]DateTimeOffset value)
            => $"'{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'"; // Interpolation okay; strings
    }
}
