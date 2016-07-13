// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet
{
    public class CommandLineOptions
    {
        public string Configuration { get; set; }
        public string StartupProject { get; set; }
        public string TargetProject { get; set; }
        public NuGetFramework Framework { get; set; }
        public string BuildBasePath { get; set; }
        public string BuildOutputPath { get; set; }
        public bool NoBuild { get; set; }
        public IList<string> RemainingArguments { get; set; }
        public bool IsVerbose { get; set; }
        public bool IsHelp { get; set; }

        public static CommandLineOptions Parse(string[] args)
        {
            // ReSharper disable once ArgumentsStyleLiteral
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "dotnet ef",
                FullName = "Entity Framework .NET Core CLI Commands"
            };

            var options = new CommandLineOptions();

            Configure(app, options);

            app.Execute(args);
            options.IsHelp = app.IsShowingInformation;

            return options;
        }
        
        private static void Configure(CommandLineApplication app, CommandLineOptions options)
        {
            var project = app.Option(
                "-p|--project <project>",
                "The project to target (defaults to the project in the current directory). Can be a path to a project.json or a project directory.");
            var startupProject = app.Option(
                "-s|--startup-project <project>",
                "The path to the project containing Startup (defaults to the target project). Can be a path to a project.json or a project directory.");
            var configuration = app.Option(
                "--configuration <configuration>",
                $"Configuration under which to load (defaults to {Constants.DefaultConfiguration})");
            var framework = app.Option(
                "-f|--framework <framework>",
                $"Target framework to load from the startup project (defaults to the framework most compatible with {FrameworkConstants.CommonFrameworks.NetCoreApp10}).");
            var buildBasePath = app.Option(
                "-b|--build-base-path <output_dir>",
                "Directory in which to find temporary outputs.");
            var output = app.Option(
                "-o|--output <output_dir>",
                "Directory in which to find outputs");
            var noBuild = app.Option("--no-build", "Do not build before executing.");
            var verbose = app.Option("--verbose", "Show verbose output");
            app.VersionOption("--version", Program.GetVersion);

            app.OnExecute(() =>
                {
                    options.TargetProject = project.Value();
                    options.StartupProject = startupProject.Value();
                    options.Configuration = configuration.Value();
                    options.Framework = framework.HasValue()
                        ? NuGetFramework.Parse(framework.Value())
                        : null;
                    options.BuildBasePath = buildBasePath.Value();
                    options.BuildOutputPath = output.Value();
                    options.NoBuild = noBuild.HasValue();
                    options.IsVerbose = verbose.HasValue();
                    options.RemainingArguments = app.RemainingArguments;
                });
        }
    }
}
