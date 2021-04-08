// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public static class EFCoreBenchmarkRunner
    {
        public static void Run(string[] args, Assembly assembly, IConfig config = null)
        {
            config ??= DefaultConfig.Instance;

            config = config
                .AddDiagnoser(MemoryDiagnoser.Default)
                .AddColumn(StatisticColumn.OperationsPerSecond);

            BenchmarkSwitcher.FromAssembly(assembly).Run(args, config);
        }
    }
}
