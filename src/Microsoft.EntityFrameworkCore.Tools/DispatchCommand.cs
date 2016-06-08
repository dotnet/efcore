// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETCOREAPP1_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class DispatchCommand
    {
        private const string DispatcherToolName
            = "Microsoft.EntityFrameworkCore.Tools";

        private const string ProjectDependencyToolName
            = "Microsoft.EntityFrameworkCore.Design";

        private static readonly Assembly ThisAssembly = typeof(DispatchCommand).GetTypeInfo().Assembly;
        private static readonly string ThisAssemblyVersion = ThisAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? ThisAssembly.GetName().Version.ToString();

        private const string DispatcherVersionArgumentName = "--dispatcher-version";
        private const string AssemblyOptionTemplate = "--assembly";
        private const string StartupAssemblyOptionTemplate = "--startup-assembly";
        private const string DataDirectoryOptionTemplate = "--data-dir";
        private const string ProjectDirectoryOptionTemplate = "--project-dir";
        private const string AppBaseDirectoryOptionTemplate = "--app-base-dir";
        private const string RootNamespaceOptionTemplate = "--root-namespace";
        private const string VerboseOptionTemplate = "--verbose";

        private static IEnumerable<string> CreateArgs(
            string assembly,
            string startupAssembly,
            string dispatcherVersion,
            string dataDir,
            string projectDir,
            string startupTargetDir,
            string rootNamespace,
            bool verbose)
            => new[]
            {
                AssemblyOptionTemplate, assembly,
                StartupAssemblyOptionTemplate, startupAssembly,
                DispatcherVersionArgumentName, dispatcherVersion,
                DataDirectoryOptionTemplate, dataDir,
                ProjectDirectoryOptionTemplate, projectDir,
                AppBaseDirectoryOptionTemplate, startupTargetDir,
                RootNamespaceOptionTemplate, rootNamespace,
                verbose ? VerboseOptionTemplate : string.Empty
            };

        public static CommandLineApplication Create()
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "dotnet ef",
                FullName = "Entity Framework .NET Core CLI Commands"
            };

            // TODO better help output https://github.com/aspnet/EntityFramework/issues/5188
            // app.HelpOption("-h|--help");

            var targetProjectOption = app.Option(
                "-p|--project <PROJECT>",
                "The project to target (defaults to the project in the current directory). Can be a path to a project.json or a project directory.");
            var startupProjectOption = app.Option(
                "-s|--startup-project <PROJECT>",
                "The path to the project containing Startup (defaults to the target project). Can be a path to a project.json or a project directory.");
            var configurationOption = app.Option(
                "-c|--configuration <CONFIGURATION>",
                $"Configuration under which to load (defaults to {Constants.DefaultConfiguration})");
            var frameworkOption = app.Option(
                "-f|--framework <FRAMEWORK>",
                $"Target framework to load from the startup project (defaults to the framework most compatible with {FrameworkConstants.CommonFrameworks.NetCoreApp10}).");
            var buildBasePathOption = app.Option(
                "-b|--build-base-path <OUTPUT_DIR>",
                "Directory in which to find temporary outputs.");
            var outputOption = app.Option(
                "-o|--output <OUTPUT_DIR>",
                "Directory in which to find outputs");
            var noBuildOption = app.Option("--no-build", "Do not build before executing.");

            app.OnExecute(() =>
            {
                var targetProjectPath = targetProjectOption.HasValue()
                    ? targetProjectOption.Value()
                    : Directory.GetCurrentDirectory();

                Project targetProject;
                if (!ProjectReader.TryGetProject(targetProjectPath, out targetProject))
                {
                    throw new OperationException($"Could not load target project '{targetProjectPath}'");
                }

                Reporter.Verbose.WriteLine(ToolsStrings.LogUsingTargetProject(targetProject.Name));

                Project startupProject;
                if (startupProjectOption.HasValue())
                {
                    var startupPath = startupProjectOption.Value();
                    if (!ProjectReader.TryGetProject(startupPath, out startupProject))
                    {
                        throw new OperationException($"Could not load project '{startupPath}'");
                    }
                }
                else
                {
                    startupProject = targetProject;
                }

                Reporter.Verbose.WriteLine(ToolsStrings.LogUsingStartupProject(startupProject.Name));

                var startupFramework = frameworkOption.HasValue()
                    ? NuGetFramework.Parse(frameworkOption.Value())
                    : null;

                if (startupFramework == null)
                {
                    var frameworks = startupProject.GetTargetFrameworks().Select(i => i.FrameworkName);
                    startupFramework = NuGetFrameworkUtility.GetNearest(frameworks, FrameworkConstants.CommonFrameworks.NetCoreApp10, f => f)
                                ?? frameworks.FirstOrDefault();

                    Reporter.Verbose.WriteLine(ToolsStrings.LogUsingFramework(startupFramework.GetShortFolderName()));
                }

                var configuration = configurationOption.Value();

                if (configuration == null)
                {
                    configuration = Constants.DefaultConfiguration;

                    Reporter.Verbose.WriteLine(ToolsStrings.LogUsingConfiguration(configuration));
                }

                if (!noBuildOption.HasValue())
                {
                    var buildExitCode = BuildCommandFactory.Create(
                            startupProject.ProjectFilePath,
                            configuration,
                            startupFramework,
                            buildBasePathOption.Value(),
                            outputOption.Value())
                        .ForwardStdErr()
                        .ForwardStdOut()
                        .Execute()
                        .ExitCode;
                    if (buildExitCode != 0)
                    {
                        throw new OperationException(ToolsStrings.BuildFailed(startupProject.Name));
                    }
                }

                var startupProjectContext = ProjectContext.Create(
                    startupProject.ProjectFilePath,
                    startupFramework,
                    RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers());

                var startupOutputPaths = startupProjectContext
                    .GetOutputPaths(configuration, buildBasePathOption.Value(), outputOption.Value());

                // TODO remove when https://github.com/dotnet/cli/issues/2645 is resolved
                Func<bool> isClassLibrary = () =>
                {
                    return startupOutputPaths.RuntimeFiles == null
                        || (
                            startupFramework.IsDesktop()
                                ? !Directory.Exists(startupOutputPaths.RuntimeFiles.BasePath)
                                : !File.Exists(startupOutputPaths.RuntimeFiles.RuntimeConfigJson) || !File.Exists(startupOutputPaths.RuntimeFiles.DepsJson)
                            );
                };

                Reporter.Verbose.WriteLine(ToolsStrings.LogDataDirectory(startupOutputPaths.RuntimeOutputPath));

                // Workaround https://github.com/dotnet/cli/issues/3164
                var isExecutable = startupProject.GetCompilerOptions(startupFramework, configuration).EmitEntryPoint.HasValue
                    ? startupProject.GetCompilerOptions(startupFramework, configuration).EmitEntryPoint.Value
                    : startupProject.GetCompilerOptions(null, configuration).EmitEntryPoint.GetValueOrDefault();

                var startupAssembly = isExecutable
                    ? startupOutputPaths.RuntimeFiles.Executable
                    : startupOutputPaths.RuntimeFiles.Assembly;

                var targetAssembly = targetProject.ProjectFilePath.Equals(startupProject.ProjectFilePath)
                    ? startupAssembly
                    // This assumes the target assembly is present in the startup project context and is a *.dll
                    // TODO create a project context for target project as well to ensure filename is correct
                    : Path.Combine(startupOutputPaths.RuntimeOutputPath,
                        targetProject.GetCompilerOptions(null, configuration).OutputName + FileNameSuffixes.DotNet.DynamicLib);

                Reporter.Verbose.WriteLine(ToolsStrings.LogBeginDispatch(ProjectDependencyToolName, startupProject.Name));

                try
                {
                    bool isVerbose;
                    bool.TryParse(Environment.GetEnvironmentVariable(CommandContext.Variables.Verbose), out isVerbose);
                    var dispatchArgs = CreateArgs(
                                    assembly: targetAssembly,
                                    startupAssembly: startupOutputPaths.RuntimeFiles.Assembly,
                                    dispatcherVersion: ThisAssemblyVersion,
                                    dataDir: startupOutputPaths.RuntimeOutputPath,
                                    startupTargetDir: startupOutputPaths.RuntimeOutputPath,
                                    projectDir: targetProject.ProjectDirectory,
                                    rootNamespace: targetProject.Name,
                                    verbose: isVerbose)
                                .Concat(app.RemainingArguments);

                    var buildBasePath = buildBasePathOption.Value();
                    if (buildBasePath != null && !Path.IsPathRooted(buildBasePath))
                    {
                        // ProjectDependenciesCommandFactory cannot handle relative build base paths.
                        buildBasePath = Path.Combine(Directory.GetCurrentDirectory(), buildBasePath);
                    }

                    return new ProjectDependenciesCommandFactory(
                            startupFramework,
                            configuration,
                            outputOption.Value(),
                            buildBasePath,
                            startupProject.ProjectDirectory)
                        .Create(ProjectDependencyToolName, dispatchArgs, startupFramework, configuration)
                        .ForwardStdErr()
                        .ForwardStdOut()
                        .Execute()
                        .ExitCode;
                }
                catch (CommandUnknownException ex)
                {
                    Reporter.Verbose.WriteLine(ex.Message);

                    var fwlink = "http://go.microsoft.com/fwlink/?LinkId=798221";

                    if (isClassLibrary())
                    {
                        Reporter.Error.WriteLine(
                            ToolsStrings.ClassLibrariesNotSupportedInCli(startupProject.Name, fwlink).Bold().Red());
                    }
                    else if (startupFramework.IsDesktop() && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Reporter.Error.WriteLine(
                            ToolsStrings.DesktopCommandsRequiresWindows(startupFramework.GetShortFolderName()).Bold().Red());
                    }
                    else
                    {
                        Reporter.Error.WriteLine(
                            ToolsStrings.ProjectDependencyCommandNotFound(
                                startupProject.Name,
                                ProjectDependencyToolName,
                                DispatcherToolName,
                                fwlink).Bold().Red());
                    }

                    return 1;
                }
            });

            return app;
        }
    }
}
#endif
