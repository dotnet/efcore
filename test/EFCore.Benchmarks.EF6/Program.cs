// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore.Benchmarks.EF6.Query;
using Microsoft.EntityFrameworkCore.Benchmarks.v2;

namespace Microsoft.EntityFrameworkCore.Benchmarks.EF6
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var benchmarkSummaryProcessor = new BenchmarkSummaryProcessor();
            
            benchmarkSummaryProcessor.Process(BenchmarkRunner.Run<SimpleQueryTests>());
        }
    }
}
