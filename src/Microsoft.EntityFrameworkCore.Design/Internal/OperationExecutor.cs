// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Tools.Cli;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    // TODO unify with the reflection-based OperationExecutor
    // This version is only used by .NET Core CLI
    public class OperationExecutor
    {
        private const string DataDirEnvName = "ADONET_DATA_DIR";

        private readonly LazyRef<MigrationsOperations> _migrationsOperations;
        private readonly LazyRef<DatabaseOperations> _databaseOperations;
        private readonly LazyRef<DbContextOperations> _contextOperations;

        public OperationExecutor(
            [NotNull] CommonOptions options,
            [CanBeNull] string environment)
        {
            if (!string.IsNullOrEmpty(options.DataDirectory))
            {
                Environment.SetEnvironmentVariable(DataDirEnvName, options.DataDirectory);
#if NET451
                AppDomain.CurrentDomain.SetData("DataDirectory", options.DataDirectory);
#endif
            }

            if (!File.Exists(options.Assembly))
            {
                throw new OperationException($"Could not find assembly '{options.Assembly}'.");
            }

            var assemblyFileName = Path.GetFileNameWithoutExtension(options.Assembly);
            
            // TODO add hooks into Assembly.Load to allow loading from other locations
            var assemblyLoader = new AssemblyLoader(Assembly.Load);
            var projectAssembly = assemblyLoader.Load(assemblyFileName);

            // optional
            var startupAssembly = string.IsNullOrWhiteSpace(options.StartupAssembly)
                ? projectAssembly
                : assemblyLoader.Load(Path.GetFileNameWithoutExtension(options.StartupAssembly));

            var projectDir = string.IsNullOrEmpty(options.ProjectDirectory)
                ? Directory.GetCurrentDirectory()
                : options.ProjectDirectory;

            var startupTargetDir = string.IsNullOrEmpty(options.StartupTargetDirectory)
                ? Directory.GetCurrentDirectory()
                : options.StartupTargetDirectory;

            var rootNamespace = string.IsNullOrEmpty(options.RootNamespace)
                ? assemblyFileName
                : options.RootNamespace;

            _contextOperations = new LazyRef<DbContextOperations>(
                () => new DbContextOperations(
                    new LoggerProvider(name => new ConsoleCommandLogger(name)),
                    assembly: projectAssembly,
                    startupAssembly: startupAssembly,
                    environment: environment,
                    startupTargetDir: startupTargetDir));
            _databaseOperations = new LazyRef<DatabaseOperations>(
                () => new DatabaseOperations(
                    new LoggerProvider(name => new ConsoleCommandLogger(name)),
                    startupAssemblyLoader: assemblyLoader,
                    startupAssembly: startupAssembly,
                    environment: environment,
                    projectDir: projectDir,
                    startupTargetDir: startupTargetDir,
                    rootNamespace: rootNamespace));
            _migrationsOperations = new LazyRef<MigrationsOperations>(
                () => new MigrationsOperations(
                    new LoggerProvider(name => new ConsoleCommandLogger(name)),
                    assembly: projectAssembly,
                    startupAssemblyLoader: assemblyLoader,
                    startupAssembly: startupAssembly,
                    environment: environment,
                    projectDir: projectDir,
                    startupTargetDir: startupTargetDir,
                    rootNamespace: rootNamespace));
        }

        public virtual void DropDatabase([CanBeNull] string contextName, [NotNull] Func<string, string, bool> confirmCheck)
            => _contextOperations.Value.DropDatabase(contextName, confirmCheck);

        public virtual MigrationFiles AddMigration(
            [NotNull] string name,
            [CanBeNull] string outputDir,
            [CanBeNull] string contextType)
        {
            if (!string.IsNullOrWhiteSpace(outputDir) && !Path.IsPathRooted(outputDir))
            {
                // relative paths adjusted to current working directory, not project directory
                // PowerShell adjusts the cwd when executing dotnet-ef.
                outputDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), outputDir));
            }

            return _migrationsOperations.Value.AddMigration(name, outputDir, contextType);
        }

        public virtual void UpdateDatabase([CanBeNull] string targetMigration, [CanBeNull] string contextType)
            => _migrationsOperations.Value.UpdateDatabase(targetMigration, contextType);

        public virtual string ScriptMigration(
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent,
            [CanBeNull] string contextType)
            => _migrationsOperations.Value.ScriptMigration(fromMigration, toMigration, idempotent, contextType);

        public virtual MigrationFiles RemoveMigration([CanBeNull] string contextType, bool force)
            => _migrationsOperations.Value.RemoveMigration(contextType, force);

        public virtual IEnumerable<Type> GetContextTypes()
            => _contextOperations.Value.GetContextTypes();

        public virtual IEnumerable<MigrationInfo> GetMigrations([CanBeNull] string contextType)
            => _migrationsOperations.Value.GetMigrations(contextType);

        public virtual Task<ReverseEngineerFiles> ReverseEngineerAsync(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            [CanBeNull] string dbContextClassName,
            [NotNull] IEnumerable<string> schemaFilters,
            [NotNull] IEnumerable<string> tableFilters,
            bool useDataAnnotations,
            bool overwriteFiles,
            CancellationToken cancellationToken = default(CancellationToken))
            => _databaseOperations.Value.ReverseEngineerAsync(
                provider,
                connectionString,
                outputDir,
                dbContextClassName,
                schemaFilters,
                tableFilters,
                useDataAnnotations,
                overwriteFiles,
                cancellationToken);
    }
}
