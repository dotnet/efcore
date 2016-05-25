// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
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
            var projectFile = Path.Combine(Directory.GetCurrentDirectory(), Project.FileName);
            var project = ProjectReader.GetProject(projectFile);

            var projectConfiguration = options.Configuration ?? Constants.DefaultConfiguration;
            var projectFramework = options.Framework;

            var projectContext = ProjectContext.Create(project.ProjectFilePath,
                projectFramework,
                RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers());

            var runtimeOutputPath = projectContext.GetOutputPaths(projectConfiguration)?.RuntimeOutputPath;
            if (!string.IsNullOrEmpty(runtimeOutputPath))
            {
                Reporter.Verbose.WriteLine(
                    ToolsCliStrings.LogDataDirectory(runtimeOutputPath));
                Environment.SetEnvironmentVariable(DataDirEnvName, runtimeOutputPath);
#if NET451
                AppDomain.CurrentDomain.SetData("DataDirectory", runtimeOutputPath);
#endif
            }

            var assemblyName = project.GetCompilerOptions(projectFramework, projectConfiguration).OutputName;
            var projectDir = project.ProjectDirectory;
            var rootNamespace = project.Name;

            var assemblyLoader = new AssemblyLoader(Assembly.Load);
            var projectAssembly = assemblyLoader.Load(assemblyName);

            _contextOperations = new LazyRef<DbContextOperations>(
                          () => new DbContextOperations(
                              new LoggerProvider(name => new ConsoleCommandLogger(name)),
                              projectAssembly,
                              projectAssembly,
                              environment,
                              projectDir));
            _databaseOperations = new LazyRef<DatabaseOperations>(
                () => new DatabaseOperations(
                    new LoggerProvider(name => new ConsoleCommandLogger(name)),
                    assemblyLoader,
                    projectAssembly,
                    environment,
                    projectDir,
                    projectDir,
                    rootNamespace));
            _migrationsOperations = new LazyRef<MigrationsOperations>(
                () => new MigrationsOperations(
                    new LoggerProvider(name => new ConsoleCommandLogger(name)),
                    projectAssembly,
                    assemblyLoader,
                    projectAssembly,
                    environment,
                    projectDir,
                    projectDir,
                    rootNamespace));
        }

        public virtual void DropDatabase([CanBeNull] string contextName, [NotNull] Func<string, string, bool> confirmCheck)
            => _contextOperations.Value.DropDatabase(contextName, confirmCheck);

        public virtual MigrationFiles AddMigration(
            [NotNull] string name,
            [CanBeNull] string outputDir,
            [CanBeNull] string contextType)
        {
            if(!string.IsNullOrWhiteSpace(outputDir) && !Path.IsPathRooted(outputDir))
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
