// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class PoolableDbContext(DbContextOptions options) : DbContext(options)
{
    protected PoolableDbContext()
        : this(new DbContextOptions<PoolableDbContext>())
    {
    }
}
