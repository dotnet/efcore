// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks
{
    public static class Extensions
    {
        public static IQueryable<TEntity> ApplyTracking<TEntity>(this IQueryable<TEntity> query, bool tracking)
            where TEntity : class
        {
            return tracking
                ? query
                : query.AsNoTracking();
        }
    }
}
