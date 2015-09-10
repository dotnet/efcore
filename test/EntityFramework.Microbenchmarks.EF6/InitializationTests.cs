// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.AdventureWorks;
using EntityFramework.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers;
using EntityFramework.Microbenchmarks.EF6.Models.AdventureWorks;
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6
{
    public class InitializationTests : IClassFixture<AdventureWorksFixture>
    {
        [Benchmark]
#if !DNX451
        [BenchmarkVariation("Cold (1 instance)", true, 1)]
#endif
        [BenchmarkVariation("Warm (100 instances)", false, 100)]
        public void CreateAndDisposeUnusedContext(MetricCollector collector, bool cold, int count)
        {
            RunColdStartEnabledTest(cold, c => c.CreateAndDisposeUnusedContext(collector, count));
        }

        [AdventureWorksDatabaseBenchmark]
#if !DNX451
        [BenchmarkVariation("Cold (1 instance)", true, 1)]
#endif
        [BenchmarkVariation("Warm (10 instances)", false, 10)]
        public void InitializeAndQuery_AdventureWorks(MetricCollector collector, bool cold, int count)
        {
            RunColdStartEnabledTest(cold, c => c.InitializeAndQuery_AdventureWorks(collector, count));
        }

        [AdventureWorksDatabaseBenchmark]
#if !DNX451
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

            var builder = new DbModelBuilder();
            AdventureWorksContext.ConfigureModel(builder);
            var model = builder.Build(new SqlConnection(AdventureWorksFixtureBase.ConnectionString));

            collector.StopCollection();

            Assert.Equal(67, model.ConceptualModel.EntityTypes.Count());
        }

        private void RunColdStartEnabledTest(bool cold, Action<ColdStartEnabledTests> test)
        {
            if (cold)
            {
                using (var sandbox = new ColdStartSandbox())
                {
                    var testClass = sandbox.CreateInstance<ColdStartEnabledTests>();
                    test(testClass);
                }
            }
            else
            {
                test(new ColdStartEnabledTests());
            }
        }

        private class ColdStartEnabledTests : MarshalByRefObject
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
