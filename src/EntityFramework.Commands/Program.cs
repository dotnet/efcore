// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50 || ASPNETCORE50

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Common.CommandLine;

namespace Microsoft.Data.Entity.Commands
{
    public class Program
    {
        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly MigrationTool _migrationTool;
        private CommandLineApplication _app;

        public Program([NotNull] IApplicationEnvironment appEnv)
        {
            Check.NotNull(appEnv, "appEnv");

            _projectDir = appEnv.ApplicationBasePath;
            _rootNamespace = appEnv.ApplicationName;

            var assemblyName = new AssemblyName(appEnv.ApplicationName);
            var assembly = Assembly.Load(assemblyName);
            _migrationTool = new MigrationTool(assembly);
        }

        public virtual int Main([NotNull] string[] args)
        {
            Check.NotNull(args, "args");

            // TODO: Enable subcommands in help
            _app = new CommandLineApplication { Name = "ef" };
            _app.VersionOption(
                "-v|--version",
                typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion);
            _app.HelpOption("-h|--help");
            _app.Command(
                "migration",
                migration =>
                {
                    migration.Description = "Commands to manage your Code First Migrations";
                    migration.HelpOption("-h|--help");
                    migration.Command(
                        "add",
                        add =>
                        {
                            add.Description = "Add a new migration";
                            var name = add.Argument("[name]", "The name of the migration");
                            var context = add.Option(
                                "-c|--context <context>",
                                "The context class to use",
                                CommandOptionType.SingleValue);
                            add.HelpOption("-h|--help");
                            add.OnExecute(() => AddMigration(name.Value, context.Value()));
                        },
                        addHelpCommand: false);
                    migration.Command(
                        "apply",
                        apply =>
                        {
                            apply.Description = "Apply migrations to the database";
                            var migrationName = apply.Argument("[migration]", "The migration to apply");
                            var context = apply.Option(
                                "-c|--context <context>",
                                "The context class to use",
                                CommandOptionType.SingleValue);
                            apply.HelpOption("-h|--help");
                            apply.OnExecute(() => UpdateDatabase(migrationName.Value, context.Value()));
                        },
                        addHelpCommand: false);
                },
                addHelpCommand: false);
            _app.Command(
                "help",
                help =>
                {
                    help.Description = "Show help information";
                    var command = help.Argument("[command]", "Command that help information explains");
                    help.OnExecute(() => ShowHelp(command.Value));
                },
                addHelpCommand: false);
            _app.OnExecute(() => ShowHelp(command: null));

            return _app.Execute(args);
        }

        public virtual int AddMigration([NotNull] string name, [CanBeNull] string context)
        {
            Check.NotEmpty(name, "name");

            var migration = _migrationTool.AddMigration(name, _rootNamespace, context);
            _migrationTool.WriteMigration(_projectDir, migration).ToArray();

            return 0;
        }

        public virtual int ApplyMigration([CanBeNull] string migration, [CanBeNull] string context)
        {
            _migrationTool.ApplyMigration(migration, context);

            return 0;
        }

        public virtual int ShowHelp([CanBeNull] string command)
        {
            // TODO: Enable multiple parameters in escape sequences
            AnsiConsole.WriteLine(
                "\x1b[1m\x1b[37m" + Environment.NewLine +
                "                     _/\\__" + Environment.NewLine +
                "               ---==/    \\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m ___  ___ \x1b[1m\x1b[37m  |\x1b[22m.\x1b[1m    \\|\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m| __|| __|\x1b[1m\x1b[37m  |  )   \\\\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m| _| | _| \x1b[1m\x1b[37m  \\_/ |  //|\\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m|___||_|  \x1b[1m\x1b[37m     /   \\\\\\/\\\\" + Environment.NewLine +
                "\x1b[22m\x1b[39m");
            _app.ShowHelp(command);

            return 0;
        }
    }
}

#endif
