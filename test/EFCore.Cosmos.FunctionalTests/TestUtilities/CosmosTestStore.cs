// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using ContainerProperties = Microsoft.Azure.Cosmos.ContainerProperties;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class CosmosTestStore : TestStore
{
    private readonly TestStoreContext _storeContext;
    private readonly Action<CosmosDbContextOptionsBuilder> _configureCosmos;
    private bool _initialized;

    private static readonly Guid _runId = Guid.NewGuid();
    private static bool? _connectionAvailable;

    // The Northwind database is shared across multiple test fixtures and is deleted at process exit
    // to avoid one fixture's disposal racing with another fixture's queries.
    private const string DeferredDeletionStoreName = "Northwind";
    private static readonly ConcurrentDictionary<string, CosmosTestStore> _deferredStores = new();

    static CosmosTestStore()
    {
        AppDomain.CurrentDomain.ProcessExit += static (_, _) =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                Task.WhenAll(_deferredStores.Select(
                    async entry =>
                    {
                        var store = entry.Value;
                        try
                        {
                            store.GetTestStoreIndex(store.ServiceProvider)
                                .RemoveShared(store.GetType().Name + store.Name);
                            await store.EnsureDeletedAsync(store._storeContext, cts.Token).ConfigureAwait(false);
                        }
                        catch
                        {
                        }

                        store._storeContext.Dispose();
                    })).GetAwaiter().GetResult();
            }
            catch
            {
            }
        };
    }

    public static CosmosTestStore Create(string name, Action<CosmosDbContextOptionsBuilder>? extensionConfiguration = null)
        => new(name, shared: false, extensionConfiguration: extensionConfiguration);

    public static async Task<CosmosTestStore> CreateInitializedAsync(
        string name,
        Action<CosmosDbContextOptionsBuilder>? extensionConfiguration = null)
    {
        var testStore = Create(name, extensionConfiguration);
        await testStore.InitializeAsync(null, (Func<DbContext>?)null).ConfigureAwait(false);
        return testStore;
    }

    public static CosmosTestStore GetOrCreate(string name)
        => new(name);

    private CosmosTestStore(
        string name,
        bool shared = true,
        Action<CosmosDbContextOptionsBuilder>? extensionConfiguration = null)
        : base(CreateName(name), shared)
    {
        ConnectionUri = CosmosTestEnvironment.DefaultConnection;
        AuthToken = CosmosTestEnvironment.AuthToken;
        ConnectionString = CosmosTestEnvironment.ConnectionString;
        TokenCredential = CosmosTestEnvironment.TokenCredential;
        _configureCosmos = extensionConfiguration == null
            ? b => b.ApplyConfiguration()
            : b =>
            {
                b.ApplyConfiguration();
                extensionConfiguration(b);
            };

        _storeContext = new TestStoreContext(this);

        if (shared && name == DeferredDeletionStoreName)
        {
            _deferredStores.TryAdd(Name, this);
        }
        else if (shared)
        {
            Check.DebugAssert(
                !_deferredStores.ContainsKey(Name) && !_deferredStores.Values.Any(s => s.Name == Name),
                $"Cosmos database '{name}' is shared across multiple fixture types. "
                + "Add it to the deferred deletion list or give each fixture a unique StoreName.");
        }
    }

    private static string CreateName(string name)
        => CosmosTestEnvironment.IsEmulator
            ? name
            : name + _runId;

    public string ConnectionUri { get; private set; }
    public string AuthToken { get; }
    public TokenCredential TokenCredential { get; }
    public string ConnectionString { get; private set; }

    private static readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    protected override DbContext CreateDefaultContext()
        => new TestStoreContext(this);

    // Cosmos has no multi-document transactions, so a partially-completed seed must be cleaned before retrying.
    public override bool SupportsTransactions
        => false;

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
    {
        var result = CosmosTestEnvironment.UseTokenCredential
            ? builder.UseCosmos(ConnectionUri, TokenCredential, Name, _configureCosmos)
            : builder.UseCosmos(ConnectionUri, AuthToken, Name, _configureCosmos);

        return result;
    }

    public static async ValueTask<bool> IsConnectionAvailableAsync()
    {
        if (CosmosTestEnvironment.SkipConnectionCheck)
        {
            return true;
        }

        if (_connectionAvailable == null)
        {
            await _connectionSemaphore.WaitAsync();

            try
            {
                _connectionAvailable ??= await TryConnectAsync().ConfigureAwait(false);
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        return _connectionAvailable.Value;
    }

    private static async Task<bool> TryConnectAsync()
    {
        CosmosTestStore? testStore = null;
        try
        {
            testStore = await CreateInitializedAsync("NonExistent").ConfigureAwait(false);

            return true;
        }
        catch (AggregateException aggregate)
        {
            if (aggregate.Flatten().InnerExceptions.Any(IsNotConfigured))
            {
                return false;
            }

            throw;
        }
        catch (Exception e)
        {
            if (IsNotConfigured(e))
            {
                return false;
            }

            throw;
        }
        finally
        {
            if (testStore != null)
            {
                await testStore.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private static bool IsNotConfigured(Exception exception)
        => exception switch
        {
            HttpRequestException re => re.InnerException is SocketException // Exception in Mac/Linux
                || re.InnerException is IOException { InnerException: SocketException }, // Exception in Windows
            _ => exception.Message.Contains(
                "The input authorization token can't serve the request. Please check that the expected payload is built as per the protocol, and check the key being used.",
                StringComparison.Ordinal),
        };

    protected override async Task InitializeAsync(Func<DbContext> createContext, Func<DbContext, Task>? seed, Func<DbContext, Task>? clean)
    {
        await CosmosTestEnvironment.InitializeAsync().ConfigureAwait(false);

        // Update connection details in case InitializeAsync changed them (e.g., testcontainer started).
        ConnectionUri = CosmosTestEnvironment.DefaultConnection;
        ConnectionString = CosmosTestEnvironment.ConnectionString;

        _initialized = true;

        if (_connectionAvailable == false)
        {
            return;
        }

        await base.InitializeAsync(createContext ?? (() => _storeContext), seed, clean).ConfigureAwait(false);
    }

    private static readonly ArmClient _armClient = new(CosmosTestEnvironment.TokenCredential);

    public async Task<bool> EnsureCreatedAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        if (!CosmosTestEnvironment.UseTokenCredential)
        {
            var cosmosClientWrapper = context.GetService<ICosmosClientWrapper>();
            return await cosmosClientWrapper.CreateDatabaseIfNotExistsAsync(null, cancellationToken).ConfigureAwait(false);
        }

        var databaseAccount = await GetDBAccount(cancellationToken).ConfigureAwait(false);
        var collection = databaseAccount.Value.GetCosmosDBSqlDatabases();
        var sqlDatabaseCreateUpdateContent = new CosmosDBSqlDatabaseCreateOrUpdateContent(
            CosmosTestEnvironment.AzureLocation,
            new CosmosDBSqlDatabaseResourceInfo(Name));
        if (await collection.ExistsAsync(Name, cancellationToken))
        {
            return false;
        }

        var model = context.GetService<IDesignTimeModel>().Model;

        var modelThroughput = model.GetThroughput();
        if (modelThroughput == null
            && GetContainersToCreate(model).All(c => c.Throughput == null))
        {
            modelThroughput = ThroughputProperties.CreateManualThroughput(400);
        }

        if (modelThroughput != null)
        {
            sqlDatabaseCreateUpdateContent.Options = new CosmosDBCreateUpdateConfig
            {
                Throughput = modelThroughput.Throughput, AutoscaleMaxThroughput = modelThroughput.AutoscaleMaxThroughput
            };
        }

        var databaseResponse = await collection.CreateOrUpdateAsync(
            WaitUntil.Completed, Name, sqlDatabaseCreateUpdateContent, cancellationToken).ConfigureAwait(false);

        return databaseResponse.GetRawResponse().Status == (int)HttpStatusCode.OK;
    }

    private async Task<bool> EnsureDeletedAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        if (!CosmosTestEnvironment.UseTokenCredential)
        {
            return await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        }

        var databaseAccount = await GetDBAccount(cancellationToken).ConfigureAwait(false);
        var collection = databaseAccount.Value.GetCosmosDBSqlDatabases();
        var database = (await collection.GetIfExistsAsync(Name, cancellationToken).ConfigureAwait(false));
        if (database == null
            || !database.HasValue)
        {
            return false;
        }

        var databaseResponse = (await database.Value!.DeleteAsync(WaitUntil.Completed, cancellationToken).ConfigureAwait(false))
            .GetRawResponse();
        return databaseResponse.Status == (int)HttpStatusCode.OK;
    }

    private Task<global::Azure.Response<CosmosDBAccountResource>> GetDBAccount(CancellationToken cancellationToken = default)
    {
        var accountName = new Uri(ConnectionUri).Host.Split('.').First();
        var databaseAccountIdentifier = CosmosDBAccountResource.CreateResourceIdentifier(
            CosmosTestEnvironment.SubscriptionId, CosmosTestEnvironment.ResourceGroup, accountName);
        return _armClient.GetCosmosDBAccountResource(databaseAccountIdentifier).GetAsync(cancellationToken);
    }

    public override Task CleanAsync(DbContext context, bool createTables = true)
    {
        context.ChangeTracker.Clear();
        return new TestCosmosExecutionStrategy().ExecuteAsync(
            (context, createTables, Retrying: new StrongBox<bool>(false)), async (_, state, ct) =>
            {
                if (state.Retrying.Value)
                {
                    state.context.ChangeTracker.Clear();
                }

                state.Retrying.Value = true;
                await CleanAsyncImpl(state.context, state.createTables).ConfigureAwait(false);
                return true;
            }, null, default);
    }

    private async Task CleanAsyncImpl(DbContext context, bool createTables)
    {
        var created = await EnsureCreatedAsync(context).ConfigureAwait(false);

        // Containers are deleted and recreated below only when the database already existed. A freshly-created
        // database has brand-new containers whose metadata cannot be stale.
        var containersRecreated = !created;
        try
        {
            if (!created)
            {
                await DeleteContainersAsync(context).ConfigureAwait(false);
            }

            if (!createTables)
            {
                return;
            }

            if (!CosmosTestEnvironment.UseTokenCredential)
            {
                created = await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
                if (!created)
                {
                    if (containersRecreated)
                    {
                        await RefreshContainerMetadataAsync(context).ConfigureAwait(false);
                    }

                    await SeedAsync(context).ConfigureAwait(false);
                }
            }
            else
            {
                await CreateContainersAsync(context).ConfigureAwait(false);

                if (containersRecreated)
                {
                    await RefreshContainerMetadataAsync(context).ConfigureAwait(false);
                }

                await SeedAsync(context).ConfigureAwait(false);
            }
        }
        catch
        {
            try
            {
                await EnsureDeletedAsync(context).ConfigureAwait(false);
            }
            catch
            {
            }

            throw;
        }
    }

    // Deleting and recreating a container gives it a new resource id (_rid), but the shared CosmosClient caches each
    // container's _rid (and the partition key ranges keyed by it) by container name. The first operation on the
    // recreated container - whether it is the reseed below or the next test's first query - can then fail with
    // "NotFound (404) ... GetTargetPartitionKeyRanges ... failed due to stale cache", and the query pipeline does not
    // transparently refresh and retry. Prime the client's caches here, right after recreating the containers and before
    // any test touches them. Each attempt re-reads the container by name (refreshing the collection cache with the new
    // _rid) and then runs a query - the exact path that otherwise fails. Because the recreate can take a moment to
    // become consistent on the emulator gateway, retry with a short delay until a query succeeds. This is the single
    // place that handles the stale-metadata problem; it is fully best-effort and must never fail the clean, so all
    // errors are ultimately swallowed.
    private async Task RefreshContainerMetadataAsync(DbContext context)
    {
        const int maxAttempts = 10;

        try
        {
            var cosmosClient = context.Database.GetCosmosClient();
            var database = cosmosClient.GetDatabase(Name);
            var model = context.GetService<IDesignTimeModel>().Model;
            var countQuery = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");

            foreach (var containerProperties in GetContainersToCreate(model))
            {
                var container = database.GetContainer(containerProperties.Id);

                for (var attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        await container.ReadContainerAsync().ConfigureAwait(false);

                        using var iterator = container.GetItemQueryIterator<int>(countQuery);
                        while (iterator.HasMoreResults)
                        {
                            await iterator.ReadNextAsync().ConfigureAwait(false);
                        }

                        break;
                    }
                    catch when (attempt < maxAttempts)
                    {
                        // The recreate has not become consistent yet; wait for the gateway to catch up and retry.
                        await Task.Delay(200).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Best-effort priming; never let it fail the clean.
                    }
                }
            }
        }
        catch
        {
            // Never let cache priming fail the clean.
        }
    }

    private async Task CreateContainersAsync(DbContext context)
    {
        var databaseAccount = await GetDBAccount().ConfigureAwait(false);
        var collection = databaseAccount.Value.GetCosmosDBSqlDatabases();
        var database = await collection.GetAsync(Name).ConfigureAwait(false);
        var model = context.GetService<IDesignTimeModel>().Model;

        foreach (var container in GetContainersToCreate(model))
        {
            var resource = new CosmosDBSqlContainerResourceInfo(container.Id)
            {
                AnalyticalStorageTtl = container.AnalyticalStoreTimeToLiveInSeconds,
                DefaultTtl = container.DefaultTimeToLive,
                PartitionKey = new CosmosDBContainerPartitionKey { Version = 2 }
            };

            if (container.PartitionKeyStoreNames.Count > 1)
            {
                resource.PartitionKey.Kind = "MultiHash";
            }

            foreach (var partitionKey in container.PartitionKeyStoreNames)
            {
                resource.PartitionKey.Paths.Add("/" + partitionKey);
            }

            var content = new CosmosDBSqlContainerCreateOrUpdateContent(CosmosTestEnvironment.AzureLocation, resource);
            if (container.Throughput != null)
            {
                content.Options = new CosmosDBCreateUpdateConfig
                {
                    AutoscaleMaxThroughput = container.Throughput.AutoscaleMaxThroughput, Throughput = container.Throughput.Throughput
                };
            }

            // TODO: see issue #35854
            // once Azure.ResourceManager.CosmosDB package supports vectors and FTS, those need to be added here

            await database.Value.GetCosmosDBSqlContainers().CreateOrUpdateAsync(
                WaitUntil.Completed, container.Id, content).ConfigureAwait(false);
        }
    }

    private static IEnumerable<Cosmos.Storage.Internal.ContainerProperties> GetContainersToCreate(IModel model)
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

        var fullTextDefaultLanguage = model.GetDefaultFullTextSearchLanguage();
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

            yield return new Cosmos.Storage.Internal.ContainerProperties(
                containerName,
                partitionKeyStoreNames,
                analyticalTtl,
                defaultTtl,
                throughput,
                indexes,
                vectors,
                fullTextDefaultLanguage ?? "en-US",
                fullTextProperties,
                AutomaticIndexingExceptions: mappedTypes.Select(et => et.GetAutomaticIndexingExceptions()).FirstOrDefault(e => e is not null),
                AutomaticIndexingEnabled: mappedTypes.Select(et => et.GetAutomaticIndexingEnabled()).FirstOrDefault(e => e is not null));
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

    private static IReadOnlyList<string> GetPartitionKeyStoreNames(IEntityType entityType)
    {
        var properties = entityType.GetPartitionKeyProperties();
        return properties.Any()
            ? properties.Select(p => p.GetJsonPropertyName()).ToList()
            : [CosmosClientWrapper.DefaultPartitionKey];
    }

    private async Task DeleteContainersAsync(DbContext context)
    {
        if (!CosmosTestEnvironment.UseTokenCredential)
        {
            var cosmosClient = context.Database.GetCosmosClient();
            var database = cosmosClient.GetDatabase(Name);
            var containers = new List<Container>();
            var containerIterator = database.GetContainerQueryIterator<ContainerProperties>();
            while (containerIterator.HasMoreResults)
            {
                foreach (var containerProperties in await containerIterator.ReadNextAsync().ConfigureAwait(false))
                {
                    containers.Add(database.GetContainer(containerProperties.Id));
                }
            }

            foreach (var container in containers)
            {
                await container.DeleteContainerAsync();
            }
        }
        else
        {
            var databaseAccount = await GetDBAccount().ConfigureAwait(false);
            var collection = databaseAccount.Value.GetCosmosDBSqlDatabases();
            var database = await collection.GetAsync(Name).ConfigureAwait(false);
            var containers = await database.Value.GetCosmosDBSqlContainers().GetAllAsync().ToListAsync().ConfigureAwait(false);
            foreach (var container in containers)
            {
                await container.DeleteAsync(WaitUntil.Completed).ConfigureAwait(false);
            }
        }
    }

    private static async Task SeedAsync(DbContext context)
    {
        var creator = (CosmosDatabaseCreator)context.GetService<IDatabaseCreator>();
        await creator.InsertDataAsync().ConfigureAwait(false);
        await creator.SeedDataAsync(created: true).ConfigureAwait(false);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_initialized || _connectionAvailable == false)
        {
            return;
        }

        if (_deferredStores.TryGetValue(Name, out var canonical))
        {
            if (!ReferenceEquals(this, canonical))
            {
                _storeContext.Dispose();
            }

            return;
        }

        if (Shared)
        {
            GetTestStoreIndex(ServiceProvider).RemoveShared(GetType().Name + Name);
        }

        await EnsureDeletedAsync(_storeContext).ConfigureAwait(false);
        _storeContext.Dispose();
    }

    private class TestStoreContext(CosmosTestStore testStore) : DbContext
    {
        private readonly CosmosTestStore _testStore = testStore;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (CosmosTestEnvironment.UseTokenCredential)
            {
                optionsBuilder.UseCosmos(
                    _testStore.ConnectionUri, _testStore.TokenCredential, _testStore.Name, _testStore._configureCosmos);
            }
            else
            {
                optionsBuilder.UseCosmos(_testStore.ConnectionUri, _testStore.AuthToken, _testStore.Name, _testStore._configureCosmos);
            }
        }
    }
}
