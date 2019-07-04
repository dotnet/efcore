// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public class Program
    {
        private static void Main(string[] args) => EFCoreBenchmarkRunner.Run(args, typeof(Program).Assembly);
    }
}
