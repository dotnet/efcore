// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Converters;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleByteArrayTypeMapping : ByteArrayTypeMapping
    {
        private readonly int _maxSpecificSize;

        public OracleByteArrayTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType = System.Data.DbType.Binary,
            int? size = null)
            : this(storeType, null, dbType, size)
        {
        }

        public OracleByteArrayTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            [CanBeNull] DbType? dbType = System.Data.DbType.Binary,
            int? size = null)
            : base(storeType, converter, dbType, size)
        {
            _maxSpecificSize = CalculateSize(size);
        }

        private static int CalculateSize(int? size)
            => size.HasValue && size < 8000 ? size.Value : 8000;

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleByteArrayTypeMapping(storeType, Converter, DbType, size);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleByteArrayTypeMapping(StoreType, ComposeConverter(converter), DbType, Size);

        protected override void ConfigureParameter(DbParameter parameter)
        {
            var value = parameter.Value;
            var length = (value as string)?.Length ?? (value as byte[])?.Length;

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
