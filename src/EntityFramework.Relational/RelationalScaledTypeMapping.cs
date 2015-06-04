// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalScaledTypeMapping : RelationalTypeMapping
    {
        private readonly byte _precision;
        private readonly byte? _scale;

        public RelationalScaledTypeMapping(
            [NotNull] string defaultTypeName,
            DbType? storeType,
            byte precision,
            byte? scale = null)
            : base(defaultTypeName, storeType)
        {
            _precision = precision;
            _scale = scale;
        }

        public RelationalScaledTypeMapping(
            [NotNull] string defaultTypeName,
            byte precision,
            byte? scale = null)
            : this(defaultTypeName, null, precision, scale)
        {
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            Check.NotNull(parameter, nameof(parameter));

            // Note: Precision/scale should not be set for input parameters because this will cause truncation
            if (parameter.Direction == ParameterDirection.Output)
            {
#if NET45
                ((IDbDataParameter)parameter).Precision = _precision;
                if (_scale != null)
                {
                    ((IDbDataParameter)parameter).Scale = (byte)_scale;
                }
#else
                parameter.Precision = _precision;
                if (_scale != null)
                {
                    parameter.Scale = (byte)_scale;
                }
#endif
            }

            base.ConfigureParameter(parameter);
        }

        public virtual byte Precision => _precision;

        public virtual byte? Scale => _scale;
    }
}
