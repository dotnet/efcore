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
        public OracleDecimalTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = null,
            int? precision = null,
            int? scale = null,
            StoreTypePostfix storeTypePostfix = StoreTypePostfix.None)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(decimal)),
                    storeType,
                    storeTypePostfix,
                    dbType,
                    precision: precision,
                    scale: scale))
        {
        }

        protected OracleDecimalTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new OracleDecimalTypeMapping(parameters);
    }
}
