// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class BatchTests : IClassFixture<EndToEndFixture>
    {
        private readonly string _testPartition;
        private DbContext _context;

        public BatchTests(EndToEndFixture fixture)
        {
            _testPartition = "unittests-" + DateTime.UtcNow.ToBinary();
            fixture.UseTableNamePrefixAndLock("Batch");
            fixture.SetContextOptions(useBatching: true);
            _context = fixture.CreateContext();
        }

        [Theory]
        [InlineData(99)]
        [InlineData(100)]
        [InlineData(101)]
        [InlineData(1000)]
        [InlineData(10000)]
        public void It_creates_many_items(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var item = new Purchase { Count = i, PartitionKey = _testPartition, RowKey = i.ToString() };
                _context.Set<Purchase>().Add(item);
            }
            var changes = _context.SaveChanges();
            Assert.Equal(count, changes);
        }

        [Fact]
        public void It_separates_by_partition_key()
        {
            var partition1 = new Purchase { PartitionKey = _testPartition + "A", RowKey = "0" };
            var partition2 = new Purchase { PartitionKey = _testPartition + "B", RowKey = "0" };
            _context.Set<Purchase>().AddRange(new[] { partition1, partition2 });
            var changes = _context.SaveChanges();
            Assert.Equal(2, changes);
        }
    }
}
