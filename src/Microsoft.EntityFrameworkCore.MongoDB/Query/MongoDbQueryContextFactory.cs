using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MongoDbQueryContextFactory : QueryContextFactory
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        public MongoDbQueryContextFactory(
            [NotNull] ICurrentDbContext currentDbContext,
            [NotNull] IConcurrencyDetector concurrencyDetector,
            [NotNull] IMongoDbConnection mongoDbConnection)
            : base(currentDbContext, concurrencyDetector)
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        public override QueryContext Create()
            => new MongoDbQueryContext(CreateQueryBuffer, _mongoDbConnection, StateManager, ConcurrencyDetector);
    }
}