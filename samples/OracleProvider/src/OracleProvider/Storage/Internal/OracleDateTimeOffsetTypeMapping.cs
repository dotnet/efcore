// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
    {
        private static readonly MethodInfo _readMethod
            = typeof(OracleDataReader).GetTypeInfo().GetDeclaredMethod(nameof(OracleDataReader.GetOracleTimeStampTZ));

        private const string DateTimeOffsetFormatConst = "TIMESTAMP '{0:yyyy-MM-dd HH:mm:ss.fff zzz}'";

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

        protected override string GenerateNonNullSqlLiteral(object value)
            => string.Format(CultureInfo.InvariantCulture, DateTimeOffsetFormatConst,
                ((OracleTimeStampTZ)Check.NotNull(value, nameof(value))).Value);

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            ((OracleParameter)parameter).OracleDbType = OracleDbType.TimeStampTZ;
        }

        public override MethodInfo GetDataReaderMethod() => _readMethod;
    }
}
