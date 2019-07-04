// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public static class EFCoreBenchmarkRunner
    {
        public static void Run(string[] args, Assembly assembly, IConfig config = null)
        {
            if (config == null)
            {
                config = DefaultConfig.Instance;
            }

            config = config.With(DefaultConfig.Instance.GetDiagnosers().Concat(new[] { MemoryDiagnoser.Default }).ToArray());

            var index = Array.FindIndex(args, s => s == "--perflab");
            if (index >= 0)
            {
                var argList = args.ToList();
                argList.RemoveAt(index);
                args = argList.ToArray();

                config = config
                    .With(StatisticColumn.OperationsPerSecond, new ParamsSummaryColumn())
                    .With(
                        MarkdownExporter.GitHub, new CsvExporter(
                        CsvSeparator.Comma,
                        new SummaryStyle
                        {
                            PrintUnitsInHeader = true,
                            PrintUnitsInContent = false,
                            TimeUnit = TimeUnit.Microsecond,
                            SizeUnit = SizeUnit.KB
                        }));
            }

            BenchmarkSwitcher.FromAssembly(assembly).Run(args, config);
        }
    }
}
