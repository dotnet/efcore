using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class MongoDbDatabase : Database
    {
        private static readonly MethodInfo _genericSaveChanges = typeof(MongoDbDatabase).GetTypeInfo()
            .GetMethod(nameof(SaveChanges), BindingFlags.NonPublic | BindingFlags.Instance)
            .GetGenericMethodDefinition();

        private static readonly MethodInfo _genericSaveChangesAsync = typeof(MongoDbDatabase).GetTypeInfo()
            .GetMethod(nameof(SaveChangesAsync), BindingFlags.NonPublic | BindingFlags.Instance)
            .GetGenericMethodDefinition();

        private readonly IMongoDbConnection _mongoDbConnection;

        public MongoDbDatabase(
            [NotNull] IQueryCompilationContextFactory queryCompilationContextFactory,
            [NotNull] IMongoDbConnection mongoDbConnection)
            : base(Check.NotNull(queryCompilationContextFactory, nameof(queryCompilationContextFactory)))
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        public override int SaveChanges([NotNull] IReadOnlyList<IUpdateEntry> entries)
            => Check.NotNull(entries, nameof(entries))
                .ToLookup(entry => GetCollectionEntityType(entry.EntityType))
                .Sum(grouping => (int)_genericSaveChanges.MakeGenericMethod(grouping.Key.ClrType)
                    .Invoke(this, new object[] { grouping }));

        private IEntityType GetCollectionEntityType(IEntityType entityType)
            => entityType.BaseType != null
                ? entityType.RootType()
                : entityType;

        private int SaveChanges<TEntity>(IEnumerable<IUpdateEntry> entries)
        {
            IEnumerable<WriteModel<TEntity>> writeModels = entries
                .Select(entry => entry.ToMongoDbWriteModel<TEntity>())
                .ToList();
            BulkWriteResult result = _mongoDbConnection.GetCollection<TEntity>().BulkWrite(writeModels);
            return (int)(result.DeletedCount + result.InsertedCount + result.ModifiedCount);
        }

        public override async Task<int> SaveChangesAsync([NotNull] IReadOnlyList<IUpdateEntry> entries,
            CancellationToken cancellationToken = new CancellationToken())
        {
            IEnumerable<Task<int>> tasks = Check.NotNull(entries, nameof(entries))
                .ToLookup(entry => GetCollectionEntityType(entry.EntityType))
                .Select(grouping => InvokeSaveChangesAsync(grouping, cancellationToken));
            return await Task.WhenAll()
                .ContinueWith(allTask => tasks.Sum(task => task.Result), cancellationToken);
        }

        private async Task<int> InvokeSaveChangesAsync(IGrouping<IEntityType, IUpdateEntry> entryGrouping, CancellationToken cancellationToken)
            => await (Task<int>)_genericSaveChangesAsync.MakeGenericMethod(entryGrouping.Key.ClrType)
                .Invoke(this, new object[] {entryGrouping, cancellationToken});

        private async Task<int> SaveChangesAsync<TEntity>(IEnumerable<IUpdateEntry> entries, CancellationToken cancellationToken)
        {
            IEnumerable<WriteModel<TEntity>> writeModels = entries.Select(entry => entry.ToMongoDbWriteModel<TEntity>());
            BulkWriteResult result = await _mongoDbConnection.GetCollection<TEntity>().BulkWriteAsync(writeModels, options: null, cancellationToken: cancellationToken);
            return (int)(result.DeletedCount + result.InsertedCount + result.ModifiedCount);
        }
    }
}