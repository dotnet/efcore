// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public abstract class ISetExtractor
    {
        public abstract IQueryable<TEntity> Set<TEntity>(DbContext context)
            where TEntity : class;
    }
}
