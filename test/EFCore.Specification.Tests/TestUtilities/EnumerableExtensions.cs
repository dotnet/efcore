// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    public static class EnumerableExtensions
    {
        public static async Task<List<TSource>> ToListAsync<TSource>(
            this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            var list = new List<TSource>();
            await foreach (var element in source.WithCancellation(cancellationToken))
            {
                list.Add(element);
            }

            return list;
        }

        public static List<TSource> ToList<TSource>(this IEnumerable source)
            => source.OfType<TSource>().ToList();
    }
}
