// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.DotNet.Cli.CommandLine;

internal class CommandParsingException(CommandLineApplication command, string message) : Exception(message)
{
    public CommandLineApplication Command { get; } = command;
}
