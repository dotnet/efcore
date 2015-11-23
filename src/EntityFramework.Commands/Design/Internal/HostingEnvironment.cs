// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451 || DNXCORE50

using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Data.Entity.Design.Internal
{
    public class HostingEnvironment : IHostingEnvironment
    {
        public virtual string EnvironmentName { get; set; }
        public virtual string WebRootPath { get; set; }
        public virtual IFileProvider WebRootFileProvider { get; set; }
        public virtual IConfiguration Configuration { get; set; }
    }
}

#endif