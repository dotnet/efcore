// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class BenchmarkJobAttribute : Attribute, IConfigSource
    {
        public BenchmarkJobAttribute()
        {
            var job = new Job("BenchmarkJob");
            job.Env.Gc.Force = true;
            job.Run.UnrollFactor = 1;
            job.Run.InvocationCount = 1;

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
