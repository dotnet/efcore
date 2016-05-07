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
        private readonly ITestOutputHelper _output;
        private readonly DotNetEfFixture _fixture;

        public EndToEndTests(DotNetEfFixture fixture, ITestOutputHelper output)
        {
            _output = output;
            _fixture = fixture;
        }

        [Fact(Skip = "Unreliable on CI")]
        public void MigrationsOnDesktop()
        {
            // TODO use xunit helpers from SpecTests. Currently this causes re-compilation of the test graph
            // because of the pre-compile script on this project
            if (PlatformServices.Default.Runtime.OperatingSystem.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                AddAndApplyMigrationImpl("DesktopAppWithTools", "TestContext", "Initial");
            }
        }

        [Fact(Skip = "Unreliable on CI")]
        public void MigrationsOnDesktopWithContextInLibrary()
        {
            if (!PlatformServices.Default.Runtime.OperatingSystem.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var startupProject = Path.Combine(_fixture.TestProjectRoot, "DesktopStartupProject/project.json");
            var targetProject = Path.Combine(_fixture.TestProjectRoot, "DesktopClassLibrary/project.json");

            _fixture.InstallTool(targetProject, _output, _fixture.TestProjectRoot);

            AssertCommand.Passes(new RestoreCommand(startupProject, _output)
                .Execute(" --verbosity error "));

            AddAndApplyMigrationImpl(
                project: "DesktopClassLibrary",
                contextName: "DesktopContext",
                migrationName: "Initial",
                startupProject: "../DesktopStartupProject");
        }

        [Fact(Skip = "Unreliable on CI")]
        public void MigrationsForAspNetApp()
        {
            var libraryProject = Path.Combine(_fixture.TestProjectRoot, "LibraryUsingSqlite", "project.json");
            AssertCommand.Passes(new RestoreCommand(libraryProject, _output)
                .Execute(" --verbosity error "));

            AddAndApplyMigrationImpl("AspNetHostingPortableApp", "LibraryContext", "initialLibrary");
            AddAndApplyMigrationImpl("AspNetHostingPortableApp", "TestContext", "initialTest");
        }

        [Fact(Skip = "Unreliable on CI")]
        public void AddMigrationToDifferentFolder()
        {
            var libraryProject = Path.Combine(_fixture.TestProjectRoot, "PortableAppWithTools", "project.json");
            Assert.False(Directory.Exists(Path.Combine(_fixture.TestProjectRoot, "SomeOtherDir")));

            _fixture.InstallTool(libraryProject, _output, _fixture.TestProjectRoot);

            AssertCommand.Passes(new MigrationAddCommand(libraryProject, "OtherFolderMigration", _output)
                .Execute($" --context TestContext --output-dir ../SomeOtherDir"));

            Assert.True(Directory.Exists(Path.Combine(_fixture.TestProjectRoot, "SomeOtherDir")));
            Assert.True(Directory.EnumerateFiles(Path.Combine(_fixture.TestProjectRoot, "SomeOtherDir"), "*.cs").Any());
        }

        [Theory(Skip = "Unreliable on CI")]
        [InlineData("PortableAppWithTools")]
        [InlineData("StandaloneAppWithTools")]
        public void MigrationCommandsForNetCoreApps(string project)
            => AddAndApplyMigrationImpl(project, "TestContext", "Initial");

        private void AddAndApplyMigrationImpl(
            string project,
            string contextName,
            string migrationName,
            string startupProject = null)
        {
            var targetProject = Path.Combine(_fixture.TestProjectRoot, project, "project.json");
            var migrationDir = Path.Combine(Path.GetDirectoryName(targetProject), "Migrations");
            var snapshotFile = contextName + "ModelSnapshot.cs";

            if (Directory.Exists(migrationDir))
            {
                Assert.False(Directory.EnumerateFiles(migrationDir, snapshotFile, SearchOption.AllDirectories).Any());
            }

            _fixture.InstallTool(targetProject, _output, _fixture.TestProjectRoot);

            var args = $" --context {contextName}";
            if (startupProject != null)
            {
                args += $" --startup-project {startupProject}";
            }

            AssertCommand.Passes(new MigrationAddCommand(targetProject, migrationName, _output)
                .Execute(args));

            Assert.True(Directory.EnumerateFiles(migrationDir, snapshotFile, SearchOption.AllDirectories).Any());

            AssertCommand.Passes(new DatabaseUpdateCommand(targetProject, _output)
                .Execute(args));
        }
    }
}