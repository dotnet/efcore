using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Driver;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public interface IMongoDbDatabaseProviderServices : IDatabaseProviderServices
    {
        IMongoClient MongoClient { get; }
    }
}