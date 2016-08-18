// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using NuGet.Frameworks;
using NuGetProgram = NuGet.CommandLine.XPlat.Program;

namespace CommandPackager
{
    public class CommandPackager
    {
        private readonly string _baseDir;
        private readonly string _config;

        public CommandPackager(string baseDir, string config)
        {
            _baseDir = baseDir;
            _config = config;
        }

        public void Run()
        {
            PackToolsDotNet();
            PackToolsVisualStudio();
        }

        private void PackToolsVisualStudio()
        {
            var project = ProjectContext.Create(Path.Combine(_baseDir, "src", "Tools.VisualStudio"), FrameworkConstants.CommonFrameworks.Net451);
            var props = "configuration=" + _config;

            var version = project.ProjectFile.Version.ToNormalizedString();
            Nuget("pack",
                Path.Combine(_baseDir, "src", "Tools.VisualStudio", "project.nuspec"),
                "--verbosity", "Verbose",
                "--output-directory", Path.Combine(_baseDir, "artifacts/build"),
                "--version", version,
                "--properties", props);
        }

        private void PackToolsDotNet()
        {
            var project = ProjectContext.Create(Path.Combine(_baseDir, "src", "Tools.DotNet"), FrameworkConstants.CommonFrameworks.NetCoreApp10);
            var props = "configuration=" + _config;
            props += ";dotnetcliutils=" + GetLockFileVersion(project, "Microsoft.DotNet.Cli.Utils");
            props += ";projectmodel=" + GetLockFileVersion(project, "Microsoft.DotNet.ProjectModel");
            props += ";cliutils=" + GetLockFileVersion(project, "Microsoft.Extensions.CommandLineUtils");
            props += ";netcoreapp=" + GetLockFileVersion(project, "Microsoft.NETCore.App");

            var version = project.ProjectFile.Version.ToNormalizedString();
            Nuget("pack",
                Path.Combine(project.ProjectDirectory, "project.nuspec"),
                "--verbosity", "Verbose",
                "--output-directory", Path.Combine(_baseDir, "artifacts/build"),
                "--version", version,
                "--properties", props);
        }

        private static string GetLockFileVersion(ProjectContext project, string name) =>
            project
                .LockFile
                .PackageLibraries
                .First(l => l.Name.Equals(name))
                .Version
                .ToNormalizedString();

        private void Nuget(params string[] args)
        {
            Console.WriteLine("nuget arguments: ".Bold().Blue() + string.Join(" ", args));

            NuGetProgram.Main(args);
        }
    }
}
