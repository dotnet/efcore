// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.NullSemantics
{
    public class NullSemanticsContext : DbContext
    {
        public static readonly string StoreName = "NullSemantics";

        public NullSemanticsContext(IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
        }

        public DbSet<NullSemanticsEntity1> Entities1 { get; set; }
        public DbSet<NullSemanticsEntity2> Entities2 { get; set; }
    }
}
