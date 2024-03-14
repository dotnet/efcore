// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NullSemanticsQueryFixtureBase : SharedStoreFixtureBase<NullSemanticsContext>, IQueryFixtureBase, ITestSqlLoggerFactory
{
    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public virtual ISetSource GetExpectedData()
        => NullSemanticsData.Instance;

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(NullSemanticsEntity1), e => ((NullSemanticsEntity1)e)?.Id },
        { typeof(NullSemanticsEntity2), e => ((NullSemanticsEntity2)e)?.Id }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(NullSemanticsEntity1), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (NullSemanticsEntity1)e;
                    var aa = (NullSemanticsEntity1)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.BoolA, aa.BoolA);
                    Assert.Equal(ee.BoolB, aa.BoolB);
                    Assert.Equal(ee.BoolC, aa.BoolC);
                    Assert.Equal(ee.IntA, aa.IntA);
                    Assert.Equal(ee.IntB, aa.IntB);
                    Assert.Equal(ee.IntC, aa.IntC);
                    Assert.Equal(ee.StringA, aa.StringA);
                    Assert.Equal(ee.StringB, aa.StringB);
                    Assert.Equal(ee.StringC, aa.StringC);
                    Assert.Equal(ee.NullableBoolA, aa.NullableBoolA);
                    Assert.Equal(ee.NullableBoolB, aa.NullableBoolB);
                    Assert.Equal(ee.NullableBoolC, aa.NullableBoolC);
                    Assert.Equal(ee.NullableIntA, aa.NullableIntA);
                    Assert.Equal(ee.NullableIntB, aa.NullableIntB);
                    Assert.Equal(ee.NullableIntC, aa.NullableIntC);
                    Assert.Equal(ee.NullableStringA, aa.NullableStringA);
                    Assert.Equal(ee.NullableStringB, aa.NullableStringB);
                    Assert.Equal(ee.NullableStringC, aa.NullableStringC);
                }
            }
        },
        {
            typeof(NullSemanticsEntity2), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (NullSemanticsEntity2)e;
                    var aa = (NullSemanticsEntity2)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.BoolA, aa.BoolA);
                    Assert.Equal(ee.BoolB, aa.BoolB);
                    Assert.Equal(ee.BoolC, aa.BoolC);
                    Assert.Equal(ee.IntA, aa.IntA);
                    Assert.Equal(ee.IntB, aa.IntB);
                    Assert.Equal(ee.IntC, aa.IntC);
                    Assert.Equal(ee.StringA, aa.StringA);
                    Assert.Equal(ee.StringB, aa.StringB);
                    Assert.Equal(ee.StringC, aa.StringC);
                    Assert.Equal(ee.NullableBoolA, aa.NullableBoolA);
                    Assert.Equal(ee.NullableBoolB, aa.NullableBoolB);
                    Assert.Equal(ee.NullableBoolC, aa.NullableBoolC);
                    Assert.Equal(ee.NullableIntA, aa.NullableIntA);
                    Assert.Equal(ee.NullableIntB, aa.NullableIntB);
                    Assert.Equal(ee.NullableIntC, aa.NullableIntC);
                    Assert.Equal(ee.NullableStringA, aa.NullableStringA);
                    Assert.Equal(ee.NullableStringB, aa.NullableStringB);
                    Assert.Equal(ee.NullableStringC, aa.NullableStringC);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    protected override string StoreName
        => "NullSemanticsQueryTest";

    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override NullSemanticsContext CreateContext()
    {
        var context = base.CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return context;
    }

    protected override Task SeedAsync(NullSemanticsContext context)
        => NullSemanticsContext.SeedAsync(context);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringA).IsRequired();
        modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringB).IsRequired();
        modelBuilder.Entity<NullSemanticsEntity1>().Property(e => e.StringC).IsRequired();

        modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringA).IsRequired();
        modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringB).IsRequired();
        modelBuilder.Entity<NullSemanticsEntity2>().Property(e => e.StringC).IsRequired();
    }
}
