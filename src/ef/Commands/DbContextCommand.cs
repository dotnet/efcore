// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal class DbContextCommand : HelpCommandBase
{
    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.DbContextDescription;

        command.Command("info", new DbContextInfoCommand().Configure);
        command.Command("list", new DbContextListCommand().Configure);
        command.Command("optimize", new DbContextOptimizeCommand().Configure);
        command.Command("scaffold", new DbContextScaffoldCommand().Configure);
        command.Command("script", new DbContextScriptCommand().Configure);

        base.Configure(command);
    }
}
