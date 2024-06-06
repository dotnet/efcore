// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal partial class DbContextOptimizeCommand : ContextCommandBase
{
    private CommandOption? _outputDir;
    private CommandOption? _namespace;
    private CommandOption? _suffix;
    private CommandOption? _noScaffold;
    private CommandOption? _precompileQueries;

    public override void Configure(CommandLineApplication command)
    {
        command.Description = Resources.DbContextOptimizeDescription;

        _outputDir = command.Option("-o|--output-dir <PATH>", Resources.OutputDirDescription);
        _namespace = command.Option("-n|--namespace <NAMESPACE>", Resources.NamespaceDescription);
        _suffix = command.Option("--suffix <SUFFIX>", Resources.SuffixDescription);
        _noScaffold = command.Option("--no-scaffold", Resources.NoScaffoldDescription);
        _precompileQueries = command.Option("--precompile-queries", Resources.PrecompileQueriesDescription);

        base.Configure(command);
    }
}
