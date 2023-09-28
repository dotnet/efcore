// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class MigrationsAddCommand : ContextCommandBase
{
    private CommandArgument? _name;
    private CommandOption? _outputDir;
    private CommandOption? _json;
    private CommandOption? _namespace;

    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.MigrationsAddDescription;

        _name = command.Argument("<NAME>", Resources.MigrationNameDescription);

        _outputDir = command.Option("-o|--output-dir <PATH>", Resources.MigrationsOutputDirDescription);
        _json = Json.ConfigureOption(command);
        _namespace = command.Option("-n|--namespace <NAMESPACE>", Resources.MigrationsNamespaceDescription);

        base.Configure(command);
    }
}
