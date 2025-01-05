// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;

namespace Microsoft.EntityFrameworkCore.Benchmarks;

public static class Extensions
{
    public static IQueryable<TEntity> ApplyTracking<TEntity>(this IQueryable<TEntity> query, bool tracking)
        where TEntity : class
        => tracking
            ? query
            : query.AsNoTracking();
}
