// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Converters;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleTimeSpanTypeMapping : TimeSpanTypeMapping
    {
        public OracleTimeSpanTypeMapping([NotNull] string storeType, [CanBeNull] DbType? dbType = null)
            : this(storeType, null, dbType)
        {
        }

        public OracleTimeSpanTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            [CanBeNull] DbType? dbType = null)
            : base(storeType, converter, dbType)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleTimeSpanTypeMapping(storeType, Converter, DbType);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleTimeSpanTypeMapping(StoreType, ComposeConverter(converter), DbType);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var ts = (TimeSpan)value;

            var milliseconds = ts.Milliseconds.ToString();

            milliseconds = milliseconds.PadLeft(4 - milliseconds.Length, '0');

            return $"INTERVAL '{ts.Days} {ts.Hours}:{ts.Minutes}:{ts.Seconds}.{milliseconds}' DAY TO SECOND";
        }
    }
}
