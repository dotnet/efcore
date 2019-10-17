// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    internal partial class DbContextInfoCommand : ContextCommandBase
    {
        private CommandOption _json;

        public override void Configure(CommandLineApplication command)
        {
            command.Description = Resources.DbContextInfoDescription;

            _json = Json.ConfigureOption(command);

            base.Configure(command);
        }
    }
}
