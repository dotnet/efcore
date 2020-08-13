// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Microsoft.EntityFrameworkCore.Benchmarks.ChangeTracker
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class SingleRunJobAttribute : Attribute, IConfigSource
    {
        public SingleRunJobAttribute()
        {
            Config = ManualConfig.CreateEmpty().With(
                new Job()
                    .WithWarmupCount(1)
                    .WithIterationCount(1)
                    .With(RunStrategy.Monitoring));
        }

        public IConfig Config { get; }
    }
}
