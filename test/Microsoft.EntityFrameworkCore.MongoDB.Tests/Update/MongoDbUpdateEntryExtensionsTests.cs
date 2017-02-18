#if !(NET451 && DRIVER_NOT_SIGNED)
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using Microsoft.EntityFrameworkCore.Update;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.Update
{
    public class MongoDbUpdateEntryExtensionsTests
    {
        private InternalEntityEntry GetEntityEntry(EntityState entityState, SimpleRecord simpleRecord)
        {
            var mockStateManager = new Mock<IStateManager>();
            var mockValueGenerationManager = new Mock<IValueGenerationManager>();
            var mockInternalEntityEntryNotifier = new Mock<IInternalEntityEntryNotifier>();
            mockStateManager.SetupGet(stateManager => stateManager.ValueGeneration)
                .Returns(() => mockValueGenerationManager.Object);
            mockStateManager.SetupGet(stateManager => stateManager.Notify)
                .Returns(() => mockInternalEntityEntryNotifier.Object);

            var model = new Model(new CoreConventionSetBuilder().CreateConventionSet());
            EntityType entityType = model.AddEntityType(typeof(SimpleRecord));
            entityType.Builder
                .GetOrCreateProperties(typeof(SimpleRecord).GetTypeInfo().GetProperties(), ConfigurationSource.Convention);
            entityType.Builder
                .MongoDb(ConfigurationSource.Convention)
                .FromCollection(collectionName: "simpleRecords");

            var entityEntry = new InternalClrEntityEntry(mockStateManager.Object, entityType, simpleRecord);
            entityEntry.SetEntityState(entityState, acceptChanges: true);
            return entityEntry;
        }

        [Fact]
        public void Creates_insert_one_model_for_added_entity()
        {
            var simpleRecord = new SimpleRecord();
            var insertOneModel = GetEntityEntry(EntityState.Added, simpleRecord)
                .ToMongoDbWriteModel<SimpleRecord>() as InsertOneModel<SimpleRecord>;
            Assert.NotNull(insertOneModel);
        }

        [Fact]
        public void Creates_replace_one_model_for_modified_entity()
        {
            var simpleRecord = new SimpleRecord(ObjectId.GenerateNewId());
            var replaceOneModel = GetEntityEntry(EntityState.Modified, simpleRecord)
                .ToMongoDbWriteModel<SimpleRecord>() as ReplaceOneModel<SimpleRecord>;
            FilterDefinition<SimpleRecord> filter = Builders<SimpleRecord>.Filter.Eq(
                record => record.Id, simpleRecord.Id);
            Assert.NotNull(replaceOneModel);
            Assert.Equal(filter.ToBsonDocument(), replaceOneModel.Filter.ToBsonDocument());
        }

        [Fact]
        public void Creates_delete_one_model_for_deleted_entity()
        {
            var simpleRecord = new SimpleRecord(ObjectId.GenerateNewId());
            var deleteOneModel = GetEntityEntry(EntityState.Deleted, simpleRecord)
                .ToMongoDbWriteModel<SimpleRecord>() as DeleteOneModel<SimpleRecord>;
            FilterDefinition<SimpleRecord> filter = Builders<SimpleRecord>.Filter.Eq(
                record => record.Id, simpleRecord.Id);
            Assert.NotNull(deleteOneModel);
            Assert.Equal(filter.ToBsonDocument(), deleteOneModel.Filter.ToBsonDocument());
        }
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)