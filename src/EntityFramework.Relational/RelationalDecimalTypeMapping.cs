// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalDecimalTypeMapping : RelationalTypeMapping
    {
        private readonly byte _precision;
        private readonly byte _scale;

        public RelationalDecimalTypeMapping(byte precision, byte scale)
            : base("decimal(" + precision + ", " + scale + ")", DbType.Decimal)
        {
            _precision = precision;
            _scale = scale;
        }

        protected override void ConfigureParameter(DbParameter parameter, ColumnModification columnModification)
        {
            Check.NotNull(parameter, nameof(parameter));
            Check.NotNull(columnModification, nameof(columnModification));

            // Note: Precision/scale should not be set for input parameters because this will cause truncation
            if (parameter.Direction == ParameterDirection.Output)
            {
#if NET45
                ((IDbDataParameter)parameter).Scale = _scale;
                ((IDbDataParameter)parameter).Precision = _precision;
#else
                parameter.Scale = _scale;
                parameter.Precision = _precision;
#endif
            }

            base.ConfigureParameter(parameter, columnModification);
        }

        public virtual byte Precision
        {
            get { return _precision; }
        }

        public virtual byte Scale
        {
            get { return _scale; }
        }
    }
}
