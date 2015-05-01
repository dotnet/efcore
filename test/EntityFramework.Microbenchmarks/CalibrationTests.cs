// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using EntityFramework.Microbenchmarks.Core;
using Xunit;

namespace EntityFramework.Microbenchmarks
{
    public class CalibrationTests
    {
        [Fact]
        public void Calibration_100ms()
        {
            new TestDefinition
                {
                    TestName = "Calibration_100ms",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Run = harness =>
                        {
                            using (harness.StartCollection())
                            {
                                Thread.Sleep(100);
                            }
                        }
                }.RunTest();
        }

        [Fact]
        public void Calibration_100ms_controlled()
        {
            new TestDefinition
                {
                    TestName = "Calibration_100ms_controlled",
                    IterationCount = 100,
                    WarmupCount = 5,
                    Run = harness =>
                        {
                            Thread.Sleep(100);
                            using (harness.StartCollection())
                            {
                                Thread.Sleep(100);
                            }
                        }
                }.RunTest();
        }
    }
}
