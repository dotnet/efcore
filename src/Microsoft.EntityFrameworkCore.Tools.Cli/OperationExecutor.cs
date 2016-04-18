// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.PlatformAbstractions;
using NuGet.Frameworks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class OperationExecutor
    {
        private const string DataDirEnvName = "ADONET_DATA_DIR";

        private readonly LazyRef<MigrationsOperations> _migrationsOperations;
        private readonly LazyRef<DatabaseOperations> _databaseOperations;
        private readonly LazyRef<DbContextOperations> _contextOperations;

        public OperationExecutor([CanBeNull] string startupProject, [CanBeNull] string environment)
        {
            var project = Path.Combine(Directory.GetCurrentDirectory(), Project.FileName);
            var buildStartup = true;
            if (startupProject == null)
            {
                startupProject = project;
                buildStartup = false;
            }
            // TODO flow through from dispatch
            var projectConfiguration = Constants.DefaultConfiguration;
            var projectFramework = FrameworkConstants.CommonFrameworks.NetCoreApp10;

            // TODO flow through from dispatch
            var startupConfiguration = Constants.DefaultConfiguration;
            string startupBuildBaseDir = null;
            string startupOutputDir = null;

            var startupProjectContext = GetCompatibleStartupProjectContext(startupProject);

            var startupFramework = startupProjectContext.TargetFramework;

            if (buildStartup)
            {
                Reporter.Verbose.WriteLine("Build started...".Bold().Black());

                var buildExitCode = BuildCommandFactory.Create(startupProject,
                        startupConfiguration,
                        startupFramework,
                        startupBuildBaseDir,
                        startupOutputDir)
                    .ForwardStdOut()
                    .ForwardStdErr()
                    .Execute()
                    .ExitCode;

                if (buildExitCode != 0)
                {
                    throw new OperationException("Build failed.");
                }

                Reporter.Verbose.WriteLine("Build succeeded.".Bold().Black());
            }

            var runtimeOutputPath = startupProjectContext.GetOutputPaths(startupConfiguration)?.RuntimeOutputPath;
            if (!string.IsNullOrEmpty(runtimeOutputPath))
            {
                Environment.SetEnvironmentVariable(DataDirEnvName, runtimeOutputPath);
#if NET451
                AppDomain.CurrentDomain.SetData("DataDirectory", runtimeOutputPath);
#endif
            }

            var projectFile = ProjectReader.GetProject(project);
            var startupAssemblyName = startupProjectContext.ProjectFile.GetCompilerOptions(startupFramework, startupConfiguration).OutputName;
            var assemblyName = projectFile.GetCompilerOptions(projectFramework, projectConfiguration).OutputName;
            var projectDir = projectFile.ProjectDirectory;
            var startupProjectDir = startupProjectContext.ProjectFile.ProjectDirectory;
            // TODO should this match assembly name?
            var rootNamespace = projectFile.Name;

            var assemblyLoader = new AssemblyLoader(Assembly.Load);
            var assembly = assemblyLoader.Load(assemblyName);
            var startupAssembly = assemblyLoader.Load(startupAssemblyName);

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

        public virtual void DropDatabase([CanBeNull] string contextName, [NotNull] Func<string, string, bool> confirmCheck)
            => _contextOperations.Value.DropDatabase(contextName, confirmCheck);

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

        private ProjectContext GetCompatibleStartupProjectContext(string projectPath)
        {
            var projectFile = ProjectReader.GetProject(projectPath);
            var frameworks = projectFile.GetTargetFrameworks().Select(f => f.FrameworkName);

#if NET451
            var currentFramework = FrameworkConstants.CommonFrameworks.Net451;
            var framework = frameworks.FirstOrDefault(f => f.IsDesktop())
                ?? NuGetFrameworkUtility.GetNearest(
                        frameworks,
                        FrameworkConstants.CommonFrameworks.NetStandard12,
                        f => new NuGetFramework(f));
#else
            var currentFramework = FrameworkConstants.CommonFrameworks.NetCoreApp10;
            var framework = NuGetFrameworkUtility.GetNearest(
                    frameworks,
                    FrameworkConstants.CommonFrameworks.NetCoreApp10,
                    f => new NuGetFramework(f))
                    // TODO remove fallback to dnxcore50
                    ?? NuGetFrameworkUtility.GetNearest(
                    frameworks,
                    FrameworkConstants.CommonFrameworks.DnxCore50,
                    f => new NuGetFramework(f));
#endif

            if (framework == null)
            {
                throw new OperationException(
                    $"The project '{projectFile.Name}' doesn't target a framework compatible with '{ currentFramework.GetShortFolderName() }'. " +
                    "The project must have a compatible framework in order to use the Entity Framework .NET Core CLI Commands.");
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
