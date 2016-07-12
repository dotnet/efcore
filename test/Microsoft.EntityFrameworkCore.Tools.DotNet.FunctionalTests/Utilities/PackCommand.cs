// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.Cli.Utils;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tools.Cli.FunctionalTests
{
    public sealed class PackCommand : TestCommand
    {
        private readonly string _projectPath;
        private readonly string _outputDir;

        public PackCommand(string projectPath, string outputDir, ITestOutputHelper output)
            : base("dotnet", output)
        {
            _projectPath = projectPath;
            _outputDir = outputDir;
        }

        public override CommandResult Execute(string args = "")
        {
            args = $"pack {_projectPath} --output {_outputDir} {args}";
            return base.Execute(args);
        }
    }
}