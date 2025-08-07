// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IdentityServer4.EntityFramework.DbContexts;

namespace Microsoft.EntityFrameworkCore;

public class PersistedGrantDbContextInMemoryTest(PersistedGrantDbContextInMemoryTest.PersistedGrantDbContextInMemoryFixture fixture)
    : PersistedGrantDbContextTestBase<PersistedGrantDbContextInMemoryTest.PersistedGrantDbContextInMemoryFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected override async Task ExecuteWithStrategyInTransactionAsync(
        Func<PersistedGrantDbContext, Task> testOperation,
        Func<PersistedGrantDbContext, Task> nestedTestOperation1 = null,
        Func<PersistedGrantDbContext, Task> nestedTestOperation2 = null,
        Func<PersistedGrantDbContext, Task> nestedTestOperation3 = null)
    {
        await base.ExecuteWithStrategyInTransactionAsync(
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
        await Fixture.ReseedAsync();
    }

    public class PersistedGrantDbContextInMemoryFixture : PersistedGrantDbContextFixtureBase
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(e => e.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override string StoreName
            => "PersistedGrantDbContext";
    }
}
