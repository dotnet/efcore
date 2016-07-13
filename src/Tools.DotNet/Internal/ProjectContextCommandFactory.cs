// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.EntityFrameworkCore.Internal;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class ProjectContextCommandFactory
    {
        private readonly ICommandResolver _resolver;

        public ProjectContextCommandFactory([NotNull] ICommandResolver resolver)
        {
            _resolver = resolver;
        }

        private const string LibraryFwLink = "http://go.microsoft.com/fwlink/?LinkId=798221";

        public virtual ICommand Create(ProjectContext context,
            string configuration,
            string buildBasePath,
            string outputPath,
            IEnumerable<string> args)
        {
            var runtimeFiles = context.GetOutputPaths(configuration, buildBasePath, outputPath).RuntimeFiles;
            if (context.TargetFramework.IsDesktop())
            {
                if (!File.Exists(runtimeFiles.Assembly))
                {
                    throw new OperationErrorException(ToolsDotNetStrings.ClassLibrariesNotSupportedInCli(context.ProjectFile.Name, LibraryFwLink));
                }
            }
            else if (!File.Exists(runtimeFiles.DepsJson))
            {
                throw new OperationErrorException(ToolsDotNetStrings.ClassLibrariesNotSupportedInCli(context.ProjectFile.Name, LibraryFwLink));
            }

            var arguments = new ResolverArguments
            {
                CommandArguments = args,
                Framework = context.TargetFramework,
                DepsJsonFile = runtimeFiles.DepsJson,
                NuGetPackageRoot = context.PackagesDirectory
            };

            return Command.Create(_resolver.Resolve(arguments));
        }
    }
}
