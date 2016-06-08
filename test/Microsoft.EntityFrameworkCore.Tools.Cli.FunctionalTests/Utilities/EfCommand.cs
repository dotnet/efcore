// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.DotNet.Cli.Utils;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.FunctionalTests
{
    public abstract class EfCommand : TestCommand
    {
        private readonly string _startupArgs;

        protected EfCommand(string targetProject, ITestOutputHelper output, string startupProject = null)
            : base("dotnet", output)
        {
            _startupArgs = startupProject != null
                ? $"--startup-project {startupProject}"
                : string.Empty;

            // always executes on startup project
            WorkingDirectory = Path.GetDirectoryName(targetProject);
        }

        public override CommandResult Execute(string args = "")
        {
            args = $"--verbose ef {_startupArgs} {BuildArgs()} {args}";
            return base.Execute(args);
        }

        public override CommandResult ExecuteWithCapturedOutput(string args = "")
        {
            args = $"--verbose ef {_startupArgs} {BuildArgs()} {args}";
            return base.ExecuteWithCapturedOutput(args);
        }

        protected abstract string BuildArgs();
    }
}