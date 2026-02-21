// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
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
    protected readonly TestStoreContext StoreContext;
    private readonly Action<CosmosDbContextOptionsBuilder> _configureCosmos;

    private static readonly Guid _runId = Guid.NewGuid();

    public CosmosTestStore(
        string name,
        bool shared = true,
        Action<CosmosDbContextOptionsBuilder>? extensionConfiguration = null)
        : base(CreateName(name, shared), shared)
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

        StoreContext = new TestStoreContext(this);
    }

    private static string CreateName(string name, bool shared)
    {
        return !TestEnvironment.IsEmulator ? name + _runId : name;
    }

    public string ConnectionUri { get; }
    public string AuthToken { get; }
    public TokenCredential TokenCredential { get; }
    public string ConnectionString { get; }

    protected override DbContext CreateDefaultContext()
        => new TestStoreContext(this);

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => TestEnvironment.UseTokenCredential
            ? builder.UseCosmos(ConnectionUri, TokenCredential, Name, _configureCosmos)
            : builder.UseCosmos(ConnectionUri, AuthToken, Name, _configureCosmos);

    protected override async Task InitializeAsync(Func<DbContext>? createContext, Func<DbContext, Task>? seed, Func<DbContext, Task>? clean)
    {
        createContext ??= (() => StoreContext);

        using (var context = createContext())
        {
            await CreateDatabaseAsync(context).ConfigureAwait(false);
        }

        await base.InitializeAsync(createContext, seed, clean).ConfigureAwait(false);
    }

    public override async Task CleanAsync(DbContext context, bool createTables = true)
    {
        await DeleteContainersAsync(context).ConfigureAwait(false);
        if (!createTables)
        {
            return;
        }

        try
        {
            await CreatedContainersAsync(context).ConfigureAwait(false);
        }
        catch
        {
            try
            {
                await DeleteDatabaseAsync(context).ConfigureAwait(false);
            }
            catch
            {
            }

            throw;
        }
    }

    private static readonly ArmClient _armClient = new(TestEnvironment.TokenCredential);

    protected virtual async Task CreateDatabaseAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        if (!TestEnvironment.UseTokenCredential)
        {
            var cosmosClientWrapper = context.GetService<ICosmosClientWrapper>();
            await cosmosClientWrapper.CreateDatabaseIfNotExistsAsync(null, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await CreateDatabaseUsingTokenCredential(context, cancellationToken).ConfigureAwait(false);
        }
    }

    protected virtual async Task<bool> DeleteDatabaseAsync(DbContext context, CancellationToken cancellationToken = default)
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

    public virtual async Task<bool> CreatedContainersAsync(DbContext context, bool skipTokenCredential = false, CancellationToken cancellationToken = default)
    {
        if (!TestEnvironment.UseTokenCredential || skipTokenCredential)
        {
            return await context.Database.EnsureCreatedAsync(CancellationToken.None).ConfigureAwait(false);
        }

        await CreateContainersUsingTokenCredentialAsync(context).ConfigureAwait(false);
        return true;
    }

    protected virtual async Task DeleteContainersAsync(DbContext context)
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
                await container.DeleteContainerAsync().ConfigureAwait(false);
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
    
    private async Task<bool> CreateDatabaseUsingTokenCredential(DbContext context, CancellationToken cancellationToken = default)
    {
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
                Throughput = modelThroughput.Throughput,
                AutoscaleMaxThroughput = modelThroughput.AutoscaleMaxThroughput
            };
        }

        var databaseResponse = await collection.CreateOrUpdateAsync(
            WaitUntil.Completed, Name, sqlDatabaseCreateUpdateContent, cancellationToken).ConfigureAwait(false);

        return databaseResponse.GetRawResponse().Status == (int)HttpStatusCode.OK;
    }

    private async Task CreateContainersUsingTokenCredentialAsync(DbContext context)
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
                    AutoscaleMaxThroughput = container.Throughput.AutoscaleMaxThroughput,
                    Throughput = container.Throughput.Throughput
                };
            }

            // TODO: see issue #35854
            // once Azure.ResourceManager.CosmosDB package supports vectors and FTS, those need to be added here

            await database.Value.GetCosmosDBSqlContainers().CreateOrUpdateAsync(
                WaitUntil.Completed, container.Id, content).ConfigureAwait(false);
        }

        await SeedAsync(context);
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

    private Task<global::Azure.Response<CosmosDBAccountResource>> GetDBAccount(CancellationToken cancellationToken = default)
    {
        var accountName = new Uri(ConnectionUri).Host.Split('.').First();
        var databaseAccountIdentifier = CosmosDBAccountResource.CreateResourceIdentifier(
            TestEnvironment.SubscriptionId, TestEnvironment.ResourceGroup, accountName);
        return _armClient.GetCosmosDBAccountResource(databaseAccountIdentifier).GetAsync(cancellationToken);
    }

    private static async Task SeedAsync(DbContext context)
    {
        var creator = (CosmosDatabaseCreator)context.GetService<IDatabaseCreator>();
        await creator.InsertDataAsync().ConfigureAwait(false);
        await creator.SeedDataAsync(created: true).ConfigureAwait(false);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!Shared)
        {
            await DeleteDatabaseAsync(StoreContext).ConfigureAwait(false);
        }

        StoreContext.Dispose();
    }

    protected class TestStoreContext(CosmosTestStore testStore) : DbContext
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

    private class FakeUpdateEntry : IUpdateEntry
    {
        public IEntityType EntityType
            => new FakeEntityType();

        public EntityState EntityState { get => EntityState.Added; set => throw new NotImplementedException(); }

        public IUpdateEntry SharedIdentityEntry
            => throw new NotImplementedException();

        public object GetCurrentValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public bool CanHaveOriginalValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public object GetOriginalValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public TProperty GetOriginalValue<TProperty>(IProperty property)
            => throw new NotImplementedException();

        public bool HasTemporaryValue(IProperty property)
            => throw new NotImplementedException();

        public bool HasExplicitValue(IProperty property)
            => throw new NotImplementedException();

        public bool HasStoreGeneratedValue(IProperty property)
            => throw new NotImplementedException();

        public bool IsModified(IProperty property)
            => throw new NotImplementedException();

        public bool IsStoreGenerated(IProperty property)
            => throw new NotImplementedException();

        public DbContext Context
            => throw new NotImplementedException();

        public void SetOriginalValue(IProperty property, object? value)
            => throw new NotImplementedException();

        public void SetPropertyModified(IProperty property)
            => throw new NotImplementedException();

        public void SetStoreGeneratedValue(IProperty property, object? value, bool setModified = true)
            => throw new NotImplementedException();

        public EntityEntry ToEntityEntry()
            => throw new NotImplementedException();

        public object GetRelationshipSnapshotValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public object GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public bool IsConceptualNull(IProperty property)
            => throw new NotImplementedException();

        public bool IsModified(IComplexProperty property)
            => throw new NotImplementedException();
    }

    public class FakeEntityType : Annotatable, IEntityType
    {
        public IEntityType? BaseType
            => null;

        public string DefiningNavigationName
            => throw new NotImplementedException();

        public IEntityType DefiningEntityType
            => throw new NotImplementedException();

        public IModel Model
            => throw new NotImplementedException();

        public string Name
            => throw new NotImplementedException();

        public Type ClrType
            => throw new NotImplementedException();

        public bool HasSharedClrType
            => throw new NotImplementedException();

        public bool IsPropertyBag
            => throw new NotImplementedException();

        public InstantiationBinding ConstructorBinding
            => throw new NotImplementedException();

        public InstantiationBinding ServiceOnlyConstructorBinding
            => throw new NotImplementedException();

        IReadOnlyEntityType IReadOnlyEntityType.BaseType
            => null!;

        ITypeBase? ITypeBase.BaseType
            => BaseType;

        IReadOnlyTypeBase? IReadOnlyTypeBase.BaseType
            => BaseType;

        IReadOnlyModel IReadOnlyTypeBase.Model
            => throw new NotImplementedException();

        public IEnumerable<IForeignKey> FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
            => throw new NotImplementedException();

        public INavigation FindDeclaredNavigation(string name)
            => throw new NotImplementedException();

        public IProperty FindDeclaredProperty(string name)
            => throw new NotImplementedException();

        public IForeignKey FindForeignKey(IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
            => throw new NotImplementedException();

        public IForeignKey FindForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
            => throw new NotImplementedException();

        public IEnumerable<IForeignKey> FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
            => throw new NotImplementedException();

        public IIndex FindIndex(IReadOnlyList<IProperty> properties)
            => throw new NotImplementedException();

        public IIndex FindIndex(string name)
            => throw new NotImplementedException();

        public IIndex FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
            => throw new NotImplementedException();

        public string GetEmbeddedDiscriminatorName()
            => throw new NotImplementedException();

        public PropertyInfo FindIndexerPropertyInfo()
            => throw new NotImplementedException();

        public IKey FindKey(IReadOnlyList<IProperty> properties)
            => throw new NotImplementedException();

        public IKey FindKey(IReadOnlyList<IReadOnlyProperty> properties)
            => throw new NotImplementedException();

        public IKey FindPrimaryKey()
            => throw new NotImplementedException();

        public IReadOnlyList<IReadOnlyProperty> FindProperties(IReadOnlyList<string> propertyNames)
            => throw new NotImplementedException();

        public IProperty? FindProperty(string name)
            => null;

        public IServiceProperty FindServiceProperty(string name)
            => throw new NotImplementedException();

        public ISkipNavigation FindSkipNavigation(string name)
            => throw new NotImplementedException();

        public ChangeTrackingStrategy GetChangeTrackingStrategy()
            => throw new NotImplementedException();

        public IEnumerable<IForeignKey> GetDeclaredForeignKeys()
            => throw new NotImplementedException();

        public IEnumerable<IIndex> GetDeclaredIndexes()
            => throw new NotImplementedException();

        public IEnumerable<IKey> GetDeclaredKeys()
            => throw new NotImplementedException();

        public IEnumerable<INavigation> GetDeclaredNavigations()
            => throw new NotImplementedException();

        public IEnumerable<IProperty> GetDeclaredProperties()
            => throw new NotImplementedException();

        public IEnumerable<IForeignKey> GetDeclaredReferencingForeignKeys()
            => throw new NotImplementedException();

        public IEnumerable<IServiceProperty> GetDeclaredServiceProperties()
            => throw new NotImplementedException();

        public IEnumerable<IReadOnlySkipNavigation> GetDeclaredSkipNavigations()
            => throw new NotImplementedException();

        public IEnumerable<IForeignKey> GetDerivedForeignKeys()
            => throw new NotImplementedException();

        public IEnumerable<IIndex> GetDerivedIndexes()
            => throw new NotImplementedException();

        public IEnumerable<IReadOnlyNavigation> GetDerivedNavigations()
            => throw new NotImplementedException();

        public IEnumerable<IReadOnlyProperty> GetDerivedProperties()
            => throw new NotImplementedException();

        public IEnumerable<IReadOnlyServiceProperty> GetDerivedServiceProperties()
            => throw new NotImplementedException();

        public bool HasServiceProperties()
            => throw new NotImplementedException();

        public IEnumerable<IReadOnlySkipNavigation> GetDerivedSkipNavigations()
            => throw new NotImplementedException();

        public IEnumerable<IReadOnlyEntityType> GetDerivedTypes()
            => throw new NotImplementedException();

        public IEnumerable<IEntityType> GetDirectlyDerivedTypes()
            => throw new NotImplementedException();

        public string GetDiscriminatorPropertyName()
            => throw new NotImplementedException();

        public IEnumerable<IProperty> GetForeignKeyProperties()
            => throw new NotImplementedException();

        public IEnumerable<IForeignKey> GetForeignKeys()
            => throw new NotImplementedException();

        public IEnumerable<IIndex> GetIndexes()
            => throw new NotImplementedException();

        public IEnumerable<IKey> GetKeys()
            => throw new NotImplementedException();

        public PropertyAccessMode GetNavigationAccessMode()
            => throw new NotImplementedException();

        public IEnumerable<INavigation> GetNavigations()
            => throw new NotImplementedException();

        public IEnumerable<IProperty> GetProperties()
            => throw new NotImplementedException();

        public IEnumerable<IProperty> GetFlattenedValueGeneratingProperties()
            => throw new NotImplementedException();

        public PropertyAccessMode GetPropertyAccessMode()
            => throw new NotImplementedException();

        public IReadOnlyDictionary<string, LambdaExpression> GetDeclaredQueryFilters()
            => throw new NotImplementedException();

        public IEnumerable<IForeignKey> GetReferencingForeignKeys()
            => throw new NotImplementedException();

        public IEnumerable<IDictionary<string, object?>> GetSeedData(bool providerValues = false)
            => throw new NotImplementedException();

        public IEnumerable<IServiceProperty> GetServiceProperties()
            => throw new NotImplementedException();

        public Func<MaterializationContext, object> GetOrCreateMaterializer(IStructuralTypeMaterializerSource source)
            => throw new NotImplementedException();

        public Func<MaterializationContext, object> GetOrCreateEmptyMaterializer(IStructuralTypeMaterializerSource source)
            => throw new NotImplementedException();

        public IEnumerable<ISkipNavigation> GetSkipNavigations()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
            => throw new NotImplementedException();

        IReadOnlyNavigation IReadOnlyEntityType.FindDeclaredNavigation(string name)
            => throw new NotImplementedException();

        IReadOnlyProperty IReadOnlyTypeBase.FindDeclaredProperty(string name)
            => throw new NotImplementedException();

        IReadOnlyForeignKey IReadOnlyEntityType.FindForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
            => throw new NotImplementedException();

        IReadOnlyIndex IReadOnlyEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
            => throw new NotImplementedException();

        IReadOnlyIndex IReadOnlyEntityType.FindIndex(string name)
            => throw new NotImplementedException();

        IReadOnlyKey IReadOnlyEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
            => throw new NotImplementedException();

        IReadOnlyKey IReadOnlyEntityType.FindPrimaryKey()
            => throw new NotImplementedException();

        IReadOnlyProperty IReadOnlyTypeBase.FindProperty(string name)
            => throw new NotImplementedException();

        IReadOnlyServiceProperty IReadOnlyEntityType.FindServiceProperty(string name)
            => throw new NotImplementedException();

        IReadOnlySkipNavigation IReadOnlyEntityType.FindSkipNavigation(string name)
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDeclaredForeignKeys()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetDeclaredIndexes()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyKey> IReadOnlyEntityType.GetDeclaredKeys()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetDeclaredNavigations()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyProperty> IReadOnlyTypeBase.GetDeclaredProperties()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDeclaredReferencingForeignKeys()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetDeclaredServiceProperties()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDerivedForeignKeys()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetDerivedIndexes()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDirectlyDerivedTypes()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetForeignKeys()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetIndexes()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyKey> IReadOnlyEntityType.GetKeys()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetNavigations()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyProperty> IReadOnlyTypeBase.GetProperties()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetReferencingForeignKeys()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetServiceProperties()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetSkipNavigations()
            => throw new NotImplementedException();

        IReadOnlyTrigger IReadOnlyEntityType.FindDeclaredTrigger(string name)
            => throw new NotImplementedException();

        ITrigger IEntityType.FindDeclaredTrigger(string name)
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyTrigger> IReadOnlyEntityType.GetDeclaredTriggers()
            => [];

        IEnumerable<ITrigger> IEntityType.GetDeclaredTriggers()
            => [];

        public IComplexProperty FindComplexProperty(string name)
            => throw new NotImplementedException();

        public IEnumerable<IComplexProperty> GetComplexProperties()
            => throw new NotImplementedException();

        public IEnumerable<IComplexProperty> GetDeclaredComplexProperties()
            => throw new NotImplementedException();

        IReadOnlyComplexProperty IReadOnlyTypeBase.FindComplexProperty(string name)
            => throw new NotImplementedException();

        public IReadOnlyComplexProperty FindDeclaredComplexProperty(string name)
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyComplexProperty> IReadOnlyTypeBase.GetComplexProperties()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyComplexProperty> IReadOnlyTypeBase.GetDeclaredComplexProperties()
            => throw new NotImplementedException();

        public IEnumerable<IReadOnlyComplexProperty> GetDerivedComplexProperties()
            => throw new NotImplementedException();

        public IEnumerable<IPropertyBase> GetMembers()
            => throw new NotImplementedException();

        public IEnumerable<IPropertyBase> GetDeclaredMembers()
            => throw new NotImplementedException();

        public IPropertyBase FindMember(string name)
            => throw new NotImplementedException();

        public IEnumerable<IPropertyBase> FindMembersInHierarchy(string name)
            => throw new NotImplementedException();

        public IEnumerable<IPropertyBase> GetSnapshottableMembers()
            => throw new NotImplementedException();

        public IEnumerable<IProperty> GetFlattenedProperties()
            => throw new NotImplementedException();

        public IEnumerable<IComplexProperty> GetFlattenedComplexProperties()
            => throw new NotImplementedException();

        public IEnumerable<IProperty> GetFlattenedDeclaredProperties()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyPropertyBase> IReadOnlyTypeBase.GetMembers()
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyPropertyBase> IReadOnlyTypeBase.GetDeclaredMembers()
            => throw new NotImplementedException();

        IReadOnlyPropertyBase IReadOnlyTypeBase.FindMember(string name)
            => throw new NotImplementedException();

        IEnumerable<IReadOnlyPropertyBase> IReadOnlyTypeBase.FindMembersInHierarchy(string name)
            => throw new NotImplementedException();

        IEnumerable<ITypeBase> ITypeBase.GetDirectlyDerivedTypes()
            => GetDirectlyDerivedTypes();

        IEnumerable<IReadOnlyTypeBase> IReadOnlyTypeBase.GetDerivedTypes()
            => GetDerivedTypes();

        IEnumerable<IReadOnlyTypeBase> IReadOnlyTypeBase.GetDirectlyDerivedTypes()
            => GetDirectlyDerivedTypes();

        IReadOnlyCollection<IQueryFilter> IReadOnlyEntityType.GetDeclaredQueryFilters()
            => throw new NotImplementedException();

        public LambdaExpression? GetQueryFilter()
            => throw new NotImplementedException();

        public IQueryFilter? FindDeclaredQueryFilter(string? filterKey)
            => throw new NotImplementedException();
    }
}
