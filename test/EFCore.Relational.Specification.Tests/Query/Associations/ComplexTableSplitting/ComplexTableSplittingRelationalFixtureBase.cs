﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

/// <summary>
///     Base fixture for tests exercising table splitting, where the entity and its contained complex type are mapped to the same
///     table, and the complex type's properties are mapped to columns in that table.
/// </summary>
/// <remarks>
///     Note that collections aren't supported with table splitting, so this fixture ignores them in the model configuration and
///     removes them from the seeding data.
/// </remarks>
public abstract class ComplexTableSplittingRelationalFixtureBase : ComplexPropertiesFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "ComplexTableSplittingQueryTest";

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RootEntity>(b =>
        {
            // Collections are not supported with table splitting, only JSON
            b.ComplexProperty(e => e.RequiredAssociate, rrb => rrb.Ignore(r => r.NestedCollection));
            b.ComplexProperty(e => e.OptionalAssociate, orb => orb.Ignore(o => o.NestedCollection));
            b.Ignore(r => r.AssociateCollection);
        });

        modelBuilder.Entity<ValueRootEntity>(b =>
        {
            // Collections are not supported with table splitting, only JSON
            b.ComplexProperty(e => e.RequiredAssociate, rrb => rrb.Ignore(r => r.NestedCollection));
            b.ComplexProperty(e => e.OptionalAssociate, orb => orb.Ignore(o => o.NestedCollection));
            b.Ignore(r => r.AssociateCollection);
        });
    }

    protected override AssociationsData CreateData()
    {
        var data = base.CreateData();

        foreach (var rootEntity in data.RootEntities)
        {
            rootEntity.RequiredAssociate.NestedCollection = null!;
            rootEntity.OptionalAssociate?.NestedCollection = null!;
            rootEntity.AssociateCollection = null!;
        }

        return data;
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
