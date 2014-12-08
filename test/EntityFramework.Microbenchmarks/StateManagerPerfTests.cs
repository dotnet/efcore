using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.QueryExecutionPerf;
using EntityFramework.Microbenchmarks.StateManagerPerf;
using Xunit;

namespace EntityFramework.Microbenchmarks
{
    public class StateManagerPerfTests
    {
        private readonly string defaultResultDirectory;
        private PerfTestRunner runner;
        private FixupTests tests;


        public StateManagerPerfTests()
        {
            runner = new PerfTestRunner();
            tests = new FixupTests();
            defaultResultDirectory = TestConfig.Instance.ResultsDirectory;
        }

        /*
        [Fact]
        public void Relationship_Fixup_Multithreaded()
        {
            var testDefinition = new ThreadedTestDefinition()
                {
                    TestName = "RelationshipFixupMultithreaded",
                    WarmupDuration = 20,
                    TestDuration = 60,
                    ThreadCount = 10,
                    ThreadStateFactory = tests.NewContextAndLoadDependants,
                    Run = tests.RelationshipFixupMultithreaded,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }*/

        [Fact]
        public void RelationshipFixup()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "RelationshipFixup",
                    WarmupCount = 10,
                    IterationCount = 100,
                    Run = tests.RelationshipFixup,
                    Setup = tests.Setup
                };


            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
    }
}
