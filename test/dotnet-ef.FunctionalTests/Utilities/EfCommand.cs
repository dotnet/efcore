// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Commands.FunctionalTests.Utilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Commands.FunctionalTests
{
    public abstract class EfCommand : TestCommand
    {
        protected EfCommand(string projectPath, ITestOutputHelper output)
            : base("dotnet", output)
        {
            WorkingDirectory = Path.GetDirectoryName(projectPath);
        }

        public override CommandResult Execute(string args = "")
        {
            args = $"--verbose ef {BuildArgs()} {args}";
            return base.Execute(args);
        }

        protected abstract string BuildArgs();
    }
}