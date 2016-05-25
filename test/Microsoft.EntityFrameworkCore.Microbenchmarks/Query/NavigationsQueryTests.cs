// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers;
using Microsoft.EntityFrameworkCore.Microbenchmarks.Models.AdventureWorks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NavigationsQueryTests : IClassFixture<AdventureWorksFixture>
    {
        private readonly AdventureWorksFixture _fixture;

        public NavigationsQueryTests(AdventureWorksFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        [AdventureWorksDatabaseRequired]
        [BenchmarkVariation("Sync (10 queries)", false, 10)]
        [BenchmarkVariation("Async (10 queries)", true, 10)]
        public async Task PredicateAcrossOptionalNavigationAllResults(IMetricCollector collector, bool async, int queriesPerIteration)
        {
            using (var context = AdventureWorksFixture.CreateContext())
            {
                var query = context.Store.Where(s => s.SalesPerson.Bonus >= 0);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        if (async)
                        {
                            await query.ToListAsync();
                        }
                        else
                        {
                            query.ToList();
                        }
                    }
                }

                Assert.Equal(701, query.Count());
            }
        }

        [Benchmark]
        [AdventureWorksDatabaseRequired]
        [BenchmarkVariation("Sync (10 queries)", false, 10)]
        [BenchmarkVariation("Async (10 queries)", true, 10)]
        public async Task PredicateAcrossOptionalNavigationFilteredResults(IMetricCollector collector, bool async, int queriesPerIteration)
        {
            using (var context = AdventureWorksFixture.CreateContext())
            {
                var query = context.Store.Where(s => s.SalesPerson.Bonus > 3000);

                using (collector.StartCollection())
                {
                    for (var i = 0; i < queriesPerIteration; i++)
                    {
                        if (async)
                        {
                            await query.ToListAsync();
                        }
                        else
                        {
                            query.ToList();
                        }
                    }
                }

                Assert.Equal(466, query.Count());
            }
        }
    }
}
