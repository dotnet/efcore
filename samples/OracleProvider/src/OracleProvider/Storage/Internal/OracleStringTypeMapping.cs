// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleStringTypeMapping : StringTypeMapping
    {
        private const int UnicodeMax = 2000;
        private const int AnsiMax = 4000;

        private readonly int _maxSpecificSize;

        private readonly StoreTypeModifierKind? _storeTypeModifier;

        public OracleStringTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType,
            bool unicode = false,
            int? size = null,
            bool fixedLength = false,
            StoreTypeModifierKind? storeTypeModifier = null)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(string)),
                    storeType,
                    GetStoreTypeModifier(storeTypeModifier, unicode, size),
                    dbType,
                    unicode,
                    size,
                    fixedLength))
        {
            _storeTypeModifier = storeTypeModifier;
        }

        protected OracleStringTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
            _maxSpecificSize = CalculateSize(parameters.Unicode, parameters.Size);
        }

        private static StoreTypeModifierKind GetStoreTypeModifier(
            StoreTypeModifierKind? storeTypeModifier,
            bool unicode,
            int? size)
            => storeTypeModifier
               ?? (unicode
                   ? size.HasValue && size <= UnicodeMax
                       ? StoreTypeModifierKind.Size
                       : StoreTypeModifierKind.None
                   : size.HasValue && size <= AnsiMax
                       ? StoreTypeModifierKind.Size
                       : StoreTypeModifierKind.None);

        private static int CalculateSize(bool unicode, int? size)
            => unicode
                ? size.HasValue && size <= UnicodeMax
                    ? size.Value
                    : UnicodeMax
                : size.HasValue && size <= AnsiMax
                    ? size.Value
                    : AnsiMax;

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleStringTypeMapping(
                Parameters.WithStoreTypeAndSize(storeType, size, GetStoreTypeModifier(_storeTypeModifier, IsUnicode, size)));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleStringTypeMapping(Parameters.WithComposedConverter(converter));

        protected override void ConfigureParameter(DbParameter parameter)
        {
            // For strings and byte arrays, set the max length to the size facet if specified, or
            // _maxSpecificSize bytes if no size facet specified, if the data will fit so as to avoid query cache
            // fragmentation by setting lots of different Size values otherwise always set to
            // 0 to avoid SQL client size inference.

            var value = parameter.Value;
            var length = (value as string)?.Length;

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
