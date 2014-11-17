using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.DbContextPerf;
using Xunit;

namespace EntityFramework.Microbenchmarks
{
    public class DbContextPerfTests
    {
        private readonly string defaultResultDirectory = @".\PerfResults";
        private string testName;
        private PerfTestRunner runner;
        private DbContextPerf.DbContextPerfTests tests;


        public DbContextPerfTests()
        {
            runner = new PerfTestRunner();
            tests = new DbContextPerf.DbContextPerfTests();
            defaultResultDirectory = TestConfig.Instance.ResultsDirectory;
        }

        [Fact] 
        public void DbContext_delete_perf()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextDelete",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextDelete,
                    Setup = tests.DbContextDeleteSetup,
                    Cleanup = tests.Cleanup
                };
            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }


        [Fact]
        public void DbContext_insert()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextInsert",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextInsert,
                    Setup = tests.Setup,
                    Cleanup = tests.Cleanup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void DbContext_query()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextQuery",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextQuery,
                    Setup = tests.Setup,
                    Cleanup = tests.Cleanup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void DbContext_query_no_tracking()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextQueryNoTracking",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextQueryNoTracking,
                    Setup = tests.Setup,
                    Cleanup = tests.Cleanup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void DbContext_query_with_threads_no_tracking()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextQueryWithThreadsNoTracking",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextQueryWithThreadsNoTracking,
                    Setup = tests.Setup,
                    Cleanup = tests.Cleanup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void DbContext_context_update()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextUpdate",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextUpdate,
                    Setup = tests.DbContextUpdateSetup,
                    Cleanup = tests.Cleanup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void DbContext_query_on_existing_context_with_threads()
        {

            var existingDbContextTests = new DbContextPerfTestsWithExistingDbContext();
            
            var testDefinition = new ThreadedTestDefinition
                {
                    TestName = "DbContextQueryOnExistingContextWithThreads",
                    ThreadCount = 64,
                    WarmupDuration = 20000,
                    TestDuration = 120000,
                    ThreadStateFactory = existingDbContextTests.NewContext,
                    Run = existingDbContextTests.DbContextQueryOnExistingContextWithThreads,
                    Setup = existingDbContextTests.Setup,
                    Cleanup = existingDbContextTests.Cleanup
                };
            
            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void DbContext_relationship_fixup()
        {
            var associationTests = new DbContextAssociationPerfTests();
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextRelationshipFixup",
                    IterationCount = 100,
                    WarmupCount = 10,
                    Run = associationTests.DbContextRelationshipFixup,
                    Setup = associationTests.Setup,
                    Cleanup = associationTests.Cleanup
                };
            
            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

    }
}
