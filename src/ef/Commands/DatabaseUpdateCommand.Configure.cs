// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    internal partial class DatabaseUpdateCommand : ContextCommandBase
    {
        private CommandArgument _migration;

        public override void Configure(CommandLineApplication command)
        {
            command.Description = Resources.DatabaseUpdateDescription;

            _migration = command.Argument("<MIGRATION>", Resources.MigrationDescription);

            base.Configure(command);
        }
    }
}
