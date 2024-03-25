// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Sockets;
using Azure.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ContainerProperties = Microsoft.Azure.Cosmos.ContainerProperties;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class CosmosTestStore : TestStore
{
    private readonly TestStoreContext _storeContext;
    private readonly string? _dataFilePath;
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
        await testStore.InitializeAsync(null, (Func<DbContext>?)null);
        return testStore;
    }

    public static CosmosTestStore GetOrCreate(string name)
        => new(name);

    public static CosmosTestStore GetOrCreate(string name, string dataFilePath)
        => new(name, dataFilePath: dataFilePath);

    private CosmosTestStore(
        string name,
        bool shared = true,
        string? dataFilePath = null,
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

        if (dataFilePath != null)
        {
            _dataFilePath = Path.Combine(
                Path.GetDirectoryName(typeof(CosmosTestStore).Assembly.Location)!,
                dataFilePath);
        }
    }

    private static string CreateName(string name)
        => TestEnvironment.IsEmulator || name == "Northwind"
            ? name
            : name + _runId;

    public string ConnectionUri { get; }
    public string AuthToken { get; }
    public TokenCredential TokenCredential { get; }
    public string ConnectionString { get; }

    protected override DbContext CreateDefaultContext()
        => new TestStoreContext(this);

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => builder.UseCosmos(
            ConnectionUri,
            AuthToken,
            Name,
            _configureCosmos);

    public static async ValueTask<bool> IsConnectionAvailableAsync()
    {
        if (_connectionAvailable == null)
        {
            _connectionAvailable = await TryConnectAsync();
        }

        return _connectionAvailable.Value;
    }

    private static async Task<bool> TryConnectAsync()
    {
        CosmosTestStore? testStore = null;
        try
        {
            testStore = await CreateInitializedAsync("NonExistent");

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
                await testStore.DisposeAsync();
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
        _initialized = true;

        if (_connectionAvailable == false)
        {
            return;
        }

        if (_dataFilePath == null)
        {
            await base.InitializeAsync(createContext ?? (() => _storeContext), seed, clean);
        }
        else
        {
            using var context = createContext();
            await CreateFromFile(context);
        }
    }

    private async Task CreateFromFile(DbContext context)
    {
        if (await context.Database.EnsureCreatedAsync())
        {
            var cosmosClient = context.GetService<ICosmosClientWrapper>();
            var serializer = CosmosClientWrapper.Serializer;
            using var fs = new FileStream(_dataFilePath!, FileMode.Open, FileAccess.Read);
            using var sr = new StreamReader(fs);
            using var reader = new JsonTextReader(sr);
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    NextEntityType:
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            string? entityName = null;
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonToken.PropertyName)
                                {
                                    switch (reader.Value)
                                    {
                                        case "Name":
                                            reader.Read();
                                            entityName = (string)reader.Value;
                                            break;
                                        case "Data":
                                            while (reader.Read())
                                            {
                                                if (reader.TokenType == JsonToken.StartObject)
                                                {
                                                    var document = serializer.Deserialize<JObject>(reader)!;

                                                    document["id"] = $"{entityName}|{document["id"]}";
                                                    document["Discriminator"] = entityName;

                                                    await cosmosClient.CreateItemAsync(
                                                        "NorthwindContext", document, new FakeUpdateEntry());
                                                }
                                                else if (reader.TokenType == JsonToken.EndObject)
                                                {
                                                    goto NextEntityType;
                                                }
                                            }

                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public override async Task CleanAsync(DbContext context)
    {
        var cosmosClientWrapper = context.GetService<ICosmosClientWrapper>();
        var created = await cosmosClientWrapper.CreateDatabaseIfNotExistsAsync(null);
        try
        {
            if (!created)
            {
                var cosmosClient = context.Database.GetCosmosClient();
                var database = cosmosClient.GetDatabase(Name);
                var containerIterator = database.GetContainerQueryIterator<ContainerProperties>();
                while (containerIterator.HasMoreResults)
                {
                    foreach (var containerProperties in await containerIterator.ReadNextAsync())
                    {
                        var container = database.GetContainer(containerProperties.Id);
                        var partitionKey = containerProperties.PartitionKeyPath[1..];
                        var itemIterator = container.GetItemQueryIterator<JObject>(
                            new QueryDefinition("SELECT * FROM c"));

                        var items = new List<(string Id, string PartitionKey)>();
                        while (itemIterator.HasMoreResults)
                        {
                            foreach (var item in await itemIterator.ReadNextAsync())
                            {
                                items.Add((item["id"]!.ToString(), item[partitionKey]?.ToString()!));
                            }
                        }

                        foreach (var item in items)
                        {
                            await container.DeleteItemAsync<object>(
                                item.Id,
                                item.PartitionKey == null ? PartitionKey.None : new PartitionKey(item.PartitionKey));
                        }
                    }
                }

                created = await context.Database.EnsureCreatedAsync();
                if (!created)
                {
                    var creator = (CosmosDatabaseCreator)context.GetService<IDatabaseCreator>();
                    await creator.SeedAsync();
                }
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }
        }
        catch (Exception)
        {
            try
            {
                await context.Database.EnsureDeletedAsync();
            }
            catch (Exception)
            {
            }

            throw;
        }
    }

    public override void Dispose()
        => throw new InvalidOperationException("Calling Dispose can cause deadlocks. Use DisposeAsync instead.");

    public override async Task DisposeAsync()
    {
        if (_initialized
            && _dataFilePath == null)
        {
            if (_connectionAvailable == false)
            {
                return;
            }

            if (Shared)
            {
                GetTestStoreIndex(ServiceProvider).RemoveShared(GetType().Name + Name);
            }

            await _storeContext.Database.EnsureDeletedAsync();
        }

        _storeContext.Dispose();
    }

    private class TestStoreContext(CosmosTestStore testStore) : DbContext
    {
        private readonly CosmosTestStore _testStore = testStore;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(_testStore.ConnectionUri, _testStore.AuthToken, _testStore.Name, _testStore._configureCosmos);
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

        public object GetOriginalOrCurrentValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public object GetOriginalValue(IPropertyBase propertyBase)
            => throw new NotImplementedException();

        public TProperty GetOriginalValue<TProperty>(IProperty property)
            => throw new NotImplementedException();

        public bool HasTemporaryValue(IProperty property)
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
    }

    public class FakeEntityType : Annotatable, IEntityType
    {
        public IEntityType BaseType
            => throw new NotImplementedException();

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
            => throw new NotImplementedException();

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

        public PropertyAccessMode GetPropertyAccessMode()
            => throw new NotImplementedException();

        public LambdaExpression GetQueryFilter()
            => throw new NotImplementedException();

        public IEnumerable<IForeignKey> GetReferencingForeignKeys()
            => throw new NotImplementedException();

        public IEnumerable<IDictionary<string, object?>> GetSeedData(bool providerValues = false)
            => throw new NotImplementedException();

        public IEnumerable<IServiceProperty> GetServiceProperties()
            => throw new NotImplementedException();

        public Func<MaterializationContext, object> GetOrCreateMaterializer(IEntityMaterializerSource source)
            => throw new NotImplementedException();

        public Func<MaterializationContext, object> GetOrCreateEmptyMaterializer(IEntityMaterializerSource source)
            => throw new NotImplementedException();

        public IEnumerable<ISkipNavigation> GetSkipNavigations()
            => throw new NotImplementedException();

        public IEnumerable<IProperty> GetValueGeneratingProperties()
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
            => throw new NotImplementedException();

        IEnumerable<ITrigger> IEntityType.GetDeclaredTriggers()
            => throw new NotImplementedException();

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
    }
}
