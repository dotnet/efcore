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
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using NuGet.Frameworks;

#if !NET451
using Microsoft.EntityFrameworkCore.Tools.Cli.Loader;
#endif

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class OperationExecutor
    {
        private const string DataDirEnvName = "ADONET_DATA_DIR";

        private readonly LazyRef<MigrationsOperations> _migrationsOperations;
        private readonly LazyRef<DatabaseOperations> _databaseOperations;
        private readonly LazyRef<DbContextOperations> _contextOperations;

#if NET451
        private static readonly NuGetFramework DefaultFramework = FrameworkConstants.CommonFrameworks.Net451;
#else
        private static readonly NuGetFramework DefaultFramework = FrameworkConstants.CommonFrameworks.NetCoreApp10;
#endif

        public OperationExecutor(
            [NotNull] CommonOptions options,
            [CanBeNull] string startupProjectPath,
            [CanBeNull] string environment)
        {
            var projectFile = Path.Combine(Directory.GetCurrentDirectory(), Project.FileName);
            var project = ProjectReader.GetProject(projectFile);

            startupProjectPath = startupProjectPath ?? projectFile;

            var projectConfiguration = options.Configuration ?? Constants.DefaultConfiguration;
            var projectFramework = options.Framework;

            var startupConfiguration = options.Configuration ?? Constants.DefaultConfiguration;
            var startupProjectContext = GetCompatibleStartupProjectContext(startupProjectPath, projectFramework);
            var startupFramework = startupProjectContext.TargetFramework;

            var externalStartup = project.ProjectFilePath != startupProjectContext.ProjectFile.ProjectFilePath;

            if (externalStartup && !options.NoBuild)
            {
                Reporter.Verbose.WriteLine(ToolsCliStrings.LogBuildFailed.Bold().Black());

                var buildExitCode = BuildCommandFactory.Create(startupProjectPath,
                        startupConfiguration,
                        startupFramework,
                        options.BuildBasePath,
                        output: null)
                    .ForwardStdOut()
                    .ForwardStdErr()
                    .Execute()
                    .ExitCode;

                if (buildExitCode != 0)
                {
                    throw new OperationException(ToolsCliStrings.LogBuildFailed);
                }

                Reporter.Verbose.WriteLine(ToolsCliStrings.LogBuildSucceeded.Bold().Black());
            }

            var runtimeOutputPath = startupProjectContext.GetOutputPaths(startupConfiguration)?.RuntimeOutputPath;
            if (!string.IsNullOrEmpty(runtimeOutputPath))
            {
                Reporter.Verbose.WriteLine(
                    ToolsCliStrings.LogDataDirectory(runtimeOutputPath));
                Environment.SetEnvironmentVariable(DataDirEnvName, runtimeOutputPath);
#if NET451
                AppDomain.CurrentDomain.SetData("DataDirectory", runtimeOutputPath);
#endif
            }

            var startupAssemblyName = startupProjectContext.ProjectFile.GetCompilerOptions(startupFramework, startupConfiguration).OutputName;
            var assemblyName = project.GetCompilerOptions(projectFramework, projectConfiguration).OutputName;
            var projectDir = project.ProjectDirectory;
            var startupProjectDir = startupProjectContext.ProjectFile.ProjectDirectory;
            var rootNamespace = project.Name;

            var projectAssembly = Assembly.Load(new AssemblyName { Name = assemblyName });
#if NET451
            // TODO use app domains
            var startupAssemblyLoader = new AssemblyLoader(Assembly.Load);
#else
            AssemblyLoader startupAssemblyLoader;
            if (externalStartup)
            {
                var assemblyLoadContext = startupProjectContext.CreateLoadContext(
                    RuntimeEnvironment.GetRuntimeIdentifier(),
                    Constants.DefaultConfiguration);
                startupAssemblyLoader = new AssemblyLoader(assemblyLoadContext.LoadFromAssemblyName);
            }
            else
            {
                startupAssemblyLoader = new AssemblyLoader(Assembly.Load);
            }
#endif
            var startupAssembly = startupAssemblyLoader.Load(startupAssemblyName);

            _contextOperations = new LazyRef<DbContextOperations>(
                          () => new DbContextOperations(
                              new LoggerProvider(name => new ConsoleCommandLogger(name)),
                              projectAssembly,
                              startupAssembly,
                              environment,
                              startupProjectDir));
            _databaseOperations = new LazyRef<DatabaseOperations>(
                () => new DatabaseOperations(
                    new LoggerProvider(name => new ConsoleCommandLogger(name)),
                    startupAssemblyLoader,
                    startupAssembly,
                    environment,
                    projectDir,
                    startupProjectDir,
                    rootNamespace));
            _migrationsOperations = new LazyRef<MigrationsOperations>(
                () => new MigrationsOperations(
                    new LoggerProvider(name => new ConsoleCommandLogger(name)),
                    projectAssembly,
                    startupAssemblyLoader,
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

        private ProjectContext GetCompatibleStartupProjectContext(string startupProjectPath, NuGetFramework projectFramework)
        {
            var startupProject = ProjectReader.GetProject(startupProjectPath);
            var frameworks = startupProject.GetTargetFrameworks()
                .Select(f => f.FrameworkName);

            var startupFramework = frameworks.FirstOrDefault(f => f.Equals(projectFramework));

            if (startupFramework == null)
            {
                if (projectFramework.IsDesktop())
                {
                    startupFramework = frameworks.FirstOrDefault(f => f.IsDesktop())
                        ?? NuGetFrameworkUtility.GetNearest(
                            frameworks,
                            FrameworkConstants.CommonFrameworks.NetStandard15,
                            f => f);
                }
                else
                {
                    startupFramework = NuGetFrameworkUtility.GetNearest(
                            frameworks,
                            FrameworkConstants.CommonFrameworks.NetCoreApp10,
                            f => f)
                        // TODO remove fallback to dnxcore50
                        ?? NuGetFrameworkUtility.GetNearest(
                            frameworks,
                            FrameworkConstants.CommonFrameworks.DnxCore50,
                            f => f);
                }
            }

            if (startupFramework == null)
            {
                throw new OperationException(
                    ToolsCliStrings.IncompatibleStartupProject(startupProject.Name, projectFramework.GetShortFolderName()));
            }

            Reporter.Verbose.WriteLine(
                ToolsCliStrings.LogUsingFramework(startupFramework.GetShortFolderName(), startupProject.Name).Bold().Black());

            return new ProjectContextBuilder()
                .WithProject(startupProject)
                .WithTargetFramework(startupFramework)
                .WithRuntimeIdentifiers(RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers())
                .Build();
        }
    }
}
