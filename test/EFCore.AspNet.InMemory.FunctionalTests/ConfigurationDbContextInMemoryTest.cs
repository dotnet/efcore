// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IdentityServer4.EntityFramework.DbContexts;

namespace Microsoft.EntityFrameworkCore;

public class ConfigurationDbContextInMemoryTest(ConfigurationDbContextInMemoryTest.ConfigurationDbContextInMemoryFixture fixture)
    : ConfigurationDbContextTestBase<ConfigurationDbContextInMemoryTest.ConfigurationDbContextInMemoryFixture>(fixture)
{
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected override async Task ExecuteWithStrategyInTransactionAsync(
        Func<ConfigurationDbContext, Task> testOperation,
        Func<ConfigurationDbContext, Task> nestedTestOperation1 = null,
        Func<ConfigurationDbContext, Task> nestedTestOperation2 = null,
        Func<ConfigurationDbContext, Task> nestedTestOperation3 = null)
    {
        await base.ExecuteWithStrategyInTransactionAsync(
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
        await Fixture.ReseedAsync();
    }

    public class ConfigurationDbContextInMemoryFixture : ConfigurationDbContextFixtureBase
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(e => e.Ignore(InMemoryEventId.TransactionIgnoredWarning));

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override string StoreName
            => "ConfigurationDbContext";
    }
}
