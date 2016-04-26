// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.CommandLineUtils;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class CommonCommandOptions
    {
        public CommandOption Framework { get; set; }
        public CommandOption Configuration { get; set; }
        public CommandOption NoBuild { get; set; }
        public CommandOption BuildBasePath { get; set; }

        public CommonOptions Value()
            => new CommonOptions
            {
                Framework = Framework.HasValue()
                    ? NuGetFramework.Parse(Framework.Value())
                    : null,
                Configuration = Configuration.Value(),
                BuildBasePath = BuildBasePath.Value(),
                NoBuild = NoBuild.HasValue(),
            };
    }
}