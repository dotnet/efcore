// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.EntitySplitting;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class EntitySplittingQueryFixtureBase : SharedStoreFixtureBase<EntitySplittingContext>, IQueryFixtureBase
{
    protected EntitySplittingQueryFixtureBase()
    {
    }

    protected override string StoreName
        => "EntitySplittingQueryTest";

    public TestSqlLoggerFactory TestSqlLoggerFactory
       => (TestSqlLoggerFactory)ListLoggerFactory;

    public Func<DbContext> GetContextCreator() => () => CreateContext();

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(SplitEntityOne), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (SplitEntityOne)e;
                    var aa = (SplitEntityOne)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Value, aa.Value);
                    Assert.Equal(ee.SharedValue, aa.SharedValue);
                    Assert.Equal(ee.SplitValue, aa.SplitValue);
                }
            }
        }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } =
        new Dictionary<Type, Func<object, object>> { { typeof(SplitEntityOne), e => ((SplitEntityOne)e)?.Id }, }.ToDictionary(
            e => e.Key, e => (object)e.Value);

    public ISetSource GetExpectedData()
        => SplitEntityData.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<SplitEntityOne>(b =>
        {
            b.ToTable("SplitEntityOneMain", tb =>
            {
                tb.Property(e => e.SharedValue);
            });

            b.SplitToTable("SplitEntityOneOther", tb =>
            {
                tb.Property(e => e.SharedValue).HasColumnName("OtherSharedValue");
                tb.Property(e => e.SplitValue);
            });
        });

        base.OnModelCreating(modelBuilder, context);
    }

    protected override void Seed(EntitySplittingContext context)
        => SplitEntityData.Instance.Seed(context);
}
