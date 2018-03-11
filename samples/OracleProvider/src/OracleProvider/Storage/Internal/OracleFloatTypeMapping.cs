// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleFloatTypeMapping : FloatTypeMapping
    {
        public OracleFloatTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType = null)
            : this(storeType, null, null, null, dbType)
        {
        }

        public OracleFloatTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            [CanBeNull] ValueComparer comparer,
            [CanBeNull] ValueComparer keyComparer,
            [CanBeNull] DbType? dbType = null)
            : base(storeType, converter, comparer, keyComparer, dbType)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleFloatTypeMapping(storeType, Converter, Comparer, KeyComparer, DbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleFloatTypeMapping(StoreType, ComposeConverter(converter), Comparer, KeyComparer, DbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            return $"CAST({base.GenerateNonNullSqlLiteral(value)} AS {StoreType})";
        }
    }
}
