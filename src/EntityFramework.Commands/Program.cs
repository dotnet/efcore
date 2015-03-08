// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if DNX451 || DNXCORE50

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Common.CommandLine;

namespace Microsoft.Data.Entity.Commands
{
    // TODO: Add verbose option
    public class Program
    {
        public static readonly string _defaultReverseEngineeringProviderAssembly = "EntityFramework.SqlServer.Design";

        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly ILibraryManager _libraryManager;
        private readonly MigrationTool _migrationTool;
        private readonly DatabaseTool _databaseTool;
        private CommandLineApplication _app;

        public Program([NotNull] IServiceProvider serviceProvider,
            [NotNull] IApplicationEnvironment appEnv, [NotNull] ILibraryManager libraryManager)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));
            Check.NotNull(appEnv, nameof(appEnv));
            Check.NotNull(libraryManager, nameof(libraryManager));

            _projectDir = appEnv.ApplicationBasePath;
            _rootNamespace = appEnv.ApplicationName;

            var loggerProvider = new LoggerProvider(name => new ConsoleCommandLogger(name, verbose: true));
            var assemblyName = new AssemblyName(appEnv.ApplicationName);
            var assembly = Assembly.Load(assemblyName);
            _migrationTool = new MigrationTool(loggerProvider, assembly);
            _databaseTool = new DatabaseTool(serviceProvider, loggerProvider);
            _libraryManager = libraryManager;
        }

        public virtual int Main([NotNull] string[] args)
        {
            Check.NotNull(args, nameof(args));

            // TODO: Enable subcommands in help
            _app = new CommandLineApplication { Name = "ef" };
            _app.VersionOption(
                "-v|--version",
                typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion);
            _app.HelpOption("-h|--help");
            _app.Command(
                "context",
                context =>
                {
                    context.Description = "Commands to manage your DbContext";
                    context.HelpOption("-h|--help");
                    context.Command(
                        "list",
                        list =>
                        {
                            list.Description = "List the contexts";
                            list.HelpOption("-h|--help");
                            list.OnExecute(() => ListContexts());
                        },
                        addHelpCommand: false);
                    context.OnExecute(() => ShowHelp(context.Name));
                },
                addHelpCommand: false);
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
                            var startupProject = add.Option(
                                "-s|--startupProjectName <projectName>",
                                "The name of the project to use as the startup project",
                                CommandOptionType.SingleValue);
                            add.HelpOption("-h|--help");
                            add.OnExecute(() => AddMigration(name.Value, context.Value(), startupProject.Value()));
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
                            var startupProject = apply.Option(
                                "-s|--startupProjectName <projectName>",
                                "The name of the project to use as the startup project",
                                CommandOptionType.SingleValue);
                            apply.HelpOption("-h|--help");
                            apply.OnExecute(() => ApplyMigration(migrationName.Value, context.Value(), startupProject.Value()));
                        },
                        addHelpCommand: false);
                    migration.Command(
                        "list",
                        list =>
                        {
                            list.Description = "List the migrations";
                            var context = list.Option(
                                "-c|--context <context>",
                                "The context class to use",
                                CommandOptionType.SingleValue);
                            list.HelpOption("-h|--help");
                            list.OnExecute(() => ListMigrations(context.Value()));
                        },
                        addHelpCommand: false);
                    migration.Command(
                        "script",
                        script =>
                        {
                            script.Description = "Generate a SQL script from migrations";
                            var from = script.Argument("[from]", "The starting migration");
                            var to = script.Argument("[to]", "The ending migration");
                            var output = script.Option(
                                "-o|--output <file>",
                                "The file to write the script to instead of stdout",
                                CommandOptionType.SingleValue);
                            var idempotent = script.Option(
                                "-i|--idempotent",
                                "Generate an idempotent script",
                                CommandOptionType.NoValue);
                            var context = script.Option(
                                "-c|--context <context>",
                                "The context class to use",
                                CommandOptionType.SingleValue);
                            var startupProject = script.Option(
                                "-s|--startupProjectName <projectName>",
                                "The name of the project to use as the startup project",
                                CommandOptionType.SingleValue);
                            script.HelpOption("-h|--help");
                            script.OnExecute(() => ScriptMigration(from.Value, to.Value, output.Value(), idempotent.HasValue(), context.Value(), startupProject.Value()));
                        },
                        addHelpCommand: false);
                    migration.Command(
                        "remove",
                        remove =>
                        {
                            remove.Description = "Remove the last migration";
                            var context = remove.Option(
                                "-c|--context <context>",
                                "The context class to use",
                                CommandOptionType.SingleValue);
                            remove.HelpOption("-h|--help");
                            remove.OnExecute(() => RemoveMigration(context.Value()));
                        },
                        addHelpCommand: false);
                    migration.OnExecute(() => ShowHelp(migration.Name));
                },
                addHelpCommand: false);
            _app.Command(
                "revEng",
                revEng =>
                {
                    revEng.Description = "Command to reverse engineer code from a database";
                    revEng.HelpOption("-h|--help");
                    var connectionString = revEng.Argument(
                            "[connectionString]",
                            "The connection string of the database");

                    revEng.OnExecute(() => ReverseEngineer(connectionString.Value));
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

        public virtual int ListContexts()
        {
            var contexts = _migrationTool.GetContextTypes();
            var any = false;
            foreach (var context in contexts)
            {
                // TODO: Show simple names
                Console.WriteLine(context.FullName);
                any = true;
            }

            if (!any)
            {
                Console.WriteLine("No DbContext was found.");
            }

            return 0;
        }

        public virtual int AddMigration([CanBeNull] string name, [CanBeNull] string context, [CanBeNull] string startupProject)
        {
            if (string.IsNullOrEmpty(name))
            {
                _app.ShowHelp("migration add");

                return 1;
            }

            return ExecuteInDirectory(
                startupProject,
                () =>
                {
                    _migrationTool.AddMigration(name, context, _rootNamespace, _projectDir);

                    return 0;
                });
        }

        public virtual int ApplyMigration([CanBeNull] string migration, [CanBeNull] string context, [CanBeNull] string startupProject)
        {
            return ExecuteInDirectory(
                startupProject,
                () =>
                {
                    _migrationTool.ApplyMigration(migration, context);

                    return 0;
                });
        }

        public virtual int ListMigrations([CanBeNull] string context)
        {
            var migrations = _migrationTool.GetMigrations(context);
            var any = false;
            foreach (var migration in migrations)
            {
                // TODO: Show simple names
                Console.WriteLine(migration.Id);
                any = true;
            }

            if (!any)
            {
                Console.WriteLine("No migrations were found.");
            }

            return 0;
        }

        public virtual int ScriptMigration(
            [CanBeNull] string from,
            [CanBeNull] string to,
            [CanBeNull] string output,
            bool idempotent,
            [CanBeNull] string context,
            [CanBeNull] string startupProject)
        {
            return ExecuteInDirectory(
                startupProject,
                () =>
                {
                    var sql = _migrationTool.ScriptMigration(from, to, idempotent, context);

                    if (string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine(sql);
                    }
                    else
                    {
                        File.WriteAllText(output, sql);
                    }

                    return 0;
                });
        }

        public virtual int RemoveMigration([CanBeNull] string context)
        {
            _migrationTool.RemoveMigration(context, _rootNamespace, _projectDir);

            return 0;
        }

        public virtual int ReverseEngineer([NotNull] string connectionString)
        {
            var providerAssembly = GetReverseEngineerProviderAssembly(_defaultReverseEngineeringProviderAssembly);
            if (providerAssembly == null)
            {
                Console.WriteLine("No provider assembly was found with name " + _defaultReverseEngineeringProviderAssembly);
                return 1;
            }

            _databaseTool.ReverseEngineer(providerAssembly, connectionString, _rootNamespace, _projectDir);

            return 0;
        }

        public virtual int ShowHelp([CanBeNull] string command)
        {
            // TODO: Enable multiple parameters in escape sequences
            AnsiConsole.Output.WriteLine(
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

        private int ExecuteInDirectory(string startupProject, Func<int> invoke)
        {
            var returnDirectory = Directory.GetCurrentDirectory();
            try
            {
                var startupProjectDir = GetProjectPath(startupProject);
                if (startupProjectDir != null)
                {
                    Console.WriteLine("Executing in startup Directory: {0}", startupProjectDir);
                    Directory.SetCurrentDirectory(startupProjectDir);
                }

                return invoke.Invoke();
            }
            finally
            {
                Directory.SetCurrentDirectory(returnDirectory);
            }
        }

        private string GetProjectPath(string projectName)
        {
            if (projectName == null)
            {
                return null;
            }

            string projectDir = null;
            var library = _libraryManager.GetLibraryInformation(projectName);
            var libraryPath = library.Path;
            if (library.Type == "Project")
            {
                projectDir = Path.GetDirectoryName(libraryPath);
            }

            return projectDir;
        }

        private Assembly GetReverseEngineerProviderAssembly(string providerAssemblyName)
        {
            return _libraryManager.GetReferencingLibraries("EntityFramework.Relational.Design")
                .Distinct()
                .Where(l => l.Name == providerAssemblyName)
                .SelectMany(l => l.LoadableAssemblies)
                .Select((assemblyName, assembly) => Assembly.Load(assemblyName))
                .FirstOrDefault();
        }
    }
}

#endif
