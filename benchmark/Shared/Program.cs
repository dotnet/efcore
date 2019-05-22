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

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var config = DefaultConfig.Instance
                .With(DefaultConfig.Instance.GetDiagnosers().Concat(new[] { MemoryDiagnoser.Default }).ToArray());

            var index = Array.FindIndex(args, s => s == "--perflab");
            if (index >= 0)
            {
                var argList = args.ToList();
                argList.RemoveAt(index);
                args = argList.ToArray();

                config = config
                    .With(new[]
                    {
                        StatisticColumn.OperationsPerSecond,
                        new ParamsSummaryColumn()
                    })
                    .With(new[]
                    {
                        MarkdownExporter.GitHub,
                        new CsvExporter(
                            CsvSeparator.Comma,
                            new SummaryStyle
                            {
                                PrintUnitsInHeader = true,
                                PrintUnitsInContent = false,
                                TimeUnit = TimeUnit.Microsecond,
                                SizeUnit = SizeUnit.KB
                            })
                    });
            }

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}
