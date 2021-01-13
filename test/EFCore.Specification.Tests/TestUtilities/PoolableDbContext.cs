// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class PoolableDbContext : DbContext
    {
        protected PoolableDbContext()
            : this(new DbContextOptions<PoolableDbContext>())
        {
        }

        public PoolableDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
