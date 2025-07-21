// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Relationships.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public abstract class ComplexJsonRelationalFixtureBase : ComplexPropertiesFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName => "ComplexJsonQueryTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.ComplexProperty(e => e.RequiredRelated, rrb =>
            {
                rrb.ToJson();

                rrb.ComplexProperty(r => r.OptionalNested).IsRequired(false);
            });

            b.ComplexProperty(e => e.OptionalRelated, orb =>
            {
                orb.ToJson();
                orb.IsRequired(false);

                orb.ComplexProperty(r => r.OptionalNested).IsRequired(false);

                orb.ComplexProperty(r => r.RequiredNested);
            });

            b.ComplexCollection(e => e.RelatedCollection,rcb =>
            {
                rcb.ToJson();

                rcb.ComplexProperty(r => r.OptionalNested).IsRequired(false);
            });
        });
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
