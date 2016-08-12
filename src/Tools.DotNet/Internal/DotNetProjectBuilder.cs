// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class DotNetProjectBuilder : IProjectBuilder
    {
        public void EnsureBuild(IProjectContext project)
        {
            var buildExitCode = CreateBuildCommand(project)
                .ForwardStdErr()
                .ForwardStdOut()
                .Execute()
                .ExitCode;

            if (buildExitCode != 0)
            {
                throw new OperationErrorException(ToolsDotNetStrings.BuildFailed(project.ProjectName));
            }
        }

        private static ICommand CreateBuildCommand([NotNull] IProjectContext projectContext)
        {
            if (!(projectContext is DotNetProjectContext))
            {
                throw new PlatformNotSupportedException("Currently only .NET Core Projects (project.json/xproj) are supported");
            }

            var args = new List<string>
            {
                projectContext.ProjectFullPath,
                "--configuration", projectContext.Configuration,
                "--framework", projectContext.TargetFramework.GetShortFolderName()
            };

            if (projectContext.TargetDirectory != null)
            {
                args.Add("--output");
                args.Add(projectContext.TargetDirectory);
            }

            return Command.CreateDotNet(
                "build",
                args,
                projectContext.TargetFramework,
                projectContext.Configuration);
        }
    }
}
