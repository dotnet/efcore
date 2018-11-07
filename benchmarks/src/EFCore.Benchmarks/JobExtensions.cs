// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public static class JobExtensions
    {
        public static Job ConfigureJob(this Job job, bool singleRun = false)
        {
            job.WithGcForce(true).WithUnrollFactor(1).WithInvocationCount(1).With(Platform.AnyCpu);

            if (singleRun)
            {
                job.WithWarmupCount(1).WithIterationCount(1).With(RunStrategy.Monitoring);
            }

            if (!BenchmarkConfig.Instance.RunIterations)
            {
                job.With(RunStrategy.ColdStart).WithIterationCount(1);
            }

            return job;
        }
    }
}
