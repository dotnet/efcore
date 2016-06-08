// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.Cli.Utils;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.FunctionalTests
{
    public sealed class BuildCommand : TestCommand
    {
        private readonly string _projectPath;

        public BuildCommand(string projectPath, ITestOutputHelper output)
            : base("dotnet", output)
        {
            _projectPath = projectPath;
        }

        public override CommandResult Execute(string args = "")
        {
            args = $"build {_projectPath} {args}";
            return base.Execute(args);
        }
    }
}