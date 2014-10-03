// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class BatchTests : TestBase, IClassFixture<TestFixture>, IDisposable
    {
        public BatchTests(TestFixture fixture)
        {
            TestPartition = "batchunitest" + DateTime.UtcNow.ToBinary();
            Context = fixture.CreateContext(TestPartition);
            Context.Database.EnsureCreated();
            Context.Set<Purchase>().AddRange(TestFixture.SampleData(TestPartition));
            Context.SaveChanges();
            Context.Configuration.Connection.AsAtsConnection().UseBatching(true);
        }

        [Theory]
        [InlineData(99)]
        [InlineData(100)]
        [InlineData(101)]
        [InlineData(1000)]
        [InlineData(10001)]
        public void It_creates_many_items(int count)
        {
            var pk = TestPartition + count;
            for (var i = 0; i < count; i++)
            {
                var item = new Purchase { Count = i, PartitionKey = pk, RowKey = i.ToString() };
                Context.Set<Purchase>().Add(item);
            }
            var changes = Context.SaveChanges();
            Assert.Equal(count, changes);
        }

        [Fact]
        public void It_separates_by_partition_key()
        {
            var partition1 = new Purchase { PartitionKey = TestPartition + "A", RowKey = "0" };
            var partition2 = new Purchase { PartitionKey = TestPartition + "B", RowKey = "0" };
            Context.Set<Purchase>().AddRange(new[] { partition1, partition2 });
            var changes = Context.SaveChanges();
            Assert.Equal(2, changes);
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
        }
    }
}
