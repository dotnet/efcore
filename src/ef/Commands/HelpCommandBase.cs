// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal class HelpCommandBase : EFCommandBase
{
    private CommandLineApplication? _command;

    public override void Configure(CommandLineApplication command)
    {
        _command = command;

        base.Configure(command);
    }

    protected override int Execute(string[] args)
    {
        _command!.ShowHelp();

        return base.Execute(args);
    }
}
