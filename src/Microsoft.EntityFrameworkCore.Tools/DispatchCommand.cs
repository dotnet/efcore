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
        private const string DataDirectoryOptionTemplate = "--data-dir";
        private const string ProjectDirectoryOptionTemplate = "--project-dir";
        private const string RootNamespaceOptionTemplate = "--root-namespace";
        private const string VerboseOptionTemplate = "--verbose";
            
        private static IEnumerable<string> CreateArgs(
            string assembly,
            string dispatcherVersion,
            string dataDir,
            string projectDir,
            string rootNamespace,
            bool verbose)
            => new[]
            {
                AssemblyOptionTemplate, assembly,
                DispatcherVersionArgumentName, dispatcherVersion,
                DataDirectoryOptionTemplate, dataDir,
                ProjectDirectoryOptionTemplate, projectDir,
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

            var noBuildOption = app.Option("--no-build", "Do not build before executing");

            var configurationOption = app.Option(
                      "-c|--configuration <CONFIGURATION>",
                      "Configuration under which to load");
            var frameworkOption = app.Option(
                      "-f|--framework <FRAMEWORK>",
                      "Target framework to load");
            var buildBasePathOption = app.Option(
                      "-b|--build-base-path <OUTPUT_DIR>",
                      "Directory in which to find temporary outputs");
            var outputOption = app.Option(
                      "-o|--output <OUTPUT_DIR>",
                      "Directory in which to find outputs");

            app.OnExecute(() =>
            {
                var project = Directory.GetCurrentDirectory();

                Reporter.Verbose.WriteLine(ToolsStrings.LogUsingProject(project));

                var projectFile = ProjectReader.GetProject(project);

                var framework = frameworkOption.HasValue()
                    ? NuGetFramework.Parse(frameworkOption.Value())
                    : null;

                if (framework == null)
                {
                    var frameworks = projectFile.GetTargetFrameworks().Select(i => i.FrameworkName);
                    framework = NuGetFrameworkUtility.GetNearest(frameworks, FrameworkConstants.CommonFrameworks.NetCoreApp10, f => f)
                                ?? frameworks.FirstOrDefault();

                    Reporter.Verbose.WriteLine(ToolsStrings.LogUsingFramework(framework.GetShortFolderName()));
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
                            projectFile.ProjectFilePath,
                            configuration,
                            framework,
                            buildBasePathOption.Value(),
                            outputOption.Value())
                        .ForwardStdErr()
                        .ForwardStdOut()
                        .Execute()
                        .ExitCode;
                    if (buildExitCode != 0)
                    {
                        throw new OperationException(ToolsStrings.BuildFailed(projectFile.Name));
                    }
                }

                var projectContext = ProjectContext.Create(
                    projectFile.ProjectFilePath,
                    framework,
                    RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers());

                var outputPaths = projectContext
                    .GetOutputPaths(configuration, buildBasePathOption.Value(), outputOption.Value());

                // TODO remove when https://github.com/dotnet/cli/issues/2645 is resolved
                Func<bool> isClassLibrary = () =>
                {
                    return outputPaths.RuntimeFiles == null
                        || (
                            framework.IsDesktop()
                                ? !Directory.Exists(outputPaths.RuntimeFiles.BasePath)
                                : !File.Exists(outputPaths.RuntimeFiles.RuntimeConfigJson) || !File.Exists(outputPaths.RuntimeFiles.DepsJson)
                            );
                };

                Reporter.Verbose.WriteLine(ToolsStrings.LogDataDirectory(outputPaths.RuntimeOutputPath));

                var assembly = Path.Combine(outputPaths.RuntimeOutputPath,
                    projectFile.GetCompilerOptions(framework, configuration).OutputName + ".dll");

                Reporter.Verbose.WriteLine(ToolsStrings.LogBeginDispatch(ProjectDependencyToolName, projectFile.Name));

                try
                {
                    bool isVerbose;
                    bool.TryParse(Environment.GetEnvironmentVariable(CommandContext.Variables.Verbose), out isVerbose);
                    var dispatchArgs = CreateArgs(
                                    assembly: assembly,
                                    dispatcherVersion: ThisAssemblyVersion,
                                    dataDir: outputPaths.RuntimeOutputPath,
                                    projectDir: projectFile.ProjectDirectory,
                                    rootNamespace: projectFile.Name,
                                    verbose: isVerbose)
                                .Concat(app.RemainingArguments);
                                
                    var buildBasePath = buildBasePathOption.Value();
                    if (buildBasePath != null && !Path.IsPathRooted(buildBasePath))
                    {
                        // ProjectDependenciesCommandFactory cannot handle relative build base paths.
                        buildBasePath = Path.Combine(Directory.GetCurrentDirectory(), buildBasePath);
                    }

                    return new ProjectDependenciesCommandFactory(
                            framework,
                            configuration,
                            outputOption.Value(),
                            buildBasePath,
                            projectFile.ProjectDirectory)
                        .Create(ProjectDependencyToolName, dispatchArgs, framework, configuration)
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
                        Reporter.Error.WriteLine(ToolsStrings.ClassLibrariesNotSupportedInCli(fwlink));
                    }
                    else if (framework.IsDesktop() && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Reporter.Error.WriteLine(ToolsStrings.DesktopCommandsRequiresWindows(framework.GetShortFolderName()));
                    }
                    else
                    {
                        Reporter.Error.WriteLine(ToolsStrings.ProjectDependencyCommandNotFound(DispatcherToolName, ProjectDependencyToolName, fwlink));
                    }

                    return 1;
                }
            });

            return app;
        }
    }
}
#endif