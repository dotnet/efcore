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
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<InitializationSqlServerTests>());

            // ChangeTracker
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<DbSetOperationSqlServerTests.AddDataVariations>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<DbSetOperationSqlServerTests.ExistingDataVariations>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FixupSqlServerTests.ChildVariations>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FixupSqlServerTests.ParentVariations>());

            // Query
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FuncletizationSqlServerTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<NavigationsQuerySqlServerTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<QueryCompilationSqlServerTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<RawSqlQuerySqlServerTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleQuerySqlServerTests>());

            // Update
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineSqlServerTests.Insert>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineSqlServerTests.Update>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineSqlServerTests.Delete>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineSqlServerTests.Mixed>());
        }
    }
}
