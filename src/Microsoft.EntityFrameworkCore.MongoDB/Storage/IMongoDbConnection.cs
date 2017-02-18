using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public interface IMongoDbConnection
    {
        IMongoDatabase GetDatabase();

        Task<IMongoDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken));

        void DropDatabase();

        Task DropDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken));

        IMongoCollection<TEntity> GetCollection<TEntity>();
    }
}