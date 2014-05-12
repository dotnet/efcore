// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    public class RelationalDecimalTypeMapping : RelationalTypeMapping
    {
        private readonly byte _scale;
        private readonly byte _precision;

        public RelationalDecimalTypeMapping(byte scale, byte precision)
            : base("decimal(" + scale + ", " + precision + ")", DbType.Decimal)
        {
            _scale = scale;
            _precision = precision;
        }

        protected override void ConfigureParameter(DbParameter parameter, ColumnModification columnModification)
        {
            Check.NotNull(parameter, "parameter");
            Check.NotNull(columnModification, "columnModification");

            // Note: Precision/scale should not be set for input parameters because this will cause truncation
            // TODO: Uncomment this--not doing for alpha because it requires all dependencies updated to 4.5.1
            //if (parameter.Direction == ParameterDirection.Output)
            //{
            //    parameter.Scale = _scale;
            //    parameter.Precision = _precision;
            //}

            base.ConfigureParameter(parameter, columnModification);
        }
    }
}
