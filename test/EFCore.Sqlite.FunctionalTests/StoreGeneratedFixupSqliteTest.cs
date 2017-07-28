// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class StoreGeneratedFixupSqliteTest : StoreGeneratedFixupTestBase<StoreGeneratedFixupSqliteTest.StoreGeneratedFixupSqliteFixture>
    {
        public StoreGeneratedFixupSqliteTest(StoreGeneratedFixupSqliteFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void Temp_values_can_be_made_permanent()
        {
            using (var context = CreateContext())
            {
                var entry = context.Add(new TestTemp());

                Assert.True(entry.Property(e => e.Id).IsTemporary);
                Assert.False(entry.Property(e => e.NotId).IsTemporary);

                var tempValue = entry.Property(e => e.Id).CurrentValue;

                entry.Property(e => e.Id).IsTemporary = false;

                context.SaveChanges();

                Assert.False(entry.Property(e => e.Id).IsTemporary);
                Assert.Equal(tempValue, entry.Property(e => e.Id).CurrentValue);
            }
        }

        protected override bool EnforcesFKs => true;
        
        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class StoreGeneratedFixupSqliteFixture : StoreGeneratedFixupFixtureBase
        {
            protected override ITestStoreFactory<TestStore> TestStoreFactory => SqliteTestStoreFactory.Instance;
        }
    }
}
