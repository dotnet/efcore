// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class SqlBatch
    {
        public SqlBatch([NotNull] string sql)
        {
            Check.NotNull(sql, nameof(sql));

            Sql = sql;
        }

        public virtual string Sql { get; }

        public virtual bool SuppressTransaction { get; set; }
    }
}
