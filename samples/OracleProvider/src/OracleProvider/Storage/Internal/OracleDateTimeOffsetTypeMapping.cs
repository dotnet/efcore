// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
    {
        private const string DateTimeOffsetFormatConst = "{0:yyyy-MM-ddTHH:mm:ss.fffzzz}";

        public OracleDateTimeOffsetTypeMapping([NotNull] string storeType)
            : this(storeType, null)
        {
        }

        public OracleDateTimeOffsetTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter)
            : base(storeType, converter)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleDateTimeOffsetTypeMapping(storeType, Converter);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleDateTimeOffsetTypeMapping(StoreType, ComposeConverter(converter));

        protected override string SqlLiteralFormatString => "'" + DateTimeOffsetFormatConst + "'";

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            ((OracleParameter)parameter).OracleDbType = OracleDbType.TimeStampTZ;
        }
    }
}
