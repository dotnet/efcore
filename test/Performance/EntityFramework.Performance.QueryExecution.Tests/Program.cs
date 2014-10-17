// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microbenchmarks.Core;

namespace QueryExecution
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var resultDirectory = string.Empty;
            var testName = string.Empty;
            var runner = new PerfTestRunner();
            var tests = new QueryExecutionTestsTPT();

            var allTests = new List<TestDefinition>();

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
                    TestName = "Query_Execution_TPT_model_Filter_Where",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Filter_Where,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Projection_Select",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Projection_Select,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Projection_SelectMany",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Projection_SelectMany,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Projection_Nested",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Projection_Nested,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Ordering_OrderBy",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Ordering_OrderBy,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Aggregate_Count",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Aggregate_Count,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Partitioning_Skip",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Partitioning_Skip,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Join_Join",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Join_Join,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Grouping_Groupby",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Grouping_Groupby,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Include",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Include,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_OfType_Linq",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_OfType_Linq,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_Filter_Not_PK_Parameter",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Filter_Not_PK_Parameter,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_Filter_Not_NF_Parameter",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Filter_Not_NF_Parameter,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_Filter_Not_NNF_Parameter",
                    IterationCount = 500,
                    WarmupCount = 10,
                    Run = tests.TPT_Filter_Not_NNF_Parameter,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Funcletization_Case1_WithMember",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.TPT_Funcletization_Case1_WithMember,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Funcletization_Case2_WithMember",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.TPT_Funcletization_Case2_WithMember,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Funcletization_Case1_WithProperty",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.TPT_Funcletization_Case1_WithProperty,
                    Setup = tests.Setup
                });

            allTests.Add(new TestDefinition
                {
                    TestName = "Query_Execution_TPT_model_Funcletization_Case2_WithProperty",
                    IterationCount = 1,
                    WarmupCount = 0,
                    Run = tests.TPT_Funcletization_Case2_WithProperty,
                    Setup = tests.Setup
                });

            if (!string.IsNullOrEmpty(testName))
            {
                var testDefinition = allTests.SingleOrDefault(t => t.TestName == testName);
                if (testDefinition != null)
                {
                    runner.Register(testDefinition);
                    Console.WriteLine(testName);
                }
                else
                {
                    Console.WriteLine("Specified test not found");
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
