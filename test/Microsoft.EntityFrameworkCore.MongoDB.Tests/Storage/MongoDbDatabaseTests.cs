using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.Storage
{
    public class MongoDbDatabaseTests
    {
        [Fact]
        public void Save_changes_returns_requested_document_count()
        {
            var queryCompilationContextFactory = Mock.Of<IQueryCompilationContextFactory>();
            var mockStateManager = new Mock<IStateManager>();
            var mockMongoDbConnection = new Mock<IMongoDbConnection>();
            var mockMongoCollection = new Mock<IMongoCollection<SimpleRecord>>();
            var mockValueGenerationManager = new Mock<IValueGenerationManager>();
            var mockInternalEntityEntryNotifier = new Mock<IInternalEntityEntryNotifier>();
            mockStateManager.SetupGet(stateManager => stateManager.ValueGeneration)
                .Returns(() => mockValueGenerationManager.Object);
            mockStateManager.SetupGet(stateManager => stateManager.Notify)
                .Returns(() => mockInternalEntityEntryNotifier.Object);
            mockMongoDbConnection.Setup(mockedMongoDbConnection => mockedMongoDbConnection.GetCollection<SimpleRecord>())
                .Returns(() => mockMongoCollection.Object);
            mockMongoCollection.Setup(mongoCollection => mongoCollection.BulkWrite(
                    It.IsAny<IEnumerable<WriteModel<SimpleRecord>>>(),
                    It.IsAny<BulkWriteOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<WriteModel<SimpleRecord>> list, BulkWriteOptions options, CancellationToken token)
                    => new BulkWriteResult<SimpleRecord>.Acknowledged(
                        list.Count(),
                        matchedCount: 0,
                        deletedCount: list.OfType<DeleteOneModel<SimpleRecord>>().Count(),
                        insertedCount: list.OfType<InsertOneModel<SimpleRecord>>().Count(),
                        modifiedCount: list.OfType<ReplaceOneModel<SimpleRecord>>().Count(),
                        processedRequests: list,
                        upserts: new List<BulkWriteUpsert>()));
            var mongoDbDatabase = new MongoDbDatabase(queryCompilationContextFactory, mockMongoDbConnection.Object);

            var model = new Model(new CoreConventionSetBuilder().CreateConventionSet());
            EntityType entityType = model.AddEntityType(typeof(SimpleRecord));
            entityType.Builder
                .GetOrCreateProperties(typeof(SimpleRecord).GetTypeInfo().GetProperties(), ConfigurationSource.Convention);
            entityType.Builder
                .MongoDb(ConfigurationSource.Convention)
                .FromCollection(collectionName: "simpleRecords");

            IReadOnlyList<InternalEntityEntry> entityEntries = new[] { EntityState.Added, EntityState.Deleted, EntityState.Modified }
                .Select(entityState =>
                    {
                        var entityEntry = new InternalClrEntityEntry(mockStateManager.Object, entityType, new SimpleRecord());
                        entityEntry.SetEntityState(entityState, acceptChanges: true);
                        return entityEntry;
                    })
                .ToList();

            Assert.Equal(entityEntries.Count, mongoDbDatabase.SaveChanges(entityEntries));
        }
    }
}
