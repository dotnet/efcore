// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools;

internal static class Json
{
    public static CommandOption ConfigureOption(CommandLineApplication command)
        => command.Option("--json", Resources.JsonDescription);

    public static string Literal(string? text)
    {
        if (text == null)
        {
            return "null";
        }

        var builder = new StringBuilder("\"", text.Length + 2);
        foreach (var c in text)
        {
            switch (c)
            {
                case '\\': builder.Append("\\\\"); break;
                case '"':  builder.Append("\\\""); break;
                case '\b': builder.Append("\\b"); break;
                case '\f': builder.Append("\\f"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                default:
                    if (char.IsControl(c))
                    {
                        builder.Append($"\\u{(int)c:x4}");
                    }
                    else
                    {
                        builder.Append(c);
                    }
                    break;
            }
        }

        builder.Append('"');
        return builder.ToString();
    }

    public static string Literal(bool? value)
        => value.HasValue
            ? value.Value
                ? "true"
                : "false"
            : "null";
}
