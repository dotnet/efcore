#if !(NET451 && DRIVER_NOT_SIGNED)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests
{
    [MongoDbInstalledTestConditionAttribute]
    public class MongoDbContextTests : IClassFixture<MongoDbFixture>, IDisposable
    {
        private TestMongoDbContext _testMongoDbContext;

        public MongoDbContextTests(MongoDbFixture mongoDbFixture)
        {
            _testMongoDbContext = mongoDbFixture.TestMongoDbContext;
            _testMongoDbContext.Database.EnsureCreated();
        }

        [ConditionalFact]
        public void Can_query_from_mongodb()
        {
            Assert.Empty(_testMongoDbContext.SimpleRecords.ToList());
            Assert.Empty(_testMongoDbContext.ComplexRecords.ToList());
            Assert.Empty(_testMongoDbContext.RootTypes.ToList());
        }

        [ConditionalFact]
        public void Can_write_simple_record()
        {
            var simpleRecord = new SimpleRecord();
            _testMongoDbContext.Add(simpleRecord);
            _testMongoDbContext.SaveChanges(acceptAllChangesOnSuccess: true);
            Assert.Equal(simpleRecord, _testMongoDbContext.SimpleRecords.Single());
        }

        [ConditionalFact]
        public void Can_write_complex_record()
        {
            var complexRecord = new ComplexRecord();
            _testMongoDbContext.Add(complexRecord);
            _testMongoDbContext.SaveChanges(acceptAllChangesOnSuccess: true);
            Assert.Equal(complexRecord, _testMongoDbContext.ComplexRecords.Single());
        }

        [ConditionalFact]
        public void Can_write_polymorphic_records()
        {
            IList<RootType> insertedEntities = new RootType[]
                {
                    new DerivedType1(),
                    new SubDerivedType1(),
                    new SubDerivedType2()
                }
                .OrderBy(rootType => rootType.StringProperty)
                .ToList();
            _testMongoDbContext.AddRange(insertedEntities);
            _testMongoDbContext.SaveChanges(acceptAllChangesOnSuccess: true);
            IList<RootType> queriedEntities = _testMongoDbContext.RootTypes
                .OrderBy(rootType => rootType.StringProperty)
                .ToList();
            Assert.Equal(insertedEntities.Count, queriedEntities.Count);
            for (var i = 0; i < insertedEntities.Count; i++)
            {
                Assert.Equal(insertedEntities[i], queriedEntities[i]);
            }
        }

        [ConditionalFact]
        public void Can_query_polymorphic_sub_types()
        {
            IList<RootType> insertedEntities = new RootType[]
                {
                    new DerivedType1(),
                    new SubDerivedType1(),
                    new SubDerivedType2()
                }
                .OrderBy(rootType => rootType.StringProperty)
                .ToList();
            _testMongoDbContext.AddRange(insertedEntities);
            _testMongoDbContext.SaveChanges(acceptAllChangesOnSuccess: true);
            Assert.Equal(
                insertedEntities.OfType<DerivedType1>().Single(),
                _testMongoDbContext.RootTypes.OfType<DerivedType1>().Single());
            Assert.Equal(
                insertedEntities.OfType<SubDerivedType1>().Single(),
                _testMongoDbContext.RootTypes.OfType<SubDerivedType1>().Single());
            Assert.Equal(
                insertedEntities.OfType<SubDerivedType2>().Single(),
                _testMongoDbContext.RootTypes.OfType<SubDerivedType2>().Single());
            IList<SubRootType1> insertedSubRootTypes = insertedEntities
                .OfType<SubRootType1>()
                .OrderBy(rootType => rootType.StringProperty)
                .ToList();
            IList<SubRootType1> queriedSubRootTypes = _testMongoDbContext.RootTypes
                .OfType<SubRootType1>()
                .OrderBy(rootType => rootType.StringProperty)
                .ToList();
            Assert.Equal(insertedSubRootTypes.Count, queriedSubRootTypes.Count);
            for (var i = 0; i < insertedSubRootTypes.Count; i++)
            {
                Assert.Equal(insertedSubRootTypes[i], queriedSubRootTypes[i]);
            }
        }

        public void Dispose()
        {
            if (_testMongoDbContext != null)
            {
                _testMongoDbContext.Database.EnsureDeleted();
                _testMongoDbContext.Dispose();
                _testMongoDbContext = null;
            }
        }
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)