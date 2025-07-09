// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public abstract class ComplexJsonRelationalFixtureBase : ComplexPropertiesFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName => "ComplexJsonQueryTest";

    // TODO: Temporary, until we have update pipeline support for complex JSON
    protected override Task SeedAsync(PoolableDbContext context)
        => throw new NotImplementedException("Must be implemented in derived provider implementations using SQL, since the update pipeline does not yet support complex JSON.");

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.ComplexProperty(e => e.RequiredRelated, rrb => rrb.ToJson());
            b.ComplexProperty(e => e.OptionalRelated, orb => orb.ToJson());
            b.ComplexCollection(e => e.RelatedCollection, rcb => rcb.ToJson());
        });
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
