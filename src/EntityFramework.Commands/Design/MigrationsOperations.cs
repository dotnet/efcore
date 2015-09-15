// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Design;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Strings = Microsoft.Data.Entity.Design.Internal.Strings;

namespace Microsoft.Data.Entity.Design
{
    public class MigrationsOperations
    {
        private readonly ILoggerProvider _loggerProvider;
        private readonly LazyRef<ILogger> _logger;
        private readonly Assembly _assembly;
        private readonly string _startupAssemblyName;
        private readonly string _projectDir;
        private readonly string _rootNamespace;
        private readonly IServiceProvider _services;
        private readonly DbContextOperations _contextOperations;

        public MigrationsOperations(
            [NotNull] ILoggerProvider loggerProvider,
            [NotNull] Assembly assembly,
            [CanBeNull] string startupAssemblyName,
            [NotNull] string projectDir,
            [NotNull] string rootNamespace,
            [CanBeNull] IServiceProvider dnxServices = null)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));
            Check.NotNull(assembly, nameof(assembly));
            Check.NotNull(projectDir, nameof(projectDir));
            Check.NotNull(rootNamespace, nameof(rootNamespace));

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(loggerProvider);

            _loggerProvider = loggerProvider;
            _logger = new LazyRef<ILogger>(() => loggerFactory.CreateLogger<MigrationsOperations>());
            _assembly = assembly;
            _startupAssemblyName = startupAssemblyName;
            _projectDir = projectDir;
            _rootNamespace = rootNamespace;
            _services = dnxServices;
            _contextOperations = new DbContextOperations(
                loggerProvider,
                assembly,
                startupAssemblyName,
                dnxServices);
        }

        public virtual MigrationFiles AddMigration(
            [NotNull] string name,
            [CanBeNull] string contextType)
        {
            Check.NotEmpty(name, nameof(name));

            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = DesignTimeServices.Build(context);

                var scaffolder = CreateScaffolder(services);
                var migration = scaffolder.ScaffoldMigration(name, _rootNamespace);
                var files = scaffolder.Save(_projectDir, migration);

                return files;
            }
        }

        public virtual IEnumerable<MigrationInfo> GetMigrations(
            [CanBeNull] string contextType)
        {
            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = DesignTimeServices.Build(context);
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
                var services = DesignTimeServices.Build(context);
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
                var services = DesignTimeServices.Build(context);
                var migrator = services.GetRequiredService<IMigrator>();

                migrator.Migrate(targetMigration);
            }

            _logger.Value.LogInformation(Strings.Done);
        }

        public virtual MigrationFiles RemoveMigration(
            [CanBeNull] string contextType)
        {
            using (var context = _contextOperations.CreateContext(contextType))
            {
                var services = DesignTimeServices.Build(context);
                var scaffolder = CreateScaffolder(services);

                var files = scaffolder.RemoveMigration(_projectDir, _rootNamespace);

                _logger.Value.LogInformation(Strings.Done);

                return files;
            }
        }

        private MigrationsScaffolder CreateScaffolder(IServiceProvider services)
        {
            // TODO: Can this be hidden in AggregateServiceProvider?
            return ActivatorUtilities.CreateInstance<MigrationsScaffolder>(services);
        }
    }
}
