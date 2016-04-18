// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.FunctionalTests
{
    public class EndToEndTests : IClassFixture<DotNetEfFixture>
    {
        private static readonly string _testProjectRoot = Path.Combine(
            AppContext.BaseDirectory,
            "TestAssets",
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
                AddAndApplyMigrationImpl("DesktopAppWithTools");
            }
        }
        

        [Theory(Skip = "Unreliable on CI")]
        [InlineData("PortableAppWithTools")]
        [InlineData("AspNetHostingPortableApp")]
        // TODO support dotnet-ef in libraries and standlon
        // [InlineData("StandaloneAppWithTools")]
        // [InlineData("LibraryWithTools")] 
        public void RunsMigrationCommands(string project)
            => AddAndApplyMigrationImpl(project);
            
        private void AddAndApplyMigrationImpl(string project)
        {
            var ignoredJson = Path.Combine(_testProjectRoot, project, "project.json.ignore");
            var testProject = Path.Combine(_testProjectRoot, project, "project.json");
            File.Move(ignoredJson, testProject);

            var migrationDir = Path.Combine(Path.GetDirectoryName(testProject), "Migrations");
            var snapshotFile = Path.Combine(migrationDir, "TestContextModelSnapshot.cs");

            try
            {
                Assert.False(File.Exists(snapshotFile));

                _fixture.InstallTool(testProject, _output);

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