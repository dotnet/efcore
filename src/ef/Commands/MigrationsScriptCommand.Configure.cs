// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    internal partial class MigrationsScriptCommand : ContextCommandBase
    {
        private CommandArgument _from;
        private CommandArgument _to;
        private CommandOption _output;
        private CommandOption _idempotent;

        public override void Configure(CommandLineApplication command)
        {
            command.Description = Resources.MigrationsScriptDescription;

            _from = command.Argument("<FROM>", Resources.MigrationFromDescription);
            _to = command.Argument("<TO>", Resources.MigrationToDescription);

            _output = command.Option("-o|--output <FILE>", Resources.OutputDescription);
            _idempotent = command.Option("-i|--idempotent", Resources.IdempotentDescription);

            base.Configure(command);
        }
    }
}
