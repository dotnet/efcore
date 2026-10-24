// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class TemporalCompiledQuerySqlServerFixture : SharedStoreFixtureBase<TemporalCompiledQueryContext>
{
    // A dedicated store: the GearsOfWar temporal fixture mutates its data to build history and is
    // not idempotent, so sharing its StoreName across test classes corrupts it.
    protected override string StoreName
        => "TemporalCompiledQueryTest";

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        => modelBuilder.Entity<TemporalCompiledQueryCustomer>().ToTable("Customers", t => t.IsTemporal());
}

public class TemporalCompiledQueryContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<TemporalCompiledQueryCustomer> Customers
        => Set<TemporalCompiledQueryCustomer>();
}

public class TemporalCompiledQueryCustomer
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
