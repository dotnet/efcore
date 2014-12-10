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
        private readonly string _defaultResultDirectory;
        private readonly PerfTestRunner _runner;
        private readonly DbContextPerf.DbContextPerfTests _tests;


        public DbContextPerfTests()
        {
            _runner = new PerfTestRunner();
            _tests = new DbContextPerf.DbContextPerfTests();
            _defaultResultDirectory = TestConfig.Instance.ResultsDirectory;
        }

        [Fact] 
        public void DbContext_delete_perf()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextDelete",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = _tests.DbContextDelete,
                    Setup = _tests.DbContextDeleteSetup,
                    Cleanup = _tests.Cleanup
                };
            _runner.Register(testDefinition);
            _runner.RunTests(_defaultResultDirectory);
        }


        [Fact]
        public void DbContext_insert()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextInsert",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = _tests.DbContextInsert,
                    Setup = _tests.Setup,
                    Cleanup = _tests.Cleanup
                };

            _runner.Register(testDefinition);
            _runner.RunTests(_defaultResultDirectory);
        }

        [Fact]
        public void DbContext_query()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextQuery",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = _tests.DbContextQuery,
                    Setup = _tests.Setup,
                    Cleanup = _tests.Cleanup
                };

            _runner.Register(testDefinition);
            _runner.RunTests(_defaultResultDirectory);
        }

        [Fact]
        public void DbContext_query_no_tracking()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextQueryNoTracking",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = _tests.DbContextQueryNoTracking,
                    Setup = _tests.Setup,
                    Cleanup = _tests.Cleanup
                };

            _runner.Register(testDefinition);
            _runner.RunTests(_defaultResultDirectory);
        }

        [Fact]
        public void DbContext_query_with_threads_no_tracking()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextQueryWithThreadsNoTracking",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = _tests.DbContextQueryWithThreadsNoTracking,
                    Setup = _tests.Setup,
                    Cleanup = _tests.Cleanup
                };

            _runner.Register(testDefinition);
            _runner.RunTests(_defaultResultDirectory);
        }

        [Fact]
        public void DbContext_context_update()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "DbContextUpdate",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = _tests.DbContextUpdate,
                    Setup = _tests.DbContextUpdateSetup,
                    Cleanup = _tests.Cleanup
                };

            _runner.Register(testDefinition);
            _runner.RunTests(_defaultResultDirectory);
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
            
            _runner.Register(testDefinition);
            _runner.RunTests(_defaultResultDirectory);
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
            
            _runner.Register(testDefinition);
            _runner.RunTests(_defaultResultDirectory);
        }

    }
}
