// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Design;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Design
{
    public class MigrationsOperations
    {
        private readonly ILoggerProvider _loggerProvider;
        private readonly LazyRef<ILogger> _logger;
        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly DesignTimeServicesBuilder _servicesBuilder;
        private readonly DbContextOperations _contextOperations;

        public MigrationsOperations(
            [NotNull] ILoggerProvider loggerProvider,
            [NotNull] string assemblyName,
            [NotNull] string startupAssemblyName,
            [CanBeNull] string environment,
            [NotNull] string projectDir,
            [NotNull] string rootNamespace)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));
            Check.NotEmpty(assemblyName, nameof(assemblyName));
            Check.NotEmpty(startupAssemblyName, nameof(startupAssemblyName));
            Check.NotNull(projectDir, nameof(projectDir));
            Check.NotNull(rootNamespace, nameof(rootNamespace));

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(loggerProvider);

            _loggerProvider = loggerProvider;
            _logger = new LazyRef<ILogger>(() => loggerFactory.CreateCommandsLogger());
            _projectDir = projectDir;
            _rootNamespace = rootNamespace;
            _contextOperations = new DbContextOperations(
                loggerProvider,
                assemblyName,
                startupAssemblyName,
                environment);

            var startup = new StartupInvoker(startupAssemblyName, environment);
            _servicesBuilder = new DesignTimeServicesBuilder(startup);
        }

        public virtual MigrationFiles AddMigration(
            [NotNull] string name,
            [CanBeNull] string outputDir,
            [CanBeNull] string contextType)
        {
            Check.NotEmpty(name, nameof(name));

            var subNamespace = outputDir != null
                ? string.Join(".", outputDir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                : null;

            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = _servicesBuilder.Build(context);
                EnsureServices(services);

                var scaffolder = services.GetRequiredService<MigrationsScaffolder>();
                var migration = scaffolder.ScaffoldMigration(name, _rootNamespace, subNamespace);
                var files = scaffolder.Save(_projectDir, migration);

                return files;
            }
        }

        public virtual IEnumerable<MigrationInfo> GetMigrations(
            [CanBeNull] string contextType)
        {
            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = _servicesBuilder.Build(context);
                EnsureServices(services);

                var migrationsAssembly = services.GetRequiredService<IMigrationsAssembly>();
                var idGenerator = services.GetRequiredService<IMigrationsIdGenerator>();

                return from id in migrationsAssembly.Migrations.Keys
                       select new MigrationInfo { Id = id, Name = idGenerator.GetName(id) };
            }
        }

        public virtual string ScriptMigration(
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent,
            [CanBeNull] string contextType)
        {
            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = _servicesBuilder.Build(context);
                EnsureServices(services);

                var migrator = services.GetRequiredService<IMigrator>();

                return migrator.GenerateScript(fromMigration, toMigration, idempotent);
            }
        }

        public virtual void UpdateDatabase(
            [CanBeNull] string targetMigration,
            [CanBeNull] string contextType)
        {
            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = _servicesBuilder.Build(context);
                EnsureServices(services);

                var migrator = services.GetRequiredService<IMigrator>();

                migrator.Migrate(targetMigration);
            }

            _logger.Value.LogInformation(CommandsStrings.Done);
        }

        public virtual MigrationFiles RemoveMigration(
            [CanBeNull] string contextType)
        {
            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = _servicesBuilder.Build(context);
                EnsureServices(services);

                var scaffolder = services.GetRequiredService<MigrationsScaffolder>();

                var files = scaffolder.RemoveMigration(_projectDir, _rootNamespace);

                _logger.Value.LogInformation(CommandsStrings.Done);

                return files;
            }
        }

        private static void EnsureServices(IServiceProvider services)
        {
            var providerServices = services.GetRequiredService<IDatabaseProviderServices>();
            if (!(providerServices is IRelationalDatabaseProviderServices))
            {
                throw new OperationException(CommandsStrings.NonRelationalProvider(providerServices.InvariantName));
            }
        }
    }
}
