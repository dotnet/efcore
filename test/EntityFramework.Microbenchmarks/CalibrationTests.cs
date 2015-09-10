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
        public void Calibration_100ms(MetricCollector collector)
        {
            using (collector.StartCollection())
            {
                Thread.Sleep(100);
            }
        }

        [Benchmark]
        public void Calibration_100ms_controlled(MetricCollector collector)
        {

            Thread.Sleep(100);
            using (collector.StartCollection())
            {
                Thread.Sleep(100);
            }
        }

#if !DNXCORE50 && !DNX451
        [Benchmark]
        public void ColdStartSandbox_100ms(MetricCollector collector)
        {
            using (var sandbox = new ColdStartSandbox())
            {
                var testClass = sandbox.CreateInstance<ColdStartEnabledTests>();
                testClass.Sleep100ms(collector);
            }
        }

        private partial class ColdStartEnabledTests : MarshalByRefObject
        {
            public void Sleep100ms(MetricCollector collector)
            {
                using (collector.StartCollection())
                {
                    Thread.Sleep(100);
                }
            }
        }
#endif
    }
}
