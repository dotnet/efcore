// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class ResolverArguments
    {
        public IEnumerable<string> CommandArguments { get; set; }
        public NuGetFramework Framework { get; set; }
        public string DepsJsonFile { get; set; }
        public string NuGetPackageRoot { get; set; }
        public string RuntimeConfigJson { get; set; }
    }
}
