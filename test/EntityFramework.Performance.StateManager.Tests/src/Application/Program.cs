// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace StateManager
{
    using System;
    using Microbenchmarks.Core;

    class Program
    {
        static void Main(string[] args)
        {
            var runner = new PerfTestRunner();

            var tests = new FixupTests();

            /*runner.Register(
                new ThreadedTestDefinition()
                {
                    TestName = "RelationshipFixupMultithreaded",
                    WarmupDuration = 20,
                    TestDuration = 60,
                    ThreadCount = 10,
                    ThreadStateFactory = tests.NewContextAndLoadDependants,
                    Run = tests.RelationshipFixupMultithreaded,
                    Setup = tests.Setup
                });*/

            runner.Register(
                new TestDefinition()
                {
                    TestName = "RelationshipFixup",
                    WarmupCount = 10,
                    IterationCount = 100,
                    Run = tests.RelationshipFixup,
                    Setup = tests.Setup
                });

            string resultDirectory;

            if (args.Length == 0)
            {
                resultDirectory = ".";
            }
            else
            {
                resultDirectory = args[0];
                if (args.Length == 0 || string.IsNullOrEmpty(resultDirectory))
                {
                    resultDirectory = ".";
                }
            }
            Console.WriteLine("resultDirectory = " + resultDirectory);
            runner.RunTests(resultDirectory);
        }
    }
}
