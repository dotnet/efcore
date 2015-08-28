// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.AdventureWorks;
using EntityFramework.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers;
using EntityFramework.Microbenchmarks.EF6.Models.AdventureWorks;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.EF6
{
    public class InitializationTests : IClassFixture<AdventureWorksFixture>
    {
        private readonly AdventureWorksFixture _fixture;

        public InitializationTests(AdventureWorksFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        [BenchmarkVariation("Warm")]
        public void CreateAndDisposeUnusedContext(MetricCollector collector)
        {
            using (collector.StartCollection())
            {
                for (int i = 0; i < 100; i++)
                {
                    using (var context = _fixture.CreateContext())
                    {
                    }
                }
            }
        }

        [AdventureWorksDatabaseBenchmark]
        [BenchmarkVariation("Warm")]
        public void InitializeAndQuery_AdventureWorks(MetricCollector collector)
        {
            using (collector.StartCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    using (var context = _fixture.CreateContext())
                    {
                        context.Department.First();
                    }
                }
            }
        }

        [AdventureWorksDatabaseBenchmark]
        [BenchmarkVariation("Warm")]
        public void InitializeAndSaveChanges_AdventureWorks(MetricCollector collector)
        {
            using (collector.StartCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    using (var context = _fixture.CreateContext())
                    {
                        context.Department.Add(new Department
                        {
                            Name = "Benchmarking",
                            GroupName = "Engineering"
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
    }
}
