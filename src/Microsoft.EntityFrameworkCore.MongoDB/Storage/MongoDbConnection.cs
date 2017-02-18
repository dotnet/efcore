using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class MongoDbConnection : IMongoDbConnection
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IModel _model;

        public MongoDbConnection([NotNull] IMongoClient mongoClient,
            [NotNull] IModel model)
        {
            _model = Check.NotNull(model, nameof(model));

            _mongoClient = Check.NotNull(mongoClient, nameof(mongoClient));
            _mongoDatabase = _mongoClient.GetDatabase(new MongoDbModelAnnotations(model).Database);
        }

        public virtual IMongoDatabase GetDatabase()
            => _mongoDatabase;

        public virtual async Task<IMongoDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await Task.FromResult(_mongoDatabase);

        public virtual void DropDatabase()
            => _mongoClient.DropDatabase(new MongoDbModelAnnotations(_model).Database);

        public virtual async Task DropDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await _mongoClient.DropDatabaseAsync(new MongoDbModelAnnotations(_model).Database, cancellationToken);

        public virtual IMongoCollection<TEntity> GetCollection<TEntity>()
        {
            var entityType = _model.FindEntityType(typeof(TEntity));
            var annotations = new MongoDbEntityTypeAnnotations(entityType);
            return _mongoDatabase.GetCollection<TEntity>(annotations.CollectionName, annotations.CollectionSettings);
        }
    }
}