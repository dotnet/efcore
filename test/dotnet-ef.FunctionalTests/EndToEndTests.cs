// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Commands.FunctionalTests
{
    public class EndToEndTests
    {
        private static readonly string _testProjectRoot = Path.Combine(
            AppContext.BaseDirectory,
            "TestAssets",
            "TestProjects");

        private readonly ITestOutputHelper _output;

        public EndToEndTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private void Setup(string testProject)
        {
            var suffix = "t" + DateTime.UtcNow.ToString("yyMMddHHmmss");
            var testLocalArtifacts = Path.Combine(AppContext.BaseDirectory, "artifacts");

            // TODO can remove when https://github.com/NuGet/Home/issues/2469 is available
            AssertCommand.Passes(
                new PackCommand(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "src", "dotnet-ef"), // count carefully
                    Path.Combine(AppContext.BaseDirectory, "artifacts"),
                    _output)
                    .Execute($"--version-suffix {suffix}"));

            var json = File.ReadAllText(testProject);
            File.WriteAllText(testProject, json.Replace("$toolVersion$", suffix));

            AssertCommand.Passes(
                new RestoreCommand(testProject, _output)
                    .Execute($"--fallbacksource {testLocalArtifacts}")
                );
        }

        [Fact]
        public void AddAndApplyMigration()
        {
            var ignoredJson = Path.Combine(_testProjectRoot, "PortableAppWithTools", "project.json.ignore");
            var testProject = Path.Combine(_testProjectRoot, "PortableAppWithTools", "project.json");
            File.Move(ignoredJson, testProject);

            var migrationDir = Path.Combine(Path.GetDirectoryName(testProject), "Migrations");
            var snapshotFile = Path.Combine(migrationDir, "TestContextModelSnapshot.cs");

            try
            {
                Assert.False(File.Exists(snapshotFile));

                Setup(testProject);

                AssertCommand.Passes(new MigrationAddCommand(testProject, "TestMigration", _output)
                    .Execute());

                Assert.True(File.Exists(snapshotFile));

                AssertCommand.Passes(new DatabaseUpdateCommand(testProject, _output)
                    .Execute());
            }
            finally
            {
                // project.json, even nested in bin/, will fail dotnet-restore on the solution
                File.Move(testProject, ignoredJson);
            }
        }
    }
}