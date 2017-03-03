// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class SkipForNonBenchmarkTestRuns : Attribute, ITestCondition
    {
        public SkipForNonBenchmarkTestRuns(string reason)
        {
            SkipReason = reason;
        }

        public bool IsMet => BenchmarkConfig.Instance.RunIterations;

        public string SkipReason { get; set; }
    }
}
