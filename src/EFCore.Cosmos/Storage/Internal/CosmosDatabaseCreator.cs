// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

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
        IDatabase database)
    {
        _cosmosClient = cosmosClient;
        _designTimeModel = designTimeModel;
        _updateAdapterFactory = updateAdapterFactory;
        _database = database;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool EnsureCreated()
    {
        var model = _designTimeModel.Model;
        var created = _cosmosClient.CreateDatabaseIfNotExists(model.GetThroughput());

        foreach (var container in GetContainersToCreate(model))
        {
            created |= _cosmosClient.CreateContainerIfNotExists(container);
        }

        if (created)
        {
            Seed();
        }

        return created;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        var model = _designTimeModel.Model;
        var created = await _cosmosClient.CreateDatabaseIfNotExistsAsync(model.GetThroughput(), cancellationToken)
            .ConfigureAwait(false);

        foreach (var container in GetContainersToCreate(model))
        {
            created |= await _cosmosClient.CreateContainerIfNotExistsAsync(container, cancellationToken)
                .ConfigureAwait(false);
        }

        if (created)
        {
            await SeedAsync(cancellationToken).ConfigureAwait(false);
        }

        return created;
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

        foreach (var (containerName, mappedTypes) in containers)
        {
            string? partitionKey = null;
            int? analyticalTtl = null;
            int? defaultTtl = null;
            ThroughputProperties? throughput = null;

            foreach (var entityType in mappedTypes)
            {
                partitionKey ??= GetPartitionKeyStoreName(entityType);
                analyticalTtl ??= entityType.GetAnalyticalStoreTimeToLive();
                defaultTtl ??= entityType.GetDefaultTimeToLive();
                throughput ??= entityType.GetThroughput();
            }

            yield return new ContainerProperties(
                containerName,
                partitionKey!,
                analyticalTtl,
                defaultTtl,
                throughput);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Seed()
    {
        var updateAdapter = AddSeedData();

        _database.SaveChanges(updateAdapter.GetEntriesToSave());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var updateAdapter = AddSeedData();

        return _database.SaveChangesAsync(updateAdapter.GetEntriesToSave(), cancellationToken);
    }

    private IUpdateAdapter AddSeedData()
    {
        var updateAdapter = _updateAdapterFactory.CreateStandalone();
        foreach (var entityType in _designTimeModel.Model.GetEntityTypes())
        {
            foreach (var targetSeed in entityType.GetSeedData())
            {
                updateAdapter.Model.FindEntityType(entityType.Name);
                var entry = updateAdapter.CreateEntry(targetSeed, entityType);
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
    public virtual bool EnsureDeleted()
        => _cosmosClient.DeleteDatabase();

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
    public virtual bool CanConnect()
        => throw new NotSupportedException(CosmosStrings.CanConnectNotSupported);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException(CosmosStrings.CanConnectNotSupported);

    /// <summary>
    ///     Returns the store name of the property that is used to store the partition key.
    /// </summary>
    /// <param name="entityType">The entity type to get the partition key property name for.</param>
    /// <returns>The name of the partition key property.</returns>
    private static string GetPartitionKeyStoreName(IEntityType entityType)
    {
        var name = entityType.GetPartitionKeyPropertyName();
        return name != null
            ? entityType.FindProperty(name)!.GetJsonPropertyName()
            : CosmosClientWrapper.DefaultPartitionKey;
    }
}
