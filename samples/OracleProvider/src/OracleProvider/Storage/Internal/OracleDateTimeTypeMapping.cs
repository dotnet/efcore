// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleDateTimeTypeMapping : DateTimeTypeMapping
    {
        private const string DateTimeFormatConst = "TO_DATE('{0:yyyy-MM-dd HH:mm:ss}', 'YYYY-MM-DD HH24:MI:SS')";

        public OracleDateTimeTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType = null)
            : this(storeType, null, null, dbType)
        {
        }

        public OracleDateTimeTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            [CanBeNull] ValueComparer comparer,
            [CanBeNull] DbType? dbType = null)
            : base(storeType, converter, comparer, dbType)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleDateTimeTypeMapping(storeType, Converter, Comparer, DbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleDateTimeTypeMapping(StoreType, ComposeConverter(converter), Comparer, DbType);

        protected override string SqlLiteralFormatString => DateTimeFormatConst;
    }
}
