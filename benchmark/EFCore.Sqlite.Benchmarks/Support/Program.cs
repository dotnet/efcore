// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Benchmarks;

public class Program
{
    private static void Main(string[] args)
        => EFCoreBenchmarkRunner.Run(args, typeof(Program).Assembly);
}
