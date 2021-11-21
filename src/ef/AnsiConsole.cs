// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Tools;

internal static class AnsiConsole
{
    public static readonly AnsiTextWriter _out = new(Console.Out);

    public static void WriteLine(string? text)
        => _out.WriteLine(text);
}
