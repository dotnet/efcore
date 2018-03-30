// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleDecimalTypeMapping : DecimalTypeMapping
    {
        public OracleDecimalTypeMapping([NotNull] string storeType, DbType? dbType = null)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        typeof(decimal)),
                    storeType,
                    StoreTypePostfix.PrecisionAndScale,
                    dbType,
                    precision: 29,
                    scale: 4))
        {
        }

        protected OracleDecimalTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new OracleDecimalTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new OracleDecimalTypeMapping(Parameters.WithComposedConverter(converter));
    }
}
