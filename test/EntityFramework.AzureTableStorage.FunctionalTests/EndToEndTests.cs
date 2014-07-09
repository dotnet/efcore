// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class EndToEndTests : IClassFixture<EndToEndFixture>, IDisposable
    {
        private readonly DbContext _context;
        private readonly string _testPartition;

        public EndToEndTests(EndToEndFixture fixture)
        {
            _testPartition = "endtoendtests" + DateTime.UtcNow.ToBinary();
            _context = fixture.CreateContext(_testPartition);
            _context.Database.EnsureCreated();
            _context.Set<Purchase>().AddRange(EndToEndFixture.SampleData(_testPartition));
            _context.SaveChanges();
        }

        [Fact]
        public void It_adds_entity()
        {
            _context.Set<Purchase>().Add(new Purchase
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
            _context.Set<Purchase>().Add(lowDate);
            Assert.Throws<AggregateException>(() => { _context.SaveChanges(); });
        }

        [Fact]
        public void It_materializes_entity()
        {
            var expected = EndToEndFixture.SampleData(_testPartition).First(s => s.PartitionKey == _testPartition && s.RowKey == "Sample_entity");
            var actual = _context.Set<Purchase>().First(s => s.PartitionKey == _testPartition && s.RowKey == "Sample_entity");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void It_deletes_entity()
        {
            var tableRow = _context.Set<Purchase>().First(s => s.PartitionKey == _testPartition && s.RowKey == "It_deletes_entity_test");
            _context.Delete(tableRow);
            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var afterDelete = _context.Set<Purchase>().FirstOrDefault(s => s.PartitionKey == _testPartition && s.RowKey == "It_deletes_entity_test");
            Assert.Null(afterDelete);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
