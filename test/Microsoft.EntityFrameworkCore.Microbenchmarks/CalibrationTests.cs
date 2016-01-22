// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using EntityFramework.Microbenchmarks.Core;

namespace EntityFramework.Microbenchmarks
{
    public class CalibrationTests
    {
        [Benchmark]
        public void Calibration_100ms(IMetricCollector collector)
        {
            using (collector.StartCollection())
            {
                Thread.Sleep(100);
            }
        }

        [Benchmark]
        public void Calibration_100ms_controlled(IMetricCollector collector)
        {
            Thread.Sleep(100);
            using (collector.StartCollection())
            {
                Thread.Sleep(100);
            }
        }
    }
}
