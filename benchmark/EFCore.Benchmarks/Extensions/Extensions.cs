// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public static class Extensions
    {
        public static IQueryable<TEntity> ApplyTracking<TEntity>(this IQueryable<TEntity> query, bool tracking)
            where TEntity : class
            => tracking ? query : query.AsNoTracking();
    }
}
