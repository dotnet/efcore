// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal class DatabaseCommand : HelpCommandBase
{
    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.DatabaseDescription;

        command.Command("drop", new DatabaseDropCommand().Configure);
        command.Command("update", new DatabaseUpdateCommand().Configure);

        base.Configure(command);
    }
}
