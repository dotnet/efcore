// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using NuGet.Frameworks;

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

        public async Task Run()
        {
            await PackToolsDotNet();
            await PackToolsVisualStudio();
        }

        private async Task PackToolsVisualStudio()
        {
            var project = ProjectContext.Create(Path.Combine(_baseDir, "src", "Tools.VisualStudio"), FrameworkConstants.CommonFrameworks.Net451);
            var props = "configuration=" + _config;

            var version = project.ProjectFile.Version.ToNormalizedString();
            await Nuget("pack",
                Path.Combine(_baseDir, "src", "Tools.VisualStudio", "project.nuspec"),
                "-Verbosity", "detailed",
                "-OutputDirectory", Path.Combine(_baseDir, "artifacts/build"),
                "-Version", version,
                "-Properties", props);
        }

        private async Task PackToolsDotNet()
        {
            var project = ProjectContext.Create(Path.Combine(_baseDir, "src", "Tools.DotNet"), FrameworkConstants.CommonFrameworks.NetCoreApp10);
            var props = "configuration=" + _config;
            props += ";dotnetcliutils=" + GetLockFileVersion(project, "Microsoft.DotNet.Cli.Utils");
            props += ";projectmodel=" + GetLockFileVersion(project, "Microsoft.DotNet.ProjectModel");
            props += ";cliutils=" + GetLockFileVersion(project, "Microsoft.Extensions.CommandLineUtils");
            props += ";netcoreapp=" + GetLockFileVersion(project, "Microsoft.NETCore.App");

            var version = project.ProjectFile.Version.ToNormalizedString();
            await Nuget("pack",
                Path.Combine(project.ProjectDirectory, "project.nuspec"),
                "-Verbosity", "detailed",
                "-OutputDirectory", Path.Combine(_baseDir, "artifacts/build"),
                "-Version", version,
                "-Properties", props);
        }

        private static string GetLockFileVersion(ProjectContext project, string name) =>
            project
                .LockFile
                .PackageLibraries
                .First(l => l.Name.Equals(name))
                .Version
                .ToNormalizedString();

        private async Task Nuget(params string[] args)
        {
            var pInfo = new ProcessStartInfo
            {
                Arguments = ArgumentEscaper.EscapeAndConcatenateArgArrayForProcessStart(args),
                FileName = await GetNugetExePath()
            };
            Console.WriteLine("command:   ".Bold().Blue() + pInfo.FileName);
            Console.WriteLine("arguments: ".Bold().Blue() + pInfo.Arguments);

            Process.Start(pInfo).WaitForExit();
        }

        private async Task<string> GetNugetExePath()
        {
            if (Environment.GetEnvironmentVariable("KOREBUILD_NUGET_EXE") != null)
            {
                return Environment.GetEnvironmentVariable("KOREBUILD_NUGET_EXE");
            }

            var nugetPath = Path.Combine(_baseDir, ".build", "nuget.3.5.0-beta2.exe");
            if (File.Exists(nugetPath))
            {
                return nugetPath;
            }
            Console.WriteLine("log : Downloading nuget.exe 3.5.0-beta2".Bold().Black());
            var response = await new HttpClient().GetAsync("https://dist.nuget.org/win-x86-commandline/v3.5.0-beta2/NuGet.exe");
            using (var file = new FileStream(nugetPath, FileMode.CreateNew))
            {
                response.EnsureSuccessStatusCode();
                await response.Content.LoadIntoBufferAsync();
                await response.Content.CopyToAsync(file);
            }
            return nugetPath;
        }
    }
}
