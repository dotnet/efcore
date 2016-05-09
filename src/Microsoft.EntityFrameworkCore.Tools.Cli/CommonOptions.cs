// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.CommandLineUtils;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class CommonOptions
    {
        public NuGetFramework Framework { get; set; }
        public string Configuration { get; set; }
    }
}