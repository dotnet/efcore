// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class AdventureWorksPoolableSqliteContext : AdventureWorksContextBase
{
    public AdventureWorksPoolableSqliteContext(DbContextOptions<AdventureWorksPoolableSqliteContext> options)
        : base(options)
    {
    }

    protected override void ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
    {
    }
}
