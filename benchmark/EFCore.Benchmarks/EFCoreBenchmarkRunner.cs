// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace Microsoft.EntityFrameworkCore.Benchmarks;

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
