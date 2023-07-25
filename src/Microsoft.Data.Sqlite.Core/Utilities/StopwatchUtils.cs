// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Sqlite;

internal static class StopwatchUtils
{
    internal static TimeSpan GetElapsedTime(long startTimestamp)
#if NET7_0_OR_GREATER
        => Stopwatch.GetElapsedTime(startTimestamp);
#else
        => new((long)((Stopwatch.GetTimestamp() - startTimestamp) * StopWatchTickFrequency));

    private const long TicksPerMicrosecond = 10;
    private const long TicksPerMillisecond = TicksPerMicrosecond * 1000;
    private const long TicksPerSecond = TicksPerMillisecond * 1000;   // 10,000,000
    private static readonly double StopWatchTickFrequency = (double)TicksPerSecond / Stopwatch.Frequency;
#endif
}
