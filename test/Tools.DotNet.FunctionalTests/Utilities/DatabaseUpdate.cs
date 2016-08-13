// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.FunctionalTests.Utilities
{
    public class DatabaseUpdate : DotnetEf
    {
        public DatabaseUpdate(string targetProject, ITestOutputHelper output, string startupProject = null)
            : base(targetProject, output, startupProject)
        {
        }

        protected override IEnumerable<string> BuildArgs()
            => new[] { "database", "update" };
    }
}