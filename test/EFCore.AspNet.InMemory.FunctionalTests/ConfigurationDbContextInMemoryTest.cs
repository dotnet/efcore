// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class ConfigurationDbContextInMemoryTest
        : ConfigurationDbContextTestBase<ConfigurationDbContextInMemoryTest.ConfigurationDbContextInMemoryFixture>
    {
        public ConfigurationDbContextInMemoryTest(ConfigurationDbContextInMemoryFixture fixture)
            : base(fixture)
        {
        }

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
}
