// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.AzureTableStorage.FunctionalTests.Helpers;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Core;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class EndToEndTests : IClassFixture<CloudTableFixture>
    {
        private readonly PurchaseContext _context;
        private readonly Purchase _sampleEntity;
        private readonly List<Purchase> _sampleSet = new List<Purchase>();
        private readonly string _testPartition;

        #region setup

        public EndToEndTests(CloudTableFixture fixture)
        {
            var connectionString = ConfigurationManager.AppSettings["TestConnectionString"];
            _testPartition = "unittests-" + DateTime.UtcNow.ToString("R");
            var tableName = "Table" + DateTime.UtcNow.ToBinary();
            _context = new PurchaseContext(tableName);
            var table = fixture.GetOrCreateTable(tableName, connectionString);
            var deleteTest = new Purchase
                {
                    PartitionKey = _testPartition,
                    RowKey = "It_deletes_entity_test",
                    Purchased = DateTime.Now,
                };

            _sampleSet.Add(deleteTest);

            for (var i = 0; i < 20; i++)
            {
                var findTest = new Purchase
                    {
                        PartitionKey = _testPartition,
                        RowKey = "It_finds_entity_test_" + i,
                        Purchased = DateTime.Now,
                        Count = i,
                    };
                _sampleSet.Add(findTest);
            }

            _sampleEntity = new Purchase
                {
                    PartitionKey = _testPartition,
                    RowKey = "Sample_entity",
                    Name = "Sample",
                    GlobalGuid = new Guid(),
                    Cost = -234.543,
                    Count = 359,
                    Purchased = DateTime.Parse("Tue, 1 Jan 2013 22:11:20 GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                    Awesomeness = true,
                };
            _sampleSet.Add(_sampleEntity);

            var setupBatch = new TableBatchOperation();
            _sampleSet.ForEach(s => setupBatch.Add(TableOperation.InsertOrReplace(new PocoTableEntityAdapter<Purchase>(s))));
            var setup = table.ExecuteBatch(setupBatch);
            if (setup.Any(s => s.HttpStatusCode >= 400))
            {
                throw new Exception("Could not setup for test correctly");
            }
        }

        #endregion

        [Fact]
        public void It_adds_entity()
        {
            _context.Purchases.Add(new Purchase
                {
                    PartitionKey = _testPartition,
                    RowKey = "It_adds_entity_test",
                    Name = "Anchorage",
                    GlobalGuid = new Guid(),
                    Cost = 32145.2342,
                    Count = 324234959,
                    Purchased = DateTime.Parse("Tue, 13 May 2014 01:08:13 GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                    Awesomeness = true,
                });
            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);
        }

        [Fact]
        public void It_finds_entities()
        {
            var rows = _context.Purchases.Where(s => s.PartitionKey == _testPartition && s.RowKey.StartsWith("It_finds_entity_test_"));
            Assert.Equal(20, rows.Count());
        }

        //Emulator accepts out of range dates, but production servers do not
        [Fact]
        public void It_handles_out_of_range_dates()
        {
            var lowDate = new Purchase
                {
                    PartitionKey = _testPartition,
                    RowKey = "DateOutOfRange",
                    Purchased = DateTime.Parse("Dec 31, 1600 23:59:00 GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                };
            _context.Purchases.Add(lowDate);
            Assert.Throws<AggregateException>(() => { _context.SaveChanges(); });
        }

        [Fact]
        public void It_materializes_entity()
        {
            var actual = _context.Purchases.First(s => s.PartitionKey == _testPartition && s.RowKey == "Sample_entity");
            Assert.True(_sampleEntity.Equals(actual));
        }

        [Fact]
        public void It_deletes_entity()
        {
            var tableRow = _context.Purchases.First(s => s.PartitionKey == _testPartition && s.RowKey == "It_deletes_entity_test");
            _context.Delete(tableRow);
            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);
        }
    }
}
