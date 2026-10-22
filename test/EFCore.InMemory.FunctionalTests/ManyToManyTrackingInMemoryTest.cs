// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

public class ManyToManyTrackingInMemoryTest(ManyToManyTrackingInMemoryTest.ManyToManyTrackingInMemoryFixture fixture)
    : ManyToManyTrackingTestBase<ManyToManyTrackingInMemoryTest.ManyToManyTrackingInMemoryFixture>(fixture)
{
    protected override async Task ExecuteWithStrategyInTransactionAsync(
        Func<ManyToManyContext, Task> testOperation,
        Func<ManyToManyContext, Task> nestedTestOperation1 = null,
        Func<ManyToManyContext, Task> nestedTestOperation2 = null,
        Func<ManyToManyContext, Task> nestedTestOperation3 = null)
    {
        await base.ExecuteWithStrategyInTransactionAsync(
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

        await Fixture.ReseedAsync();
    }

    protected override bool SupportsDatabaseDefaults
        => false;

    public class ManyToManyTrackingInMemoryFixture : ManyToManyTrackingFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    }
}
