// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore;

public class AspNetIdentityIntKeyInMemoryTest(AspNetIdentityIntKeyInMemoryTest.AspNetIdentityIntKeyInMemoryFixture fixture)
    : AspNetIdentityIntKeyTestBase<AspNetIdentityIntKeyInMemoryTest.AspNetIdentityIntKeyInMemoryFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected override async Task ExecuteWithStrategyInTransactionAsync(
        Func<IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>, Task> testOperation,
        Func<IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>, Task> nestedTestOperation1 = null,
        Func<IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>, Task> nestedTestOperation2 = null,
        Func<IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>, Task> nestedTestOperation3 = null)
    {
        await base.ExecuteWithStrategyInTransactionAsync(
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
        await Fixture.ReseedAsync();
    }

    public class AspNetIdentityIntKeyInMemoryFixture : AspNetIdentityFixtureBase
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(e => e.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override string StoreName
            => "AspNetIntKeyIdentity";
    }
}
