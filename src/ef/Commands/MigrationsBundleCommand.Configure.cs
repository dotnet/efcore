// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    internal partial class MigrationsBundleCommand : ContextCommandBase
    {
        private CommandOption? _selfContained;
        private CommandOption? _runtime;
        private CommandOption? _configuration;

        public override void Configure(CommandLineApplication command)
        {
            command.Description = Resources.MigrationsBundleDescription;

            // TODO: --no-self-contained (after matching the startup project)
            _selfContained = command.Option("--self-contained", Resources.SelfContainedDescription);
            _runtime = command.Option("-r|--runtime <RUNTIME_IDENTIFIER>", Resources.RuntimeDescription);
            _configuration = command.Option("--configuration <CONFIGURATION>", Resources.ConfigurationDescription);

            base.Configure(command);
        }
    }
}
