// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class JsonQueryFixtureBase : SharedStoreFixtureBase<JsonQueryContext>, IQueryFixtureBase
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

                    AssertOwnedRoot(ee.OwnedReferenceRoot, aa.OwnedReferenceRoot);

                    Assert.Equal(ee.OwnedCollectionRoot.Count, aa.OwnedCollectionRoot.Count);
                    for (var i = 0; i < ee.OwnedCollectionRoot.Count; i++)
                    {
                        AssertOwnedRoot(ee.OwnedCollectionRoot[i], aa.OwnedCollectionRoot[i]);
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
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    private static void AssertOwnedRoot(JsonOwnedRoot expected, JsonOwnedRoot actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Number, actual.Number);

        AssertOwnedBranch(expected.OwnedReferenceBranch, actual.OwnedReferenceBranch);
        Assert.Equal(expected.OwnedCollectionBranch.Count, actual.OwnedCollectionBranch.Count);
        for (var i = 0; i < expected.OwnedCollectionBranch.Count; i++)
        {
            AssertOwnedBranch(expected.OwnedCollectionBranch[i], actual.OwnedCollectionBranch[i]);
        }
    }

    private static void AssertOwnedBranch(JsonOwnedBranch expected, JsonOwnedBranch actual)
    {
        Assert.Equal(expected.Date, actual.Date);
        Assert.Equal(expected.Fraction, actual.Fraction);
        Assert.Equal(expected.Enum, actual.Enum);

        AssertOwnedLeaf(expected.OwnedReferenceLeaf, actual.OwnedReferenceLeaf);
        Assert.Equal(expected.OwnedCollectionLeaf.Count, actual.OwnedCollectionLeaf.Count);
        for (var i = 0; i < expected.OwnedCollectionLeaf.Count; i++)
        {
            AssertOwnedLeaf(expected.OwnedCollectionLeaf[i], actual.OwnedCollectionLeaf[i]);
        }
    }

    private static void AssertOwnedLeaf(JsonOwnedLeaf expected, JsonOwnedLeaf actual)
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
        Assert.Equal(expected.TestUnsignedInt16, actual.TestUnsignedInt16);
        Assert.Equal(expected.TestUnsignedInt32, actual.TestUnsignedInt32);
        Assert.Equal(expected.TestUnsignedInt64, actual.TestUnsignedInt64);
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

    protected override void Seed(JsonQueryContext context)
        => JsonQueryContext.Seed(context);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<JsonEntityBasic>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntityBasicForReference>().Property(x => x.Id).ValueGeneratedNever();
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
                b.Property(x => x.TestDecimal).HasPrecision(18, 3);
            });
        modelBuilder.Entity<JsonEntityAllTypes>().OwnsMany(
            x => x.Collection, b =>
            {
                b.ToJson();
                b.Property(x => x.TestDecimal).HasPrecision(18, 3);
            });
    }
}
