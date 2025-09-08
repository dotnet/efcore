// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public abstract class ComplexJsonRelationalFixtureBase : ComplexPropertiesFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "ComplexJsonQueryTest";

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.ComplexProperty(e => e.RequiredRelated, rrb => rrb.ToJson());
            b.ComplexProperty(e => e.OptionalRelated, orb => orb.ToJson());
            b.ComplexCollection(e => e.RelatedCollection, rcb => rcb.ToJson());
        });

        modelBuilder.Entity<ValueRootEntity>(b =>
        {
            b.ComplexProperty(e => e.RequiredRelated, rrb => rrb.ToJson());

            b.ComplexProperty(e => e.OptionalRelated, orb =>
            {
                orb.ToJson();

                // TODO: Without the following, we get an ambiguous property error
                orb.ComplexProperty(r => r.OptionalNested).IsRequired(false);
            });

            b.ComplexCollection(e => e.RelatedCollection, rcb => rcb.ToJson());
        });
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
