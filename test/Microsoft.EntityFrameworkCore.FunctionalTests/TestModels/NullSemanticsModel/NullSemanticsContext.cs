// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.NullSemanticsModel
{
    public class NullSemanticsContext : DbContext
    {
        public static readonly string StoreName = "NullSemantics";

        public NullSemanticsContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<NullSemanticsEntity1> Entities1 { get; set; }
        public DbSet<NullSemanticsEntity2> Entities2 { get; set; }
    }
}
