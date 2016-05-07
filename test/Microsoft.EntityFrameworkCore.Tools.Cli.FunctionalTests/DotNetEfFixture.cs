// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.Cli.Utils;
using Newtonsoft.Json;
using NuGet.Configuration;
using System;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.FunctionalTests
{
    public class DotNetEfFixture : IDisposable
    {
        private readonly string _solutionDir;
        private string _suffix;
        private bool _toolsBuilt;
        private static readonly string _localArtifacts = Path.Combine(AppContext.BaseDirectory, "artifacts");
        private static readonly string[] PackagesToBuild =
            {
                "Microsoft.EntityFrameworkCore.Tools",
                "Microsoft.EntityFrameworkCore.Tools.Cli",
                "Microsoft.EntityFrameworkCore.Tools.Core",
                "Microsoft.EntityFrameworkCore.Relational.Design",
                "Microsoft.EntityFrameworkCore.Relational",
                "Microsoft.EntityFrameworkCore",
             };

        public string TestProjectRoot { get; } = Path.Combine(AppContext.BaseDirectory, "TestProjects");

        public DotNetEfFixture()
        {
            var dir = AppContext.BaseDirectory;
            while (dir != null && !File.Exists(Path.Combine(dir, "global.json")))
            {
                dir = Path.GetDirectoryName(dir);
            }
            _solutionDir = dir;

            foreach (var file in Directory.EnumerateFiles(TestProjectRoot, "project.json.ignore", SearchOption.AllDirectories))
            {
                File.Move(file, Path.Combine(Path.GetDirectoryName(file), "project.json"));
            }
        }

        public void Dispose()
        {
            // cleanup to prevent later errors with restore
            foreach (var file in Directory.EnumerateFiles(TestProjectRoot, "project.json", SearchOption.AllDirectories))
            {
                File.Move(file, Path.Combine(Path.GetDirectoryName(file), "project.json.ignore"));
            }
        }

        private void BuildToolsPackages(ITestOutputHelper logger)
        {
            if (_toolsBuilt)
            {
                return;
            }
            _toolsBuilt = true;

            _suffix = "t" + DateTime.UtcNow.ToString("yyMMddHHmmss");

            foreach (var pkg in PackagesToBuild)
            {
                logger?.WriteLine($"Building {pkg} version {_suffix}");
                // TODO can remove when https://github.com/NuGet/Home/issues/2469 is available
                AssertCommand.Passes(
                    new PackCommand(
                        Path.Combine(_solutionDir, "src", pkg, Constants.ProjectFileName),
                        _localArtifacts,
                        logger)
                        .Execute($"--version-suffix {_suffix}"));
            }
        }

        public void InstallTool(string projectPath, ITestOutputHelper logger, params string[] fallbackRestoreLocations)
        {
            BuildToolsPackages(logger);

            var json = File.ReadAllText(projectPath);
            File.WriteAllText(projectPath, json.Replace("$toolVersion$", _suffix));
            var fallbacks = new[] { _localArtifacts }
                .Concat(fallbackRestoreLocations ?? Enumerable.Empty<string>())
                .Select(s => "--fallbacksource " + s);

            AssertCommand.Passes(
                new RestoreCommand(projectPath, logger)
                    .Execute(" --verbosity error " + string.Join(" ", fallbacks))
                );
        }
    }
}