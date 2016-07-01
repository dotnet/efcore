// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Tools.Internal;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class DatabaseDropCommand : ICommand
    {
        public static void ParseOptions([NotNull] CommandLineApplication command, [NotNull] CommandLineOptions commonOptions)
        {
            command.Description = "Drop the database for specific environment";
            command.HelpOption();

            var context = command.Option(
                "-c|--context <context>",
                "The DbContext to use. If omitted, the default DbContext is used");
            var force = command.Option(
                "-f|--force",
                "Drop without confirmation");

            command.OnExecute(() => { commonOptions.Command = new DatabaseDropCommand(context.Value(), force.HasValue()); });
        }

        private readonly bool _force;
        private readonly string _context;

        public DatabaseDropCommand(string context, bool force)
        {
            _context = context;
            _force = force;
        }

        public void Run(IOperationExecutor executor)
        {
            if (!_force)
            {
                var result = executor.GetDatabase(_context);
                if (result == null)
                {
                    Reporter.Output("Could not find database to drop");
                    return;
                }

                Reporter.Output(
                    $"Are you sure you want to drop the database '{result["DatabaseName"]}' on server '{result["DataSource"]}'? (y/N)");
                var readedKey = Console.ReadKey().KeyChar;
                var confirmed = (readedKey == 'y') || (readedKey == 'Y');
                if (!confirmed)
                {
                    Reporter.Output("Cancelled");
                    return;
                }
            }

            executor.DropDatabase(_context);
        }
    }
}
