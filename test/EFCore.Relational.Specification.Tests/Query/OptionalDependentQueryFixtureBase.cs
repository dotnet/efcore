// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.OptionalDependent;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class OptionalDependentQueryFixtureBase : SharedStoreFixtureBase<OptionalDependentContext>,
    IQueryFixtureBase,
    ITestSqlLoggerFactory
{
    private OptionalDependentData _expectedData;

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public virtual ISetSource GetExpectedData()
        => _expectedData ??= new OptionalDependentData();

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(OptionalDependentEntityAllOptional), e => ((OptionalDependentEntityAllOptional)e)?.Id },
        { typeof(OptionalDependentEntitySomeRequired), e => ((OptionalDependentEntitySomeRequired)e)?.Id },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(OptionalDependentEntityAllOptional), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (OptionalDependentEntityAllOptional)e;
                    var aa = (OptionalDependentEntityAllOptional)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);

                    if (ee.Json is not null || aa.Json is not null)
                    {
                        AssertOptionalDependentJsonAllOptional(ee.Json, aa.Json);
                    }
                }
            }
        },
        {
            typeof(OptionalDependentEntitySomeRequired), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (OptionalDependentEntitySomeRequired)e;
                    var aa = (OptionalDependentEntitySomeRequired)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);

                    if (ee.Json is not null || aa.Json is not null)
                    {
                        AssertOptionalDependentJsonSomeRequired(ee.Json, aa.Json);
                    }
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public static void AssertOptionalDependentJsonAllOptional(
        OptionalDependentJsonAllOptional expected,
        OptionalDependentJsonAllOptional actual)
    {
        Assert.Equal(expected.OpProp1, actual.OpProp1);
        Assert.Equal(expected.OpProp2, actual.OpProp2);

        if (expected.OpNav1 is not null || actual.OpNav1 is not null)
        {
            AssertOptionalDependentNestedJsonAllOptional(expected.OpNav1, actual.OpNav1);
        }

        if (expected.OpNav2 is not null || actual.OpNav2 is not null)
        {
            AssertOptionalDependentNestedJsonSomeRequired(expected.OpNav2, actual.OpNav2);
        }
    }

    public static void AssertOptionalDependentJsonSomeRequired(
        OptionalDependentJsonSomeRequired expected,
        OptionalDependentJsonSomeRequired actual)
    {
        Assert.Equal(expected.OpProp1, actual.OpProp1);
        Assert.Equal(expected.OpProp2, actual.OpProp2);
        Assert.Equal(expected.ReqProp, actual.ReqProp);


        if (expected.OpNav1 is not null || actual.OpNav1 is not null)
        {
            AssertOptionalDependentNestedJsonAllOptional(expected.OpNav1, actual.OpNav1);
        }

        if (expected.OpNav2 is not null || actual.OpNav2 is not null)
        {
            AssertOptionalDependentNestedJsonSomeRequired(expected.OpNav2, actual.OpNav2);
        }

        AssertOptionalDependentNestedJsonAllOptional(expected.ReqNav1, actual.ReqNav1);
        AssertOptionalDependentNestedJsonSomeRequired(expected.ReqNav2, actual.ReqNav2);
    }

    public static void AssertOptionalDependentNestedJsonAllOptional(
        OptionalDependentNestedJsonAllOptional expected,
        OptionalDependentNestedJsonAllOptional actual)
    {
        Assert.Equal(expected.OpNested1, actual.OpNested1);
        Assert.Equal(expected.OpNested2, actual.OpNested2);
    }

    public static void AssertOptionalDependentNestedJsonSomeRequired(
        OptionalDependentNestedJsonSomeRequired expected,
        OptionalDependentNestedJsonSomeRequired actual)
    {
        Assert.Equal(expected.OpNested1, actual.OpNested1);
        Assert.Equal(expected.OpNested2, actual.OpNested2);
        Assert.Equal(expected.ReqNested1, actual.ReqNested1);
        Assert.Equal(expected.ReqNested2, actual.ReqNested2);
    }

    protected override string StoreName { get; } = "OptionalDependentQueryTest";

    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override Task SeedAsync(OptionalDependentContext context)
        => OptionalDependentContext.SeedAsync(context);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<OptionalDependentEntityAllOptional>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<OptionalDependentEntitySomeRequired>().Property(x => x.Id).ValueGeneratedNever();

        modelBuilder.Entity<OptionalDependentEntityAllOptional>().OwnsOne(
            x => x.Json, b =>
            {
                b.ToJson();

                b.OwnsOne(x => x.OpNav1);
                b.OwnsOne(x => x.OpNav2);
            });

        modelBuilder.Entity<OptionalDependentEntitySomeRequired>().OwnsOne(
            x => x.Json, b =>
            {
                b.ToJson();

                b.OwnsOne(x => x.OpNav1);
                b.OwnsOne(x => x.OpNav2);

                b.OwnsOne(x => x.ReqNav1);
                b.Navigation(x => x.ReqNav1).IsRequired();
                b.OwnsOne(x => x.ReqNav2);
                b.Navigation(x => x.ReqNav2).IsRequired();
            });
    }
}
