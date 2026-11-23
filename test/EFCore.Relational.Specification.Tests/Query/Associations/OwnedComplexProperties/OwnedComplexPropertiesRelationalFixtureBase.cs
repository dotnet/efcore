// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

/// <summary>
///     Base fixture for tests exercising complex properties on owned entity types: the associates are owned entity types sharing
///     the table of their owner (the collection is mapped to a separate table). The required nested associate is mapped to columns
///     in the owned type's table, while the optional nested associate and the nested collection are mapped to JSON columns.
/// </summary>
public abstract class OwnedComplexPropertiesRelationalFixtureBase : OwnedComplexPropertiesFixtureBase, ITestSqlLoggerFactory
{
    public override bool AreCollectionsOrdered
        => false;

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // The key of the owned collection is declared on a complex type
        modelBuilder.Entity<RootEntity>().OwnsMany(
            e => e.AssociateCollection, b =>
            {
                b.HasKey($"{nameof(AssociateType.RequiredNestedAssociate)}.{nameof(NestedAssociateType.Id)}");
                b.ComplexProperty(r => r.RequiredNestedAssociate).Property(n => n.Id).ValueGeneratedNever();
            });
    }

    protected override void ConfigureAssociate(OwnedNavigationBuilder<RootEntity, AssociateType> builder)
    {
        base.ConfigureAssociate(builder);

        builder.ComplexProperty(r => r.OptionalNestedAssociate).ToJson();
        builder.ComplexCollection(r => r.NestedCollection).ToJson();
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
