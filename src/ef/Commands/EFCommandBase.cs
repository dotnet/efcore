// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal abstract class EFCommandBase : CommandBase
{
    public override void Configure(CommandLineApplication command)
    {
        command.HelpOption("-h|--help");

        base.Configure(command);
    }
}
