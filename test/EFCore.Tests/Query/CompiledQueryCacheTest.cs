// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompiledQueryCacheTest
    {
        [Fact]
        public void Least_used_queries_are_evicted_from_cache()
        {
            const int threadCount = 10;
            const int compilersCount = 1000;

            var compilers = new (int Frequency, int HitCount, Func<Func<QueryContext, int>> Compiler)[compilersCount];

            for (var i = 0; i < compilersCount; i++)
            {
                var index = i;

                compilers[i] = (0, 0, (Func<Func<QueryContext, int>>)(() =>
                {
                    compilers[index].HitCount++;

                    return q => -1;
                }));
            }

            var used = new ConcurrentStack<int>();

            ICompiledQueryCache cache = new CompiledQueryCache();

            Parallel.For(
                0, threadCount,
                _ =>
                {
                    var mod = 1;
                    for (var i = 0; i < 2000; i++)
                    {
                        for (var j = 0; j < compilersCount; j += mod)
                        {
                            compilers[j].Frequency++;
                            cache.GetOrAddQuery(j, compilers[j].Compiler);
                            used.Push(j);
                        }

                        mod = (mod * 2) % 256;
                        mod = mod == 0 ? 1 : mod;
                    }
                });

            for (var i = 0; i < 300; i++)
            {
                used.TryPop(out var index);
                var compiledCount = compilers[index].HitCount;

                // Should be in the cache, so this should not trigger re-compiling.
                cache.GetOrAddQuery(index, compilers[index].Compiler);
                Assert.Equal(compiledCount, compilers[index].HitCount);
            }
        }
    }
}
