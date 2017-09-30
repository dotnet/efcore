// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using BenchmarkDotNet.Attributes;

namespace Microsoft.EntityFrameworkCore.Benchmarks.EFCore
{
    public class CalibrationTests
    {
        [Benchmark]
        public virtual void Calibration_100ms()
        {
            Thread.Sleep(100);
        }
    }
}
