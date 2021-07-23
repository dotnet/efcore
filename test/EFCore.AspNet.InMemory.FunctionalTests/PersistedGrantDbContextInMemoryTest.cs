// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class PersistedGrantDbContextInMemoryTest
        : PersistedGrantDbContextTestBase<PersistedGrantDbContextInMemoryTest.PersistedGrantDbContextInMemoryFixture>
    {
        public PersistedGrantDbContextInMemoryTest(PersistedGrantDbContextInMemoryFixture fixture)
            : base(fixture)
        {
        }

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
}
