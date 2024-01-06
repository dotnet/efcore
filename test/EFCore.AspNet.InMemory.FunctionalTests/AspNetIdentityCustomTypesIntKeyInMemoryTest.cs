// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class AspNetIdentityCustomTypesIntKeyInMemoryTest(AspNetIdentityCustomTypesIntKeyInMemoryTest.AspNetIdentityCustomTypesIntKeyInMemoryFixture fixture)
    : AspNetIdentityCustomTypesIntKeyTestBase<
        AspNetIdentityCustomTypesIntKeyInMemoryTest.AspNetIdentityCustomTypesIntKeyInMemoryFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected override async Task ExecuteWithStrategyInTransactionAsync(
        Func<CustomTypesIdentityContextInt, Task> testOperation,
        Func<CustomTypesIdentityContextInt, Task> nestedTestOperation1 = null,
        Func<CustomTypesIdentityContextInt, Task> nestedTestOperation2 = null,
        Func<CustomTypesIdentityContextInt, Task> nestedTestOperation3 = null)
    {
        await base.ExecuteWithStrategyInTransactionAsync(
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
        await Fixture.ReseedAsync();
    }

    public class AspNetIdentityCustomTypesIntKeyInMemoryFixture : AspNetIdentityFixtureBase
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(e => e.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override string StoreName
            => "AspNetCustomTypesIntKeyIdentity";
    }
}
