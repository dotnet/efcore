// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Tools.Internal;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class MigrationsRemoveCommand : ICommand
    {
        public static void ParseOptions([NotNull] CommandLineApplication command, [NotNull] CommandLineOptions options)
        {
            command.Description = "Remove the last migration";
            command.HelpOption();

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var force = command.Option(
                "-f|--force",
                "Removes the last migration without checking the database. If the last migration has been applied to the database, you will need to manually reverse the changes it made.");

            command.OnExecute(() => { options.Command = new MigrationsRemoveCommand(context.Value(), force.HasValue()); });
        }

        private readonly string _context;
        private readonly bool _force;

        public MigrationsRemoveCommand(string context, bool force)
        {
            _context = context;
            _force = force;
        }

        public void Run(IOperationExecutor executor)
            => executor.RemoveMigration(_context, _force);
    }
}
