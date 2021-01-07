// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public static class EFCoreBenchmarkRunner
    {
        public static void Run(string[] args, Assembly assembly, IConfig config = null)
        {
            BenchmarkSwitcher.FromAssembly(assembly).Run(args);
        }
    }
}
