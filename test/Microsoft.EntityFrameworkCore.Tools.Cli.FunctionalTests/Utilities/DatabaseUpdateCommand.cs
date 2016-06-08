// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.FunctionalTests
{
    public class DatabaseUpdateCommand : EfCommand
    {
        public DatabaseUpdateCommand(string targetProject, ITestOutputHelper output, string startupProject = null)
            : base(targetProject, output, startupProject)
        {
        }

        protected override string BuildArgs()
            => "database update";
    }
}