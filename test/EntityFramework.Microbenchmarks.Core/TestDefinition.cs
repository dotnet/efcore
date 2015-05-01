// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace EntityFramework.Microbenchmarks.Core
{
    public class TestDefinition
    {
        public TestDefinition()
        {
            IterationCount = 100;
        }

        public string TestName { get; set; }
        public Action Setup { get; set; }
        public Action Cleanup { get; set; }
        public int IterationCount { get; set; }
        public int WarmupCount { get; set; }
        public Action<TestHarness> Run { get; set; }
    }
}
