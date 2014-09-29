using System;
using System.Collections.Generic;
using System.Linq;
using Microbenchmarks.Core;

namespace Cud
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var resultDirectory = string.Empty;
            var testName = string.Empty;
            var runner = new PerfTestRunner();
            var tests = new PocoCudTests();

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

            allTests.Add(new TestDefinition
                {
                    TestName = "PocoCUD_Create",
                    IterationCount = 5000,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Create,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "PocoCUD_Create_TCPIP",
                    IterationCount = 5000,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Create_TCPIP,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "PocoCUD_Update",
                    IterationCount = 5000,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Update,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "PocoCUD_Update_TCPIP",
                    IterationCount = 5000,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Update_TCPIP,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "PocoCUD_Delete",
                    IterationCount = 500,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Delete,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "PocoCUD_Delete_TCPIP",
                    IterationCount = 500,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Delete_TCPIP,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "PocoCUD_Batch",
                    IterationCount = 200,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Batch,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "PocoCUD_Batch_TCPIP",
                    IterationCount = 200,
                    WarmupCount = 5,
                    Run = tests.PocoCUD_Batch_TCPIP,
                    Setup = tests.Setup
                });

            if (!string.IsNullOrEmpty(testName))
            {
                var testDefinition = allTests.FirstOrDefault(t => t.TestName == testName); 
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