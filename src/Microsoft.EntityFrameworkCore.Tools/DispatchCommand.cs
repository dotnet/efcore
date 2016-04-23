// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETCOREAPP1_0
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Tools.Cli;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class DispatchCommand
    {
        private static readonly string ProjectCommand
            = typeof(ExecuteCommand).GetTypeInfo().Assembly.GetName().Name;

        public static CommandLineApplication Create()
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "dotnet ef",
                FullName = "Entity Framework .NET Core CLI Commands Dispatcher"
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

                Reporter.Verbose.WriteLine(ToolsStrings.LogBeginDispatch(ProjectCommand, projectFile.Name));

                var buildBasePath = buildBasePathOption.Value();
                if (buildBasePath != null && !Path.IsPathRooted(buildBasePath))
                {
                    // TODO this is a workaround for https://github.com/dotnet/cli/issues/2682
                    buildBasePath = Path.Combine(Directory.GetCurrentDirectory(), buildBasePath);
                }

                try
                {
                    bool isVerbose;
                    bool.TryParse(Environment.GetEnvironmentVariable(CommandContext.Variables.Verbose), out isVerbose);

                    return new ProjectDependenciesCommandFactory(
                       framework,
                       configuration,
                       outputOption.Value(),
                       buildBasePath,
                       projectFile.ProjectDirectory)
                       .Create(
                           ProjectCommand,
                           ExecuteCommand.CreateArgs(framework, configuration, buildBasePath, noBuildOption.HasValue(), isVerbose)
                                .Concat(app.RemainingArguments),
                           framework,
                           configuration)
                        .ForwardStdErr()
                        .ForwardStdOut()
                        .Execute()
                        .ExitCode;
                }
                catch (CommandUnknownException ex)
                {
                    Reporter.Verbose.WriteLine(ex.Message);
                    Reporter.Error.WriteLine(ToolsStrings.ProjectDependencyCommandNotFound(ProjectCommand));
                    return 1;
                }
            });

            return app;
        }
    }
}
#endif