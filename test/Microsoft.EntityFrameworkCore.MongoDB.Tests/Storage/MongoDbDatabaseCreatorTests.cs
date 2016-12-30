using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.Storage
{
    public class MongoDbDatabaseCreatorTests
    {
        [Fact]
        public void Ensure_created_succeeds()
        {
            var mockMongoDbConnection = new Mock<IMongoDbConnection>();
            mockMongoDbConnection
                .Setup(mongoDbConnection => mongoDbConnection.GetDatabase())
                .Returns(new Mock<IMongoDatabase>().Object)
                .Verifiable();
            var mongoDbDatabaseCreator = new MongoDbDatabaseCreator(mockMongoDbConnection.Object);
            Assert.True(mongoDbDatabaseCreator.EnsureCreated());
            mockMongoDbConnection.Verify(
                mongoDbConnection => mongoDbConnection.GetDatabase(),
                Times.Exactly(callCount: 1));
        }

        [Fact]
        public async Task Ensure_created_async_succeeds()
        {
            var mockMongoDbConnection = new Mock<IMongoDbConnection>();
            mockMongoDbConnection
                .Setup(mongoDbConnection => mongoDbConnection.GetDatabaseAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Mock<IMongoDatabase>().Object))
                .Verifiable();
            var mongoDbDatabaseCreator = new MongoDbDatabaseCreator(mockMongoDbConnection.Object);
            Assert.True(await mongoDbDatabaseCreator.EnsureCreatedAsync());
            mockMongoDbConnection.Verify(
                mongoDbConnection => mongoDbConnection.GetDatabaseAsync(It.IsAny<CancellationToken>()),
                Times.Exactly(callCount: 1));
        }

        [Fact]
        public void Ensure_deleted_succeeds()
        {
            var mockMongoDbConnection = new Mock<IMongoDbConnection>();
            mockMongoDbConnection
                .Setup(mongoDbConnection => mongoDbConnection.DropDatabase())
                .Verifiable();
            var mongoDbDatabaseCreator = new MongoDbDatabaseCreator(mockMongoDbConnection.Object);
            Assert.True(mongoDbDatabaseCreator.EnsureDeleted());
            mockMongoDbConnection.Verify(
                mongoDbConnection => mongoDbConnection.DropDatabase(),
                Times.Exactly(callCount: 1));
        }

        [Fact]
        public async Task Ensure_deleted_async_succeeds()
        {
            var mockMongoDbConnection = new Mock<IMongoDbConnection>();
            mockMongoDbConnection
                .Setup(mongoDbConnection => mongoDbConnection.DropDatabaseAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(result: 0))
                .Verifiable();
            var mongoDbDatabaseCreator = new MongoDbDatabaseCreator(mockMongoDbConnection.Object);
            Assert.True(await mongoDbDatabaseCreator.EnsureDeletedAsync());
            mockMongoDbConnection.Verify(
                mongoDbConnection => mongoDbConnection.DropDatabaseAsync(It.IsAny<CancellationToken>()),
                Times.Exactly(callCount: 1));
        }
    }
}