// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleStringTypeMapping : StringTypeMapping
    {
        private readonly int _maxSpecificSize;

        public OracleStringTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType,
            bool unicode = false,
            int? size = null,
            bool fixedLength = false)
            : this(storeType, null, null, dbType, unicode, size, fixedLength)
        {
        }

        public OracleStringTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            [CanBeNull] ValueComparer comparer,
            [CanBeNull] DbType? dbType,
            bool unicode = false,
            int? size = null,
            bool fixedLength = false)
            : base(storeType, converter, comparer, dbType, unicode, size, fixedLength)
        {
            _maxSpecificSize = CalculateSize(unicode, size);
        }

        private static int CalculateSize(bool unicode, int? size)
            => unicode
                ? size.HasValue && size < 2000
                    ? size.Value
                    : 2000
                : size.HasValue && size < 4000
                    ? size.Value
                    : 4000;

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleStringTypeMapping(storeType, Converter, Comparer, DbType, IsUnicode, size, IsFixedLength);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleStringTypeMapping(StoreType, ComposeConverter(converter), Comparer, DbType, IsUnicode, Size, IsFixedLength);

        protected override void ConfigureParameter(DbParameter parameter)
        {
            // For strings and byte arrays, set the max length to the size facet if specified, or
            // _maxSpecificSize bytes if no size facet specified, if the data will fit so as to avoid query cache
            // fragmentation by setting lots of different Size values otherwise always set to
            // 0 to avoid SQL client size inference.

            var value = parameter.Value;
            var length = (value as string)?.Length ?? (value as byte[])?.Length;

            try
            {
                parameter.Size = value == null || value == DBNull.Value || length != null && length <= _maxSpecificSize
                    ? _maxSpecificSize
                    : 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected override string GenerateNonNullSqlLiteral(object value)
            => IsUnicode
                ? $"N'{EscapeSqlLiteral((string)value)}'" // Interpolation okay; strings
                : $"'{EscapeSqlLiteral((string)value)}'";
    }
}
