using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MongoDbQueryContext : QueryContext
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        public MongoDbQueryContext(
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] IMongoDbConnection mongoDbConnection,
            [NotNull] LazyRef<IStateManager> stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector)
            : base(queryBufferFactory, stateManager, concurrencyDetector)
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }
    }
}