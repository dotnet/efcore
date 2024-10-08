// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.DatepartModel;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class DatepartQueryFixtureBase : SharedStoreFixtureBase<ExpeditionContext>, IQueryFixtureBase
{
    protected override string StoreName => "DatepartQueryTest";

    public Func<DbContext> GetContextCreator()
        => CreateContext;

    public virtual ISetSource GetExpectedData()
        => ExpeditionData.Instance;

    public virtual Dictionary<(Type, string), Func<object, object>> GetShadowPropertyMappings()
        => [];

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        {
            typeof(Expedition), e => ((Expedition)e)?.Id!
        }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(Expedition), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Expedition)e!;
                    var aa = (Expedition)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Destination, aa.Destination);
                    Assert.Equal(ee.StartDate, aa.StartDate);
                    Assert.Equal(ee.EndDate, aa.EndDate);
                    Assert.Equal(ee.Duration, aa.Duration);
                    Assert.Equal(ee.StartTime, aa.StartTime);
                }
            }
        }
    }.ToDictionary(e => e.Key, e => (object)e.Value);


    public override ExpeditionContext CreateContext()
    {
        var context = base.CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return context;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<Expedition>(e =>
        {
            e.HasIndex(e => e.StartDate);
            e.Property(e => e.Id).ValueGeneratedNever();
        });
    }

    protected override Task SeedAsync(ExpeditionContext context)
        => ExpeditionContext.SeedAsync(context);
}
