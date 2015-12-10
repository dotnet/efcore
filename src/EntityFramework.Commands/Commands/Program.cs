// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451 || DNXCORE50

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Dnx.Runtime;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.Data.Entity.Commands
{
    public class Program
    {
        private static readonly AnsiConsole _ansiConsole;

        static Program()
        {
            var useConsoleColors = PlatformServices.Default.Runtime.OperatingSystem == "Windows";
            _ansiConsole = AnsiConsole.GetOutput(useConsoleColors);
        }

        public static int Main([NotNull] string[] args)
        {
            Check.NotNull(args, nameof(args));

            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (_, __) => cancellationTokenSource.Cancel();

            var app = new CommandLineApplication
            {
                Name = "dnx ef",
                FullName = "Entity Framework Commands"
            };
            app.VersionOption(
                "--version",
                ProductInfo.GetVersion());
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
                                "The target migration. If '0', all migrations will be reverted. If omitted, all pending migrations will be applied");
                            var context = update.Option(
                                "-c|--context <context>",
                                "The DbContext to use. If omitted, the default DbContext is used");
                            var targetProject = update.Option(
                                "-p|--target-project <project>",
                                "The project with the Migration classes. If omitted, the current project is used.");
                            var environment = update.Option(
                                "-e|--environment <environment>",
                                "The environment to use. If omitted, \"Development\" is used.");
                            var verbose = update.Option(
                                "-v|--verbose",
                                "Show verbose output");
                            update.HelpOption("-?|-h|--help");
                            update.OnExecute(
                                () =>
                                {
                                    if (!ValidateProject(targetProject.Value()))
                                    {
                                        return 1;
                                    }

                                    return CreateExecutor(
                                            targetProject.Value(),
                                            environment.Value(),
                                            verbose.HasValue())
                                        .UpdateDatabase(
                                            migrationName.Value,
                                            context.Value());
                                });
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
                            var targetProject = list.Option(
                                "-p|--target-project <project>",
                                "The project with the DbContext classes. If omitted, the current project is used.");
                            var environment = list.Option(
                                "-e|--environment <environment>",
                                "The environment to use. If omitted, \"Development\" is used.");
                            var json = list.Option(
                                "--json",
                                "Use json output");
                            var verbose = list.Option(
                                "-v|--verbose",
                                "Show verbose output");
                            list.HelpOption("-?|-h|--help");
                            list.OnExecute(
                                () =>
                                {
                                    if (!ValidateProject(targetProject.Value()))
                                    {
                                        return 1;
                                    }

                                    return CreateExecutor(
                                            targetProject.Value(),
                                            environment.Value(),
                                            verbose.HasValue())
                                        .ListContexts(
                                            json.HasValue());
                                });
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
                                "The provider to use. For example, EntityFramework.MicrosoftSqlServer");
                            var useDataAnnotations = scaffold.Option(
                                "-a|--data-annotations",
                                "Use DataAnnotation attributes to configure the model where possible. If omitted, the output code will use only the fluent API.",
                                CommandOptionType.NoValue);
                            var dbContextClassName = scaffold.Option(
                                "-c|--context <name>",
                                "Name of the generated DbContext class.");
                            var overwriteFiles = scaffold.Option(
                                "-f|--force",
                                "Force scaffolding to overwrite existing files. Otherwise, the code will only proceed if no output files would be overwritten.",
                                CommandOptionType.NoValue);
                            var outputDir = scaffold.Option(
                                "-o|--output-dir <path>",
                                "Directory of the project where the classes should be output. If omitted, the top-level project directory is used.");
                            var schemaFilters = scaffold.Option(
                                "-s|--schema <schema_name.table_name>",
                                "Selects a schema for which to generate classes.",
                                CommandOptionType.MultipleValue);
                            var tableFilters = scaffold.Option(
                                "-t|--table <schema_name.table_name>",
                                "Selects a table for which to generate classes.",
                                CommandOptionType.MultipleValue);
                            var targetProject = scaffold.Option(
                                "-p|--target-project <project>",
                                "The project to scaffold the model into. If omitted, the current project is used.");
                            var environment = scaffold.Option(
                                "-e|--environment <environment>",
                                "The environment to use. If omitted, \"Development\" is used.");
                            var verbose = scaffold.Option(
                                "-v|--verbose",
                                "Show verbose output");
                            scaffold.HelpOption("-?|-h|--help");
                            scaffold.OnExecute(
                                async () =>
                                {
                                    if (string.IsNullOrEmpty(connection.Value))
                                    {
                                        LogError("Missing required argument '{0}'", connection.Name);

                                        scaffold.ShowHelp();

                                        return 1;
                                    }
                                    if (string.IsNullOrEmpty(provider.Value))
                                    {
                                        LogError("Missing required argument '{0}'", provider.Name);

                                        scaffold.ShowHelp();

                                        return 1;
                                    }
                                    if (!ValidateProject(targetProject.Value()))
                                    {
                                        return 1;
                                    }

                                    return await CreateExecutor(
                                            targetProject.Value(),
                                            environment.Value(),
                                            verbose.HasValue())
                                        .ReverseEngineerAsync(
                                            connection.Value,
                                            provider.Value,
                                            outputDir.Value(),
                                            dbContextClassName.Value(),
                                            schemaFilters.Values,
                                            tableFilters.Values,
                                            useDataAnnotations.HasValue(),
                                            overwriteFiles.HasValue(),
                                            cancellationTokenSource.Token);
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
                            var outputDir = add.Option(
                                "-o|--output-dir <path>",
                                "The directory (and sub-namespace) to use. If omitted, \"Migrations\" is used.");
                            var context = add.Option(
                                "-c|--context <context>",
                                "The DbContext to use. If omitted, the default DbContext is used");
                            var targetProject = add.Option(
                                "-p|--target-project <project>",
                                "The project to add the migration to. If omitted, the current project is used.");
                            var environment = add.Option(
                                "-e|--environment <environment>",
                                "The environment to use. If omitted, \"Development\" is used.");
                            var verbose = add.Option(
                                "-v|--verbose",
                                "Show verbose output");
                            add.HelpOption("-?|-h|--help");
                            add.OnExecute(
                                () =>
                                {
                                    if (string.IsNullOrEmpty(name.Value))
                                    {
                                        LogError("Missing required argument '{0}'", name.Name);

                                        add.ShowHelp();

                                        return 1;
                                    }
                                    if (!ValidateProject(targetProject.Value()))
                                    {
                                        return 1;
                                    }

                                    return CreateExecutor(
                                            targetProject.Value(),
                                            environment.Value(),
                                            verbose.HasValue())
                                        .AddMigration(
                                            name.Value,
                                            outputDir.Value(),
                                            context.Value());
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
                            var targetProject = list.Option(
                                "-p|--target-project <project>",
                                "The project with the Migration classes. If omitted, the current project is used.");
                            var environment = list.Option(
                                "-e|--environment <environment>",
                                "The environment to use. If omitted, \"Development\" is used.");
                            var json = list.Option(
                                "--json",
                                "Use json output");
                            var verbose = list.Option(
                                "-v|--verbose",
                                "Show verbose output");
                            list.HelpOption("-?|-h|--help");
                            list.OnExecute(
                                () =>
                                {
                                    if (!ValidateProject(targetProject.Value()))
                                    {
                                        return 1;
                                    }

                                    return CreateExecutor(
                                            targetProject.Value(),
                                            environment.Value(),
                                            verbose.HasValue())
                                        .ListMigrations(
                                            context.Value(),
                                            json.HasValue());
                                });
                        });
                    migration.Command(
                        "remove",
                        remove =>
                        {
                            remove.Description = "Remove the last migration";
                            var context = remove.Option(
                                "-c|--context <context>",
                                "The DbContext to use. If omitted, the default DbContext is used");
                            var targetProject = remove.Option(
                                "-p|--target-project <project>",
                                "The project with the Migration classes. If omitted, the current project is used.");
                            var environment = remove.Option(
                                "-e|--environment <environment>",
                                "The environment to use. If omitted, \"Development\" is used.");
                            var verbose = remove.Option(
                                "-v|--verbose",
                                "Show verbose output");
                            remove.HelpOption("-?|-h|--help");
                            remove.OnExecute(
                                () =>
                                {
                                    if (!ValidateProject(targetProject.Value()))
                                    {
                                        return 1;
                                    }

                                    return CreateExecutor(
                                            targetProject.Value(),
                                            environment.Value(),
                                            verbose.HasValue())
                                        .RemoveMigration(
                                            context.Value());
                                });
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
                            var targetProject = script.Option(
                                "-p|--target-project <project>",
                                "The project with the Migration classes. If omitted, the current project is used.");
                            var environment = script.Option(
                                "-e|--environment <environment>",
                                "The environment to use. If omitted, \"Development\" is used.");
                            var verbose = script.Option(
                                "-v|--verbose",
                                "Show verbose output");
                            script.HelpOption("-?|-h|--help");
                            script.OnExecute(
                                () =>
                                {
                                    if (!string.IsNullOrEmpty(to.Value) && string.IsNullOrEmpty(from.Value))
                                    {
                                        LogError("Missing required argument '{0}'", from.Name);

                                        return 1;
                                    }
                                    if (!ValidateProject(targetProject.Value()))
                                    {
                                        return 1;
                                    }

                                    return CreateExecutor(
                                            targetProject.Value(),
                                            environment.Value(),
                                            verbose.HasValue())
                                        .ScriptMigration(
                                            from.Value,
                                            to.Value,
                                            output.Value(),
                                            idempotent.HasValue(),
                                            context.Value());
                                });
                        });
                });

            return app.Execute(args);
        }

        private static void ShowLogo()
            => _ansiConsole.WriteLine(
                "\x1b[1m\x1b[37m" + Environment.NewLine +
                "                     _/\\__" + Environment.NewLine +
                "               ---==/    \\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m ___  ___ \x1b[1m\x1b[37m  |\x1b[22m.\x1b[1m    \\|\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m| __|| __|\x1b[1m\x1b[37m  |  )   \\\\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m| _| | _| \x1b[1m\x1b[37m  \\_/ |  //|\\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m|___||_|  \x1b[1m\x1b[37m     /   \\\\\\/\\\\" + Environment.NewLine +
                "\x1b[22m\x1b[39m");

        private static Executor CreateExecutor(string targetProject, string environment, bool verbose)
            => new Executor(targetProject, environment, verbose);

        private static void LogError(string format, params object[] arg)
            => _ansiConsole.WriteLine("\x1b[1m\x1b[31m" + string.Format(format, arg) + "\x1b[22m\x1b[39m");

        private static bool ValidateProject(string targetProject)
        {
            if (string.IsNullOrEmpty(targetProject))
            {
                return true;
            }

            var libraryManager = PlatformServices.Default.LibraryManager;

            var targetLibrary = libraryManager.GetLibrary(targetProject);
            if (targetLibrary == null || targetLibrary.Type != LibraryTypes.Project)
            {

                LogError("The project '" + targetProject + "' wasn't found. Ensure it's referenced by the current project.");

                return false;
            }

            return true;
        }

        private class Executor
        {
            private readonly LazyRef<ILogger> _logger;
            private readonly LazyRef<MigrationsOperations> _migrationsOperations;
            private readonly LazyRef<DbContextOperations> _contextOperations;
            private readonly LazyRef<DatabaseOperations> _databaseOperations;

            public Executor(string targetProject, string environment, bool verbose)
            {
                var appEnv = PlatformServices.Default.Application;
                var libraryManager = PlatformServices.Default.LibraryManager;

                var loggerProvider = new LoggerProvider(name => new ConsoleCommandLogger(name, verbose, _ansiConsole));
                _logger = new LazyRef<ILogger>(() => loggerProvider.CreateCommandsLogger());

                var targetName = !string.IsNullOrEmpty(targetProject)
                    ? targetProject
                    : appEnv.ApplicationName;
                var startupTargetName = appEnv.ApplicationName;
                var targetLibrary = libraryManager.GetLibrary(targetName);
                var projectDir = Path.GetDirectoryName(targetLibrary.Path);
                var rootNamespace = targetLibrary.Name;

                _contextOperations = new LazyRef<DbContextOperations>(
                    () => new DbContextOperations(
                        loggerProvider,
                        targetName,
                        startupTargetName,
                        projectDir,
                        environment));
                _databaseOperations = new LazyRef<DatabaseOperations>(
                    () => new DatabaseOperations(
                        loggerProvider,
                        targetName,
                        startupTargetName,
                        environment,
                        projectDir,
                        rootNamespace));
                _migrationsOperations = new LazyRef<MigrationsOperations>(
                    () => new MigrationsOperations(
                        loggerProvider,
                        targetName,
                        startupTargetName,
                        environment,
                        projectDir,
                        rootNamespace));
            }

            public virtual int ListContexts(bool json)
                => Execute(
                    () =>
                    {
                        var contexts = _contextOperations.Value.GetContextTypes().ToList();
                        if (json)
                        {
                            var builder = new IndentedStringBuilder();

                            builder.AppendLine("[");

                            using (builder.Indent())
                            {
                                for (var i = 0; i < contexts.Count; i++)
                                {
                                    var context = contexts[i];

                                    builder
                                        .Append("{ \"fullName\": \"")
                                        .Append(context.FullName)
                                        .Append("\" }");

                                    if (i != contexts.Count - 1)
                                    {
                                        builder.Append(",");
                                    }

                                    builder.AppendLine();
                                }
                            }

                            builder.AppendLine("]");

                            _logger.Value.LogInformation(builder.ToString());
                        }
                        else
                        {
                            var any = false;
                            foreach (var context in contexts)
                            {
                                _logger.Value.LogInformation(context.FullName);
                                any = true;
                            }

                            if (!any)
                            {
                                _logger.Value.LogInformation("No DbContext was found");
                            }
                        }
                    });

            public virtual int AddMigration(
                [NotNull] string name,
                [CanBeNull] string outputDir,
                [CanBeNull] string context)
                => Execute(
                    () =>
                    {
                        _migrationsOperations.Value.AddMigration(name, outputDir, context);

                        _logger.Value.LogInformation("Done. To undo this action, use 'ef migrations remove'");
                    });

            public virtual int UpdateDatabase([CanBeNull] string migration, [CanBeNull] string context)
                => Execute(() => _migrationsOperations.Value.UpdateDatabase(migration, context));

            public virtual int ListMigrations([CanBeNull] string context, bool json)
                => Execute(
                    () =>
                    {
                        var migrations = _migrationsOperations.Value.GetMigrations(context).ToList();
                        if (json)
                        {
                            var builder = new IndentedStringBuilder();

                            builder.AppendLine("[");

                            using (builder.Indent())
                            {
                                for (var i = 0; i < migrations.Count; i++)
                                {
                                    var migration = migrations[i];

                                    builder.AppendLine("{");

                                    using (builder.Indent())
                                    {
                                        builder
                                            .Append("\"id\": \"")
                                            .Append(migration.Id)
                                            .AppendLine("\",")
                                            .Append("\"name\": \"")
                                            .Append(migration.Name)
                                            .AppendLine("\"");
                                    }

                                    builder.Append("}");

                                    if (i != migrations.Count - 1)
                                    {
                                        builder.Append(",");
                                    }

                                    builder.AppendLine();
                                }
                            }

                            builder.AppendLine("]");

                            _logger.Value.LogInformation(builder.ToString());
                        }
                        else
                        {
                            var any = false;
                            foreach (var migration in migrations)
                            {
                                _logger.Value.LogInformation(migration.Id);
                                any = true;
                            }

                            if (!any)
                            {
                                _logger.Value.LogInformation("No migrations were found");
                            }
                        }
                    });

            public virtual int ScriptMigration(
                [CanBeNull] string from,
                [CanBeNull] string to,
                [CanBeNull] string output,
                bool idempotent,
                [CanBeNull] string context)
                => Execute(
                    () =>
                    {
                        var sql = _migrationsOperations.Value.ScriptMigration(from, to, idempotent, context);

                        if (string.IsNullOrEmpty(output))
                        {
                            _logger.Value.LogInformation(sql);
                        }
                        else
                        {
                            _logger.Value.LogDebug("Writing SQL script to '{0}'", output);
                            File.WriteAllText(output, sql);

                            _logger.Value.LogInformation("Done");
                        }
                    });

            public virtual int RemoveMigration([CanBeNull] string context)
                => Execute(() => _migrationsOperations.Value.RemoveMigration(context));

            public virtual Task<int> ReverseEngineerAsync(
                [NotNull] string connectionString,
                [NotNull] string providerAssemblyName,
                [CanBeNull] string outputDirectory,
                [CanBeNull] string dbContextClassName,
                [CanBeNull] List<string> schemaFilters,
                [CanBeNull] List<string> tableFilters,
                bool useDataAnnotations,
                bool overwriteFiles,
                CancellationToken cancellationToken = default(CancellationToken))
                => ExecuteAsync(
                    async () =>
                    {
                        await _databaseOperations.Value.ReverseEngineerAsync(
                            providerAssemblyName, connectionString, outputDirectory,
                            dbContextClassName, schemaFilters, tableFilters,
                            useDataAnnotations, overwriteFiles);

                        _logger.Value.LogInformation("Done");
                    });

            private int Execute(Action action)
            {
                try
                {
                    action();

                    return 0;
                }
                catch (Exception ex)
                {
                    LogException(ex);

                    return 1;
                }
            }

            private async Task<int> ExecuteAsync(Func<Task> action)
            {
                try
                {
                    await action();

                    return 0;
                }
                catch (Exception ex)
                {
                    LogException(ex);

                    return 1;
                }
            }

            private void LogException(Exception ex)
            {
                if (ex is OperationException)
                {
                    _logger.Value.LogDebug(ex.ToString());
                }
                else
                {
                    _logger.Value.LogInformation(ex.ToString());
                }

                _logger.Value.LogError(ex.Message);
            }
        }
    }
}

#endif
