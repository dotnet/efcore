// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class InheritanceBulkUpdatesInMemoryFixture : InheritanceBulkUpdatesFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
        // TODO: Fake transactions needed for real tests.
    }

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder)
            .ConfigureWarnings(e => e.Ignore(InMemoryEventId.TransactionIgnoredWarning));
}
