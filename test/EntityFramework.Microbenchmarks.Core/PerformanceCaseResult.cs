// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace EntityFramework.Microbenchmarks.Core
{
    public class PerformanceCaseResult
    {
        public PerformanceCaseResult()
        {
            Metrics = new PerformanceMetric[] { };
            Failures = new string[] { };
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string[] Failures { get; set; }
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
