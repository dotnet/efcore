// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;
using EFCommand = Microsoft.EntityFrameworkCore.Tools.Commands.RootCommand;

namespace Microsoft.EntityFrameworkCore.Tools;

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
                    "Duplicate short names on command '"
                    + GetFullName(command)
                    + "': "
                    + string.Join("; ", group.Select(o => o.Template)));
            }
        }
    }

    [Fact]
    public void Long_names_are_unique()
    {
        foreach (var command in GetCommands())
        {
            foreach (var group in command.Options.GroupBy(o => o.LongName))
            {
                Assert.True(
                    group.Key == null || group.Count() == 1,
                    "Duplicate option names on command '"
                    + GetFullName(command)
                    + "': "
                    + string.Join("; ", group.Select(o => o.Template)));
            }
        }
    }

    private static IEnumerable<CommandLineApplication> GetCommands()
    {
        var app = new CommandLineApplication { Name = "dotnet ef" };

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

        var c = command;

        while (c != null)
        {
            names.Push(c.Name!);

            c = c.Parent;
        }

        return string.Join(" ", names);
    }
}
