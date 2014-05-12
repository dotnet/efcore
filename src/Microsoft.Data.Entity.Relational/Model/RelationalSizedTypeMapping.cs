// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    public class RelationalSizedTypeMapping : RelationalTypeMapping
    {
        private readonly int _size;

        public RelationalSizedTypeMapping([NotNull] string storeTypeName, DbType storeType, int size)
            : base(storeTypeName, storeType)
        {
            _size = size;
        }

        protected override void ConfigureParameter(DbParameter parameter, ColumnModification columnModification)
        {
            Check.NotNull(parameter, "parameter");
            Check.NotNull(columnModification, "columnModification");

            parameter.Size = _size;

            base.ConfigureParameter(parameter, columnModification);
        }
    }
}
