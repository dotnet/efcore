// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
