// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class AdventureWorksSqlServerContext : AdventureWorksContextBase
{
    protected override void ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(AdventureWorksSqlServerFixture.ConnectionString);
}
