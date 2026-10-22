// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ExecuteMergeModel;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class ExecuteMergeFixtureBase : QueryFixtureBase<ExecuteMergeContext>
{
    private ExecuteMergeData? _expectedData;

    protected override string StoreName
        => "ExecuteMergeTest";

    protected override Task SeedAsync(ExecuteMergeContext context)
    {
        var data = new ExecuteMergeData();
        context.AddRange(data.Blogs);
        context.AddRange(data.Visits);
        context.AddRange(data.Accounts);
        context.AddRange(data.Measurements);
        return context.SaveChangesAsync();
    }

    public override ISetSource GetExpectedData()
        => _expectedData ??= new ExecuteMergeData();

    public override IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
    {
        { typeof(Blog), e => ((Blog?)e)?.Id },
        { typeof(DailyVisit), e => (((DailyVisit?)e)?.UserId, ((DailyVisit?)e)?.Day) },
        { typeof(Account), e => ((Account?)e)?.Id }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public override IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object?, object?>>
    {
        {
            typeof(Blog), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (Blog)e!;
                    var aa = (Blog)a;
                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Note, aa.Note);
                }
            }
        },
        {
            typeof(DailyVisit), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (DailyVisit)e!;
                    var aa = (DailyVisit)a;
                    Assert.Equal(ee.UserId, aa.UserId);
                    Assert.Equal(ee.Day, aa.Day);
                    Assert.Equal(ee.Visits, aa.Visits);
                }
            }
        },
        {
            typeof(Account), (e, a) =>
            {
                Assert.Equal(e == null, a == null);
                if (a != null)
                {
                    var ee = (Account)e!;
                    var aa = (Account)a;
                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.ExternalId, aa.ExternalId);
                    Assert.Equal(ee.Balance, aa.Balance);
                }
            }
        }
    }.ToDictionary(e => e.Key, e => (object)e.Value);
}
