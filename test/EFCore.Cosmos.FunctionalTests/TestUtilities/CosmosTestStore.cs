// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Sockets;
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
        ConnectionUri = TestEnvironment.DefaultConnection;
        AuthToken = TestEnvironment.AuthToken;
        ConnectionString = TestEnvironment.ConnectionString;
        TokenCredential = TestEnvironment.TokenCredential;
        _configureCosmos = extensionConfiguration == null
            ? b => b.ApplyConfiguration()
            : b =>
            {
                b.ApplyConfiguration();
                extensionConfiguration(b);
            };

        _storeContext = new TestStoreContext(this);
    }

    private static string CreateName(string name)
        => TestEnvironment.IsEmulator
            ? name
            : name + _runId;

    public string ConnectionUri { get; private set; }
    public string AuthToken { get; }
    public TokenCredential TokenCredential { get; }
    public string ConnectionString { get; private set; }

    private static readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    protected override DbContext CreateDefaultContext()
        => new TestStoreContext(this);

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
    {
        var result = TestEnvironment.UseTokenCredential
            ? builder.UseCosmos(ConnectionUri, TokenCredential, Name, _configureCosmos)
            : builder.UseCosmos(ConnectionUri, AuthToken, Name, _configureCosmos);

        if (TestEnvironment.IsLinuxEmulator)
        {
            result.AddInterceptors(LinuxEmulatorSaveChangesInterceptor.Instance);
        }

        return result;
    }

    public static async ValueTask<bool> IsConnectionAvailableAsync()
    {
        if (TestEnvironment.SkipConnectionCheck)
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
        await TestEnvironment.InitializeAsync().ConfigureAwait(false);

        // Update connection details in case InitializeAsync changed them (e.g., testcontainer started).
        ConnectionUri = TestEnvironment.DefaultConnection;
        ConnectionString = TestEnvironment.ConnectionString;

        _initialized = true;

        if (_connectionAvailable == false)
        {
            return;
        }

        await base.InitializeAsync(createContext ?? (() => _storeContext), seed, clean).ConfigureAwait(false);
    }

    private static readonly ArmClient _armClient = new(TestEnvironment.TokenCredential);

    public async Task<bool> EnsureCreatedAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        if (!TestEnvironment.UseTokenCredential)
        {
            var cosmosClientWrapper = context.GetService<ICosmosClientWrapper>();
            return await cosmosClientWrapper.CreateDatabaseIfNotExistsAsync(null, cancellationToken).ConfigureAwait(false);
        }

        var databaseAccount = await GetDBAccount(cancellationToken).ConfigureAwait(false);
        var collection = databaseAccount.Value.GetCosmosDBSqlDatabases();
        var sqlDatabaseCreateUpdateContent = new CosmosDBSqlDatabaseCreateOrUpdateContent(
            TestEnvironment.AzureLocation,
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
        if (!TestEnvironment.UseTokenCredential)
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
            TestEnvironment.SubscriptionId, TestEnvironment.ResourceGroup, accountName);
        return _armClient.GetCosmosDBAccountResource(databaseAccountIdentifier).GetAsync(cancellationToken);
    }

    public override Task CleanAsync(DbContext context, bool createTables = true)
        => new TestCosmosExecutionStrategy().ExecuteAsync(
            (context, createTables), async (_, state, ct) =>
            {
                await CleanAsyncImpl(state.context, state.createTables).ConfigureAwait(false);
                return true;
            }, null, default);

    private async Task CleanAsyncImpl(DbContext context, bool createTables)
    {
        var created = await EnsureCreatedAsync(context).ConfigureAwait(false);
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

            if (!TestEnvironment.UseTokenCredential)
            {
                created = await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
                if (!created)
                {
                    await SeedAsync(context).ConfigureAwait(false);
                }
            }
            else
            {
                await CreateContainersAsync(context).ConfigureAwait(false);
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

            var content = new CosmosDBSqlContainerCreateOrUpdateContent(TestEnvironment.AzureLocation, resource);
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

    private static IReadOnlyList<string> GetPartitionKeyStoreNames(IEntityType entityType)
    {
        var properties = entityType.GetPartitionKeyProperties();
        return properties.Any()
            ? properties.Select(p => p.GetJsonPropertyName()).ToList()
            : [CosmosClientWrapper.DefaultPartitionKey];
    }

    private async Task DeleteContainersAsync(DbContext context)
    {
        if (!TestEnvironment.UseTokenCredential)
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
        if (_initialized)
        {
            if (_connectionAvailable == false)
            {
                return;
            }

            if (Shared)
            {
                GetTestStoreIndex(ServiceProvider).RemoveShared(GetType().Name + Name);
            }

            await EnsureDeletedAsync(_storeContext).ConfigureAwait(false);
        }

        _storeContext.Dispose();
    }

    private class TestStoreContext(CosmosTestStore testStore) : DbContext
    {
        private readonly CosmosTestStore _testStore = testStore;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (TestEnvironment.UseTokenCredential)
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
