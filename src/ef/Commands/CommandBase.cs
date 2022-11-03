// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands;

internal abstract class CommandBase
{
    public virtual void Configure(CommandLineApplication command)
    {
        var verbose = command.Option("-v|--verbose", Resources.VerboseDescription);
        var noColor = command.Option("--no-color", Resources.NoColorDescription);
        var prefixOutput = command.Option("--prefix-output", Resources.PrefixDescription);

        command.HandleResponseFiles = true;

        command.OnExecute(
            args =>
            {
                Reporter.IsVerbose = verbose.HasValue();
                Reporter.NoColor = noColor.HasValue();
                Reporter.PrefixOutput = prefixOutput.HasValue();

                Validate();

                return Execute(args);
            });
    }

    protected virtual void Validate()
    {
    }

    protected virtual int Execute(string[] args)
        => 0;
}
