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
using Microsoft.DotNet.ProjectModel.Loader;
using Microsoft.Extensions.PlatformAbstractions;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class OperationExecutor
    {
        private const string ExecutorTypeName = "Microsoft.EntityFrameworkCore.Design.OperationExecutor";

        private readonly Assembly _commandsAssembly;
        private readonly object _executor;
        private readonly string _startupProjectDir;

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
                throw new CommandException("Build failed.");
            }

            Reporter.Verbose.WriteLine("Build succeeded.".Bold().Black());

            var runtimeOutputPath = startupProjectContext.GetOutputPaths(DefaultConfiguration)?.RuntimeOutputPath;
            if (!string.IsNullOrEmpty(runtimeOutputPath))
            {
                // TODO set data directory in AppDomain when/if this supports desktop .NET
                Environment.SetEnvironmentVariable(DataDirEnvName, runtimeOutputPath);
            }

            var projectFile = ProjectReader.GetProject(project);
            var startupAssemblyName = startupProjectContext.ProjectFile.Name;
            var assemblyName = projectFile.Name;
            var projectDir = projectFile.ProjectDirectory;
            _startupProjectDir = startupProjectContext.ProjectFile.ProjectDirectory;
            var rootNamespace = projectFile.Name;
            var assemblyLoadContext = startupProjectContext.CreateLoadContext();

            _commandsAssembly = assemblyLoadContext.LoadFromAssemblyName(
                new AssemblyName("Microsoft.EntityFrameworkCore.Commands"));

            var assemblyLoader = Activator.CreateInstance(
                _commandsAssembly.GetType(
                    "Microsoft.EntityFrameworkCore.Design.AssemblyLoader",
                    throwOnError: true,
                    ignoreCase: false),
                (Func<AssemblyName, Assembly>)assemblyLoadContext.LoadFromAssemblyName);

            var logHandler = Activator.CreateInstance(
                _commandsAssembly.GetType(
                    "Microsoft.EntityFrameworkCore.Design.OperationLogHandler",
                    throwOnError: true,
                    ignoreCase: false),
                (Action<string>)(m => Reporter.Error.WriteLine(m.Bold().Red())),
                (Action<string>)(m => Reporter.Error.WriteLine(m.Bold().Yellow())),
                (Action<string>)Reporter.Error.WriteLine,
                (Action<string>)(m => Reporter.Verbose.WriteLine(m.Bold().Black())),
                (Action<string>)(m => Reporter.Verbose.WriteLine(m.Bold().Black())));

            _executor = Activator.CreateInstance(
                _commandsAssembly.GetType(ExecutorTypeName, throwOnError: true, ignoreCase: false),
                logHandler,
                new Dictionary<string, string>
            {
                    ["targetName"] = assemblyName,
                    ["startupTargetName"] = startupAssemblyName,
                    ["environment"] = environment,
                    ["projectDir"] = projectDir,
                    ["rootNamespace"] = rootNamespace
                },
                assemblyLoader);
        }

        public virtual void DropDatabase(
            [CanBeNull] string contextType,
            [NotNull] Func<string, string, bool> confirmCheck)
            => Execute<object>(
                "DropDatabase",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType,
                    ["confirmCheck"] = confirmCheck
                });

        public virtual IDictionary AddMigration(
            [NotNull] string name,
            [CanBeNull] string outputDir,
            [CanBeNull] string contextType)
           => Execute<IDictionary>(
               "AddMigration",
               new Dictionary<string, object>
               {
                   ["name"] = name,
                   ["outputDir"] = outputDir,
                   ["contextType"] = contextType
               });

        public virtual void UpdateDatabase([CanBeNull] string targetMigration, [CanBeNull] string contextType)
            => Execute<object>(
                "UpdateDatabase",
                new Dictionary<string, object>
                {
                    ["targetMigration"] = targetMigration,
                    ["contextType"] = contextType
                });

        public virtual string ScriptMigration(
            [CanBeNull] string fromMigration,
            [CanBeNull] string toMigration,
            bool idempotent,
            [CanBeNull] string contextType)
            => Execute<string>(
                "ScriptMigration",
                new Dictionary<string, object>
                {
                    ["fromMigration"] = fromMigration,
                    ["toMigration"] = toMigration,
                    ["idempotent"] = idempotent,
                    ["contextType"] = contextType
                });

        public virtual IEnumerable<string> RemoveMigration([CanBeNull] string contextType, bool force)
            => Execute<IEnumerable<string>>(
                "RemoveMigration",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType,
                    ["force"] = force
                });

        public virtual IEnumerable<IDictionary> GetContextTypes()
            => Execute<IEnumerable<IDictionary>>(
                "GetContextTypes",
                new Dictionary<string, object>());

        public virtual IEnumerable<IDictionary> GetMigrations([CanBeNull] string contextType)
            => Execute<IEnumerable<IDictionary>>(
                "GetMigrations",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType
                });

        public virtual IEnumerable<string> ReverseEngineer(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            [CanBeNull] string dbContextClassName,
            [NotNull] IEnumerable<string> schemaFilters,
            [NotNull] IEnumerable<string> tableFilters,
            bool useDataAnnotations,
            bool overwriteFiles)
            => Execute<IEnumerable<string>>(
                "ReverseEngineer",
                new Dictionary<string, object>
                {
                    ["provider"] = provider,
                    ["connectionString"] = connectionString,
                    ["outputDir"] = outputDir,
                    ["dbContextClassName"] = dbContextClassName,
                    ["schemaFilters"] = schemaFilters,
                    ["tableFilters"] = tableFilters,
                    ["useDataAnnotations"] = useDataAnnotations,
                    ["overwriteFiles"] = overwriteFiles
                });

        private ProjectContext GetCompatibleProjectContext(string projectPath)
        {
            var projectFile = ProjectReader.GetProject(projectPath);
            var frameworks = projectFile.GetTargetFrameworks().Select(f => f.FrameworkName);
            var framework = NuGetFrameworkUtility.GetNearest(
                frameworks,
                // TODO: Use netcoreapp
                FrameworkConstants.CommonFrameworks.NetStandardApp15,
                f => new NuGetFramework(f))
                // TODO: Remove with dnxcore50
                ?? NuGetFrameworkUtility.GetNearest(
                    frameworks,
                    FrameworkConstants.CommonFrameworks.DnxCore50,
                    f => new NuGetFramework(f));
            if (framework == null)
            {
                throw new CommandException(
                    "The project '" + projectFile.Name + "' doesn't target a framework compatible with .NET Standard "+
                    "App 1.5. You must target a compatible framework such as 'netstandard1.3' in order to use the " +
                    "Entity Framework .NET Core CLI Commands.");
            }

            Reporter.Verbose.WriteLine(
                ("Using framework '" + framework + "' for project '" + projectFile.Name + "'").Bold().Black());

            return new ProjectContextBuilder()
                .WithProject(projectFile)
                .WithTargetFramework(framework)
                .WithRuntimeIdentifiers(PlatformServices.Default.Runtime.GetAllCandidateRuntimeIdentifiers())
                .Build();
        }

        private T Execute<T>(string operation, IDictionary args)
        {
            var resultHandler = (dynamic)Activator.CreateInstance(
                _commandsAssembly.GetType(
                    "Microsoft.EntityFrameworkCore.Design.OperationResultHandler",
                    throwOnError: true,
                    ignoreCase: false));

            var currentDirectory = Directory.GetCurrentDirectory();

            Reporter.Verbose.WriteLine(("Using current directory '" + _startupProjectDir + "'.").Bold().Black());

            Directory.SetCurrentDirectory(_startupProjectDir);
            try
            {
                Activator.CreateInstance(
                    _commandsAssembly.GetType(ExecutorTypeName + "+" + operation, throwOnError: true, ignoreCase: true),
                    _executor,
                    resultHandler,
                    args);
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
            }

            if (resultHandler.ErrorType != null)
            {
                throw new OperationException(
                    resultHandler.ErrorType,
                    resultHandler.ErrorStackTrace,
                    resultHandler.ErrorMessage);
            }
            if (resultHandler.HasResult)
            {
                return (T)resultHandler.Result;
            }

            return default(T);
        }
    }
}
