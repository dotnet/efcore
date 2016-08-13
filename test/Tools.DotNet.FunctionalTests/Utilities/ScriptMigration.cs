// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.FunctionalTests.Utilities
{
    public class ScriptMigration : DotnetEf
    {
        public ScriptMigration(string targetProject, ITestOutputHelper output, string startupProject = null)
            : base(targetProject, output, startupProject)
        {
        }

        protected override IEnumerable<string> BuildArgs()
            => new[] { "migrations", "script" };
    }
}
