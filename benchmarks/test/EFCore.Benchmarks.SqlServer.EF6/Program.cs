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
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<InitializationTests>());

            // ChangeTracker
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<DbSetOperationTests.AddDataVariationsWithAutoDetectChangesOn>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<DbSetOperationTests.AddDataVariationsWithAutoDetectChangesOff>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<DbSetOperationTests.ExistingDataVariationsWithAutoDetectChangesOn>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<DbSetOperationTests.ExistingDataVariationsWithAutoDetectChangesOff>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FixupTests.ChildVariationsWithAutoDetectChangesOn>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FixupTests.ChildVariationsWithAutoDetectChangesOff>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FixupTests.ParentVariationsWithAutoDetectChangesOn>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FixupTests.ParentVariationsWithAutoDetectChangesOff>());

            // Query
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<FuncletizationTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<NavigationsQueryTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<QueryCompilationTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<RawSqlQueryTests>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleQueryTests>());

            // Update
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineTests.Insert>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineTests.Update>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineTests.Delete>());
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleUpdatePipelineTests.Mixed>());
        }
    }
}
