// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Configuration;
using Microsoft.Data.Entity.AzureTableStorage.FunctionalTests.Helpers;
using Microsoft.Data.Entity.Metadata;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class BatchTests : IClassFixture<CloudTableFixture>
    {
        private readonly TestContext _context;
        private readonly string _testParition;

        public BatchTests(CloudTableFixture fixture)
        {
            var connectionString = ConfigurationManager.AppSettings["TestConnectionString"];
            _context = new TestContext();
            fixture.GetOrCreateTable("AzureStorageBatchEmulatorEntity", connectionString);
            //fixture.DeleteOnDispose = true;
            _testParition = "BatchTests-" + DateTime.UtcNow.ToString("O");
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
                var item = new AzureStorageBatchEmulatorEntity { Count = i, PartitionKey = _testParition, RowKey = (count + i).ToString() };
                _context.Items.Add(item);
            }
            var changes = _context.SaveChanges();
            Assert.Equal(count, changes);
        }

        [Fact]
        public void It_separates_by_partition_key()
        {
            var partition1 = new AzureStorageBatchEmulatorEntity { PartitionKey = _testParition + "A", RowKey = "0" };
            var partition2 = new AzureStorageBatchEmulatorEntity { PartitionKey = _testParition + "B", RowKey = "0" };
            _context.Items.AddRange(new[] { partition1, partition2 });
            var changes = _context.SaveChanges();
            Assert.Equal(2, changes);
        }

        //TODO see https://github.com/aspnet/EntityFramework/issues/251
        // [Fact]
        public void It_handles_many_changes()
        {
            var item = new AzureStorageBatchEmulatorEntity { PartitionKey = _testParition, RowKey = "z" };
            _context.Items.Add(item);
            item.Count = 12435;
            _context.Items.Remove(item);
            var changes = _context.SaveChanges();
            Assert.Equal(2, changes);
        }

        private class TestContext : DbContext
        {
            public DbSet<AzureStorageBatchEmulatorEntity> Items { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<AzureStorageBatchEmulatorEntity>().Key(s => s.Key);
            }

            protected override void OnConfiguring(DbContextOptions builder)
            {
                builder.UseAzureTableStorge(ConfigurationManager.AppSettings["TestConnectionString"], true);
            }
        }

        private class AzureStorageBatchEmulatorEntity : TableEntity
        {
            public string Key
            {
                get { return PartitionKey + RowKey; }
            }

            public int Count { get; set; }
        }
    }
}
