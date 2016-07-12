// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.FunctionalTests
{
    public partial class EndToEndTests 
    {
        [Fact(Skip = SkipReason)]
        public void Unsupported_MigrationsOnDesktopClassLibrary()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            var targetProject = Path.Combine(_fixture.TestProjectRoot, "DesktopClassLibrary/project.json");

            _fixture.InstallTool(targetProject, _output, _fixture.TestProjectRoot);

            var result = new MigrationAddCommand(targetProject, "Initial", _output)
                .ExecuteWithCapturedOutput();
            AssertCommand.Fail(result);
            Assert.Contains("does not support", result.StdErr);
        }


        [Fact(Skip = SkipReason)]
        public void Unsupported_MigrationsOnNetCoreClassLibrary()
        {
            var targetProject = Path.Combine(_fixture.TestProjectRoot, "NetStandardClassLibrary/project.json");

            _fixture.InstallTool(targetProject, _output, _fixture.TestProjectRoot);

            var result = new MigrationAddCommand(targetProject, "Initial", _output)
               .ExecuteWithCapturedOutput();
            AssertCommand.Fail(result);
            Assert.Contains("does not support", result.StdErr);
        }
    }
}