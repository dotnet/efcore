// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.FunctionalTests
{
    public class EndToEndTests : IClassFixture<DotNetEfFixture>
    {
        private static readonly string _testProjectRoot = Path.Combine(
            AppContext.BaseDirectory,
            "TestProjects");

        private readonly ITestOutputHelper _output;
        private readonly DotNetEfFixture _fixture;

        public EndToEndTests(DotNetEfFixture fixture, ITestOutputHelper output)
        {
            _output = output;
            _fixture = fixture;
        }

        [Fact(Skip = "Unreliable on CI")]
        public void RunsMigrationCommandsOnDesktop()
        {
            // TODO use xunit helpers from SpecTests. Currently this causes re-compilation of the test graph
            // because of the pre-compile script on this project
            if (PlatformServices.Default.Runtime.OperatingSystem.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                AddAndApplyMigrationImpl("DesktopAppWithTools", "TestContext", "Initial");
            }
        }

        [Fact(Skip = "Unreliable on CI")]
        public void RunsMigrationsForAspNetApp()
        {
            var ignoredJson = Path.Combine(_testProjectRoot, "LibraryUsingSqlite", "project.json.ignore");
            var libraryProject = Path.Combine(_testProjectRoot, "LibraryUsingSqlite", "project.json");
            File.Move(ignoredJson, libraryProject);

            try
            {
                AssertCommand.Passes(new RestoreCommand(libraryProject, _output)
                    .Execute());

                AddAndApplyMigrationImpl("AspNetHostingPortableApp", "LibraryContext", "initialLibrary");
                AddAndApplyMigrationImpl("AspNetHostingPortableApp", "TestContext", "initialTest");
            }
            finally
            {
                File.Move(libraryProject, ignoredJson);
            }
        }

        [Theory(Skip = "Unreliable on CI")]
        [InlineData("PortableAppWithTools")]
        [InlineData("StandaloneAppWithTools")]
        public void RunsMigrationCommandsForNetCoreApps(string project)
            => AddAndApplyMigrationImpl(project, "TestContext", "Initial");

        private void AddAndApplyMigrationImpl(string project, string contextName, string migrationName)
        {
            var ignoredJson = Path.Combine(_testProjectRoot, project, "project.json.ignore");
            var testProject = Path.Combine(_testProjectRoot, project, "project.json");
            File.Move(ignoredJson, testProject);

            var migrationDir = Path.Combine(Path.GetDirectoryName(testProject), "Migrations");
            var snapshotFile = contextName + "ModelSnapshot.cs";

            try
            {
                if (Directory.Exists(migrationDir))
                {
                    Assert.False(Directory.EnumerateFiles(migrationDir, snapshotFile, SearchOption.AllDirectories).Any());
                }

                _fixture.InstallTool(testProject, _output, _testProjectRoot);

                AssertCommand.Passes(new MigrationAddCommand(testProject, migrationName, _output)
                    .Execute($" --context {contextName}"));

                Assert.True(Directory.EnumerateFiles(migrationDir, snapshotFile, SearchOption.AllDirectories).Any());

                AssertCommand.Passes(new DatabaseUpdateCommand(testProject, _output)
                    .Execute($" --context {contextName}"));
            }
            finally
            {
                // project.json, even nested in bin/, will fail dotnet-restore on the solution
                File.Move(testProject, ignoredJson);
            }
        }

    }
}