using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.Storage
{
    public class MongoDbConnectionTests
    {
        private readonly Mock<IMongoDatabase> _mockMongoDatabase;
        private readonly Mock<IMongoClient> _mockMongoClient;
        private readonly Mock<IMongoCollection<SimpleRecord>> _mockSimpleRecords;
        private readonly IModel _model;

        public MongoDbConnectionTests()
        {
            _model = GetModel();
            _mockMongoClient = MockMongoClient();
            _mockMongoDatabase = MockMongoDatabase();
            _mockSimpleRecords = MockSimpleRecords();
        }

        private IModel GetModel()
        {
            var model = new Model();
            model.Builder
                .MongoDb(ConfigurationSource.Explicit)
                .FromDatabase("testdb")
                .InternalModelBuilder
                .Entity(typeof(SimpleRecord), ConfigurationSource.Explicit)
                .MongoDb(ConfigurationSource.Explicit)
                .FromCollection("simpleRecords");
            return model;
        }

        private Mock<IMongoClient> MockMongoClient()
        {
            var mockMongoClient = new Mock<IMongoClient>();
            mockMongoClient
                .Setup(mongoClient => mongoClient.GetDatabase("testdb", It.IsAny<MongoDatabaseSettings>()))
                .Returns(() => _mockMongoDatabase.Object)
                .Verifiable();
            mockMongoClient
                .Setup(mongoClient => mongoClient.DropDatabase("testdb", It.IsAny<CancellationToken>()))
                .Verifiable();
            return mockMongoClient;
        }

        private Mock<IMongoDatabase> MockMongoDatabase()
        {
            var mockMongoDatabase = new Mock<IMongoDatabase>();
            mockMongoDatabase
                .Setup(mongoDatabase => mongoDatabase.GetCollection<SimpleRecord>("simpleRecords", It.IsAny<MongoCollectionSettings>()))
                .Returns(() => _mockSimpleRecords.Object)
                .Verifiable();
            return mockMongoDatabase;
        }

        private Mock<IMongoCollection<SimpleRecord>> MockSimpleRecords()
            => new Mock<IMongoCollection<SimpleRecord>>();

        [Fact]
        public void Get_database_calls_mongo_client_get_database()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClient.Object, _model);
            Assert.Equal(_mockMongoDatabase.Object, mongoDbConnection.GetDatabase());
            _mockMongoClient
                .Verify(mongoClient => mongoClient.GetDatabase("testdb", It.IsAny<MongoDatabaseSettings>()), Times.Once);
        }

        [Fact]
        public async Task Get_database_async_calls_mongo_client_get_database()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClient.Object, _model);
            Assert.Equal(_mockMongoDatabase.Object, await mongoDbConnection.GetDatabaseAsync());
            _mockMongoClient
                .Verify(mongoClient => mongoClient.GetDatabase("testdb", It.IsAny<MongoDatabaseSettings>()), Times.Once);
        }

        [Fact]
        public void Drop_database_calls_mongo_client_drop_database()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClient.Object, _model);
            mongoDbConnection.DropDatabase();
            _mockMongoClient
                .Verify(mongoClient => mongoClient.DropDatabase("testdb", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Drop_database_async_calls_mongo_client_drop_database_async()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClient.Object, _model);
            await mongoDbConnection.DropDatabaseAsync();
            _mockMongoClient
                .Verify(mongoClient => mongoClient.DropDatabaseAsync("testdb", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Get_collection_calls_mongo_database_get_collection()
        {
            IMongoDbConnection mongoDbConnection = new MongoDbConnection(_mockMongoClient.Object, _model);
            Assert.Equal(_mockSimpleRecords.Object, mongoDbConnection.GetCollection<SimpleRecord>());
            _mockMongoDatabase
                .Verify(mongoDatabase => mongoDatabase.GetCollection<SimpleRecord>("simpleRecords", It.IsAny<MongoCollectionSettings>()), Times.Once);
        }
    }
}