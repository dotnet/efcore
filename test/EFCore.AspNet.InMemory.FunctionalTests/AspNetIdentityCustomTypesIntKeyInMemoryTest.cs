// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class AspNetIdentityCustomTypesIntKeyInMemoryTest
        : AspNetIdentityCustomTypesIntKeyTestBase<AspNetIdentityCustomTypesIntKeyInMemoryTest.AspNetIdentityCustomTypesIntKeyInMemoryFixture
        >
    {
        public AspNetIdentityCustomTypesIntKeyInMemoryTest(AspNetIdentityCustomTypesIntKeyInMemoryFixture fixture)
            : base(fixture)
        {
        }

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
}
