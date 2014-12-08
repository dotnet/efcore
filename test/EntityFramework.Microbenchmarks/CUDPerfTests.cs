using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.CudPerf;
using Xunit;

namespace EntityFramework.Microbenchmarks
{
    public class CUDPerfTests
    {
        private readonly string defaultResultDirectory;
        private PerfTestRunner runner;
        private PocoCudTests tests;


        public CUDPerfTests()
        {
            runner = new PerfTestRunner();
            tests = new PocoCudTests();
            defaultResultDirectory = TestConfig.Instance.ResultsDirectory;
        }

        [Fact]
        public void PocoCUD_Create()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "PocoCUD_Create",
                    IterationCount = 5000,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Create,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void PocoCUD_Create_TCPIP()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "PocoCUD_Create_TCPIP",
                    IterationCount = 5000,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Create_TCPIP,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void PocoCUD_Update()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "PocoCUD_Update",
                    IterationCount = 5000,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Update,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void PocoCUD_Update_TCPIP()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "PocoCUD_Update_TCPIP",
                    IterationCount = 5000,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Update_TCPIP,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void PocoCUD_Delete()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "PocoCUD_Delete",
                    IterationCount = 500,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Delete,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void PocoCUD_Delete_TCPIP()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "PocoCUD_Delete_TCPIP",
                    IterationCount = 500,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Delete_TCPIP,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void PocoCUD_Batch()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "PocoCUD_Batch",
                    IterationCount = 200,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Batch,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }

        [Fact]
        public void PocoCUD_Batch_TCPIP()
        {
            var testDefinition = new TestDefinition
                {
                    TestName = "PocoCUD_Batch_TCPIP",
                    IterationCount = 200,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Batch_TCPIP,
                    Setup = tests.Setup
                };

            runner.Register(testDefinition);
            runner.RunTests(defaultResultDirectory);
        }
    }
}
