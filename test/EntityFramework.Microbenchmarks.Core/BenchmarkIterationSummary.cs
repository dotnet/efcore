// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Sdk;

namespace EntityFramework.Microbenchmarks.Core
{
    public class BenchmarkIterationSummary : RunSummary
    {
        public long TimeElapsed { get; set; }
        public long MemoryDelta { get; set; }

        public void Aggregate(RunSummary other, MetricCollector collector)
        {
            Aggregate(other);
            TimeElapsed = collector.TimeElapsed;
            MemoryDelta = collector.MemoryDelta;
        }
    }
}
