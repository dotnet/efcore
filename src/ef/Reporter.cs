// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.EntityFrameworkCore.Tools.AnsiConstants;

namespace Microsoft.EntityFrameworkCore.Tools;

internal static class Reporter
{
    public static bool IsVerbose { get; set; }
    public static bool NoColor { get; set; }
    public static bool PrefixOutput { get; set; }

    [return: NotNullIfNotNull("value")]
    public static string? Colorize(string? value, Func<string?, string> colorizeFunc)
        => NoColor ? value : colorizeFunc(value);

    public static void WriteError(string? message)
        => WriteLine(Prefix("error:   ", Colorize(message, x => Bold + Red + x + Reset)));

    public static void WriteWarning(string? message)
        => WriteLine(Prefix("warn:    ", Colorize(message, x => Bold + Yellow + x + Reset)));

    public static void WriteInformation(string? message)
        => WriteLine(Prefix("info:    ", message));

    public static void WriteData(string? message)
        => WriteLine(Prefix("data:    ", Colorize(message, x => Bold + Gray + x + Reset)));

    public static void WriteVerbose(string? message)
    {
        if (IsVerbose)
        {
            WriteLine(Prefix("verbose: ", Colorize(message, x => Bold + Black + x + Reset)));
        }
    }

    private static string? Prefix(string prefix, string? value)
        => PrefixOutput
            ? value == null
                ? prefix
                : string.Join(
                    Environment.NewLine,
                    value.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Select(l => prefix + l))
            : value;

    private static void WriteLine(string? value)
    {
        if (NoColor)
        {
            Console.WriteLine(value);
        }
        else
        {
            AnsiConsole.WriteLine(value);
        }
    }
}
