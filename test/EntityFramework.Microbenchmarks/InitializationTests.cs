// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.AdventureWorks;
using EntityFramework.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers;
using EntityFramework.Microbenchmarks.Models.AdventureWorks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.SqlServer;
using System;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks
{
    public partial class InitializationTests : IClassFixture<AdventureWorksFixture>
    {
        [Benchmark]
#if !DNXCORE50 && !DNX451
        [BenchmarkVariation("Cold (1 instance)", true, 1)]
#endif
        [BenchmarkVariation("Warm (100 instances)", false, 100)]
        public void CreateAndDisposeUnusedContext(MetricCollector collector, bool cold, int count)
        {
            RunColdStartEnabledTest(cold, c => c.CreateAndDisposeUnusedContext(collector, count));
        }

        [AdventureWorksDatabaseBenchmark]
#if !DNXCORE50 && !DNX451
        [BenchmarkVariation("Cold (1 instance)", true, 1)]
#endif
        [BenchmarkVariation("Warm (10 instances)", false, 10)]
        public void InitializeAndQuery_AdventureWorks(MetricCollector collector, bool cold, int count)
        {
            RunColdStartEnabledTest(cold, c => c.InitializeAndQuery_AdventureWorks(collector, count));
        }

        [AdventureWorksDatabaseBenchmark]
#if !DNXCORE50 && !DNX451
        [BenchmarkVariation("Cold (1 instance)", true, 1)]
#endif
        [BenchmarkVariation("Warm (10 instances)", false, 10)]
        public void InitializeAndSaveChanges_AdventureWorks(MetricCollector collector, bool cold, int count)
        {
            RunColdStartEnabledTest(cold, t => t.InitializeAndSaveChanges_AdventureWorks(collector, count));
        }

        [Benchmark]
        public void BuildModel_AdventureWorks(MetricCollector collector)
        {
            collector.StartCollection();

            var conventions = new SqlServerConventionSetBuilder()
                .AddConventions(new CoreConventionSetBuilder().CreateConventionSet());

            var builder = new ModelBuilder(conventions);
            AdventureWorksContext.ConfigureModel(builder);

            var model = builder.Model;

            collector.StopCollection();

            Assert.Equal(67, model.EntityTypes.Count());
        }

        private void RunColdStartEnabledTest(bool cold, Action<ColdStartEnabledTests> test)
        {
            if (cold)
            {
#if DNXCORE50
                throw new NotSupportedException("ColdStartSandbox can not be used on CoreCLR.");
#else
                using (var sandbox = new ColdStartSandbox())
                {
                    var testClass = sandbox.CreateInstance<ColdStartEnabledTests>();
                    test(testClass);
                }
#endif
            }
            else
            {
                test(new ColdStartEnabledTests());
            }
        }

#if !DNXCORE50
        private partial class ColdStartEnabledTests : MarshalByRefObject
        {
        }
#endif

        private partial class ColdStartEnabledTests
        {
            public void CreateAndDisposeUnusedContext(MetricCollector collector, int count)
            {
                using (collector.StartCollection())
                {
                    for (int i = 0; i < count; i++)
                    {
                        using (var context = AdventureWorksFixture.CreateContext())
                        {
                        }
                    }
                }
            }

            public void InitializeAndQuery_AdventureWorks(MetricCollector collector, int count)
            {
                using (collector.StartCollection())
                {
                    for (int i = 0; i < count; i++)
                    {
                        using (var context = AdventureWorksFixture.CreateContext())
                        {
                            context.Department.First();
                        }
                    }
                }
            }

            public void InitializeAndSaveChanges_AdventureWorks(MetricCollector collector, int count)
            {
                using (collector.StartCollection())
                {
                    for (int i = 0; i < count; i++)
                    {
                        using (var context = AdventureWorksFixture.CreateContext())
                        {
                            context.Currency.Add(new Currency
                            {
                                CurrencyCode = "TMP",
                                Name = "Temporary"
                            });

                            using (context.Database.BeginTransaction())
                            {
                                context.SaveChanges();

                                // Don't mesure transaction rollback
                                collector.StopCollection();
                            }
                            collector.StartCollection();
                        }
                    }
                }
            }
        }
    }
}
