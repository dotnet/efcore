// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Converters;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerStringTypeMapping : StringTypeMapping
    {
        private readonly int _maxSpecificSize;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerStringTypeMapping(
            [NotNull] string storeType,
            DbType? dbType,
            bool unicode = false,
            int? size = null)
            : this(storeType, null, dbType, unicode, size)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerStringTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            DbType? dbType,
            bool unicode = false,
            int? size = null)
            : base(storeType, converter, dbType, unicode, size)
        {
            _maxSpecificSize = CalculateSize(unicode, size);
        }

        private static int CalculateSize(bool unicode, int? size)
            => unicode
                ? size.HasValue && size < 4000
                    ? size.Value
                    : 4000
                : size.HasValue && size < 8000
                    ? size.Value
                    : 8000;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new SqlServerStringTypeMapping(storeType, Converter, DbType, IsUnicode, size);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override CoreTypeMapping Clone(ValueConverter converter)
            => new SqlServerStringTypeMapping(StoreType, ComposeConverter(converter), DbType, IsUnicode, Size);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            // For strings and byte arrays, set the max length to the size facet if specified, or
            // 8000 bytes if no size facet specified, if the data will fit so as to avoid query cache
            // fragmentation by setting lots of different Size values otherwise always set to
            // -1 (unbounded) to avoid SQL client size inference.

            var value = parameter.Value;
            var length = (value as string)?.Length ?? (value as byte[])?.Length;

            parameter.Size = value == null || value == DBNull.Value || length != null && length <= _maxSpecificSize
                ? _maxSpecificSize
                : -1;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
            => IsUnicode
                ? $"N'{EscapeSqlLiteral((string)value)}'" // Interpolation okay; strings
                : $"'{EscapeSqlLiteral((string)value)}'";
    }
}
