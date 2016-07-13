// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using NuGet.Frameworks;

// ReSharper disable ArgumentsStyleNamedExpression
namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class DispatchOperationExecutor
    {
        private readonly ProjectContextCommandFactory _commandFactory;
        private readonly string _dispatcherVersion;

        public DispatchOperationExecutor([NotNull] ProjectContextCommandFactory commandFactory, [CanBeNull] string dispatcherVersion)
        {
            Check.NotNull(commandFactory, nameof(commandFactory));
            _commandFactory = commandFactory;

            _dispatcherVersion = dispatcherVersion;
        }

        public virtual int Execute([NotNull] CommandLineOptions options)
        {
            Check.NotNull(options, nameof(options));

            var targetProjectPath = options.TargetProject ?? Directory.GetCurrentDirectory();

            var targetProject = ProjectReader.GetProject(targetProjectPath);

            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogUsingTargetProject(targetProject.Name));

            var startupProject = string.IsNullOrEmpty(options.StartupProject)
                ? targetProject
                : ProjectReader.GetProject(options.StartupProject);

            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogUsingStartupProject(startupProject.Name));

            options.Framework = options.Framework ?? GetCompatibleFramework(startupProject);
            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogUsingFramework(options.Framework.GetShortFolderName()));

            options.Configuration = options.Configuration ?? Constants.DefaultConfiguration;
            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogUsingConfiguration(options.Configuration));

            if (!options.NoBuild)
            {
                var buildExitCode = BuildCommandFactory.Create(
                    startupProject.ProjectFilePath,
                    options.Configuration,
                    options.Framework,
                    options.BuildBasePath,
                    options.BuildOutputPath)
                    .ForwardStdErr()
                    .ForwardStdOut()
                    .Execute()
                    .ExitCode;
                if (buildExitCode != 0)
                {
                    throw new OperationErrorException(ToolsDotNetStrings.BuildFailed(startupProject.Name));
                }
            }

            var startupProjectContext = ProjectContext.Create(
                startupProject.ProjectFilePath,
                options.Framework,
                RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers());

            var startupOutputPaths = startupProjectContext
                .GetOutputPaths(options.Configuration, options.BuildBasePath, options.BuildOutputPath);

            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogDataDirectory(startupOutputPaths.RuntimeOutputPath));

            // Workaround https://github.com/dotnet/cli/issues/3164
            var isExecutable = startupProject.GetCompilerOptions(options.Framework, options.Configuration).EmitEntryPoint
                               ?? startupProject.GetCompilerOptions(null, options.Configuration).EmitEntryPoint.GetValueOrDefault();

            var startupAssembly = isExecutable && (startupProjectContext.IsPortable || startupProjectContext.TargetFramework.IsDesktop())
                ? startupOutputPaths.RuntimeFiles.Executable
                : startupOutputPaths.RuntimeFiles.Assembly;

            var targetAssembly = targetProject.ProjectFilePath.Equals(startupProject.ProjectFilePath)
                ? startupAssembly
                // This assumes the target assembly is present in the startup project context and is a *.dll
                // TODO create a project context for target project as well to ensure filename is correct
                : Path.Combine(startupOutputPaths.RuntimeOutputPath,
                    targetProject.GetCompilerOptions(null, options.Configuration).OutputName + FileNameSuffixes.DotNet.DynamicLib);

            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogBeginDispatch(startupProject.Name));

            // TODO config file
            var dispatchArgs = CreateArgs(
                assembly: targetAssembly,
                startupAssembly: startupAssembly,
                dataDir: startupOutputPaths.RuntimeOutputPath,
                contentRootPath: startupProject.ProjectDirectory,
                projectDir: targetProject.ProjectDirectory,
                rootNamespace: targetProject.Name,
                dispatcherVersion: _dispatcherVersion,
                verbose: options.IsVerbose)
                .Concat(options.RemainingArguments);

            return _commandFactory.Create(startupProjectContext,
                options.Configuration,
                options.BuildBasePath,
                options.BuildOutputPath,
                dispatchArgs)
                .ForwardStdErr()
                .ForwardStdOut()
                .Execute()
                .ExitCode;
        }

        private static NuGetFramework GetCompatibleFramework(Project startupProject)
        {
            var frameworks = startupProject.GetTargetFrameworks().Select(i => i.FrameworkName);
            return NuGetFrameworkUtility.GetNearest(frameworks, FrameworkConstants.CommonFrameworks.NetCoreApp10, f => f)
                   ?? frameworks.FirstOrDefault();
        }

        private const string DispatcherVersionArgumentName = "--dispatcher-version";
        private const string AssemblyOptionTemplate = "--assembly";
        private const string StartupAssemblyOptionTemplate = "--startup-assembly";
        private const string DataDirectoryOptionTemplate = "--data-dir";
        private const string ProjectDirectoryOptionTemplate = "--project-dir";
        private const string ContentRootPathOptionTemplate = "--content-root-path";
        private const string RootNamespaceOptionTemplate = "--root-namespace";
        private const string VerboseOptionTemplate = "--verbose";

        private static IEnumerable<string> CreateArgs(
            [NotNull] string assembly,
            [NotNull] string startupAssembly,
            [NotNull] string dataDir,
            [NotNull] string projectDir,
            [NotNull] string contentRootPath,
            [NotNull] string rootNamespace,
            [CanBeNull] string dispatcherVersion,
            bool verbose)
            => new[]
            {
                AssemblyOptionTemplate, Check.NotEmpty(assembly, nameof(assembly)),
                StartupAssemblyOptionTemplate, Check.NotEmpty(startupAssembly, nameof(startupAssembly)),
                DataDirectoryOptionTemplate, Check.NotEmpty(dataDir, nameof(dataDir)),
                ProjectDirectoryOptionTemplate, Check.NotEmpty(projectDir, nameof(projectDir)),
                ContentRootPathOptionTemplate, Check.NotEmpty(contentRootPath, nameof(contentRootPath)),
                RootNamespaceOptionTemplate, Check.NotEmpty(rootNamespace, nameof(rootNamespace)),
            }
            .Concat(dispatcherVersion != null
                ? new[] { DispatcherVersionArgumentName, dispatcherVersion }
                : Enumerable.Empty<string>())
            .Concat(verbose
                ? new[] { VerboseOptionTemplate }
                : Enumerable.Empty<string>());
    }
}
