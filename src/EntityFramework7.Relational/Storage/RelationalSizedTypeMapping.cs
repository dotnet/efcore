// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalSizedTypeMapping : RelationalTypeMapping
    {
        public RelationalSizedTypeMapping(
            [NotNull] string defaultTypeName,
            DbType? storeType,
            int size)
            : base(defaultTypeName, storeType)
        {
            Size = size;
        }

        public RelationalSizedTypeMapping(
            [NotNull] string defaultTypeName,
            int size)
            : this(defaultTypeName, null, size)
        {
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            Check.NotNull(parameter, nameof(parameter));

            parameter.Size = Size;

            base.ConfigureParameter(parameter);
        }

        public virtual int Size { get; }
    }
}
