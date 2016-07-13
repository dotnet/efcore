// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Tools.DotNet.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.FunctionalTests
{
    public partial class EndToEndTests : IClassFixture<DotNetEfFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly DotNetEfFixture _fixture;

        public EndToEndTests(DotNetEfFixture fixture, ITestOutputHelper output)
        {
            _output = output;
            _fixture = fixture;
        }

        [ConditionalFact]
        [PlatformSkipCondition(TestPlatform.Linux | TestPlatform.Mac)]
        public void MigrationsOnDesktop()
        {
            AddAndApplyMigrationImpl("DesktopApp", "TestContext", "Initial");
        }

        [Fact]
        public void MigrationsOnNetStandardClassLibraryWithExternalStartup()
        {
            AddAndApplyMigrationImpl("NetStandardClassLibrary", "NetStandardContext", "initialLibrary", startupProjectName: "NetCoreStartupApp");
        }

        [Fact]
        public void MigrationsOnDesktopClassLibraryWithExternalStartup()
        {
            AddAndApplyMigrationImpl("DesktopClassLibrary", "DesktopContext", "initialLibrary", startupProjectName: "DesktopStartupApp");
        }

        [Fact]
        public void AddMigrationToDifferentFolder()
        {
            var project = Path.Combine(_fixture.TestProjectRoot, "PortableApp", "project.json");
            Assert.False(Directory.Exists(Path.Combine(_fixture.TestProjectRoot, "SomeOtherDir")));

            AssertCommand.Pass(new AddMigration(project, "OtherFolderMigration", _output)
                .Execute("--context", "TestContext", "--output-dir", "../SomeOtherDir"));

            Assert.True(Directory.Exists(Path.Combine(_fixture.TestProjectRoot, "SomeOtherDir")));
            Assert.True(Directory.EnumerateFiles(Path.Combine(_fixture.TestProjectRoot, "SomeOtherDir"), "*.cs").Any());
        }

        [Theory]
        [InlineData("PortableApp")]
        [InlineData("StandaloneApp")]
        [InlineData("AspNetHostingPortableApp")]
        public void MigrationCommandsForNetCoreApps(string project)
            => AddAndApplyMigrationImpl(project, "TestContext", "Initial");

        private void AddAndApplyMigrationImpl(
            string targetProjectName,
            string contextName,
            string migrationName,
            string startupProjectName = null)
        {
            var targetProject = Path.Combine(_fixture.TestProjectRoot, targetProjectName, "project.json");
            var startupProject = startupProjectName != null
                ? Path.Combine(_fixture.TestProjectRoot, startupProjectName, "project.json")
                : null;

            _output.WriteLine("Target dir = " + targetProject);

            var migrationDir = Path.Combine(Path.GetDirectoryName(targetProject), "Migrations");
            var snapshotFile = contextName + "ModelSnapshot.cs";

            if (Directory.Exists(migrationDir))
            {
                Assert.False(Directory.EnumerateFiles(migrationDir, snapshotFile, SearchOption.AllDirectories).Any());
            }

            AssertCommand.Pass(new AddMigration(targetProject, migrationName, _output, startupProject)
                .Execute("--context", contextName));

            Assert.True(Directory.EnumerateFiles(migrationDir, snapshotFile, SearchOption.AllDirectories).Any());

            AssertCommand.Pass(new DatabaseUpdate(targetProject, _output, startupProject)
                .Execute("--context", contextName));

            AssertCommand.Pass(new ScriptMigration(targetProject, _output, startupProject)
                .Execute("--output", Path.Combine(Path.GetDirectoryName(targetProject), "obj/dotnet-ef/migrations.sql")));
        }
    }
}