// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Internal;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class EfConsoleCommandSpecFactory
    {
        private readonly EfConsoleCommandResolver _resolver;

        public EfConsoleCommandSpecFactory([NotNull] EfConsoleCommandResolver resolver)
        {
            _resolver = resolver;
        }

        public virtual CommandSpec Create(IProjectContext startupProject, IProjectContext targetProject, bool verbose, IEnumerable<string> additionalArguments)
        {
            if (startupProject.IsClassLibrary)
            {
                throw new OperationErrorException(ToolsDotNetStrings.ClassLibrariesNotSupported(startupProject.ProjectName));
            }

            var targetAssembly = targetProject.ProjectFullPath.Equals(startupProject.ProjectFullPath)
                ? startupProject.AssemblyFullPath
                // This assumes the target assembly is present in the startup project context build output folder
                : Path.Combine(startupProject.TargetDirectory, Path.GetFileName(targetProject.AssemblyFullPath));

            var args = CreateArgs(
                assembly: targetAssembly,
                startupAssembly: startupProject.AssemblyFullPath,
                dataDir: startupProject.TargetDirectory,
                contentRootPath: Path.GetDirectoryName(startupProject.ProjectFullPath),
                projectDir: Path.GetDirectoryName(targetProject.ProjectFullPath),
                rootNamespace: targetProject.RootNamespace,
                verbose: verbose);

            if (startupProject.TargetFramework.IsDesktop())
            {
                if (startupProject.Config != null)
                {
                    args = args.Concat(new [] { ConfigOptionTemplate, startupProject.Config });
                }
                args = args.Concat(additionalArguments);
                return ResolveDesktopCommand(startupProject, args);
            }

            args = args.Concat(additionalArguments);
            return ResolveDotNetCommand(startupProject, args);
        }

        private CommandSpec ResolveDesktopCommand(IProjectContext startupProject, IEnumerable<string> args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new OperationErrorException(ToolsDotNetStrings.DesktopCommandsRequiresWindows(startupProject.TargetFramework));
            }

            var arguments = new ResolverArguments
            {
                IsDesktop = true,
                CommandArguments = args,
                NuGetPackageRoot = startupProject.PackagesDirectory
            };

            return _resolver.Resolve(arguments);
        }

        private CommandSpec ResolveDotNetCommand(IProjectContext startupProject, IEnumerable<string> args)
        {
            if (!File.Exists(startupProject.DepsJson))
            {
                throw new OperationErrorException(ToolsDotNetStrings.MissingDepsJsonFile(startupProject.DepsJson));
            }

            var arguments = new ResolverArguments
            {
                IsDesktop = false,
                CommandArguments = args,
                DepsJsonFile = startupProject.DepsJson,
                RuntimeConfigJson = startupProject.RuntimeConfigJson,
                NuGetPackageRoot = startupProject.PackagesDirectory
            };

            return _resolver.Resolve(arguments);
        }

        private const string ConfigOptionTemplate = "--config";
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
            bool verbose)
        {
            Debug.Assert(!string.IsNullOrEmpty(assembly), "assembly is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(startupAssembly), "startupAssembly is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(dataDir), "dataDir is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(projectDir), "projectDir is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(contentRootPath), "contentRootPath is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(rootNamespace), "rootNamespace is null or empty.");

            return new[]
            {
                AssemblyOptionTemplate, assembly,
                StartupAssemblyOptionTemplate, startupAssembly,
                DataDirectoryOptionTemplate, dataDir,
                ProjectDirectoryOptionTemplate, projectDir,
                ContentRootPathOptionTemplate, contentRootPath,
                RootNamespaceOptionTemplate, rootNamespace,
            }
            .Concat(verbose
                ? new[] { VerboseOptionTemplate }
                : Enumerable.Empty<string>());
        }
    }
}
