// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Configuration;
using Microsoft.Data.Entity.AzureTableStorage.FunctionalTests.Helpers;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class BatchTests : IClassFixture<CloudTableFixture>
    {
        private readonly PurchaseContext _context;
        private readonly string _testParition;

        public BatchTests(CloudTableFixture fixture)
        {
            var connectionString = ConfigurationManager.AppSettings["TestConnectionString"];
            var tableName = "BatchTestsTable" + DateTime.UtcNow.ToBinary();
            fixture.GetOrCreateTable(tableName, connectionString);
            _context = new PurchaseContext(tableName,true);
            _testParition = "BatchTests-" + DateTime.UtcNow.ToString("R");
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
                var item = new Purchase { Count = i, PartitionKey = _testParition, RowKey = (count + i).ToString() };
                _context.Purchases.Add(item);
            }
            var changes = _context.SaveChanges();
            Assert.Equal(count, changes);
        }

        [Fact]
        public void It_separates_by_partition_key()
        {
            var partition1 = new Purchase { PartitionKey = _testParition + "A", RowKey = "0" };
            var partition2 = new Purchase { PartitionKey = _testParition + "B", RowKey = "0" };
            _context.Purchases.AddRange(new[] { partition1, partition2 });
            var changes = _context.SaveChanges();
            Assert.Equal(2, changes);
        }
    }
}
