using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class MongoDbDatabaseCreator : IDatabaseCreator
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        public MongoDbDatabaseCreator([NotNull] IMongoDbConnection mongoDbConnection)
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        public virtual bool EnsureCreated()
        {
            _mongoDbConnection.GetDatabase();
            return true;
        }

        public virtual async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await _mongoDbConnection.GetDatabaseAsync(cancellationToken);
            return true;
        }

        public virtual bool EnsureDeleted()
        {
            _mongoDbConnection.DropDatabase();
            return true;
        }

        public virtual async Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await _mongoDbConnection.DropDatabaseAsync(cancellationToken);
            return true;
        }
    }
}