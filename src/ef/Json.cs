// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools;

internal static class Json
{
    public static CommandOption ConfigureOption(CommandLineApplication command)
        => command.Option("--json", Resources.JsonDescription);

    public static string Literal(string? text)
        => text != null
            ? "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""
            : "null";

    public static string Literal(bool? value)
        => value.HasValue
            ? value.Value
                ? "true"
                : "false"
            : "null";
}
