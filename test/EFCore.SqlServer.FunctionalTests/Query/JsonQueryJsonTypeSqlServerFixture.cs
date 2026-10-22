// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonQueryJsonTypeSqlServerFixture : JsonQuerySqlServerFixture
{
    protected override string StoreName
        => "JsonQueryJsonTypeTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<JsonEntityBasic>(
            b =>
            {
                b.OwnsOne(x => x.OwnedReferenceRoot).ToJson().HasColumnType("json");
                b.OwnsMany(x => x.OwnedCollectionRoot).ToJson().HasColumnType("json");
            });

        modelBuilder.Entity<JsonEntityCustomNaming>(
            b =>
            {
                b.OwnsOne(x => x.OwnedReferenceRoot).ToJson("json_reference_custom_naming").HasColumnType("json");
                b.OwnsMany(x => x.OwnedCollectionRoot).HasColumnType("json").ToJson("json_collection_custom_naming");
            });

        modelBuilder.Entity<JsonEntitySingleOwned>().OwnsMany(x => x.OwnedCollection).ToJson().HasColumnType("json");

        modelBuilder.Entity<JsonEntityInheritanceBase>(
            b =>
            {
                b.OwnsOne(x => x.ReferenceOnBase).ToJson().HasColumnType("json");
                b.OwnsMany(x => x.CollectionOnBase).ToJson().HasColumnType("json");
            });

        modelBuilder.Entity<JsonEntityInheritanceDerived>(
            b =>
            {
                b.HasBaseType<JsonEntityInheritanceBase>();
                b.OwnsOne(x => x.ReferenceOnDerived).ToJson().HasColumnType("json");
                b.OwnsMany(x => x.CollectionOnDerived).ToJson().HasColumnType("json");
            });

        modelBuilder.Entity<JsonEntityAllTypes>(
            b =>
            {
                b.OwnsOne(x => x.Reference).ToJson().HasColumnType("json");
                b.OwnsMany(x => x.Collection).ToJson().HasColumnType("json");
                b.PrimitiveCollection(e => e.TestDefaultStringCollection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestMaxLengthStringCollection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestInt16Collection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestInt32Collection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestDecimalCollection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestDateTimeCollection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestDateTimeOffsetCollection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestTimeSpanCollection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestInt64Collection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestDoubleCollection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestSingleCollection).HasColumnType("json").IsRequired();
                b.PrimitiveCollection(e => e.TestBooleanCollection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestCharacterCollection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestByteCollection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestGuidCollection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestUnsignedInt16Collection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestUnsignedInt32Collection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestUnsignedInt64Collection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestSignedByteCollection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestNullableInt32Collection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestEnumCollection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestEnumWithIntConverterCollection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestNullableEnumCollection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestNullableEnumWithIntConverterCollection).HasColumnType("json");
                b.PrimitiveCollection(e => e.TestNullableEnumWithConverterThatHandlesNullsCollection).HasColumnType("json");
            });

        modelBuilder.Entity<JsonEntityConverters>().OwnsOne(x => x.Reference).ToJson().HasColumnType("json");
    }
}
