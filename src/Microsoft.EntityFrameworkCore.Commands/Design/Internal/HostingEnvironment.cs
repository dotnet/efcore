// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class HostingEnvironment : IHostingEnvironment
    {
        public virtual IConfiguration Configuration { get; set; }
        public virtual string EnvironmentName { get; set; }
        public virtual IFileProvider WebRootFileProvider { get; set; }
        public virtual string WebRootPath { get; set; }
    }
}
