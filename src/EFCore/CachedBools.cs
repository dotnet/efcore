// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Microsoft.EntityFrameworkCore;

public static class CachedBools
{
    public static IReadOnlyList<bool> True = [true];
    public static IReadOnlyList<bool> False = [false];
    public static IReadOnlyList<bool> FalseTrue = [false, true];
    public static IReadOnlyList<bool> TrueFalse = [true,false];
    public static IReadOnlyList<bool> TrueTrue = [true, true];
    public static IReadOnlyList<bool> TrueTrueTrue = [true, true, true];
    public static IReadOnlyList<bool> FalseTrueTrue = [false, true, true];
    public static IReadOnlyList<bool> TrueFalseFalse = [true, true, false];
    public static IReadOnlyList<bool> FalseFalse = [false, false];
}
