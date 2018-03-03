// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
    {
        private static readonly MethodInfo _readMethod
            = typeof(OracleDataReader).GetTypeInfo().GetDeclaredMethod(nameof(OracleDataReader.GetOracleTimeStampTZ));

        private const string DateTimeOffsetFormatConst = "{0:yyyy-MM-ddTHH:mm:ss.fffzzz}";

        public OracleDateTimeOffsetTypeMapping([NotNull] string storeType)
            : this(
                storeType, new ValueConverter<DateTimeOffset, OracleTimeStampTZ>(
                    v => new OracleTimeStampTZ(v.DateTime, v.Offset.ToString()),
                    v => new DateTimeOffset(v.Value, v.GetTimeZoneOffset())))
        {
        }

        public OracleDateTimeOffsetTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            [CanBeNull] ValueComparer comparer = null)

            : base(storeType, converter, comparer)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleDateTimeOffsetTypeMapping(storeType, Converter, Comparer);

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleDateTimeOffsetTypeMapping(StoreType, ComposeConverter(converter), Comparer);

        protected override string SqlLiteralFormatString => "'" + DateTimeOffsetFormatConst + "'";

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            ((OracleParameter)parameter).OracleDbType = OracleDbType.TimeStampTZ;
        }

        public override MethodInfo GetDataReaderMethod() => _readMethod;
    }
}
