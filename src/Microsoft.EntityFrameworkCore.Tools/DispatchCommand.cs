// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETCOREAPP1_0
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Tools.Cli;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.PlatformAbstractions;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class DispatchCommand
    {
        private readonly static string ProjectCommand
            = typeof(ExecuteCommand).GetTypeInfo().Assembly.GetName().Name;

        public static CommandLineApplication Configure([NotNull] string[] args)
        {
            EfCommandLineApplication app;
            var help = false;
            if (args.Length == 0
                || args.FirstOrDefault(a => a.Equals("-h") || a.Equals("--help") || a.Equals("--version")) != null)
            {
                // show common output options
                app = ExecuteCommand.Configure();
                help = true;
            }
            else
            {
                app = new EfCommandLineApplication(throwOnUnexpectedArg: false);
            }

            var noBuildOption = app.Option("--no-build", "Do not build project before executing");

            var configurationOption = app.Option(
                      "-c|--configuration <CONFIGURATION>",
                      "Configuration under which to load");
            var outputOption = app.Option(
                      "-o|--output <OUTPUT_DIR>",
                      "Directory in which to find outputs");
            var buildBasePathOption = app.Option(
                      "-b|--build-base-path <OUTPUT_DIR>",
                      "Directory in which to find temporary outputs");
            var frameworkOption = app.Option(
                      "-f|--framework <FRAMEWORK>",
                      "Target framework to load");

            app.OnExecute(() =>
            {
                if (help)
                {
                    app.WriteLogo();
                    app.ShowHelp();
                    return 0;
                }

                var project = Directory.GetCurrentDirectory();

                Reporter.Verbose.WriteLine("Using project '" + project + "'.");

                var projectFile = ProjectReader.GetProject(project);

                var framework = frameworkOption.HasValue()
                    ? NuGetFramework.Parse(frameworkOption.Value())
                    : null;

                if (framework == null)
                {
                    var frameworks = projectFile.GetTargetFrameworks().Select(i => i.FrameworkName);
                    framework = NuGetFrameworkUtility.GetNearest(frameworks, FrameworkConstants.CommonFrameworks.NetCoreApp10, f => f)
                                ?? frameworks.FirstOrDefault();

                    Reporter.Verbose.WriteLine("Using framework '" + framework.GetShortFolderName() + "'.");
                }

                var configuration = configurationOption.Value();

                if (configuration == null)
                {
                    configuration = Constants.DefaultConfiguration;

                    Reporter.Verbose.WriteLine("Using configuration '" + configuration + "'.");
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
                        throw new OperationException("Build failed.");
                    }
                }

                var projectContext = new ProjectContextBuilder()
                   .WithProject(projectFile)
                   .WithTargetFramework(framework)
                   .WithRuntimeIdentifiers(PlatformServices.Default.Runtime.GetAllCandidateRuntimeIdentifiers())
                   .Build();

                Reporter.Verbose.WriteLine($"Dispatching to {ProjectCommand} in project");

                try
                {
                    return new ProjectDependenciesCommandFactory(
                       projectContext.TargetFramework,
                       configuration,
                       outputOption.Value(),
                       buildBasePathOption.Value(),
                       projectContext.ProjectDirectory)
                       .Create(
                           ProjectCommand,
                           args,
                           projectContext.TargetFramework,
                           configuration)
                        .ForwardStdErr()
                        .ForwardStdOut()
                        .Execute()
                        .ExitCode;
                }
                catch(CommandUnknownException)
                {
                    Reporter.Error.WriteLine(
                        $"Could not invoke this command on the project. Check that the version of {ProjectCommand} in \"tools\" and \"dependencies\" are the same");
                    return 1;
                }
            });

            return app;
        }
    }
}
#endif