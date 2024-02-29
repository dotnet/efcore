// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using static Microsoft.EntityFrameworkCore.Tools.AnsiConstants;

namespace Microsoft.EntityFrameworkCore.Tools;

internal static class Reporter
{
    public const string ErrorPrefix = "error:   ";
    public const string WarningPrefix = "warn:    ";
    public const string InfoPrefix = "info:    ";
    public const string DataPrefix = "data:    ";
    public const string VerbosePrefix = "verbose: ";

    public static bool IsVerbose { get; set; }
    public static bool NoColor { get; set; }
    public static bool PrefixOutput { get; set; }

    [return: NotNullIfNotNull("value")]
    public static string? Colorize(string? value, Func<string?, string> colorizeFunc)
        => NoColor ? value : colorizeFunc(value);

    public static void WriteError(string? message)
        => WriteLine(Prefix(ErrorPrefix, Colorize(message, x => Bold + Red + x + Reset)));

    public static void WriteWarning(string? message)
        => WriteLine(Prefix(WarningPrefix, Colorize(message, x => Bold + Yellow + x + Reset)));

    public static void WriteInformation(string? message)
        => WriteLine(Prefix(InfoPrefix, message));

    public static void WriteData(string? message)
        => WriteLine(Prefix(DataPrefix, Colorize(message, x => Bold + Gray + x + Reset)));

    public static void WriteVerbose(string? message)
    {
        if (IsVerbose)
        {
            WriteLine(Prefix(VerbosePrefix, Colorize(message, x => Bold + Black + x + Reset)));
        }
    }

    private static string? Prefix(string prefix, string? value)
        => PrefixOutput
            ? value == null
                ? prefix
                : string.Join(
                    Environment.NewLine,
                    value.Split([Environment.NewLine], StringSplitOptions.None).Select(l => prefix + l))
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
