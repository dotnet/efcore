// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
