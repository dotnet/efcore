// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleDateTimeTypeMapping : DateTimeTypeMapping
    {
        private const string DateTimeFormatConst = "TO_DATE('{0:yyyy-MM-dd HH:mm:ss}', 'YYYY-MM-DD HH24:MI:SS')";

        public OracleDateTimeTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType = null)
            : this(storeType, null, null, null, dbType)
        {
        }

        public OracleDateTimeTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            [CanBeNull] ValueComparer comparer,
            [CanBeNull] ValueComparer keyComparer,
            [CanBeNull] DbType? dbType = null)
            : base(storeType, converter, comparer, keyComparer, dbType)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleDateTimeTypeMapping(storeType, Converter, Comparer, KeyComparer, DbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleDateTimeTypeMapping(StoreType, ComposeConverter(converter), Comparer, KeyComparer, DbType);

        protected override string SqlLiteralFormatString => DateTimeFormatConst;
    }
}
