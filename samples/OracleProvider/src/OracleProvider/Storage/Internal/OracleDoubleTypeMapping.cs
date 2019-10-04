// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleDoubleTypeMapping : DoubleTypeMapping
    {
        public OracleDoubleTypeMapping(
             [NotNull] string storeType,
             DbType? dbType = null)
             : base(storeType, dbType)
        {
        }

        protected OracleDoubleTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new OracleDoubleTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var literal = base.GenerateNonNullSqlLiteral(value);

            var doubleValue = (double)value;
            if (!literal.Contains("E")
                && !literal.Contains("e")
                && !double.IsNaN(doubleValue)
                && !double.IsInfinity(doubleValue))
            {
                return literal + "E0";
            }

            return literal;
        }
    }
}
