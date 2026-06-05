// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosDatabaseCreator : IDatabaseCreator
{
    private readonly ICosmosClientWrapper _cosmosClient;
    private readonly IDesignTimeModel _designTimeModel;
    private readonly IUpdateAdapterFactory _updateAdapterFactory;
    private readonly IDatabase _database;
    private readonly ICurrentDbContext _currentContext;
    private readonly IDbContextOptions _contextOptions;
    private readonly IExecutionStrategy _executionStrategy;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Infrastructure> _logger;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosDatabaseCreator(
        ICosmosClientWrapper cosmosClient,
        IDesignTimeModel designTimeModel,
        IUpdateAdapterFactory updateAdapterFactory,
        IDatabase database,
        ICurrentDbContext currentContext,
        IDbContextOptions contextOptions,
        IExecutionStrategy executionStrategy,
        IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger)
    {
        _cosmosClient = cosmosClient;
        _designTimeModel = designTimeModel;
        _updateAdapterFactory = updateAdapterFactory;
        _database = database;
        _currentContext = currentContext;
        _contextOptions = contextOptions;
        _executionStrategy = executionStrategy;
        _logger = logger;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        if (_currentContext.Context.ChangeTracker.Entries().Any(
                e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            _logger.EnsureCreatedWithTrackedEntitiesWarning();
        }

        var created = new StrongBox<bool>(false);
        var dataInserted = new StrongBox<bool>(false);
        var seeded = new StrongBox<bool>(false);
        return _executionStrategy.ExecuteAsync(
            (Creator: this, Created: created, DataInserted: dataInserted, Seeded: seeded),
            static async (_, state, ct) =>
            {
                var creator = state.Creator;

                if (!state.DataInserted.Value)
                {
                    var model = creator._designTimeModel.Model;
                    state.Created.Value |= await creator._cosmosClient
                        .CreateDatabaseIfNotExistsAsync(model.GetThroughput(), ct)
                        .ConfigureAwait(false);

                    foreach (var container in GetContainersToCreate(model))
                    {
                        state.Created.Value |= await creator._cosmosClient
                            .CreateContainerIfNotExistsAsync(container, ct)
                            .ConfigureAwait(false);
                    }

                    if (state.Created.Value)
                    {
                        await creator.InsertDataAsync(ct).ConfigureAwait(false);
                        state.DataInserted.Value = true;
                    }
                }

                if (!state.Seeded.Value)
                {
                    await creator.SeedDataAsync(state.Created.Value, cancellationToken: ct)
                        .ConfigureAwait(false);
                    state.Seeded.Value = true;
                }

                return state.Created.Value;
            }, verifySucceeded: null, cancellationToken);
    }

    private static IEnumerable<ContainerProperties> GetContainersToCreate(IModel model)
    {
        var containers = new Dictionary<string, List<IEntityType>>();
        foreach (var entityType in model.GetEntityTypes().Where(et => et.FindPrimaryKey() != null))
        {
            var container = entityType.GetContainer();
            if (container == null)
            {
                continue;
            }

            if (!containers.TryGetValue(container, out var mappedTypes))
            {
                mappedTypes = [];
                containers[container] = mappedTypes;
            }

            mappedTypes.Add(entityType);
        }

        var defaultFullTextLanguage = model.GetDefaultFullTextSearchLanguage();
        foreach (var (containerName, mappedTypes) in containers)
        {
            IReadOnlyList<string> partitionKeyStoreNames = [];
            int? analyticalTtl = null;
            int? defaultTtl = null;
            ThroughputProperties? throughput = null;
            var indexes = new List<IIndex>();
            var vectors = new List<(IProperty Property, CosmosVectorType VectorType)>();
            var fullTextProperties = new List<(IProperty Property, string? Language)>();

            foreach (var entityType in mappedTypes)
            {
                if (!partitionKeyStoreNames.Any())
                {
                    partitionKeyStoreNames = GetPartitionKeyStoreNames(entityType);
                }

                analyticalTtl ??= entityType.GetAnalyticalStoreTimeToLive();
                defaultTtl ??= entityType.GetDefaultTimeToLive();
                throughput ??= entityType.GetThroughput();

                ProcessEntityType(entityType, indexes, vectors, fullTextProperties);
            }

            yield return new ContainerProperties(
                containerName,
                partitionKeyStoreNames,
                analyticalTtl,
                defaultTtl,
                throughput,
                indexes,
                vectors,
                defaultFullTextLanguage ?? "en-US",
                fullTextProperties);
        }

        static void ProcessEntityType(
            IEntityType entityType,
            List<IIndex> indexes,
            List<(IProperty Property, CosmosVectorType VectorType)> vectors,
            List<(IProperty Property, string? Language)> fullTextProperties)
        {
            indexes.AddRange(entityType.GetIndexes());

            foreach (var property in entityType.GetProperties())
            {
                if (property.FindTypeMapping() is CosmosVectorTypeMapping vectorTypeMapping)
                {
                    vectors.Add((property, vectorTypeMapping.VectorType));
                }

                if (property.GetIsFullTextSearchEnabled() == true)
                {
                    fullTextProperties.Add((property, property.GetFullTextSearchLanguage()));
                }
            }

            foreach (var ownedType in entityType.GetNavigations()
                         .Where(x => x.ForeignKey.IsOwnership && !x.IsOnDependent && !x.TargetEntityType.IsDocumentRoot())
                         .Select(x => x.TargetEntityType))
            {
                ProcessEntityType(ownedType, indexes, vectors, fullTextProperties);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task InsertDataAsync(CancellationToken cancellationToken = default)
    {
        var updateAdapter = AddModelData();

        return _database.SaveChangesAsync(updateAdapter.GetEntriesToSave(), cancellationToken);
    }

    private IUpdateAdapter AddModelData()
    {
        var updateAdapter = _updateAdapterFactory.CreateStandalone();
        foreach (var entityType in _designTimeModel.Model.GetEntityTypes())
        {
            foreach (var targetSeed in entityType.GetSeedData())
            {
                var runtimeEntityType = updateAdapter.Model.FindEntityType(entityType.Name)!;
                var values = new Dictionary<IProperty, object?>();
                foreach (var (name, value) in targetSeed)
                {
                    if (runtimeEntityType.FindProperty(name) is { } property)
                    {
                        values[property] = value;
                    }
                }

                var entry = updateAdapter.CreateEntry(values, runtimeEntityType);
                entry.EntityState = EntityState.Added;
            }
        }

        return updateAdapter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task SeedDataAsync(
        bool created, CancellationToken cancellationToken = default)
    {
        var coreOptionsExtension =
            _contextOptions.FindExtension<CoreOptionsExtension>();

        if (coreOptionsExtension?.AsyncSeeder is not null)
        {
            await coreOptionsExtension.AsyncSeeder(_currentContext.Context, created, cancellationToken).ConfigureAwait(false);
        }
        else if (coreOptionsExtension?.Seeder is not null)
        {
            throw new InvalidOperationException(CoreStrings.MissingSeeder);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        => _cosmosClient.DeleteDatabaseAsync(cancellationToken);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException(CosmosStrings.CanConnectNotSupported);

    /// <summary>
    ///     Returns the store names of the properties that is used to store the partition keys.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    /// <param name="entityType">The entity type to get the partition key property names for.</param>
    /// <returns>The names of the partition key property.</returns>
    private static IReadOnlyList<string> GetPartitionKeyStoreNames(IEntityType entityType)
    {
        var properties = entityType.GetPartitionKeyProperties();
        return properties.Any()
            ? properties.Select(p => p.GetJsonPropertyName()).ToList()
            : [CosmosClientWrapper.DefaultPartitionKey];
    }

    #region Unsupported sync methods

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool EnsureCreated()
        => throw new InvalidOperationException(CosmosStrings.SyncNotSupported);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool EnsureDeleted()
        => throw new InvalidOperationException(CosmosStrings.SyncNotSupported);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CanConnect()
        => throw new InvalidOperationException(CosmosStrings.SyncNotSupported);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SeedData(bool created)
        => throw new InvalidOperationException(CosmosStrings.SyncNotSupported);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [Obsolete("Azure Cosmos DB does not support synchronous I/O, use InsertDataAsync.", error: true)]
    public virtual void InsertData()
    {
    }

    #endregion
}
