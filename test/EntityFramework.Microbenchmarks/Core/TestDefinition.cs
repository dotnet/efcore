// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace EntityFramework.Microbenchmarks.Core
{
    public abstract class TestDefinitionBase
    {
        public string TestName { get; set; }
        public Action Setup { get; set; }
        public Action Cleanup { get; set; }
    }

    public class TestDefinition : TestDefinitionBase
    {
        public int? IterationCount { get; set; }
        public int? WarmupCount { get; set; }
        public Action Run { get; set; }
    }

    public class ThreadedTestDefinition : TestDefinitionBase
    {
        public int? ThreadCount { get; set; }
        public int? WarmupDuration { get; set; }
        public int? TestDuration { get; set; }
        public Action<object> Run { get; set; }
        public Func<object> ThreadStateFactory { get; set; }
    }

    public class AsyncTestDefinition : ThreadedTestDefinition
    {
    }
}
