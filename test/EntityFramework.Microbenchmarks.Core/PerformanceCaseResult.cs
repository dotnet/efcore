// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace EntityFramework.Microbenchmarks.Core
{
    public class PerformanceCaseResult
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public PerformanceMetric[] Metrics { get; set; }

        public void StartTimer()
        {
            StartTime = DateTime.Now;
        }

        public void StopTimer()
        {
            EndTime = DateTime.Now;
        }
    }
}
