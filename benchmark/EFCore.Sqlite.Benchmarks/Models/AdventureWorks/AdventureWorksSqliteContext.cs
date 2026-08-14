// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class AdventureWorksSqliteContext : AdventureWorksContextBase
{
    protected override void ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(AdventureWorksSqliteFixture.ConnectionString);
}
