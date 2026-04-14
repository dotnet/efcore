// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class JsonQueryCosmosFixture : JsonQueryFixtureBase
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(
            builder.ConfigureWarnings(
                w => w.Ignore(CosmosEventId.NoPartitionKeyDefined)));

    public Task NoSyncTest(bool async, Func<bool, Task> testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(async, testCode);

    public void NoSyncTest(Action testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(testCode);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<JsonEntityBasic>()
            .ToContainer("JsonEntities")
            .HasDiscriminatorInJsonId()
            .HasDiscriminator<string>("Discriminator").HasValue("Basic");

        modelBuilder.Entity<EntityBasic>().ToContainer("EntitiesBasic");

        modelBuilder.Entity<JsonEntityBasicForReference>().ToContainer("EntitiesBasicForReference");

        modelBuilder.Entity<JsonEntityBasicForCollection>().ToContainer("EntitiesBasicForCollection");
        modelBuilder.Entity<JsonEntityBasic>().OwnsOne(
            x => x.OwnedReferenceRoot, b =>
            {
                // Issue #29380
                b.Ignore(x => x.Id);

                b.OwnsOne(
                    x => x.OwnedReferenceBranch, bb =>
                    {
                        //issue #34026
                        bb.Ignore(x => x.Enums);
                        bb.Ignore(x => x.NullableEnums);

                        // Issue #29380
                        bb.Ignore(x => x.Id);
                    });

                b.OwnsMany(
                    x => x.OwnedCollectionBranch, bb =>
                    {
                        //issue #34026
                        bb.Ignore(x => x.Enums);
                        bb.Ignore(x => x.NullableEnums);

                        // Issue #29380
                        bb.Ignore(x => x.Id);
                    });
            });

        modelBuilder.Entity<JsonEntityBasic>().OwnsMany(
            x => x.OwnedCollectionRoot, b =>
            {
                // Issue #29380
                b.Ignore(x => x.Id);

                b.OwnsOne(
                    x => x.OwnedReferenceBranch, bb =>
                    {
                        //issue #34026
                        bb.Ignore(x => x.Enums);
                        bb.Ignore(x => x.NullableEnums);

                        // Issue #29380
                        bb.Ignore(x => x.Id);
                    });

                b.OwnsMany(
                    x => x.OwnedCollectionBranch, bb =>
                    {
                        //issue #34026
                        bb.Ignore(x => x.Enums);
                        bb.Ignore(x => x.NullableEnums);

                        // Issue #29380
                        bb.Ignore(x => x.Id);
                    });
            });

        modelBuilder.Entity<JsonEntityCustomNaming>()
            .ToContainer("JsonEntities")
            .HasDiscriminatorInJsonId()
            .HasDiscriminator<string>("Discriminator").HasValue("CustomNaming");

        modelBuilder.Entity<JsonEntitySingleOwned>()
            .ToContainer("JsonEntities")
            .HasDiscriminatorInJsonId()
            .HasDiscriminator<string>("Discriminator").HasValue("SingleOwned");

        modelBuilder.Entity<JsonEntitySingleOwned>().OwnsMany(
            x => x.OwnedCollection, b =>
            {
                b.Ignore(x => x.Parent);
            });

        modelBuilder.Entity<JsonEntityInheritanceBase>().ToContainer("JsonEntitiesInheritance");

        modelBuilder.Entity<JsonEntityInheritanceBase>(
            b =>
            {
                b.OwnsOne(
                    x => x.ReferenceOnBase, bb =>
                    {
                        //issue #34026
                        bb.Ignore(x => x.Enums);
                        bb.Ignore(x => x.NullableEnums);

                        // Issue #29380
                        bb.Ignore(e => e.Id);
                    });

                b.OwnsMany(
                    x => x.CollectionOnBase, bb =>
                    {
                        //issue #34026
                        bb.Ignore(x => x.Enums);
                        bb.Ignore(x => x.NullableEnums);

                        // Issue #29380
                        bb.Ignore(e => e.Id);
                    });
            });

        modelBuilder.Entity<JsonEntityInheritanceDerived>(
            b =>
            {
                b.OwnsOne(
                    x => x.ReferenceOnDerived, bb =>
                    {
                        //issue #34026
                        bb.Ignore(x => x.Enums);
                        bb.Ignore(x => x.NullableEnums);

                        // Issue #29380
                        bb.Ignore(e => e.Id);
                    });

                b.OwnsMany(
                    x => x.CollectionOnDerived, bb =>
                    {
                        //issue #34026
                        bb.Ignore(x => x.Enums);
                        bb.Ignore(x => x.NullableEnums);

                        // Issue #29380
                        bb.Ignore(e => e.Id);
                    });
            });

        modelBuilder.Entity<JsonEntityAllTypes>()
            .ToContainer("JsonEntities")
            .HasDiscriminatorInJsonId()
            .HasDiscriminator<string>("Discriminator").HasValue("AllTypes");

        modelBuilder.Entity<JsonEntityAllTypes>().OwnsOne(
            x => x.Reference, b =>
            {
                //issue #34026
                b.Ignore(x => x.TestEnumCollection);
                b.Ignore(x => x.TestEnumWithIntConverterCollection);
                b.Ignore(x => x.TestGuidCollection);
                b.Ignore(x => x.TestNullableEnumCollection);
                b.Ignore(x => x.TestNullableEnumCollectionCollection);
                b.Ignore(x => x.TestNullableEnumWithConverterThatHandlesNullsCollection);
                b.Ignore(x => x.TestNullableEnumWithIntConverterCollection);
                b.Ignore(x => x.TestNullableEnumWithIntConverterCollectionCollection);
            });
        modelBuilder.Entity<JsonEntityAllTypes>().OwnsMany(
            x => x.Collection, b =>
            {
                //issue #34026
                b.Ignore(x => x.TestEnumCollection);
                b.Ignore(x => x.TestEnumWithIntConverterCollection);
                b.Ignore(x => x.TestGuidCollection);
                b.Ignore(x => x.TestNullableEnumCollection);
                b.Ignore(x => x.TestNullableEnumCollectionCollection);
                b.Ignore(x => x.TestNullableEnumWithConverterThatHandlesNullsCollection);
                b.Ignore(x => x.TestNullableEnumWithIntConverterCollection);
                b.Ignore(x => x.TestNullableEnumWithIntConverterCollectionCollection);
            });

        //issue #34026
        modelBuilder.Entity<JsonEntityAllTypes>().Ignore(x => x.TestEnumCollection);
        modelBuilder.Entity<JsonEntityAllTypes>().Ignore(x => x.TestEnumWithIntConverterCollection);
        modelBuilder.Entity<JsonEntityAllTypes>().Ignore(x => x.TestGuidCollection);
        modelBuilder.Entity<JsonEntityAllTypes>().Ignore(x => x.TestNullableEnumCollection);
        modelBuilder.Entity<JsonEntityAllTypes>().Ignore(x => x.TestNullableEnumCollectionCollection);
        modelBuilder.Entity<JsonEntityAllTypes>().Ignore(x => x.TestNullableEnumWithConverterThatHandlesNullsCollection);
        modelBuilder.Entity<JsonEntityAllTypes>().Ignore(x => x.TestNullableEnumWithIntConverterCollection);
        modelBuilder.Entity<JsonEntityAllTypes>().Ignore(x => x.TestNullableEnumWithIntConverterCollectionCollection);

        modelBuilder.Entity<JsonEntityConverters>()
            .ToContainer("JsonEntities")
            .HasDiscriminatorInJsonId()
            .HasDiscriminator<string>("Discriminator").HasValue("Converters");
    }

    // TODO: remove all this infra once we support converters on cosmos
    // also undo virtual on base
    // issue #34026
    public override IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(EntityBasic), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (EntityBasic)e;
                    var aa = (EntityBasic)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        },
        {
            typeof(JsonEntityBasic), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonEntityBasic)e;
                    var aa = (JsonEntityBasic)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);

                    if (ee.OwnedReferenceRoot is not null || aa.OwnedReferenceRoot is not null)
                    {
                        AssertCosmosOwnedRoot(ee.OwnedReferenceRoot, aa.OwnedReferenceRoot);

                        Assert.Equal(ee.OwnedCollectionRoot.Count, aa.OwnedCollectionRoot.Count);
                        for (var i = 0; i < ee.OwnedCollectionRoot.Count; i++)
                        {
                            AssertCosmosOwnedRoot(ee.OwnedCollectionRoot[i], aa.OwnedCollectionRoot[i]);
                        }
                    }
                }
            }
        },
        {
            typeof(JsonEntityBasicForReference), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonEntityBasicForReference)e;
                    var aa = (JsonEntityBasicForReference)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentId, aa.ParentId);
                }
            }
        },
        {
            typeof(JsonEntityBasicForCollection), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonEntityBasicForCollection)e;
                    var aa = (JsonEntityBasicForCollection)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ParentId, aa.ParentId);
                }
            }
        },
        {
            typeof(JsonOwnedRoot), (e, a) =>
            {
                if (a != null)
                {
                    var ee = (JsonOwnedRoot)e;
                    var aa = (JsonOwnedRoot)a;

                    AssertCosmosOwnedRoot(ee, aa);
                }
            }
        },
        {
            typeof(JsonOwnedBranch), (e, a) =>
            {
                if (a != null)
                {
                    var ee = (JsonOwnedBranch)e;
                    var aa = (JsonOwnedBranch)a;

                    AssertCosmosOwnedBranch(ee, aa);
                }
            }
        },
        {
            typeof(JsonOwnedLeaf), (e, a) =>
            {
                if (a != null)
                {
                    var ee = (JsonOwnedLeaf)e;
                    var aa = (JsonOwnedLeaf)a;

                    AssertOwnedLeaf(ee, aa);
                }
            }
        },
        {
            typeof(JsonEntityCustomNaming), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonEntityCustomNaming)e;
                    var aa = (JsonEntityCustomNaming)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Title, aa.Title);

                    AssertCustomNameRoot(ee.OwnedReferenceRoot, aa.OwnedReferenceRoot);

                    Assert.Equal(ee.OwnedCollectionRoot.Count, aa.OwnedCollectionRoot.Count);
                    for (var i = 0; i < ee.OwnedCollectionRoot.Count; i++)
                    {
                        AssertCustomNameRoot(ee.OwnedCollectionRoot[i], aa.OwnedCollectionRoot[i]);
                    }
                }
            }
        },
        {
            typeof(JsonOwnedCustomNameRoot), (e, a) =>
            {
                if (a != null)
                {
                    var ee = (JsonOwnedCustomNameRoot)e;
                    var aa = (JsonOwnedCustomNameRoot)a;

                    AssertCustomNameRoot(ee, aa);
                }
            }
        },
        {
            typeof(JsonOwnedCustomNameBranch), (e, a) =>
            {
                if (a != null)
                {
                    var ee = (JsonOwnedCustomNameBranch)e;
                    var aa = (JsonOwnedCustomNameBranch)a;

                    AssertCustomNameBranch(ee, aa);
                }
            }
        },
        {
            typeof(JsonEntitySingleOwned), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonEntitySingleOwned)e;
                    var aa = (JsonEntitySingleOwned)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);

                    Assert.Equal(ee.OwnedCollection?.Count ?? 0, aa.OwnedCollection?.Count ?? 0);
                    for (var i = 0; i < ee.OwnedCollection.Count; i++)
                    {
                        AssertOwnedLeaf(ee.OwnedCollection[i], aa.OwnedCollection[i]);
                    }
                }
            }
        },
        {
            typeof(JsonEntityInheritanceBase), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonEntityInheritanceBase)e;
                    var aa = (JsonEntityInheritanceBase)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);

                    AssertCosmosOwnedBranch(ee.ReferenceOnBase, aa.ReferenceOnBase);
                    Assert.Equal(ee.CollectionOnBase?.Count ?? 0, aa.CollectionOnBase?.Count ?? 0);
                    for (var i = 0; i < ee.CollectionOnBase.Count; i++)
                    {
                        AssertCosmosOwnedBranch(ee.CollectionOnBase[i], aa.CollectionOnBase[i]);
                    }
                }
            }
        },
        {
            typeof(JsonEntityInheritanceDerived), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonEntityInheritanceDerived)e;
                    var aa = (JsonEntityInheritanceDerived)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Fraction, aa.Fraction);

                    AssertCosmosOwnedBranch(ee.ReferenceOnBase, aa.ReferenceOnBase);
                    AssertCosmosOwnedBranch(ee.ReferenceOnDerived, aa.ReferenceOnDerived);

                    Assert.Equal(ee.CollectionOnBase?.Count ?? 0, aa.CollectionOnBase?.Count ?? 0);
                    for (var i = 0; i < ee.CollectionOnBase.Count; i++)
                    {
                        AssertCosmosOwnedBranch(ee.CollectionOnBase[i], aa.CollectionOnBase[i]);
                    }

                    Assert.Equal(ee.CollectionOnDerived?.Count ?? 0, aa.CollectionOnDerived?.Count ?? 0);
                    for (var i = 0; i < ee.CollectionOnDerived.Count; i++)
                    {
                        AssertCosmosOwnedBranch(ee.CollectionOnDerived[i], aa.CollectionOnDerived[i]);
                    }
                }
            }
        },
        {
            typeof(JsonEntityAllTypes), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonEntityAllTypes)e;
                    var aa = (JsonEntityAllTypes)a;

                    Assert.Equal(ee.Id, aa.Id);

                    AssertCosmosAllTypes(ee.Reference, aa.Reference);

                    Assert.Equal(ee.Collection?.Count ?? 0, aa.Collection?.Count ?? 0);
                    for (var i = 0; i < ee.Collection.Count; i++)
                    {
                        AssertCosmosAllTypes(ee.Collection[i], aa.Collection[i]);
                    }
                }
            }
        },
        {
            typeof(JsonOwnedAllTypes), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonOwnedAllTypes)e;
                    var aa = (JsonOwnedAllTypes)a;

                    AssertCosmosAllTypes(ee, aa);
                }
            }
        },
        {
            typeof(JsonEntityConverters), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonEntityConverters)e;
                    var aa = (JsonEntityConverters)a;

                    Assert.Equal(ee.Id, aa.Id);

                    AssertCosmosConverters(ee.Reference, aa.Reference);
                }
            }
        },
        {
            typeof(JsonOwnedConverters), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (JsonOwnedConverters)e;
                    var aa = (JsonOwnedConverters)a;

                    AssertCosmosConverters(ee, aa);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public static void AssertCosmosOwnedRoot(JsonOwnedRoot expected, JsonOwnedRoot actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Number, actual.Number);
        Assert.Equal(expected.Names, actual.Names);
        Assert.Equal(expected.Numbers, actual.Numbers);

        AssertCosmosOwnedBranch(expected.OwnedReferenceBranch, actual.OwnedReferenceBranch);
        Assert.Equal(expected.OwnedCollectionBranch.Count, actual.OwnedCollectionBranch.Count);
        for (var i = 0; i < expected.OwnedCollectionBranch.Count; i++)
        {
            AssertCosmosOwnedBranch(expected.OwnedCollectionBranch[i], actual.OwnedCollectionBranch[i]);
        }
    }

    public static void AssertCosmosOwnedBranch(JsonOwnedBranch expected, JsonOwnedBranch actual)
    {
        Assert.Equal(expected.Date, actual.Date);
        Assert.Equal(expected.Fraction, actual.Fraction);
        Assert.Equal(expected.Enum, actual.Enum);
        Assert.Equal(expected.NullableEnum, actual.NullableEnum);

        AssertOwnedLeaf(expected.OwnedReferenceLeaf, actual.OwnedReferenceLeaf);
        Assert.Equal(expected.OwnedCollectionLeaf.Count, actual.OwnedCollectionLeaf.Count);
        for (var i = 0; i < expected.OwnedCollectionLeaf.Count; i++)
        {
            AssertOwnedLeaf(expected.OwnedCollectionLeaf[i], actual.OwnedCollectionLeaf[i]);
        }
    }

    public static void AssertCosmosAllTypes(JsonOwnedAllTypes expected, JsonOwnedAllTypes actual)
    {
        Assert.Equal(expected.TestDefaultString, actual.TestDefaultString);
        Assert.Equal(expected.TestMaxLengthString, actual.TestMaxLengthString);
        Assert.Equal(expected.TestBoolean, actual.TestBoolean);
        Assert.Equal(expected.TestCharacter, actual.TestCharacter);
        Assert.Equal(expected.TestDateTime, actual.TestDateTime);
        Assert.Equal(expected.TestDateTimeOffset, actual.TestDateTimeOffset);
        Assert.Equal(expected.TestDouble, actual.TestDouble);
        Assert.Equal(expected.TestGuid, actual.TestGuid);
        Assert.Equal(expected.TestInt16, actual.TestInt16);
        Assert.Equal(expected.TestInt32, actual.TestInt32);
        Assert.Equal(expected.TestInt64, actual.TestInt64);
        Assert.Equal(expected.TestSignedByte, actual.TestSignedByte);
        Assert.Equal(expected.TestSingle, actual.TestSingle);
        Assert.Equal(expected.TestTimeSpan, actual.TestTimeSpan);
        Assert.Equal(expected.TestDateOnly, actual.TestDateOnly);
        Assert.Equal(expected.TestTimeOnly, actual.TestTimeOnly);
        Assert.Equal(expected.TestUnsignedInt16, actual.TestUnsignedInt16);
        Assert.Equal(expected.TestUnsignedInt32, actual.TestUnsignedInt32);
        Assert.Equal(expected.TestUnsignedInt64, actual.TestUnsignedInt64);
        Assert.Equal(expected.TestNullableInt32, actual.TestNullableInt32);
        Assert.Equal(expected.TestEnum, actual.TestEnum);
        Assert.Equal(expected.TestEnumWithIntConverter, actual.TestEnumWithIntConverter);
        Assert.Equal(expected.TestNullableEnum, actual.TestNullableEnum);
        Assert.Equal(expected.TestNullableEnumWithIntConverter, actual.TestNullableEnumWithIntConverter);
        Assert.Equal(expected.TestNullableEnumWithConverterThatHandlesNulls, actual.TestNullableEnumWithConverterThatHandlesNulls);

        AssertPrimitiveCollection(expected.TestDefaultStringCollection, actual.TestDefaultStringCollection);
        AssertPrimitiveCollection(expected.TestMaxLengthStringCollection, actual.TestMaxLengthStringCollection);
        AssertPrimitiveCollection(expected.TestBooleanCollection, actual.TestBooleanCollection);
        AssertPrimitiveCollection(expected.TestCharacterCollection, actual.TestCharacterCollection);
        AssertPrimitiveCollection(expected.TestDateTimeCollection, actual.TestDateTimeCollection);
        AssertPrimitiveCollection(expected.TestDateTimeOffsetCollection, actual.TestDateTimeOffsetCollection);
        AssertPrimitiveCollection(expected.TestDoubleCollection, actual.TestDoubleCollection);
        //AssertPrimitiveCollection(expected.TestGuidCollection, actual.TestGuidCollection);
        AssertPrimitiveCollection((IList<short>)expected.TestInt16Collection, (IList<short>)actual.TestInt16Collection);
        AssertPrimitiveCollection(expected.TestInt32Collection, actual.TestInt32Collection);
        AssertPrimitiveCollection(expected.TestInt64Collection, actual.TestInt64Collection);
        AssertPrimitiveCollection(expected.TestSignedByteCollection, actual.TestSignedByteCollection);
        AssertPrimitiveCollection(expected.TestSingleCollection, actual.TestSingleCollection);
        AssertPrimitiveCollection(expected.TestTimeSpanCollection, actual.TestTimeSpanCollection);
        AssertPrimitiveCollection(expected.TestDateOnlyCollection, actual.TestDateOnlyCollection);
        AssertPrimitiveCollection(expected.TestTimeOnlyCollection, actual.TestTimeOnlyCollection);
        AssertPrimitiveCollection(expected.TestUnsignedInt16Collection, actual.TestUnsignedInt16Collection);
        AssertPrimitiveCollection(expected.TestUnsignedInt32Collection, actual.TestUnsignedInt32Collection);
        AssertPrimitiveCollection(expected.TestUnsignedInt64Collection, actual.TestUnsignedInt64Collection);
        AssertPrimitiveCollection(expected.TestNullableInt32Collection, actual.TestNullableInt32Collection);
        //AssertPrimitiveCollection(expected.TestEnumCollection, actual.TestEnumCollection);
        //AssertPrimitiveCollection(expected.TestEnumWithIntConverterCollection, actual.TestEnumWithIntConverterCollection);
        //AssertPrimitiveCollection(expected.TestNullableEnumCollection, actual.TestNullableEnumCollection);
        //AssertPrimitiveCollection(expected.TestNullableEnumWithIntConverterCollection, actual.TestNullableEnumWithIntConverterCollection);
        //AssertPrimitiveCollection(
        //    expected.TestNullableEnumWithConverterThatHandlesNullsCollection,
        //    actual.TestNullableEnumWithConverterThatHandlesNullsCollection);
    }

    public static void AssertCosmosConverters(JsonOwnedConverters expected, JsonOwnedConverters actual)
    {
        Assert.Equal(expected.BoolConvertedToIntZeroOne, actual.BoolConvertedToIntZeroOne);
        Assert.Equal(expected.BoolConvertedToStringTrueFalse, actual.BoolConvertedToStringTrueFalse);
        Assert.Equal(expected.BoolConvertedToStringYN, actual.BoolConvertedToStringYN);
        Assert.Equal(expected.IntZeroOneConvertedToBool, actual.IntZeroOneConvertedToBool);
        Assert.Equal(expected.StringTrueFalseConvertedToBool, actual.StringTrueFalseConvertedToBool);
        Assert.Equal(expected.StringYNConvertedToBool, actual.StringYNConvertedToBool);
    }
}
