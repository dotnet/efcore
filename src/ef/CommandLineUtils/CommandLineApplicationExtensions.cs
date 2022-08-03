// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.DotNet.Cli.CommandLine;

internal static class CommandLineApplicationExtensions
{
    public static CommandOption Option(this CommandLineApplication command, string template, string? description)
        => command.Option(
            template,
            description,
            template.IndexOf('<') != -1
                ? template.EndsWith(">...", StringComparison.Ordinal)
                    ? CommandOptionType.MultipleValue
                    : CommandOptionType.SingleValue
                : CommandOptionType.NoValue);
}
