// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Tools.Internal;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class DatabaseUpdateCommand : ICommand
    {
        public static void ParseOptions([NotNull] CommandLineApplication command, [NotNull] CommandLineOptions commonOptions)
        {
            command.Description = "Updates the database to a specified migration";
            command.HelpOption();

            var migration = command.Argument(
                "[migration]",
                "The target migration. If '0', all migrations will be reverted. If omitted, all pending migrations will be applied");

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");

            command.OnExecute(() => { commonOptions.Command = new DatabaseUpdateCommand(migration.Value, context.Value()); });
        }

        private readonly string _targetMigration;
        private readonly string _contextType;

        public DatabaseUpdateCommand(string targetMigration, string contextType)
        {
            _targetMigration = targetMigration;
            _contextType = contextType;
        }

        public void Run(IOperationExecutor executor)
            => executor.UpdateDatabase(_targetMigration, _contextType);
    }
}
