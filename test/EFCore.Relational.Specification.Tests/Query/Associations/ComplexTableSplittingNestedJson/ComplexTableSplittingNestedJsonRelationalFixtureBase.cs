// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplittingNestedJson;

/// <summary>
///     Base fixture for tests exercising JSON complex properties nested in table-split complex properties: the associates are mapped
///     to columns in the table of the entity, while their nested associates and nested collection are each mapped to a JSON column in
///     the same table. The associate collection is mapped to a JSON column as a whole.
/// </summary>
public abstract class ComplexTableSplittingNestedJsonRelationalFixtureBase : ComplexPropertiesFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "ComplexTableSplittingNestedJsonQueryTest";

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RootEntity>(b =>
        {
            b.ComplexProperty(e => e.RequiredAssociate, ConfigureAssociate);
            b.ComplexProperty(e => e.OptionalAssociate, ConfigureAssociate);
            b.ComplexCollection(e => e.AssociateCollection, rcb => rcb.ToJson());

            static void ConfigureAssociate(ComplexPropertyBuilder<AssociateType> builder)
            {
                builder.ComplexProperty(r => r.RequiredNestedAssociate, nb => nb.ToJson());
                builder.ComplexProperty(r => r.OptionalNestedAssociate, nb => nb.ToJson());
                builder.ComplexCollection(r => r.NestedCollection, nb => nb.ToJson());
            }
        });

        modelBuilder.Entity<ValueRootEntity>(b =>
        {
            b.ComplexProperty(e => e.RequiredAssociate, ConfigureAssociate);
            b.ComplexProperty(e => e.OptionalAssociate, ConfigureAssociate);
            b.ComplexCollection(e => e.AssociateCollection, rcb => rcb.ToJson());

            static void ConfigureAssociate(ComplexPropertyBuilder<ValueAssociateType> builder)
            {
                builder.ComplexProperty(r => r.RequiredNested, nb => nb.ToJson());
                builder.ComplexProperty(r => r.OptionalNested).ToJson();
                builder.ComplexCollection(r => r.NestedCollection, nb => nb.ToJson());
            }
        });
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
