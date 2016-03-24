// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Loader;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.PlatformAbstractions;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class OperationExecutor
    {
        private readonly LazyRef<DbContextOperations> _contextOperations;
        private readonly LazyRef<DatabaseOperations> _databaseOperations;
        private readonly LazyRef<MigrationsOperations> _migrationsOperations;

        private const string DataDirEnvName = "ADONET_DATA_DIR";
        private const string DefaultConfiguration = "Debug";

        public OperationExecutor([CanBeNull] string startupProject, [CanBeNull] string environment)
        {
            var project = Path.Combine(Directory.GetCurrentDirectory(), Project.FileName);

            if (startupProject == null)
            {
                startupProject = project;
            }
            else if (!startupProject.EndsWith(Project.FileName))
            {
                startupProject = Path.Combine(startupProject, Project.FileName);
            }

            Reporter.Verbose.WriteLine(("Using startup project '" + startupProject + "'.").Bold().Black());
            Reporter.Verbose.WriteLine(("Using project '" + project + "'.").Bold().Black());

            var startupProjectContext = GetCompatibleProjectContext(startupProject);

            Reporter.Verbose.WriteLine("Build started...".Bold().Black());

            var buildCommand = Command.CreateDotNet(
                "build",
                new[] { startupProject, "-f", startupProjectContext.TargetFramework.GetShortFolderName() });
            if (buildCommand.Execute().ExitCode != 0)
            {
                throw new OperationException("Build failed.");
            }

            Reporter.Verbose.WriteLine("Build succeeded.".Bold().Black());

            var runtimeOutputPath = startupProjectContext.GetOutputPaths(DefaultConfiguration)?.RuntimeOutputPath;
            if (!string.IsNullOrEmpty(runtimeOutputPath))
            {
                // TODO set data directory in AppDomain when/if this supports desktop .NET
                Environment.SetEnvironmentVariable(DataDirEnvName, runtimeOutputPath);
            }

            var projectFile = ProjectReader.GetProject(project);
            var startupAssemblyName = new AssemblyName(startupProjectContext.ProjectFile.Name);
            var assemblyName = new AssemblyName(projectFile.Name);
            var projectDir = projectFile.ProjectDirectory;
            var startupProjectDir = startupProjectContext.ProjectFile.ProjectDirectory;
            var rootNamespace = projectFile.Name;
            var assemblyLoadContext = startupProjectContext.CreateLoadContext();
            var assemblyLoader = new AssemblyLoader(assemblyLoadContext.LoadFromAssemblyName);

            var startupAssembly = assemblyLoadContext.LoadFromAssemblyName(startupAssemblyName);

            Assembly assembly;
            try
            {
                assembly = assemblyLoadContext.LoadFromAssemblyName(assemblyName);
            }
            catch (Exception ex)
            {
                throw new OperationException(
                    CommandsStrings.UnreferencedAssembly(projectFile.Name, startupProjectContext.ProjectFile.Name),
                    ex);
            }

            _contextOperations = new LazyRef<DbContextOperations>(
                () => new DbContextOperations(
                    new LoggerProvider(name => new ConsoleCommandLogger(name)),
                    assembly,
                    startupAssembly,
                    environment,
                    startupProjectDir));
            _databaseOperations = new LazyRef<DatabaseOperations>(
                () => new DatabaseOperations(
                    assemblyLoader,
                    new LoggerProvider(name => new ConsoleCommandLogger(name)),
                    startupAssembly,
                    environment,
                    projectDir,
                    startupProjectDir,
                    rootNamespace));
            _migrationsOperations = new LazyRef<MigrationsOperations>(
                () => new MigrationsOperations(
                    assemblyLoader,
                    new LoggerProvider(name => new ConsoleCommandLogger(name)),
                    assembly,
                    startupAssembly,
                    environment,
                    projectDir,
                    startupProjectDir,
                    rootNamespace));
        }

        public virtual MigrationFiles AddMigration(
            [NotNull] string name,
            [CanBeNull] string outputDir,
            [CanBeNull] string contextType)
            => _migrationsOperations.Value.AddMigration(name, outputDir, contextType);

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
            [NotNull] IEnumerable<string> schemas,
            [NotNull] IEnumerable<string> tables,
            bool useDataAnnotations,
            bool overwriteFiles,
            CancellationToken cancellationToken = default(CancellationToken))
            => _databaseOperations.Value.ReverseEngineerAsync(
                provider,
                connectionString,
                outputDir,
                dbContextClassName,
                schemas,
                tables,
                useDataAnnotations,
                overwriteFiles,
                cancellationToken);

        private ProjectContext GetCompatibleProjectContext(string projectPath)
        {
            var projectFile = ProjectReader.GetProject(projectPath);
            var frameworks = projectFile.GetTargetFrameworks().Select(f => f.FrameworkName);
            var framework = NuGetFrameworkUtility.GetNearest(
                frameworks,
                FrameworkConstants.CommonFrameworks.NetStandardApp15,
                f => new NuGetFramework(f))
                // TODO: Remove with dnxcore50
                ?? NuGetFrameworkUtility.GetNearest(
                    frameworks,
                    FrameworkConstants.CommonFrameworks.DnxCore50,
                    f => new NuGetFramework(f));
            if (framework == null)
            {
                throw new OperationException(
                    "The project '" + projectFile.Name + "' doesn't target a framework compatible with DNX Core 5.0. You must " +
                    "target a compatible framework like 'dotnet5.4' before using the Entity Framework Core .NET Core CLI " +
                    "Commands.");
            }

            Reporter.Verbose.WriteLine(
                ("Using framework '" + framework + "' for project '" + projectFile.Name + "'").Bold().Black());

            return new ProjectContextBuilder()
                .WithProject(projectFile)
                .WithTargetFramework(framework)
                .WithRuntimeIdentifiers(PlatformServices.Default.Runtime.GetAllCandidateRuntimeIdentifiers())
                .Build();
        }
    }
}
