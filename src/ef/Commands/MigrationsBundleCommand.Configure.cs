// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class MigrationsBundleCommand : ContextCommandBase
{
    private CommandOption? _output;
    private CommandOption? _force;
    private CommandOption? _selfContained;
    private CommandOption? _runtime;

    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.MigrationsBundleDescription;

        _output = command.Option("-o|--output <FILE>", Resources.MigrationsBundleOutputDescription);
        _force = command.Option("-f|--force", Resources.DbContextScaffoldForceDescription);
        _selfContained = command.Option("--self-contained", Resources.SelfContainedDescription);
        _runtime = command.Option("-r|--target-runtime <RUNTIME_IDENTIFIER>", Resources.MigrationsBundleRuntimeDescription);

        base.Configure(command);
    }
}
