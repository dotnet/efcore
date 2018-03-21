// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleByteArrayTypeMapping : ByteArrayTypeMapping
    {
        private const int MaxSize = 8000;

        private readonly int _maxSpecificSize;

        private readonly StoreTypeModifierKind? _storeTypeModifier;

        public OracleByteArrayTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType = System.Data.DbType.Binary,
            int? size = null,
            bool fixedLength = false,
            ValueComparer comparer = null,
            StoreTypeModifierKind? storeTypeModifier = null)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(byte[]), null, comparer),
                    storeType,
                    GetStoreTypeModifier(storeTypeModifier, size),
                    dbType,
                    size: size,
                    fixedLength: fixedLength))
        {
            _storeTypeModifier = storeTypeModifier;
        }

        protected OracleByteArrayTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
            _maxSpecificSize = CalculateSize(parameters.Size);
        }

        private static StoreTypeModifierKind GetStoreTypeModifier(StoreTypeModifierKind? storeTypeModifier, int? size)
            => storeTypeModifier
               ?? (size != null && size <= MaxSize ? StoreTypeModifierKind.Size : StoreTypeModifierKind.None);

        private static int CalculateSize(int? size)
            => size.HasValue && size < MaxSize ? size.Value : MaxSize;

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleByteArrayTypeMapping(
                Parameters.WithStoreTypeAndSize(storeType, size, GetStoreTypeModifier(_storeTypeModifier, size)));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleByteArrayTypeMapping(Parameters.WithComposedConverter(converter));

        protected override void ConfigureParameter(DbParameter parameter)
        {
            var value = parameter.Value;
            var length = (value as byte[])?.Length;

            parameter.Size
                = value == null
                  || value == DBNull.Value
                  || length != null
                  && length <= _maxSpecificSize
                    ? _maxSpecificSize
                    : parameter.Size;
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var builder = new StringBuilder();
            builder.Append("'");

            foreach (var @byte in (byte[])value)
            {
                builder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            builder.Append("'");

            return builder.ToString();
        }
    }
}
