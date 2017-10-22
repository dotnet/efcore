// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Converters;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleFloatTypeMapping : FloatTypeMapping
    {
        public OracleFloatTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType = null)
            : this(storeType, null, dbType)
        {
        }

        public OracleFloatTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            [CanBeNull] DbType? dbType = null)
            : base(storeType, converter, dbType)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleFloatTypeMapping(storeType, Converter, DbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleFloatTypeMapping(StoreType, ComposeConverter(converter), DbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            return $"CAST({base.GenerateNonNullSqlLiteral(value)} AS {StoreType})";
        }
    }
}
