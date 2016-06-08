// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.FunctionalTests
{
    public partial class EndToEndTests : IClassFixture<DotNetEfFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly DotNetEfFixture _fixture;
        private const string SkipReason = "Unreliable on CI";

        public EndToEndTests(DotNetEfFixture fixture, ITestOutputHelper output)
        {
            _output = output;
            _fixture = fixture;
        }

        [Fact(Skip = SkipReason)]
        public void MigrationsOnDesktop()
        {
            // TODO use xunit helpers from SpecTests. Currently this causes re-compilation of the test graph
            // because of the pre-compile script on this project
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                AddAndApplyMigrationImpl("DesktopApp", "TestContext", "Initial");
            }
        }

        [Fact(Skip = SkipReason)]
        public void MigrationsOnNetStandardClassLibraryWithExternalStartup()
        {
            AddAndApplyMigrationImpl("NetStandardClassLibrary", "NetStandardContext", "initialLibrary", startupProject: "NetCoreStartupApp");
        }

        [Fact(Skip = SkipReason)]
        public void MigrationsOnDesktopClassLibraryWithExternalStartup()
        {
            AddAndApplyMigrationImpl("DesktopClassLibrary", "DesktopContext", "initialLibrary", startupProject: "DesktopStartupApp");
        }

        [Fact(Skip = SkipReason)]
        public void AddMigrationToDifferentFolder()
        {
            var project = Path.Combine(_fixture.TestProjectRoot, "PortableApp", "project.json");
            Assert.False(Directory.Exists(Path.Combine(_fixture.TestProjectRoot, "SomeOtherDir")));

            _fixture.InstallTool(project, _output, _fixture.TestProjectRoot);

            AssertCommand.Pass(new MigrationAddCommand(project, "OtherFolderMigration", _output)
                .Execute($" --context TestContext --output-dir ../SomeOtherDir"));

            Assert.True(Directory.Exists(Path.Combine(_fixture.TestProjectRoot, "SomeOtherDir")));
            Assert.True(Directory.EnumerateFiles(Path.Combine(_fixture.TestProjectRoot, "SomeOtherDir"), "*.cs").Any());
        }

        [Theory(Skip = SkipReason)]
        [InlineData("PortableApp")]
        [InlineData("StandaloneApp")]
        [InlineData("AspNetHostingPortableApp")]
        public void MigrationCommandsForNetCoreApps(string project)
            => AddAndApplyMigrationImpl(project, "TestContext", "Initial");

        private void AddAndApplyMigrationImpl(
            string targetProject,
            string contextName,
            string migrationName,
            string startupProject = null)
        {
            var targetDir = Path.Combine(_fixture.TestProjectRoot, targetProject, "project.json");
            var startupProjectDir = startupProject != null
                ? Path.Combine(_fixture.TestProjectRoot, startupProject, "project.json")
                : null;

            _output.WriteLine("Target dir = " + targetDir);

            var migrationDir = Path.Combine(Path.GetDirectoryName(targetDir), "Migrations");
            var snapshotFile = contextName + "ModelSnapshot.cs";

            if (Directory.Exists(migrationDir))
            {
                Assert.False(Directory.EnumerateFiles(migrationDir, snapshotFile, SearchOption.AllDirectories).Any());
            }

            _fixture.InstallTool(targetDir, _output, _fixture.TestProjectRoot);

            if (startupProjectDir != null)
            {
                _fixture.InstallTool(startupProjectDir, _output, _fixture.TestProjectRoot);
            }

            var args = $"--context {contextName}";

            AssertCommand.Pass(new MigrationAddCommand(targetDir, migrationName, _output, startupProjectDir)
                .Execute(args));

            Assert.True(Directory.EnumerateFiles(migrationDir, snapshotFile, SearchOption.AllDirectories).Any());

            AssertCommand.Pass(new DatabaseUpdateCommand(targetDir, _output, startupProjectDir)
                .Execute(args));
        }
    }
}