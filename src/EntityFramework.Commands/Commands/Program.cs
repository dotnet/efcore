// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if DNX451 || DNXCORE50

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Commands
{
    public class Program
    {
        private readonly IApplicationShutdown _applicationShutdown;
        private readonly bool _useConsoleColors;
        private readonly LazyRef<ILogger> _logger;
        private readonly LazyRef<MigrationsOperations> _migrationsOperations;
        private readonly LazyRef<DbContextOperations> _contextOperations;
        private readonly LazyRef<DatabaseOperations> _databaseOperations;

        public Program([NotNull] IServiceProvider dnxServices)
        {
            Check.NotNull(dnxServices, nameof(dnxServices));

            var appEnv = dnxServices.GetRequiredService<IApplicationEnvironment>();
            var runtimeEnv = dnxServices.GetRequiredService<IRuntimeEnvironment>();
            _applicationShutdown = dnxServices.GetRequiredService<IApplicationShutdown>();
            _useConsoleColors = runtimeEnv.OperatingSystem == "Windows";

            var loggerProvider = new LoggerProvider(name => new ConsoleCommandLogger(name, verbose: true));
            _logger = new LazyRef<ILogger>(() => loggerProvider.CreateLogger(typeof(Program).FullName));

            var projectDir = appEnv.ApplicationBasePath;
            var rootNamespace = appEnv.ApplicationName;

            var startupAssemblyName = appEnv.ApplicationName;

            var assemblyName = new AssemblyName(appEnv.ApplicationName);
            var assembly = Assembly.Load(assemblyName);

            _contextOperations = new LazyRef<DbContextOperations>(
                () => new DbContextOperations(
                    loggerProvider,
                    assembly,
                    startupAssemblyName,
                    dnxServices));
            _databaseOperations = new LazyRef<DatabaseOperations>(
                () => new DatabaseOperations(
                    loggerProvider,
                    assembly,
                    startupAssemblyName,
                    projectDir,
                    rootNamespace,
                    dnxServices));
            _migrationsOperations = new LazyRef<MigrationsOperations>(
                () => new MigrationsOperations(
                    loggerProvider,
                    assembly,
                    startupAssemblyName,
                    projectDir,
                    rootNamespace,
                    dnxServices));
        }

        public virtual int Main([NotNull] string[] args)
        {
            Check.NotNull(args, nameof(args));

            Console.CancelKeyPress += (_, __) => _applicationShutdown.RequestShutdown();

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
                                "the target migration. If '0', all migrations will be reverted. If omitted, all pending migrations will be applied");
                            var context = update.Option(
                                "-c|--context <context>",
                                "The DbContext to use. If omitted, the default DbContext is used");
                            update.HelpOption("-?|-h|--help");
                            update.OnExecute(() => UpdateDatabase(migrationName.Value, context.Value()));
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
                            var relativeOutputPath = scaffold.Option(
                                "-o|--output-path <path>",
                                "Relative path to the sub-directory of the project where the classes should be output. If omitted, the top-level project directory is used.");
                            var useFluentApiOnly = scaffold.Option(
                                "-u|--fluent-api",
                                "Exclusively use fluent API to configure the model. If omitted, the output code will use attributes, where possible, instead.");
                            scaffold.HelpOption("-?|-h|--help");
                            scaffold.OnExecute(
                                async () =>
                                {
                                    if (string.IsNullOrEmpty(connection.Value))
                                    {
                                        _logger.Value.LogError("Missing required argument '{0}'", connection.Name);

                                        scaffold.ShowHelp();

                                        return 1;
                                    }
                                    if (string.IsNullOrEmpty(provider.Value))
                                    {
                                        _logger.Value.LogError("Missing required argument '{0}'", provider.Name);

                                        scaffold.ShowHelp();

                                        return 1;
                                    }

                                    await ReverseEngineerAsync(
                                        connection.Value,
                                        provider.Value,
                                        relativeOutputPath.Value(),
                                        useFluentApiOnly.HasValue(),
                                        _applicationShutdown.ShutdownRequested);

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
                            add.HelpOption("-?|-h|--help");
                            add.OnExecute(
                                () =>
                                {
                                    if (string.IsNullOrEmpty(name.Value))
                                    {
                                        _logger.Value.LogError("Missing required argument '{0}'", name.Name);

                                        add.ShowHelp();

                                        return 1;
                                    }

                                    AddMigration(name.Value, context.Value());

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
                            list.HelpOption("-?|-h|--help");
                            list.OnExecute(() => ListMigrations(context.Value()));
                        });
                    migration.Command(
                        "remove",
                        remove =>
                        {
                            remove.Description = "Remove the last migration";
                            var context = remove.Option(
                                "-c|--context <context>",
                                "The DbContext to use. If omitted, the default DbContext is used");
                            remove.HelpOption("-?|-h|--help");
                            remove.OnExecute(() => RemoveMigration(context.Value()));
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
                            script.HelpOption("-?|-h|--help");
                            script.OnExecute(
                                () =>
                                {
                                    if (!string.IsNullOrEmpty(to.Value) && string.IsNullOrEmpty(from.Value))
                                    {
                                        _logger.Value.LogError("Missing required argument '{0}'", from.Name);

                                        return 1;
                                    }

                                    ScriptMigration(
                                        from.Value,
                                        to.Value,
                                        output.Value(),
                                        idempotent.HasValue(),
                                        context.Value());

                                    return 0;
                                });
                        });
                });

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                LogException(ex);

                return 1;
            }
        }

        public virtual void ListContexts()
        {
            var contexts = _contextOperations.Value.GetContextTypes();
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

        public virtual void AddMigration(
            [NotNull] string name,
            [CanBeNull] string context)
        {
            _migrationsOperations.Value.AddMigration(name, context);

            _logger.Value.LogInformation("Done. To undo this action, use 'ef migrations remove'");
        }

        public virtual void UpdateDatabase(
            [CanBeNull] string migration,
            [CanBeNull] string context)
            => _migrationsOperations.Value.UpdateDatabase(migration, context);

        public virtual void ListMigrations([CanBeNull] string context)
        {
            var migrations = _migrationsOperations.Value.GetMigrations(context);
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

        public virtual void ScriptMigration(
            [CanBeNull] string from,
            [CanBeNull] string to,
            [CanBeNull] string output,
            bool idempotent,
            [CanBeNull] string context)
        {
            var sql = _migrationsOperations.Value.ScriptMigration(from, to, idempotent, context);

            if (string.IsNullOrEmpty(output))
            {
                _logger.Value.LogInformation(sql);
            }
            else
            {
                _logger.Value.LogVerbose("Writing SQL script to '{0}'", output);
                File.WriteAllText(output, sql);

                _logger.Value.LogInformation("Done");
            }
        }

        public virtual void RemoveMigration([CanBeNull] string context)
            => _migrationsOperations.Value.RemoveMigration(context);

        public virtual async Task ReverseEngineerAsync(
            [NotNull] string connectionString,
            [NotNull] string providerAssemblyName,
            [CanBeNull] string relativeOutputDirectory,
            bool useFluentApiOnly,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _databaseOperations.Value.ReverseEngineerAsync(
                providerAssemblyName, connectionString,
                relativeOutputDirectory, useFluentApiOnly);

            _logger.Value.LogInformation("Done");
        }

        private void ShowLogo()
        {
            // TODO: Enable multiple parameters in escape sequences
            AnsiConsole.GetOutput(_useConsoleColors).WriteLine(
                "\x1b[1m\x1b[37m" + Environment.NewLine +
                "                     _/\\__" + Environment.NewLine +
                "               ---==/    \\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m ___  ___ \x1b[1m\x1b[37m  |\x1b[22m.\x1b[1m    \\|\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m| __|| __|\x1b[1m\x1b[37m  |  )   \\\\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m| _| | _| \x1b[1m\x1b[37m  \\_/ |  //|\\\\" + Environment.NewLine +
                "        \x1b[22m\x1b[35m|___||_|  \x1b[1m\x1b[37m     /   \\\\\\/\\\\" + Environment.NewLine +
                "\x1b[22m\x1b[39m");
        }

        private void LogException(Exception ex)
        {
            if (ex is OperationException)
            {
                _logger.Value.LogVerbose(ex.ToString());
            }
            else
            {
                _logger.Value.LogInformation(ex.ToString());
            }

            _logger.Value.LogError(ex.Message);
        }
    }
}

#endif
