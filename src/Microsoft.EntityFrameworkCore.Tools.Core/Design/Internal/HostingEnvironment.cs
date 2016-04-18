// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !NETCORE50
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class HostingEnvironment : IHostingEnvironment
    {
        public virtual string EnvironmentName { get; set; }

        public virtual string ApplicationName { get; set; }

        public virtual string WebRootPath { get; set; }

        public virtual IFileProvider WebRootFileProvider { get; set; }

        public virtual string ContentRootPath { get; set; }

        public virtual IFileProvider ContentRootFileProvider { get; set; }
    }
}
#endif
