// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
    {
        private static readonly MethodInfo _readMethod
            = typeof(OracleDataReader).GetTypeInfo().GetDeclaredMethod(nameof(OracleDataReader.GetOracleTimeStampTZ));

        private const string DateTimeOffsetFormatConst = "{0:yyyy-MM-ddTHH:mm:ss.fffzzz}";

        public OracleDateTimeOffsetTypeMapping([NotNull] string storeType)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        typeof(DateTimeOffset),
                        new ValueConverter<DateTimeOffset, OracleTimeStampTZ>(
                            v => new OracleTimeStampTZ(v.DateTime, v.Offset.ToString()),
                            v => new DateTimeOffset(v.Value, v.GetTimeZoneOffset()))),
                    storeType))
        {
        }

        protected OracleDateTimeOffsetTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleDateTimeOffsetTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleDateTimeOffsetTypeMapping(Parameters.WithComposedConverter(converter));

        protected override string SqlLiteralFormatString => "'" + DateTimeOffsetFormatConst + "'";

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            ((OracleParameter)parameter).OracleDbType = OracleDbType.TimeStampTZ;
        }

        public override MethodInfo GetDataReaderMethod() => _readMethod;
    }
}
