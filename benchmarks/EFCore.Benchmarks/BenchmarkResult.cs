// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public class BenchmarkResult
    {
        // Machine info

        public string MachineName { get; set; }
        public string Framework { get; set; }
        public string Architecture { get; set; }

        // Test info

        public string TestClassFullName { get; set; }
        public string TestClass { get; set; }
        public string TestMethodName { get; set; }
        public string Variation { get; set; }

        public string EfVersion { get; set; }
        public string CustomData { get; set; }

        // Run data

        public DateTime ReportingTime { get; set; }
        public int WarmupIterations { get; set; }
        public int MainIterations { get; set; }

        // Result info
        // Computation time

        public double TimeElapsedMean { get; set; }
        public double TimeElapsedPercentile90 { get; set; }
        public double TimeElapsedPercentile95 { get; set; }
        public double TimeElapsedStandardError { get; set; }
        public double TimeElapsedStandardDeviation { get; set; }

        // Allocated Memory
        public double MemoryAllocated { get; set; }
    }
}
