// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleDoubleTypeMapping : DoubleTypeMapping
    {
        public OracleDoubleTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType = null)
            : this(storeType, null, null, null, dbType)
        {
        }

        public OracleDoubleTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            [CanBeNull] ValueComparer comparer,
            [CanBeNull] ValueComparer keyComparer,
            [CanBeNull] DbType? dbType = null)
            : base(storeType, converter, comparer, keyComparer, dbType)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleDoubleTypeMapping(storeType, Converter, Comparer, KeyComparer, DbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleDoubleTypeMapping(StoreType, ComposeConverter(converter), Comparer, KeyComparer, DbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var literal = base.GenerateNonNullSqlLiteral(value);

            if (!literal.Contains("E")
                && !literal.Contains("e"))
            {
                return literal + "E0";
            }

            return literal;
        }
    }
}
