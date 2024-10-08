// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Microsoft.EntityFrameworkCore.Tools;

internal class AnsiTextWriter(TextWriter writer)
{
    public void WriteLine(string? text)
    {
        if (text != null)
        {
            Interpret(text);
        }

        writer.Write(Environment.NewLine);
    }

    private void Interpret(string value)
    {
        var matches = Regex.Matches(value, "\x1b\\[([0-9]+)?m", RegexOptions.None, TimeSpan.FromSeconds(10));

        var start = 0;
        foreach (var match in matches.Cast<Match>())
        {
            var length = match.Index - start;
            if (length != 0)
            {
                writer.Write(value.Substring(start, length));
            }

            Apply(match.Groups[1].Value);

            start = match.Index + match.Length;
        }

        if (start != value.Length)
        {
            writer.Write(value.Substring(start));
        }
    }

    private static void Apply(string parameter)
    {
        switch (parameter)
        {
            case "1":
                ApplyBold();
                break;

            case "22":
                ResetBold();
                break;

            case "30":
                ApplyColor(ConsoleColor.Black);
                break;

            case "31":
                ApplyColor(ConsoleColor.DarkRed);
                break;

            case "32":
                ApplyColor(ConsoleColor.DarkGreen);
                break;

            case "33":
                ApplyColor(ConsoleColor.DarkYellow);
                break;

            case "34":
                ApplyColor(ConsoleColor.DarkBlue);
                break;

            case "35":
                ApplyColor(ConsoleColor.DarkMagenta);
                break;

            case "36":
                ApplyColor(ConsoleColor.DarkCyan);
                break;

            case "37":
                ApplyColor(ConsoleColor.Gray);
                break;

            case "39":
                ResetColor();
                break;

            default:
                Debug.Fail("Unsupported parameter: " + parameter);
                break;
        }
    }

    private static void ApplyBold()
        => Console.ForegroundColor = (ConsoleColor)((int)Console.ForegroundColor | 8);

    private static void ResetBold()
        => Console.ForegroundColor = (ConsoleColor)((int)Console.ForegroundColor & 7);

    private static void ApplyColor(ConsoleColor color)
    {
        var wasBold = ((int)Console.ForegroundColor & 8) != 0;

        Console.ForegroundColor = color;

        if (wasBold)
        {
            ApplyBold();
        }
    }

    private static void ResetColor()
    {
        var wasBold = ((int)Console.ForegroundColor & 8) != 0;

        Console.ResetColor();

        if (wasBold)
        {
            ApplyBold();
        }
    }
}
