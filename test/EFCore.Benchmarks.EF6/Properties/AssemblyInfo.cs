// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.v2;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: BenchmarkJob]
[assembly: MemoryDiagnoser]