// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.SqlServer.FunctionalTests;
using System;

namespace Microsoft.AspNet.Diagnostics.Entity.Tests
{
    public class BloggingContext : DbContext
    {
        public BloggingContext(IServiceProvider provider, DbContextOptions options)
            : base(provider, options)
        { }

        public DbSet<Blog> Blogs { get; set; }
    }
}