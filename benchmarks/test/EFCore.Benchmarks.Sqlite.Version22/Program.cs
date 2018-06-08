// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore.Benchmarks.ChangeTracker;
using Microsoft.EntityFrameworkCore.Benchmarks.Initialization;
using Microsoft.EntityFrameworkCore.Benchmarks.Query;
using Microsoft.EntityFrameworkCore.Benchmarks.UpdatePipeline;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var benchmarkSummaryProcessor = new BenchmarkSummaryProcessor();

            // Initialization
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<InitializationSqliteTests>());

            // ChangeTracker
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<DbSetOperationSqliteTests.AddDataVariations>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<DbSetOperationSqliteTests.ExistingDataVariations>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FixupSqliteTests.ChildVariations>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FixupSqliteTests.ParentVariations>());

            // Query
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FuncletizationSqliteTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<NavigationsQuerySqliteTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<QueryCompilationSqliteTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<RawSqlQuerySqliteTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleQuerySqliteTests>());

            // Update
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineSqliteTests.Insert>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineSqliteTests.Update>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineSqliteTests.Delete>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineSqliteTests.Mixed>());
        }
    }
}
