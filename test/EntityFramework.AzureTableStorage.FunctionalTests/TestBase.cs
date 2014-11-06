// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using Microsoft.Data.Entity.Update;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public abstract class TestBase : IClassFixture<TestFixture>, IDisposable
    {
        [Fact]
        public void It_adds_entity()
        {
            Context.Set<Purchase>().Add(new Purchase
                {
                    PartitionKey = TestPartition,
                    RowKey = "It_adds_entity_test",
                    Name = "Anchorage",
                    GlobalGuid = new Guid(),
                    Cost = 32145.2342,
                    Count = 324234959,
                    Purchased = DateTime.Parse("Tue, 13 May 2014 01:08:13 GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                    Awesomeness = true,
                });
            var changes = Context.SaveChanges();
            Assert.Equal(1, changes);
        }

        //Emulator accepts out of range dates, but production servers do not
        [Fact]
        public void It_handles_out_of_range_dates()
        {
            var lowDate = new Purchase
                {
                    PartitionKey = TestPartition,
                    RowKey = "DateOutOfRange",
                    Purchased = DateTime.Parse("Dec 31, 1600 23:59:00 GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                };
            Context.Set<Purchase>().Add(lowDate);
            Assert.Throws<DbUpdateException>(() => Context.SaveChanges());
        }

        [Fact]
        public void It_materializes_entity()
        {
            var expected = TestFixture.SampleData(TestPartition).First(s => s.PartitionKey == TestPartition && s.RowKey == "Sample_entity");
            var actual = Context.Set<Purchase>().First(s => s.PartitionKey == TestPartition && s.RowKey == "Sample_entity");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void It_deletes_entity()
        {
            var tableRow = Context.Set<Purchase>().First(s => s.PartitionKey == TestPartition && s.RowKey == "It_deletes_entity_test");
            Context.Delete(tableRow);
            var changes = Context.SaveChanges();
            Assert.Equal(1, changes);

            var afterDelete = Context.Set<Purchase>().FirstOrDefault(s => s.PartitionKey == TestPartition && s.RowKey == "It_deletes_entity_test");
            Assert.Null(afterDelete);
        }

        [Fact]
        public void It_throws_concurrency_errors()
        {
            var entity = Context.Set<Purchase>().First(s => s.PartitionKey == TestPartition && s.RowKey == "Concurrency_entity");
            var originalEtag = entity.ETag;
            entity.Name = "Updated at " + TestPartition;
            Context.SaveChanges();

            entity.ETag = originalEtag;
            Assert.Throws<DbUpdateConcurrencyException>(() => Context.SaveChanges());
        }

        protected readonly DbContext Context;
        protected readonly AtsTestStore TestStore;
        protected readonly string TestPartition;

        protected TestBase(TestFixture fixture)
        {
            TestPartition = DateTime.UtcNow.ToBinary().ToString();
            TestStore = fixture.CreateTestStore(TestPartition);
            Context = fixture.CreateContext(TestStore);
        }

        public void Dispose()
        {
            Context.Dispose();
            TestStore.Dispose();
        }
    }
}
