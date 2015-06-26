// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if DNX451 || DNXCORE50

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Common.CommandLine;

namespace Microsoft.Data.Entity.Commands
{
    // TODO: Add verbose option
    public class Program
    {
        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly ILibraryManager _libraryManager;
        private readonly MigrationTool _migrationTool;
        private readonly DatabaseTool _databaseTool;
        private readonly IRuntimeEnvironment _runtimeEnv;
        private readonly IApplicationShutdown _applicationShutdown;
        private CommandLineApplication _app;
        private readonly ILogger _logger;

        public Program([NotNull] IServiceProvider serviceProvider,
            [NotNull] IApplicationEnvironment appEnv, [NotNull] ILibraryManager libraryManager,
            [NotNull] IRuntimeEnvironment runtimeEnv, [NotNull] IApplicationShutdown applicationShutdown)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));
            Check.NotNull(appEnv, nameof(appEnv));
            Check.NotNull(libraryManager, nameof(libraryManager));
            Check.NotNull(runtimeEnv, nameof(runtimeEnv));
            Check.NotNull(applicationShutdown, nameof(applicationShutdown));

            _runtimeEnv = runtimeEnv;
            _projectDir = appEnv.ApplicationBasePath;
            _rootNamespace = appEnv.ApplicationName;
            _applicationShutdown = applicationShutdown;

            var loggerProvider = new LoggerProvider(name => new ConsoleCommandLogger(name, verbose: true));
            var assemblyName = new AssemblyName(appEnv.ApplicationName);
            var assembly = Assembly.Load(assemblyName);
            _migrationTool = new MigrationTool(loggerProvider, assembly, serviceProvider);
            _databaseTool = new DatabaseTool(serviceProvider, loggerProvider);
            _libraryManager = libraryManager;
            _logger = loggerProvider.CreateLogger(typeof(Program).FullName);
        }

        public virtual int Main([NotNull] string[] args)
        {
            Check.NotNull(args, nameof(args));

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                _applicationShutdown.RequestShutdown();
            };

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
                        });
                    context.OnExecute(
                        () =>
                        {
                            _app.ShowHelp(context.Name);

                            return 0;
                        });
                });
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
                                "-s|--startupProject <startupProject>",
                                "The start-up project to use",
                                CommandOptionType.SingleValue);
                            add.HelpOption("-h|--help");
                            add.OnExecute(
                                () =>
                                {
                                    if (name.Value == null)
                                    {
                                        _logger.LogError("Missing required argument '{0}'.", name.Name);

                                        migration.ShowHelp(add.Name);

                                        return 1;
                                    }

                                    return AddMigration(name.Value, context.Value(), startupProject.Value());
                                });
                        });
                    migration.Command(
                        "apply",
                        apply =>
                        {
                            apply.Description = "Apply migrations to the database";
                            var migrationName = apply.Argument(
                                "[migration]",
                                "The migration to apply. Use '0' to unapply all migrations");
                            var context = apply.Option(
                                "-c|--context <context>",
                                "The context class to use",
                                CommandOptionType.SingleValue);
                            var startupProject = apply.Option(
                                "-s|--startupProject <startupProject>",
                                "The start-up project to use",
                                CommandOptionType.SingleValue);
                            apply.HelpOption("-h|--help");
                            apply.OnExecute(() => ApplyMigration(migrationName.Value, context.Value(), startupProject.Value()));
                        });
                    migration.Command(
                        "list",
                        list =>
                        {
                            list.Description = "List the migrations";
                            var context = list.Option(
                                "-c|--context <context>",
                                "The context class to use",
                                CommandOptionType.SingleValue);
                            var startupProject = list.Option(
                                "-s|--startupProject <startupProject>",
                                "The start-up project to use",
                                CommandOptionType.SingleValue);
                            list.HelpOption("-h|--help");
                            list.OnExecute(() => ListMigrations(context.Value(), startupProject.Value()));
                        });
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
                                "-s|--startupProject <startupProject>",
                                "The start-up project to use",
                                CommandOptionType.SingleValue);
                            script.HelpOption("-h|--help");
                            script.OnExecute(() => ScriptMigration(from.Value, to.Value, output.Value(), idempotent.HasValue(), context.Value(), startupProject.Value()));
                        });
                    migration.Command(
                        "remove",
                        remove =>
                        {
                            remove.Description = "Remove the last migration";
                            var context = remove.Option(
                                "-c|--context <context>",
                                "The context class to use",
                                CommandOptionType.SingleValue);
                            var startupProject = remove.Option(
                                "-s|--startupProject <startupProject>",
                                "The start-up project to use",
                                CommandOptionType.SingleValue);
                            remove.HelpOption("-h|--help");
                            remove.OnExecute(() => RemoveMigration(context.Value(), startupProject.Value()));
                        });
                    migration.OnExecute(
                        () =>
                        {
                            _app.ShowHelp(migration.Name);

                            return 0;
                        });
                });
            _app.Command(
                "revEng",
                revEng =>
                {
                    revEng.Description = "Command to reverse engineer code from a database";
                    revEng.HelpOption("-h|--help");
                    var connectionString = revEng.Argument(
                            "[connectionString]",
                            "The connection string of the database");

                    revEng.OnExecute(() => ReverseEngineerAsync(
                                connectionString.Value, _applicationShutdown.ShutdownRequested));
                });
            _app.Command(
                "help",
                help =>
                {
                    help.Description = "Show help information";
                    var command = help.Argument("[command]", "Command that help information explains");
                    help.OnExecute(
                        () =>
                        {
                            if (command.Value != null)
                            {
                                _app.ShowHelp(command.Value);

                                return 0;
                            }

                            return ShowHelp();
                        });
                });
            _app.OnExecute(() => ShowHelp());

            return _app.Execute(args);
        }

        public virtual int ListContexts()
        {
            var contexts = _migrationTool.GetContextTypes();
            var any = false;
            foreach (var context in contexts)
            {
                // TODO: Show simple names
                _logger.LogInformation(context.FullName);
                any = true;
            }

            if (!any)
            {
                _logger.LogInformation("No DbContext was found.");
            }

            return 0;
        }

        public virtual int AddMigration([CanBeNull] string name, [CanBeNull] string context, [CanBeNull] string startupProject)
        {
            return Execute(
                startupProject,
                () =>
                {
                    _migrationTool.AddMigration(name, context, startupProject, _rootNamespace, _projectDir);

                    _logger.LogInformation("Done. To undo this action, use 'ef migration remove'.");

                    return 0;
                });
        }

        public virtual int ApplyMigration([CanBeNull] string migration, [CanBeNull] string context, [CanBeNull] string startupProject)
        {
            return Execute(
                startupProject,
                () =>
                {
                    _migrationTool.ApplyMigration(migration, context, startupProject);

                    return 0;
                });
        }

        public virtual int ListMigrations([CanBeNull] string context, [CanBeNull] string startupProject)
        {
            return Execute(
                startupProject,
                () =>
                {
                    var migrations = _migrationTool.GetMigrations(context, startupProject);
                    var any = false;
                    foreach (var migration in migrations)
                    {
                        // TODO: Show simple names
                        _logger.LogInformation(migration.Id);
                        any = true;
                    }

                    if (!any)
                    {
                        _logger.LogInformation("No migrations were found.");
                    }

                    return 0;
                });
        }

        public virtual int ScriptMigration(
            [CanBeNull] string from,
            [CanBeNull] string to,
            [CanBeNull] string output,
            bool idempotent,
            [CanBeNull] string context,
            [CanBeNull] string startupProject)
        {
            return Execute(
                startupProject,
                () =>
                {
                    var sql = _migrationTool.ScriptMigration(from, to, idempotent, context, startupProject);

                    if (string.IsNullOrEmpty(output))
                    {
                        _logger.LogInformation(sql);
                    }
                    else
                    {
                        _logger.LogVerbose("Writing SQL script to '{0}'.", output);
                        File.WriteAllText(output, sql);

                        _logger.LogInformation("Done.");
                    }

                    return 0;
                });
        }

        public virtual int RemoveMigration([CanBeNull] string context, [CanBeNull] string startupProject)
        {
            return Execute(
                startupProject,
                () =>
                {
                    _migrationTool.RemoveMigration(context, startupProject, _rootNamespace, _projectDir);

                    return 0;
                });
        }

        public virtual async Task<int> ReverseEngineerAsync(
            [NotNull] string connectionString,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _databaseTool.ReverseEngineerAsync(
                DatabaseTool._defaultReverseEngineeringProviderAssembly,
                connectionString, _rootNamespace, _projectDir);

            _logger.LogInformation("Done.");

            return 0;
        }

        public virtual int ShowHelp()
        {
            var useConsoleColors = _runtimeEnv.OperatingSystem == "Windows";
            // TODO: Enable multiple parameters in escape sequences
            AnsiConsole.GetOutput(useConsoleColors).WriteLine(
                "\x1b[1m\x1b[37m" + Environment.NewLine +
                "                     _/\\__" + Environment.NewLine +
                "               ---==/    \\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m ___  ___ \x1b[1m\x1b[37m  |\x1b[22m.\x1b[1m    \\|\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m| __|| __|\x1b[1m\x1b[37m  |  )   \\\\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m| _| | _| \x1b[1m\x1b[37m  \\_/ |  //|\\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m|___||_|  \x1b[1m\x1b[37m     /   \\\\\\/\\\\" + Environment.NewLine +
                "\x1b[22m\x1b[39m");
            _app.ShowHelp();

            return 0;
        }

        private int Execute(string startupProject, Func<int> invoke)
        {
            var returnDirectory = Directory.GetCurrentDirectory();
            try
            {
                UseStartupProject(startupProject);

                return invoke();
            }
            finally
            {
                Directory.SetCurrentDirectory(returnDirectory);
            }
        }

        private void UseStartupProject(string startupProject)
        {
            if (startupProject == null)
            {
                return;
            }

            var library = _libraryManager.GetLibraryInformation(startupProject);
            if (library == null || library.Type != "Project")
            {
                _logger.LogVerbose("Unable to resolve start-up project '{0}'.", startupProject);

                return;
            }

            _logger.LogVerbose("Using start-up project '{0}'.", library.Name);

            var startupProjectDir = Path.GetDirectoryName(library.Path);
            _logger.LogVerbose("Using current directory '{0}'.", startupProjectDir);
            Directory.SetCurrentDirectory(startupProjectDir);
        }
    }
}

#endif
