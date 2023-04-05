// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Update;

public abstract class JsonUpdateFixtureBase : SharedStoreFixtureBase<JsonQueryContext>
{
    protected override string StoreName
        => "JsonUpdateTest";

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<JsonEntityBasic>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntityBasic>().OwnsOne(
            x => x.OwnedReferenceRoot, b =>
            {
                b.ToJson();
                b.WithOwner(x => x.Owner);
                b.OwnsOne(
                    x => x.OwnedReferenceBranch, bb =>
                    {
                        bb.Property(x => x.Fraction).HasPrecision(18, 2);
                        bb.OwnsOne(x => x.OwnedReferenceLeaf).WithOwner(x => x.Parent);
                        bb.OwnsMany(x => x.OwnedCollectionLeaf);
                    });
                b.OwnsMany(
                    x => x.OwnedCollectionBranch, bb =>
                    {
                        bb.Property(x => x.Fraction).HasPrecision(18, 2);
                        bb.OwnsOne(x => x.OwnedReferenceLeaf);
                        bb.Navigation(x => x.OwnedReferenceLeaf).IsRequired(false);
                        bb.OwnsMany(x => x.OwnedCollectionLeaf).WithOwner(x => x.Parent);
                    });
            });

        modelBuilder.Entity<JsonEntityBasic>().Navigation(x => x.OwnedReferenceRoot).IsRequired(false);

        modelBuilder.Entity<JsonEntityBasic>().OwnsMany(
            x => x.OwnedCollectionRoot, b =>
            {
                b.OwnsOne(
                    x => x.OwnedReferenceBranch, bb =>
                    {
                        bb.Property(x => x.Fraction).HasPrecision(18, 2);
                        bb.OwnsOne(x => x.OwnedReferenceLeaf);
                        bb.OwnsMany(x => x.OwnedCollectionLeaf).WithOwner(x => x.Parent);
                    });

                b.OwnsMany(
                    x => x.OwnedCollectionBranch, bb =>
                    {
                        bb.Property(x => x.Fraction).HasPrecision(18, 2);
                        bb.OwnsOne(x => x.OwnedReferenceLeaf).WithOwner(x => x.Parent);
                        bb.OwnsMany(x => x.OwnedCollectionLeaf);
                    });
                b.ToJson();
            });

        modelBuilder.Entity<JsonEntityInheritanceBase>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntityInheritanceBase>(
            b =>
            {
                b.OwnsOne(
                    x => x.ReferenceOnBase, bb =>
                    {
                        bb.ToJson();
                        bb.OwnsOne(x => x.OwnedReferenceLeaf);
                        bb.OwnsMany(x => x.OwnedCollectionLeaf);
                        bb.Property(x => x.Fraction).HasPrecision(18, 2);
                    });

                b.OwnsMany(
                    x => x.CollectionOnBase, bb =>
                    {
                        bb.ToJson();
                        bb.OwnsOne(x => x.OwnedReferenceLeaf);
                        bb.OwnsMany(x => x.OwnedCollectionLeaf);
                        bb.Property(x => x.Fraction).HasPrecision(18, 2);
                    });
            });

        modelBuilder.Entity<JsonEntityInheritanceDerived>(
            b =>
            {
                b.HasBaseType<JsonEntityInheritanceBase>();
                b.OwnsOne(
                    x => x.ReferenceOnDerived, bb =>
                    {
                        bb.ToJson();
                        bb.OwnsOne(x => x.OwnedReferenceLeaf);
                        bb.OwnsMany(x => x.OwnedCollectionLeaf);
                        bb.Property(x => x.Fraction).HasPrecision(18, 2);
                    });

                b.OwnsMany(
                    x => x.CollectionOnDerived, bb =>
                    {
                        bb.ToJson();
                        bb.OwnsOne(x => x.OwnedReferenceLeaf);
                        bb.OwnsMany(x => x.OwnedCollectionLeaf);
                        bb.Property(x => x.Fraction).HasPrecision(18, 2);
                    });
            });

        modelBuilder.Ignore<JsonEntityCustomNaming>();
        modelBuilder.Ignore<JsonEntitySingleOwned>();

        modelBuilder.Entity<JsonEntityAllTypes>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntityAllTypes>().OwnsOne(
            x => x.Reference, b =>
            {
                b.ToJson();
                b.Property(x => x.TestDecimal).HasPrecision(18, 3);
                b.Property(x => x.TestEnumWithIntConverter).HasConversion<int>();
                b.Property(x => x.TestNullableEnumWithIntConverter).HasConversion<int>();
                b.Property(x => x.TestNullableEnumWithConverterThatHandlesNulls).HasConversion(
                    new ValueConverter<JsonEnum?, string>(
                        x => x == null
                            ? "Null"
                            : x == JsonEnum.One
                                ? "One"
                                : x == JsonEnum.Two
                                    ? "Two"
                                    : x == JsonEnum.Three
                                        ? "Three"
                                        : "INVALID",
                        x => x == "One"
                            ? JsonEnum.One
                            : x == "Two"
                                ? JsonEnum.Two
                                : x == "Three"
                                    ? JsonEnum.Three
                                    : null,
                        convertsNulls: true));
            });
        modelBuilder.Entity<JsonEntityAllTypes>().OwnsMany(
            x => x.Collection, b =>
            {
                b.ToJson();
                b.Property(x => x.TestDecimal).HasPrecision(18, 3);
                b.Property(x => x.TestEnumWithIntConverter).HasConversion<int>();
                b.Property(x => x.TestNullableEnumWithIntConverter).HasConversion<int>();
                b.Property(x => x.TestNullableEnumWithConverterThatHandlesNulls).HasConversion(
                    new ValueConverter<JsonEnum?, string>(
                        x => x == null
                            ? "Null"
                            : x == JsonEnum.One
                                ? "One"
                                : x == JsonEnum.Two
                                    ? "Two"
                                    : x == JsonEnum.Three
                                        ? "Three"
                                        : "INVALID",
                        x => x == "One"
                            ? JsonEnum.One
                            : x == "Two"
                                ? JsonEnum.Two
                                : x == "Three"
                                    ? JsonEnum.Three
                                    : null,
                        convertsNulls: true));
            });

        base.OnModelCreating(modelBuilder, context);
    }

    protected override void Seed(JsonQueryContext context)
    {
        var jsonEntitiesBasic = JsonQueryData.CreateJsonEntitiesBasic();
        var jsonEntitiesInheritance = JsonQueryData.CreateJsonEntitiesInheritance();
        var jsonEntitiesAllTypes = JsonQueryData.CreateJsonEntitiesAllTypes();

        context.JsonEntitiesBasic.AddRange(jsonEntitiesBasic);
        context.JsonEntitiesInheritance.AddRange(jsonEntitiesInheritance);
        context.JsonEntitiesAllTypes.AddRange(jsonEntitiesAllTypes);
        context.SaveChanges();
    }
}
