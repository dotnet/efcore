// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools
{
    internal class ProjectOptions
    {
        private CommandOption _project;
        private CommandOption _startupProject;
        private CommandOption _framework;
        private CommandOption _configuration;
        private CommandOption _runtime;
        private CommandOption _msbuildprojectextensionspath;
        private CommandOption _noBuild;

        public CommandOption Project
            => _project;

        public CommandOption StartupProject
            => _startupProject;

        public CommandOption Framework
            => _framework;

        public CommandOption Configuration
            => _configuration;

        public CommandOption Runtime
            => _runtime;

        public CommandOption MSBuildProjectExtensionsPath
            => _msbuildprojectextensionspath;

        public CommandOption NoBuild
            => _noBuild;

        public void Configure(CommandLineApplication command)
        {
            _project = command.Option("-p|--project <PROJECT>", Resources.ProjectDescription);
            _startupProject = command.Option("-s|--startup-project <PROJECT>", Resources.StartupProjectDescription);
            _framework = command.Option("--framework <FRAMEWORK>", Resources.FrameworkDescription);
            _configuration = command.Option("--configuration <CONFIGURATION>", Resources.ConfigurationDescription);
            _runtime = command.Option("--runtime <RUNTIME_IDENTIFIER>", Resources.RuntimeDescription);
            _msbuildprojectextensionspath = command.Option("--msbuildprojectextensionspath <PATH>", Resources.ProjectExtensionsDescription);
            _noBuild = command.Option("--no-build", Resources.NoBuildDescription);
        }
    }
}
