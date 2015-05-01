// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalSizedTypeMapping : RelationalTypeMapping
    {
        public RelationalSizedTypeMapping([NotNull] string storeTypeName, DbType storeType, int size)
            : base(storeTypeName, storeType)
        {
            Size = size;
        }

        protected override void ConfigureParameter(DbParameter parameter, ColumnModification columnModification)
        {
            Check.NotNull(parameter, nameof(parameter));
            Check.NotNull(columnModification, nameof(columnModification));

            parameter.Size = Size;

            base.ConfigureParameter(parameter, columnModification);
        }

        public virtual int Size { get; }
    }
}
