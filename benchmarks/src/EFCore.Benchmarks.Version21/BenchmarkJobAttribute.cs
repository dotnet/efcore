// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
#pragma warning disable CA1813 // Avoid unsealed attributes
    public class BenchmarkJobAttribute : Attribute, IConfigSource
#pragma warning restore CA1813 // Avoid unsealed attributes
    {
        public BenchmarkJobAttribute(bool singleRun = false)
        {
            var job = new Job().ConfigureJob(singleRun);

            Config = ManualConfig.CreateEmpty()
                .With(job.With(CsProjClassicNetToolchain.Net461))
                .With(job.With(CsProjCoreToolchain.NetCoreApp21));
        }

        public IConfig Config { get; }
    }
}
