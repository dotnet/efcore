// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class CommonOptions
    {
        public string Assembly { get; set; }
        public string StartupAssembly { get; set; }
        public string DataDirectory { get; set; }
        public string RootNamespace { get; set; }
        public string ProjectDirectory { get; set; }
        public string StartupTargetDirectory { get; set; }
    }
}