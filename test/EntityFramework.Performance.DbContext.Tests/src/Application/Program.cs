using System;
using System.Collections.Generic;
using System.Linq;
using Microbenchmarks.Core;

namespace DbContextPerfTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var resultDirectory = string.Empty;
            var testName = string.Empty;
            var runner = new PerfTestRunner();
            var tests = new DbContextPerfTests();

            var allTests = new List<TestDefinitionBase>();

            if (args.Length > 1)
            {
                // test name
                testName = args[1];
            }
            if (args.Length > 0)
            {
                // result directory
                resultDirectory = args[0];
            }

            if (string.IsNullOrWhiteSpace(resultDirectory))
            {
                resultDirectory = ".";
            }

            Console.WriteLine("resultDirectory = " + resultDirectory);
            Console.WriteLine("testName = " + testName);

            allTests.Add(new TestDefinition()
                {
                    TestName = "DbContextDelete",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextDelete,
                    Setup = tests.DbContextDeleteSetup,
                    Cleanup = tests.Cleanup
                });

            allTests.Add(new TestDefinition()
                {
                    TestName = "DbContextInsert",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextInsert,
                    Setup = tests.Setup,
                    Cleanup = tests.Cleanup
                });

            allTests.Add(new TestDefinition()
                {
                    TestName = "DbContextQuery",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextQuery,
                    Setup = tests.Setup,
                    Cleanup = tests.Cleanup
                });

            allTests.Add(new TestDefinition()
                {
                    TestName = "DbContextQueryNoTracking",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextQueryNoTracking,
                    Setup = tests.Setup,
                    Cleanup = tests.Cleanup
                });

            allTests.Add(new TestDefinition()
                {
                    TestName = "DbContextQueryWithThreadsNoTracking",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextQueryWithThreadsNoTracking,
                    Setup = tests.Setup,
                    Cleanup = tests.Cleanup
                });

            allTests.Add(new TestDefinition()
                {
                    TestName = "DbContextUpdate",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.DbContextUpdate,
                    Setup = tests.DbContextUpdateSetup,
                    Cleanup = tests.Cleanup
                });

            var existingDbContextTests = new DbContextPerfTestsWithExistingDbContext();

            allTests.Add(new ThreadedTestDefinition()
                {
                    TestName = "DbContextQueryOnExistingContextWithThreads",
                    ThreadCount = 64,
                    WarmupDuration = 20000,
                    TestDuration = 120000,
                    ThreadStateFactory = existingDbContextTests.NewContext,
                    Run = existingDbContextTests.DbContextQueryOnExistingContextWithThreads,
                    Setup = existingDbContextTests.Setup,
                    Cleanup = existingDbContextTests.Cleanup
                });

            var associationTests = new DbContextAssociationPerfTests();
            allTests.Add(new TestDefinition()
                {
                    TestName = "DbContextRelationshipFixup",
                    IterationCount = 100,
                    WarmupCount = 10,
                    Run = associationTests.DbContextRelationshipFixup,
                    Setup = associationTests.Setup,
                    Cleanup = associationTests.Cleanup
                });

            if (!string.IsNullOrEmpty(testName))
            {
                var testDefinition = allTests.SingleOrDefault(t => t.TestName == testName); 
                if (testDefinition != null)
                {
                    runner.Register(testDefinition);
                }
                else
                {
                    Console.WriteLine("Specified test not ");
                }
            }
            else
            {
                foreach (var test in allTests)
                {
                    runner.Register(test);
                    Console.WriteLine(test.TestName);
                }
            }

            runner.RunTests(resultDirectory);
        }
    }
}
