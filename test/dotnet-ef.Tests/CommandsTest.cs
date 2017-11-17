// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.Cli.CommandLine;
using Xunit;
using EFCommand = Microsoft.EntityFrameworkCore.Tools.Commands.RootCommand;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class CommandsTest
    {
        [Fact]
        public void Short_names_are_unique()
        {
            foreach (var command in GetCommands())
            {
                foreach (var group in command.Options.GroupBy(o => o.ShortName))
                {
                    Assert.True(
                        group.Key == null || group.Count() == 1,
                        "Duplicate short names on command '" + GetFullName(command) + "': " +
                        string.Join("; ", group.Select(o => o.Template)));
                }
            }
        }

        private static IEnumerable<CommandLineApplication> GetCommands()
        {
            var app = new CommandLineApplication
            {
                Name = "dotnet ef"
            };

            new EFCommand().Configure(app);

            return GetCommands(app);
        }

        private static IEnumerable<CommandLineApplication> GetCommands(CommandLineApplication command)
        {
            var commands = new Stack<CommandLineApplication>();
            commands.Push(command);

            while (commands.Count != 0)
            {
                command = commands.Pop();

                yield return command;

                foreach (var subcommand in command.Commands)
                {
                    commands.Push(subcommand);
                }
            }
        }

        private static string GetFullName(CommandLineApplication command)
        {
            var names = new Stack<string>();

            while (command != null)
            {
                names.Push(command.Name);

                command = command.Parent;
            }

            return string.Join(" ", names);
        }
    }
}
