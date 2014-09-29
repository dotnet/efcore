// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microbenchmarks.Core
{
    public class TestDefinition
    {
        public int? IterationCount { get; set; }
        public int? WarmupCount { get; set; }
        public string TestName { get; set; }
        public Action Setup { get; set; }
        public Action Run { get; set; }
        public Action Cleanup { get; set; }
    }

    public class ThreadedTestDefinition : TestDefinition
    {
        public int ThreadCount { get; set; }
    }

    public class AsyncTestDefinition : ThreadedTestDefinition
    {
    }
}
