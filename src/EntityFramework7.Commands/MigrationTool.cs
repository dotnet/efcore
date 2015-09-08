// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Migrations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Commands
{
    public class MigrationTool
    {
        private readonly ILoggerProvider _loggerProvider;
        private readonly LazyRef<ILogger> _logger;
        private readonly Assembly _assembly;
        private readonly IServiceProvider _services;

        public MigrationTool(
            [NotNull] ILoggerProvider loggerProvider,
            [NotNull] Assembly assembly,
            [CanBeNull] IServiceProvider services = null)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));
            Check.NotNull(assembly, nameof(assembly));

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(loggerProvider);

            _loggerProvider = loggerProvider;
            _logger = new LazyRef<ILogger>(() => loggerFactory.CreateLogger<MigrationTool>());
            _assembly = assembly;
            _services = services;
        }

        public virtual MigrationFiles AddMigration(
            [NotNull] string migrationName,
            [CanBeNull] string contextTypeName,
            [CanBeNull] string startupAssemblyName,
            [NotNull] string rootNamespace,
            [NotNull] string projectDir)
        {
            Check.NotEmpty(migrationName, nameof(migrationName));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));
            Check.NotEmpty(projectDir, nameof(projectDir));

            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType, startupAssemblyName))
            {
                var services = new DesignTimeServices(((IAccessor<IServiceProvider>)context).Service);

                var scaffolder = CreateScaffolder(services);
                var migration = scaffolder.ScaffoldMigration(migrationName, rootNamespace);
                var files = scaffolder.Write(projectDir, migration);

                return files;
            }
        }

        public virtual IEnumerable<Migration> GetMigrations(
            [CanBeNull] string contextTypeName,
            [CanBeNull] string startupAssemblyName)
        {
            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType, startupAssemblyName))
            {
                var services = new DesignTimeServices(((IAccessor<IServiceProvider>)context).Service);
                var migrationAssembly = services.GetRequiredService<IMigrationAssembly>();

                return migrationAssembly.Migrations;
            }
        }

        public virtual string ScriptMigration(
            [CanBeNull] string fromMigrationName,
            [CanBeNull] string toMigrationName,
            bool idempotent,
            [CanBeNull] string contextTypeName,
            [CanBeNull] string startupAssemblyName)
        {
            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType, startupAssemblyName))
            {
                var services = ((IAccessor<IServiceProvider>)context).Service;
                var migrator = services.GetRequiredService<IMigrator>();

                return migrator.ScriptMigrations(fromMigrationName, toMigrationName, idempotent);
            }
        }

        public virtual void ApplyMigration(
            [CanBeNull] string migrationName,
            [CanBeNull] string contextTypeName,
            [CanBeNull] string startupAssemblyName)
        {
            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType, startupAssemblyName))
            {
                var services = ((IAccessor<IServiceProvider>)context).Service;
                var migrator = services.GetRequiredService<IMigrator>();

                migrator.ApplyMigrations(migrationName);
            }

            _logger.Value.LogInformation(Strings.Done);
        }

        public virtual MigrationFiles RemoveMigration(
            [CanBeNull] string contextTypeName,
            [CanBeNull] string startupAssemblyName,
            [NotNull] string rootNamespace,
            [NotNull] string projectDir)
        {
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));
            Check.NotEmpty(projectDir, nameof(projectDir));

            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType, startupAssemblyName))
            {
                var services = new DesignTimeServices(((IAccessor<IServiceProvider>)context).Service);
                var scaffolder = CreateScaffolder(services);

                var files = scaffolder.RemoveMigration(projectDir, rootNamespace);

                _logger.Value.LogInformation(Strings.Done);

                return files;
            }
        }

        public virtual Type GetContextType([CanBeNull] string name)
        {
            var contextType = ContextTool.SelectType(GetContextTypes(), name);
            _logger.Value.LogVerbose(Strings.LogUseContext(contextType.Name));

            return contextType;
        }

        public virtual IEnumerable<Type> GetContextTypes() =>
            ContextTool.GetContextTypes(_assembly)
                .Concat(
                    MigrationAssembly.GetMigrationTypes(_assembly)
                        .Select(MigrationAssembly.TryGetContextType)
                        .Where(t => t != null))
                .Distinct();

        protected virtual DbContext CreateContext(Type type, string startupAssemblyName)
        {
            var context = new ContextTool(_services).CreateContext(type, startupAssemblyName);
            var services = ((IAccessor<IServiceProvider>)context).Service;

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddProvider(_loggerProvider);

            return context;
        }

        protected virtual MigrationScaffolder CreateScaffolder(IServiceProvider services)
        {
            // TODO: Can this be hidden in DesignTimeServices?
            return ActivatorUtilities.CreateInstance<MigrationScaffolder>(services);
        }
    }
}
