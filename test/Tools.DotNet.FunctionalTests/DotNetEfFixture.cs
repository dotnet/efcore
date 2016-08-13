// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Tools.DotNet.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.FunctionalTests
{
    public class DotNetEfFixture : IDisposable
    {
        public string TestProjectRoot { get; } = Path.Combine(AppContext.BaseDirectory, "TestProjects");

        public DotNetEfFixture()
        {
            foreach (var file in Directory.EnumerateFiles(TestProjectRoot, "project.json.ignore", SearchOption.AllDirectories))
            {
                File.Move(file, Path.Combine(Path.GetDirectoryName(file), "project.json"));
            }
            Console.WriteLine("Restoring test projects...".Bold().Black());
            AssertCommand.Pass(new DotnetRestore(TestProjectRoot, null).Execute());
            Console.WriteLine("Restore done.".Bold().Black());
        }

        public void Dispose()
        {
            // cleanup to prevent later errors with restore
            foreach (var file in Directory.EnumerateFiles(TestProjectRoot, "project.json", SearchOption.AllDirectories))
            {
                File.Move(file, Path.Combine(Path.GetDirectoryName(file), "project.json.ignore"));
            }
        }
    }
}
