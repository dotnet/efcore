// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class JsonQueryRelationalFixture : JsonQueryFixtureBase, ITestSqlLoggerFactory
{
    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<JsonEntityBasic>().OwnsOne(x => x.OwnedReferenceRoot).ToJson();
        modelBuilder.Entity<JsonEntityBasic>().OwnsMany(x => x.OwnedCollectionRoot).ToJson();

        modelBuilder.Entity<JsonEntityCustomNaming>().OwnsOne(
            x => x.OwnedReferenceRoot, b =>
            {
                b.ToJson("json_reference_custom_naming");
                b.OwnsOne(x => x.OwnedReferenceBranch);
                b.OwnsMany(x => x.OwnedCollectionBranch);
            });

        modelBuilder.Entity<JsonEntityCustomNaming>().OwnsMany(
            x => x.OwnedCollectionRoot, b =>
            {
                b.ToJson("json_collection_custom_naming");
                b.OwnsOne(x => x.OwnedReferenceBranch);
                b.OwnsMany(x => x.OwnedCollectionBranch);
            });

        modelBuilder.Entity<JsonEntitySingleOwned>().OwnsMany(x => x.OwnedCollection).ToJson();

        modelBuilder.Entity<JsonEntityInheritanceBase>(
            b =>
            {
                b.OwnsOne(x => x.ReferenceOnBase).ToJson();
                b.OwnsMany(x => x.CollectionOnBase).ToJson();
            });

        modelBuilder.Entity<JsonEntityInheritanceDerived>(
            b =>
            {
                b.HasBaseType<JsonEntityInheritanceBase>();
                b.OwnsOne(x => x.ReferenceOnDerived).ToJson();
                b.OwnsMany(x => x.CollectionOnDerived).ToJson();
            });

        modelBuilder.Entity<JsonEntityAllTypes>().OwnsOne(x => x.Reference).ToJson();
        modelBuilder.Entity<JsonEntityAllTypes>().OwnsMany(x => x.Collection).ToJson();

        modelBuilder.Entity<JsonEntityConverters>().OwnsOne(x => x.Reference).ToJson();
    }
}
