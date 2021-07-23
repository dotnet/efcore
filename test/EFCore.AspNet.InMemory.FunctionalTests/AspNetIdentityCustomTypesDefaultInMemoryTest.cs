// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class AspNetIdentityCustomTypesDefaultInMemoryTest
        : AspNetIdentityCustomTypesDefaultTestBase<
            AspNetIdentityCustomTypesDefaultInMemoryTest.AspNetIdentityCustomTypesDefaultInMemoryFixture>
    {
        public AspNetIdentityCustomTypesDefaultInMemoryTest(AspNetIdentityCustomTypesDefaultInMemoryFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        protected override async Task ExecuteWithStrategyInTransactionAsync(
            Func<CustomTypesIdentityContext, Task> testOperation,
            Func<CustomTypesIdentityContext, Task> nestedTestOperation1 = null,
            Func<CustomTypesIdentityContext, Task> nestedTestOperation2 = null,
            Func<CustomTypesIdentityContext, Task> nestedTestOperation3 = null)
        {
            await base.ExecuteWithStrategyInTransactionAsync(
                testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
            await Fixture.ReseedAsync();
        }

        public class AspNetIdentityCustomTypesDefaultInMemoryFixture : AspNetIdentityFixtureBase
        {
            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection).AddEntityFrameworkProxies();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder)
                    .UseLazyLoadingProxies()
                    .ConfigureWarnings(e => e.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;

            protected override string StoreName
                => "AspNetCustomTypesDefaultIdentity";
        }
    }
}
