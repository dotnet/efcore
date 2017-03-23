// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    internal class EnvironmentCommandBase : EFCommandBase
    {
        private CommandOption _environment;

        protected CommandOption Environment
            => _environment;

        public override void Configure(CommandLineApplication command)
        {
            _environment = command.Option("-e|--environment <NAME>", Resources.EnvironmentDescription);

            base.Configure(command);
        }
    }
}
