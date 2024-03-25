// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class JsonQueryFixtureBase : SharedStoreFixtureBase<JsonQueryContext>, IQueryFixtureBase, ITestSqlLoggerFactory
{
    private JsonQueryData _expectedData;

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public virtual ISetSource GetExpectedData()
    {
        if (_expectedData == null)
        {
            _expectedData = new JsonQueryData();
        }

        return _expectedData;
    }

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(EntityBasic), e => ((EntityBasic)e)?.Id },
        { typeof(JsonEntityBasic), e => ((JsonEntityBasic)e)?.Id },
        { typeof(JsonEntityBasicForReference), e => ((JsonEntityBasicForReference)e)?.Id },
        { typeof(JsonEntityBasicForCollection), e => ((JsonEntityBasicForCollection)e)?.Id },
        { typeof(JsonEntityCustomNaming), e => ((JsonEntityCustomNaming)e)?.Id },
        { typeof(JsonEntitySingleOwned), e => ((JsonEntitySingleOwned)e)?.Id },
        { typeof(JsonEntityInheritanceBase), e => ((JsonEntityInheritanceBase)e)?.Id },
        { typeof(JsonEntityInheritanceDerived), e => ((JsonEntityInheritanceDerived)e)?.Id },
        { typeof(JsonEntityAllTypes), e => ((JsonEntityAllTypes)e)?.Id },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
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
                        AssertOwnedRoot(ee.OwnedReferenceRoot, aa.OwnedReferenceRoot);

                        Assert.Equal(ee.OwnedCollectionRoot.Count, aa.OwnedCollectionRoot.Count);
                        for (var i = 0; i < ee.OwnedCollectionRoot.Count; i++)
                        {
                            AssertOwnedRoot(ee.OwnedCollectionRoot[i], aa.OwnedCollectionRoot[i]);
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

                    AssertOwnedRoot(ee, aa);
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

                    AssertOwnedBranch(ee, aa);
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

                    AssertOwnedBranch(ee.ReferenceOnBase, aa.ReferenceOnBase);
                    Assert.Equal(ee.CollectionOnBase?.Count ?? 0, aa.CollectionOnBase?.Count ?? 0);
                    for (var i = 0; i < ee.CollectionOnBase.Count; i++)
                    {
                        AssertOwnedBranch(ee.CollectionOnBase[i], aa.CollectionOnBase[i]);
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

                    AssertOwnedBranch(ee.ReferenceOnBase, aa.ReferenceOnBase);
                    AssertOwnedBranch(ee.ReferenceOnDerived, aa.ReferenceOnDerived);

                    Assert.Equal(ee.CollectionOnBase?.Count ?? 0, aa.CollectionOnBase?.Count ?? 0);
                    for (var i = 0; i < ee.CollectionOnBase.Count; i++)
                    {
                        AssertOwnedBranch(ee.CollectionOnBase[i], aa.CollectionOnBase[i]);
                    }

                    Assert.Equal(ee.CollectionOnDerived?.Count ?? 0, aa.CollectionOnDerived?.Count ?? 0);
                    for (var i = 0; i < ee.CollectionOnDerived.Count; i++)
                    {
                        AssertOwnedBranch(ee.CollectionOnDerived[i], aa.CollectionOnDerived[i]);
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

                    AssertAllTypes(ee.Reference, aa.Reference);

                    Assert.Equal(ee.Collection?.Count ?? 0, aa.Collection?.Count ?? 0);
                    for (var i = 0; i < ee.Collection.Count; i++)
                    {
                        AssertAllTypes(ee.Collection[i], aa.Collection[i]);
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

                    AssertAllTypes(ee, aa);
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

                    AssertConverters(ee.Reference, aa.Reference);
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

                    AssertConverters(ee, aa);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public static void AssertOwnedRoot(JsonOwnedRoot expected, JsonOwnedRoot actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Number, actual.Number);
        Assert.Equal(expected.Names, actual.Names);
        Assert.Equal(expected.Numbers, actual.Numbers);

        AssertOwnedBranch(expected.OwnedReferenceBranch, actual.OwnedReferenceBranch);
        Assert.Equal(expected.OwnedCollectionBranch.Count, actual.OwnedCollectionBranch.Count);
        for (var i = 0; i < expected.OwnedCollectionBranch.Count; i++)
        {
            AssertOwnedBranch(expected.OwnedCollectionBranch[i], actual.OwnedCollectionBranch[i]);
        }
    }

    public static void AssertOwnedBranch(JsonOwnedBranch expected, JsonOwnedBranch actual)
    {
        Assert.Equal(expected.Date, actual.Date);
        Assert.Equal(expected.Fraction, actual.Fraction);
        Assert.Equal(expected.Enum, actual.Enum);
        Assert.Equal(expected.NullableEnum, actual.NullableEnum);
        Assert.Equal(expected.Enums, actual.Enums);
        Assert.Equal(expected.NullableEnums, actual.NullableEnums);

        AssertOwnedLeaf(expected.OwnedReferenceLeaf, actual.OwnedReferenceLeaf);
        Assert.Equal(expected.OwnedCollectionLeaf.Count, actual.OwnedCollectionLeaf.Count);
        for (var i = 0; i < expected.OwnedCollectionLeaf.Count; i++)
        {
            AssertOwnedLeaf(expected.OwnedCollectionLeaf[i], actual.OwnedCollectionLeaf[i]);
        }
    }

    public static void AssertOwnedLeaf(JsonOwnedLeaf expected, JsonOwnedLeaf actual)
        => Assert.Equal(expected.SomethingSomething, actual.SomethingSomething);

    public static void AssertCustomNameRoot(JsonOwnedCustomNameRoot expected, JsonOwnedCustomNameRoot actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Number, actual.Number);
        Assert.Equal(expected.Enum, actual.Enum);
        AssertCustomNameBranch(expected.OwnedReferenceBranch, actual.OwnedReferenceBranch);
        Assert.Equal(expected.OwnedCollectionBranch.Count, actual.OwnedCollectionBranch.Count);
        for (var i = 0; i < expected.OwnedCollectionBranch.Count; i++)
        {
            AssertCustomNameBranch(expected.OwnedCollectionBranch[i], actual.OwnedCollectionBranch[i]);
        }
    }

    public static void AssertCustomNameBranch(JsonOwnedCustomNameBranch expected, JsonOwnedCustomNameBranch actual)
    {
        Assert.Equal(expected.Date, actual.Date);
        Assert.Equal(expected.Fraction, actual.Fraction);
    }

    public static void AssertAllTypes(JsonOwnedAllTypes expected, JsonOwnedAllTypes actual)
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
        AssertPrimitiveCollection(expected.TestGuidCollection, actual.TestGuidCollection);
        AssertPrimitiveCollection(expected.TestInt16Collection, actual.TestInt16Collection);
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
        AssertPrimitiveCollection(expected.TestEnumCollection, actual.TestEnumCollection);
        AssertPrimitiveCollection(expected.TestEnumWithIntConverterCollection, actual.TestEnumWithIntConverterCollection);
        AssertPrimitiveCollection(expected.TestNullableEnumCollection, actual.TestNullableEnumCollection);
        AssertPrimitiveCollection(expected.TestNullableEnumWithIntConverterCollection, actual.TestNullableEnumWithIntConverterCollection);
        AssertPrimitiveCollection(
            expected.TestNullableEnumWithConverterThatHandlesNullsCollection,
            actual.TestNullableEnumWithConverterThatHandlesNullsCollection);
    }

    public static void AssertPrimitiveCollection<T>(IList<T> expected, IList<T> actual)
    {
        Assert.Equal(expected?.Count, actual?.Count);
        for (var i = 0; i < (expected?.Count ?? 0); i++)
        {
            Assert.Equal(expected![i], actual![i]);
        }
    }

    public static void AssertConverters(JsonOwnedConverters expected, JsonOwnedConverters actual)
    {
        Assert.Equal(expected.BoolConvertedToIntZeroOne, actual.BoolConvertedToIntZeroOne);
        Assert.Equal(expected.BoolConvertedToStringTrueFalse, actual.BoolConvertedToStringTrueFalse);
        Assert.Equal(expected.BoolConvertedToStringYN, actual.BoolConvertedToStringYN);
        Assert.Equal(expected.IntZeroOneConvertedToBool, actual.IntZeroOneConvertedToBool);
        Assert.Equal(expected.StringTrueFalseConvertedToBool, actual.StringTrueFalseConvertedToBool);
        Assert.Equal(expected.StringYNConvertedToBool, actual.StringYNConvertedToBool);
    }

    protected override string StoreName { get; } = "JsonQueryTest";

    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override JsonQueryContext CreateContext()
    {
        var context = base.CreateContext();

        return context;
    }

    protected override async Task SeedAsync(JsonQueryContext context)
        => await JsonQueryContext.SeedAsync(context);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<JsonEntityBasic>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<EntityBasic>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntityBasicForReference>(
            b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.Property(x => x.Name);
            });

        modelBuilder.Entity<JsonEntityBasicForCollection>().Property(x => x.Id).ValueGeneratedNever();
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
                        bb.Navigation(x => x.OwnedReferenceLeaf).IsRequired();
                        bb.OwnsMany(x => x.OwnedCollectionLeaf);
                    });

                b.OwnsMany(
                    x => x.OwnedCollectionBranch, bb =>
                    {
                        bb.Property(x => x.Fraction).HasPrecision(18, 2);
                        bb.OwnsOne(x => x.OwnedReferenceLeaf);
                        bb.OwnsMany(x => x.OwnedCollectionLeaf).WithOwner(x => x.Parent);
                    });
            });

        modelBuilder.Entity<JsonEntityBasic>().Navigation(x => x.OwnedReferenceRoot).IsRequired();

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

        modelBuilder.Entity<JsonEntityCustomNaming>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntityCustomNaming>().OwnsOne(
            x => x.OwnedReferenceRoot, b =>
            {
                b.Property(x => x.Enum).HasConversion<int>();
                b.OwnsOne(x => x.OwnedReferenceBranch);
                b.OwnsMany(x => x.OwnedCollectionBranch);
                b.ToJson("json_reference_custom_naming");
            });

        modelBuilder.Entity<JsonEntityCustomNaming>().OwnsMany(
            x => x.OwnedCollectionRoot, b =>
            {
                b.ToJson("json_collection_custom_naming");
                b.Property(x => x.Enum).HasConversion<int>();
                b.OwnsOne(x => x.OwnedReferenceBranch);
                b.OwnsMany(x => x.OwnedCollectionBranch);
            });

        modelBuilder.Entity<JsonEntitySingleOwned>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntitySingleOwned>().OwnsMany(
            x => x.OwnedCollection, b =>
            {
                b.ToJson();
                b.Ignore(x => x.Parent);
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

        modelBuilder.Entity<JsonEntityAllTypes>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntityAllTypes>().OwnsOne(
            x => x.Reference, b =>
            {
                b.ToJson();
                b.Property(x => x.TestMaxLengthString).HasMaxLength(5);
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
                b.Property(x => x.TestMaxLengthString).HasMaxLength(5);
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

        modelBuilder.Entity<JsonEntityConverters>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntityConverters>().OwnsOne(
            x => x.Reference, b =>
            {
                b.ToJson();
                b.Property(x => x.BoolConvertedToIntZeroOne).HasConversion<BoolToZeroOneConverter<int>>();
                b.Property(x => x.BoolConvertedToStringTrueFalse).HasConversion(new BoolToStringConverter("False", "True"));
                b.Property(x => x.BoolConvertedToStringYN).HasConversion(new BoolToStringConverter("N", "Y"));
                b.Property(x => x.IntZeroOneConvertedToBool).HasConversion(
                    new ValueConverter<int, bool>(
                        x => x == 0 ? false : true,
                        x => x == false ? 0 : 1));

                b.Property(x => x.StringTrueFalseConvertedToBool).HasConversion(
                    new ValueConverter<string, bool>(
                        x => x == "True" ? true : false,
                        x => x == true ? "True" : "False"));

                b.Property(x => x.StringYNConvertedToBool).HasConversion(
                    new ValueConverter<string, bool>(
                        x => x == "Y" ? true : false,
                        x => x == true ? "Y" : "N"));
            });
    }
}
