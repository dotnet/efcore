// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

internal static class Statics
{
    internal static readonly bool[][] TrueArrays =
    [
        [],
        [true],
        [true, true],
        [true, true, true],
    ];

    internal static readonly bool[][] FalseArrays =
    [
        [],
        [false],
        [false, false],
        [false, false, false],
    ];

    internal static IReadOnlyList<bool> FalseTrue = [false, true];
    internal static IReadOnlyList<bool> TrueFalse = [true, false];
}
