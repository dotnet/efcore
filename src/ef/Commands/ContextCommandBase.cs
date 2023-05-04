// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal class ContextCommandBase : ProjectCommandBase
{
    protected CommandOption? Context { get; private set; }

    public override void Configure(CommandLineApplication command)
    {
        Context = command.Option("-c|--context <DBCONTEXT>", Resources.ContextDescription);

        base.Configure(command);
    }
}
