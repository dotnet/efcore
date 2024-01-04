// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore;

public class AspNetIdentityDefaultInMemoryTest(AspNetIdentityDefaultInMemoryTest.AspNetDefaultIdentityInMemoryFixture fixture)
    : AspNetIdentityDefaultTestBase<AspNetIdentityDefaultInMemoryTest.AspNetDefaultIdentityInMemoryFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected override async Task ExecuteWithStrategyInTransactionAsync(
        Func<IdentityDbContext, Task> testOperation,
        Func<IdentityDbContext, Task> nestedTestOperation1 = null,
        Func<IdentityDbContext, Task> nestedTestOperation2 = null,
        Func<IdentityDbContext, Task> nestedTestOperation3 = null)
    {
        await base.ExecuteWithStrategyInTransactionAsync(
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
        await Fixture.ReseedAsync();
    }

    public class AspNetDefaultIdentityInMemoryFixture : AspNetIdentityFixtureBase
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(e => e.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override string StoreName
            => "AspNetDefaultIdentity";
    }
}
