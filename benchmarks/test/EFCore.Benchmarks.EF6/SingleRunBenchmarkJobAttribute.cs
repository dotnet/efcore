// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Microsoft.EntityFrameworkCore.Benchmarks.EF6
{
#pragma warning disable CA1813 // Avoid unsealed attributes
    public class SingleRunBenchmarkJobAttribute : Attribute, IConfigSource
#pragma warning restore CA1813 // Avoid unsealed attributes
    {
        public SingleRunBenchmarkJobAttribute()
        {
            var job = new Job("BenchmarkJob");
            job.Env.Gc.Force = true;
            job.Run.UnrollFactor = 1;
            job.Run.InvocationCount = 1;
            job.Run.WarmupCount = 1;
            job.Run.TargetCount = 1;
            job.Run.RunStrategy = RunStrategy.Monitoring;

            if (!BenchmarkConfig.Instance.RunIterations)
            {
                job.Run.RunStrategy = RunStrategy.ColdStart;
                job.Run.TargetCount = 1;
            }

            Config = ManualConfig.CreateEmpty().With(job);
        }

        public IConfig Config { get; }
    }
}
