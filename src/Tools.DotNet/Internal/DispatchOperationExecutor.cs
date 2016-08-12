// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable ArgumentsStyleNamedExpression
namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class DispatchOperationExecutor
    {
        private readonly EfConsoleCommandSpecFactory _commandSpecFactory;
        private readonly ProjectContextFactory _projectFactory;
        private readonly IProjectBuilder _projectBuilder;

        public DispatchOperationExecutor(
            [NotNull] ProjectContextFactory projectFactory,
            [NotNull] EfConsoleCommandSpecFactory commandSpecFactory,
            [NotNull] IProjectBuilder projectBuilder)
        {
            Check.NotNull(projectFactory, nameof(projectFactory));
            Check.NotNull(commandSpecFactory, nameof(commandSpecFactory));
            Check.NotNull(projectBuilder, nameof(projectBuilder));

            _commandSpecFactory = commandSpecFactory;
            _projectFactory = projectFactory;
            _projectBuilder = projectBuilder;
        }

        public virtual int Execute([NotNull] CommandLineOptions options)
        {
            Check.NotNull(options, nameof(options));

            var targetProjectPath = options.TargetProject ?? Directory.GetCurrentDirectory();

            var targetProject = _projectFactory.Create(targetProjectPath,
                    options.Framework,
                    options.Configuration,
                    options.BuildOutputPath);

            var startupProject = string.IsNullOrEmpty(options.StartupProject)
                ? targetProject
                : _projectFactory.Create(options.StartupProject,
                    targetProject.TargetFramework,
                    options.Configuration,
                    options.BuildOutputPath);

            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogUsingTargetProject(targetProject.ProjectName));
            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogUsingStartupProject(startupProject.ProjectName));
            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogUsingFramework(startupProject.TargetFramework.GetShortFolderName()));
            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogUsingConfiguration(startupProject.Configuration));

            if (!options.NoBuild)
            {
                _projectBuilder.EnsureBuild(startupProject);
            }

            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogDataDirectory(startupProject.TargetDirectory));

            var commandSpec = _commandSpecFactory.Create(startupProject, targetProject, options.IsVerbose, options.RemainingArguments);

            Reporter.Verbose.WriteLine(ToolsDotNetStrings.LogBeginDispatch(startupProject.ProjectName));

            return Command.Create(commandSpec)
                .ForwardStdErr()
                .ForwardStdOut()
                .Execute()
                .ExitCode;
        }
    }
}
