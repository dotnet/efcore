// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace EntityFramework.Microbenchmarks.Core
{
    public static class Extensions
    {
        public static void RunTest(this TestDefinition definition)
        {
            var runner = new PerfTestRunner(definition);
            runner.Run(TestConfig.Instance.ResultsDirectory);
        }
    }
}
