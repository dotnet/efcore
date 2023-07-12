// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tools;

internal static class AnsiConstants
{
    public const string Reset = "\x1b[22m\x1b[39m";
    public const string Bold = "\x1b[1m";
    public const string Dark = "\x1b[22m";
    public const string Black = "\x1b[30m";
    public const string Red = "\x1b[31m";
    public const string Green = "\x1b[32m";
    public const string Yellow = "\x1b[33m";
    public const string Blue = "\x1b[34m";
    public const string Magenta = "\x1b[35m";
    public const string Cyan = "\x1b[36m";
    public const string Gray = "\x1b[37m";
}
