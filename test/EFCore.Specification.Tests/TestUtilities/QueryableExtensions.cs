// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class QueryableExtensions
    {
        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable source, CancellationToken cancellationToken = default)
            => ((IQueryable<TSource>)source).ToListAsync(cancellationToken);
    }
}
