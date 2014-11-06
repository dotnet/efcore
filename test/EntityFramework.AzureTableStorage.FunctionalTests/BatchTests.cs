// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    // TODO: Enable when #965 is fixed
    [RunIfConfigured]
    internal class BatchTests : TestBase
    {
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

        public BatchTests(TestFixture fixture)
            : base(fixture)
        {
            Context.Configuration.Connection.AsAtsConnection().UseBatching(true);
        }
    }
}
