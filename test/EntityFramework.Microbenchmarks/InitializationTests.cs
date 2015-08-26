// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.Core.Models.AdventureWorks;
using EntityFramework.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers;
using EntityFramework.Microbenchmarks.Models.AdventureWorks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.SqlServer;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks
{
    public class InitializationTests : IClassFixture<AdventureWorksFixture>
    {
        private readonly AdventureWorksFixture _fixture;

        public InitializationTests(AdventureWorksFixture fixture)
        {
            _fixture = fixture;
        }

        [Benchmark]
        [BenchmarkVariation("Cold", true)]
        [BenchmarkVariation("Warm", false)]
        public void CreateAndDisposeUnusedContext(MetricCollector collector, bool cold)
        {
            using (collector.StartCollection())
            {
                for (int i = 0; i < 100; i++)
                {
                    using (var context = _fixture.CreateContext(cold))
                    {
                    }
                }
            }
        }

        [AdventureWorksDatabaseBenchmark]
        [BenchmarkVariation("Cold", true)]
        [BenchmarkVariation("Warm", false)]
        public void InitializeAndQuery_AdventureWorks(MetricCollector collector, bool cold)
        {
            using (collector.StartCollection())
            {
                using (var context = _fixture.CreateContext(cold))
                {
                    context.Department.First();
                }
            }
        }

        [AdventureWorksDatabaseBenchmark]
        [BenchmarkVariation("Cold", true)]
        [BenchmarkVariation("Warm", false)]
        public void InitializeAndSaveChanges_AdventureWorks(MetricCollector collector, bool cold)
        {
            using (collector.StartCollection())
            {
                using (var context = _fixture.CreateContext(cold))
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
    }
}
