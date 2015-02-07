// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Migrations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
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

        public MigrationTool([NotNull] ILoggerProvider loggerProvider, [NotNull] Assembly assembly)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));
            Check.NotNull(assembly, nameof(assembly));

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(loggerProvider);

            _loggerProvider = loggerProvider;
            _logger = new LazyRef<ILogger>(() => loggerFactory.Create<MigrationTool>());
            _assembly = assembly;
        }

        public virtual MigrationFiles AddMigration(
            [NotNull] string migrationName,
            [CanBeNull] string contextTypeName,
            [NotNull] string rootNamespace,
            [NotNull] string projectDir)
        {
            Check.NotEmpty(migrationName, nameof(migrationName));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));
            Check.NotEmpty(projectDir, nameof(projectDir));

            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType))
            {
                var services = new DesignTimeServices(((IAccessor<IServiceProvider>)context).Service);

                var scaffolder = CreateScaffolder(services);
                var migration = scaffolder.ScaffoldMigration(migrationName, rootNamespace);
                var files = scaffolder.Write(projectDir, migration);

                return files;
            }
        }

        public virtual IEnumerable<Migration> GetMigrations([CanBeNull] string contextTypeName)
        {
            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType))
            {
                var services = new DesignTimeServices(((IAccessor<IServiceProvider>)context).Service);
                var migrationAssembly = services.GetRequiredService<MigrationAssembly>();

                return migrationAssembly.Migrations;
            }
        }

        public virtual string ScriptMigration(
            [CanBeNull] string fromMigrationName,
            [CanBeNull] string toMigrationName,
            bool idempotent,
            [CanBeNull] string contextTypeName)
        {
            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType))
            {
                var services = ((IAccessor<IServiceProvider>)context).Service;
                var migrator = services.GetRequiredService<Migrator>();

                return migrator.ScriptMigrations(fromMigrationName, toMigrationName, idempotent);
            }
        }

        public virtual void ApplyMigration([CanBeNull] string migrationName, [CanBeNull] string contextTypeName)
        {
            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType))
            {
                var services = ((IAccessor<IServiceProvider>)context).Service;
                var migrator = services.GetRequiredService<Migrator>();

                migrator.ApplyMigrations(migrationName);
            }
        }

        public virtual MigrationFiles RemoveMigration(
            [CanBeNull] string contextTypeName,
            [NotNull] string rootNamespace,
            [NotNull] string projectDir)
        {
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));
            Check.NotEmpty(projectDir, nameof(projectDir));

            var contextType = GetContextType(contextTypeName);
            using (var context = CreateContext(contextType))
            {
                var services = new DesignTimeServices(((IAccessor<IServiceProvider>)context).Service);
                var scaffolder = CreateScaffolder(services);

                return scaffolder.RemoveMigration(projectDir, rootNamespace);
            }
        }

        public virtual Type GetContextType([CanBeNull] string name)
        {
            var contextType = ContextTool.SelectType(GetContextTypes(), name);
            _logger.Value.WriteVerbose(Strings.LogUseContext(contextType.Name));

            return contextType;
        }

        public virtual IEnumerable<Type> GetContextTypes() =>
            ContextTool.GetContextTypes(_assembly)
                .Concat(
                    MigrationAssembly.GetMigrationTypes(_assembly)
                        .Select(MigrationAssembly.TryGetContextType)
                        .Where(t => t != null))
                .Distinct();

        protected virtual DbContext CreateContext(Type type)
        {
            var context = ContextTool.CreateContext(type);
            var services = ((IAccessor<IServiceProvider>)context).Service;

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddProvider(_loggerProvider);

            return context;
        }

        protected virtual MigrationScaffolder CreateScaffolder(IServiceProvider services)
        {
            var typeActivator = services.GetRequiredService<ITypeActivator>();

            // TODO: Can this be hidden in DesignTimeServices?
            return typeActivator.CreateInstance<MigrationScaffolder>(services);
        }
    }
}
