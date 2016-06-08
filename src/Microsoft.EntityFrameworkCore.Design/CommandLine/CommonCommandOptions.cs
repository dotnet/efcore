// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class CommonCommandOptions
    {
        public CommandOption Assembly { get; set; }
        public CommandOption StartupAssembly { get; set; }
        public CommandOption DataDirectory { get; set; }
        public CommandOption ProjectDirectory { get; set; }
        public CommandOption StartupTargetDirectory { get; set; }
        public CommandOption RootNamespace { get; set; }

        public CommonOptions Value()
            => new CommonOptions
            {
                Assembly = Assembly.Value(),
                StartupAssembly = StartupAssembly.Value(),
                DataDirectory = DataDirectory.Value(),
                ProjectDirectory = ProjectDirectory.Value(),
                StartupTargetDirectory = StartupTargetDirectory.Value(),
                RootNamespace = RootNamespace.Value()
            };
    }
}