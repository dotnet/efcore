// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Commands.FunctionalTests.Utilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Commands.FunctionalTests
{
    public class RestoreCommand : TestCommand
    {
        public RestoreCommand(string projectPath, ITestOutputHelper output) 
            : base("dotnet", output)
        {
            WorkingDirectory = Path.GetDirectoryName(projectPath);
        }

        public override CommandResult Execute(string args = "")
        {
            args = $"restore {args}";
            return base.Execute(args);
        }
    }
}