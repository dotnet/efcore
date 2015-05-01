// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace EntityFramework.Microbenchmarks.Core
{
    public class PerformanceMetric
    {
        public PerformanceMetric()
        {
            Unit = "Numeric";
        }

        public string Scenario { get; set; }
        public string Metric { get; set; }
        public string Unit { get; set; }
        public double Value { get; set; }
    }
}
