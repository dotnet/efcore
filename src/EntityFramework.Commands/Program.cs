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
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Common.CommandLine;

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

            var app = new CommandLineApplication
            {
                Name = "ef",
                FullName = "Entity Framework Commands"
            };
            app.VersionOption(
                "--version",
                typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion);
            app.HelpOption("-?|-h|--help");
            app.OnExecute(
                () =>
                {
                    ShowLogo();
                    app.ShowHelp();
                });
            app.Command(
                "database",
                database =>
                {
                    database.Description = "Commands to manage your database";
                    database.HelpOption("-?|-h|--help");
                    database.OnExecute(() => database.ShowHelp());
                    database.Command(
                        "update",
                        update =>
                        {
                            update.Description = "Updates the database to a specified migration";
                            var migrationName = update.Argument(
                                "[migration]",
                                "the target migration. If '0', all migrations will be reverted. If omitted, all pending migrations will be applied");
                            var context = update.Option(
                                "-c|--context <context>",
                                "The DbContext to use. If omitted, the default DbContext is used");
                            var startupProject = update.Option(
                                "-s|--startupProject <project>",
                                "The start-up project to use. If omitted, the current project is used");
                            update.HelpOption("-?|-h|--help");
                            update.OnExecute(() => ApplyMigration(migrationName.Value, context.Value(), startupProject.Value()));
                        });
                });
            app.Command(
                "dbcontext",
                dbcontext =>
                {
                    dbcontext.Description = "Commands to manage your DbContext types";
                    dbcontext.HelpOption("-?|-h|--help");
                    dbcontext.OnExecute(() => dbcontext.ShowHelp());
                    dbcontext.Command(
                        "list",
                        list =>
                        {
                            list.Description = "List your DbContext types";
                            list.HelpOption("-?|-h|--help");
                            list.OnExecute(() => ListContexts());
                        });
                    dbcontext.Command(
                        "scaffold",
                        scaffold =>
                        {
                            scaffold.Description = "Scaffolds a DbContext and entity type classes for a specified database";
                            var connection = scaffold.Argument(
                                "[connection]",
                                "The connection string of the database");
                            var provider = scaffold.Argument(
                                "[provider]",
                                "The provider to use. For example, EntityFramework.SqlServer");
                            scaffold.HelpOption("-?|-h|--help");
                            scaffold.OnExecute(
                                async () =>
                                {
                                    if (string.IsNullOrEmpty(connection.Value))
                                    {
                                        _logger.LogError("Missing required argument '{0}'", connection.Name);

                                        scaffold.ShowHelp();

                                        return 1;
                                    }
                                    if (string.IsNullOrEmpty(provider.Value))
                                    {
                                        _logger.LogError("Missing required argument '{0}'", provider.Name);

                                        return 1;
                                    }

                                    await ReverseEngineerAsync(
                                        connection.Value,
                                        provider.Value,
                                        _applicationShutdown.ShutdownRequested);

                                    return 0;
                                });
                        });
                    dbcontext.Command(
                        "scaffold-templates",
                        scaffoldTemplates =>
                        {
                            scaffoldTemplates.Description = "Scaffolds customizable DbContext and entity type templates to use during 'ef dbcontext scaffold'";
                            var provider = scaffoldTemplates.Argument(
                                "[provider]",
                                "The provider to use. For example, EntityFramework.SqlServer");
                            scaffoldTemplates.HelpOption("-?|-h|--help");
                            scaffoldTemplates.OnExecute(
                                () =>
                                {
                                    if (string.IsNullOrEmpty(provider.Value))
                                    {
                                        _logger.LogError("Missing required argument '{0}'", provider.Name);

                                        scaffoldTemplates.ShowHelp();

                                        return 1;
                                    }

                                    CustomizeReverseEngineer(provider.Value);

                                    return 0;
                                });
                        });
                });
            app.Command(
                "migrations",
                migration =>
                {
                    migration.Description = "Commands to manage your migrations";
                    migration.HelpOption("-?|-h|--help");
                    migration.OnExecute(() => migration.ShowHelp());
                    migration.Command(
                        "add",
                        add =>
                        {
                            add.Description = "Add a new migration";
                            var name = add.Argument(
                                "[name]",
                                "The name of the migration");
                            var context = add.Option(
                                "-c|--context <context>",
                                "The DbContext to use. If omitted, the default DbContext is used");
                            var startupProject = add.Option(
                                "-s|--startupProject <project>",
                                "The start-up project to use. If omitted, the current project is used");
                            add.HelpOption("-?|-h|--help");
                            add.OnExecute(
                                () =>
                                {
                                    if (string.IsNullOrEmpty(name.Value))
                                    {
                                        _logger.LogError("Missing required argument '{0}'", name.Name);

                                        add.ShowHelp();

                                        return 1;
                                    }

                                    AddMigration(name.Value, context.Value(), startupProject.Value());

                                    return 0;
                                });
                        });
                    migration.Command(
                        "list",
                        list =>
                        {
                            list.Description = "List the migrations";
                            var context = list.Option(
                                "-c|--context <context>",
                                "The DbContext to use. If omitted, the default DbContext is used");
                            var startupProject = list.Option(
                                "-s|--startupProject <project>",
                                "The start-up project to use. If omitted, the current project is used");
                            list.HelpOption("-?|-h|--help");
                            list.OnExecute(() => ListMigrations(context.Value(), startupProject.Value()));
                        });
                    migration.Command(
                        "remove",
                        remove =>
                        {
                            remove.Description = "Remove the last migration";
                            var context = remove.Option(
                                "-c|--context <context>",
                                "The DbContext to use. If omitted, the default DbContext is used");
                            var startupProject = remove.Option(
                                "-s|--startupProject <project>",
                                "The start-up project to use. If omitted, the current project is used");
                            remove.HelpOption("-?|-h|--help");
                            remove.OnExecute(() => RemoveMigration(context.Value(), startupProject.Value()));
                        });
                    migration.Command(
                        "script",
                        script =>
                        {
                            script.Description = "Generate a SQL script from migrations";
                            var from = script.Argument(
                                "[from]",
                                "The starting migration. If omitted, '0' (the initial database) is used");
                            var to = script.Argument(
                                "[to]",
                                "The ending migration. If omitted, the last migration is used");
                            var output = script.Option(
                                "-o|--output <file>",
                                "The file to write the script to instead of stdout");
                            var idempotent = script.Option(
                                "-i|--idempotent",
                                "Generates an idempotent script that can used on a database at any migration");
                            var context = script.Option(
                                "-c|--context <context>",
                                "The DbContext to use. If omitted, the default DbContext is used");
                            var startupProject = script.Option(
                                "-s|--startupProject <project>",
                                "The start-up project to use. If omitted, the current project is used");
                            script.HelpOption("-?|-h|--help");
                            script.OnExecute(
                                () =>
                                {
                                    if (!string.IsNullOrEmpty(to.Value) && string.IsNullOrEmpty(from.Value))
                                    {
                                        _logger.LogError("Missing required argument '{0}'", from.Name);

                                        return 1;
                                    }

                                    ScriptMigration(
                                        from.Value,
                                        to.Value,
                                        output.Value(),
                                        idempotent.HasValue(),
                                        context.Value(),
                                        startupProject.Value());

                                    return 0;
                                });
                        });
                });

            return app.Execute(args);
        }

        public virtual void ListContexts()
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
                _logger.LogInformation("No DbContext was found");
            }
        }

        public virtual void AddMigration(
            [CanBeNull] string name,
            [CanBeNull] string context,
            [CanBeNull] string startupProject)
        {
            Execute(
                startupProject,
                () =>
                {
                    _migrationTool.AddMigration(name, context, startupProject, _rootNamespace, _projectDir);

                    _logger.LogInformation("Done. To undo this action, use 'ef migrations remove'");
                });
        }

        public virtual void ApplyMigration(
            [CanBeNull] string migration,
            [CanBeNull] string context,
            [CanBeNull] string startupProject)
        {
            Execute(
                startupProject,
                () =>
                {
                    _migrationTool.ApplyMigration(migration, context, startupProject);
                });
        }

        public virtual void ListMigrations([CanBeNull] string context, [CanBeNull] string startupProject)
        {
            Execute(
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
                        _logger.LogInformation("No migrations were found");
                    }
                });
        }

        public virtual void ScriptMigration(
            [CanBeNull] string from,
            [CanBeNull] string to,
            [CanBeNull] string output,
            bool idempotent,
            [CanBeNull] string context,
            [CanBeNull] string startupProject)
        {
            Execute(
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
                        _logger.LogVerbose("Writing SQL script to '{0}'", output);
                        File.WriteAllText(output, sql);

                        _logger.LogInformation("Done");
                    }
                });
        }

        public virtual void RemoveMigration([CanBeNull] string context, [CanBeNull] string startupProject)
        {
            Execute(
                startupProject,
                () => _migrationTool.RemoveMigration(context, startupProject, _rootNamespace, _projectDir));
        }

        public virtual async Task ReverseEngineerAsync(
            [NotNull] string connectionString,
            [NotNull] string providerAssemblyName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _databaseTool.ReverseEngineerAsync(
                providerAssemblyName, connectionString, _rootNamespace, _projectDir);

            _logger.LogInformation("Done");
        }

        public virtual void CustomizeReverseEngineer([NotNull] string providerAssemblyName)
        {
            _logger.LogVerbose("Writing Reverse Engineering templates to '{0}'", _projectDir);

            _databaseTool.CustomizeReverseEngineer(providerAssemblyName, _projectDir);

            _logger.LogInformation("Done");
        }

        public virtual void ShowLogo()
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
        }

        private void Execute(string startupProject, Action invoke)
        {
            var returnDirectory = Directory.GetCurrentDirectory();
            try
            {
                UseStartupProject(startupProject);

                invoke();
            }
            finally
            {
                Directory.SetCurrentDirectory(returnDirectory);
            }
        }

        private void UseStartupProject(string startupProject)
        {
            if (string.IsNullOrEmpty(startupProject))
            {
                return;
            }

            var library = _libraryManager.GetLibraryInformation(startupProject);
            if (library == null || library.Type != "Project")
            {
                _logger.LogVerbose("Unable to resolve start-up project '{0}'", startupProject);

                return;
            }

            _logger.LogVerbose("Using start-up project '{0}'", library.Name);

            var startupProjectDir = Path.GetDirectoryName(library.Path);
            _logger.LogVerbose("Using current directory '{0}'", startupProjectDir);
            Directory.SetCurrentDirectory(startupProjectDir);
        }
    }
}

#endif
