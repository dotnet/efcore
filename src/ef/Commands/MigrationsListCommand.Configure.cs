// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    internal partial class MigrationsListCommand : ContextCommandBase
    {
        private CommandOption? _connection;
        private CommandOption? _noConnect;
        private CommandOption? _json;

        public override void Configure(CommandLineApplication command)
        {
            command.Description = Resources.MigrationsListDescription;

            _connection = command.Option("--connection <CONNECTION>", Resources.DbContextConnectionDescription);
            _noConnect = command.Option("--no-connect", Resources.NoConnectDescription);
            _json = Json.ConfigureOption(command);

            base.Configure(command);
        }
    }
}
