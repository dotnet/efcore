// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.QueryTestGeneration;

public static class QueryTestGenerationExtensions
{
    public static TResult? Choose<TResult>(this Random random, List<TResult> list)
        => list.Count == 0 ? default : list[random.Next(list.Count)];
}
