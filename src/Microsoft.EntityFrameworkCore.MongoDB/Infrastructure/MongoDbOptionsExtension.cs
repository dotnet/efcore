using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.MongoDB.Adapter;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class MongoDbOptionsExtension : IDbContextOptionsExtension
    {
        private string _connectionString;
        private MongoClientSettings _mongoClientSettings;
        private MongoUrl _mongoUrl;
        private IMongoClient _mongoClient;

        public MongoDbOptionsExtension([CanBeNull]MongoDbOptionsExtension existing = null)
        {
            if (existing != null)
            {
                CopyOptions(existing);
            }
        }

        private void CopyOptions(MongoDbOptionsExtension existing)
        {
            _connectionString = existing.ConnectionString;
            _mongoClient = existing.MongoClient;
            _mongoUrl = existing.MongoUrl;
            _mongoClient = existing.MongoClient;
        }

        public virtual string ConnectionString
        {
            get { return _connectionString; }
            [param: NotNull] set
            {
                _connectionString = Check.NotEmpty(value, nameof(ConnectionString));
                _mongoClient = new MongoClient(_connectionString);
                _mongoClientSettings = null;
                _mongoUrl = null;
            }
        }

        public virtual IMongoClient MongoClient
        {
            get { return _mongoClient; }
            [param: NotNull] set
            {
                _mongoClient = Check.NotNull(value, nameof(MongoClient));
                _mongoClientSettings = null;
                _mongoUrl = null;
                _connectionString = null;
            }
        }

        public virtual MongoClientSettings MongoClientSettings
        {
            get { return _mongoClientSettings; }
            [param: NotNull] set
            {
                _mongoClientSettings = Check.NotNull(value, nameof(MongoClientSettings)).Clone();
                _mongoClient = new MongoClient(_mongoClientSettings);
                _mongoUrl = null;
                _connectionString = null;
            }
        }

        public virtual MongoUrl MongoUrl
        {
            get { return _mongoUrl; }
            [param: NotNull] set
            {
                _mongoUrl = Check.NotNull(value, nameof(MongoUrl));
                _mongoClient = new MongoClient(_mongoUrl);
                _mongoClientSettings = null;
                _connectionString = null;
            }
        }

        public virtual void ApplyServices([NotNull] IServiceCollection services)
        {
            ConventionRegistry.Register(
                name: "EntityFramework.MongoDb.Conventions",
                conventions: EntityFrameworkConventionPack.Instance,
                filter: type => true);
            Check.NotNull(services, nameof(services)).AddEntityFrameworkMongoDb();
        }
    }
}