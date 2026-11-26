// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable CollectionNeverUpdated.Local
namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

public class CosmosClientWrapperTest
{
    private const string PlaceholderDatabaseName = "_";

    [Fact]
    public async Task ToPageAsync_retries_with_new_feed_iterator_from_last_successful_continuation_token()
    {
        RetryingPagingCosmosClientWrapper.Reset();

        using var context = new TestDbContext(
            new DbContextOptionsBuilder()
                .UseCosmos(
                    CosmosTestEnvironment.DefaultConnection,
                    CosmosTestEnvironment.AuthToken,
                    PlaceholderDatabaseName,
                    b => b.ExecutionStrategy(_ => new RetryingExecutionStrategy()))
                .ReplaceService<ICosmosClientWrapper, RetryingPagingCosmosClientWrapper>()
                .Options);

#pragma warning disable EF9102 // Paging is experimental
        var page = await context.Set<Root>()
            .OrderBy(r => r.Id)
            .Select(r => r.Id)
            .ToPageAsync(pageSize: 3, continuationToken: null);
#pragma warning restore EF9102 // Paging is experimental

        Assert.Collection(
            page.Values,
            id => Assert.Equal("A", id),
            id => Assert.Equal("B", id),
            id => Assert.Equal("C", id));
        Assert.Null(page.ContinuationToken);
        Assert.Equal([null, "ct1", "ct1"], RetryingPagingCosmosClientWrapper.CreateQueryContinuationTokens);
    }

    [Theory]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public async Task Query_with_owned_reference_uses_identity_resolution(QueryTrackingBehavior trackingBehavior)
    {
        await using var context = new OwnedQueryContext(
            new DbContextOptionsBuilder()
                .UseCosmos(CosmosTestEnvironment.DefaultConnection, CosmosTestEnvironment.AuthToken, PlaceholderDatabaseName)
                .UseQueryTrackingBehavior(trackingBehavior)
                .ReplaceService<ICosmosClientWrapper, OwnedQueryCosmosClientWrapper>()
                .Options);

        var owners = await context.Set<OwnedQueryOwner>().ToListAsync();

        Assert.Equal(2, owners.Count);
        Assert.Same(owners[0], owners[1]);
        Assert.Equal("owner", owners[0].Id);
        Assert.Equal("owned", owners[0].Owned.Name);
        Assert.Equal(trackingBehavior == QueryTrackingBehavior.TrackAll ? 2 : 0, context.ChangeTracker.Entries().Count());
    }

    [Fact]
    public async Task Tracking_query_projection_returns_added_owned_reference()
    {
        await using var context = new OwnedQueryContext(
            new DbContextOptionsBuilder()
                .UseCosmos(CosmosTestEnvironment.DefaultConnection, CosmosTestEnvironment.AuthToken, PlaceholderDatabaseName)
                .ReplaceService<ICosmosClientWrapper, OwnedQueryCosmosClientWrapper>()
                .Options);

        var owner = new OwnedQueryOwner { Id = "owner" };
        context.Attach(owner);
        var owned = new OwnedQueryReference { Name = "replacement" };
        owner.Owned = owned;
        context.ChangeTracker.DetectChanges();

        var query = context.Set<OwnedQueryOwner>().Select(e => new { e.Id, Owner = e, e.Owned });
        var projection = Assert.Single(await query.ToListAsync());
        Assert.Equal("owner", projection.Id);
        Assert.Same(owner, projection.Owner);
        Assert.Same(owned, projection.Owned);
        Assert.Same(owned, owner.Owned);

        projection = Assert.Single(await query.ToListAsync());
        Assert.Same(owner, projection.Owner);
        Assert.Same(owned, projection.Owned);
        Assert.Equal(EntityState.Added, context.Entry(owned).State);
        var original = Assert.Single(context.ChangeTracker.Entries<OwnedQueryReference>(), e => e.State == EntityState.Deleted);
        Assert.Equal("owned", original.Entity.Name);
    }

    [Fact]
    public async Task Tracking_query_attaches_owner_with_added_owned_reference()
    {
        await using var context = new OwnedQueryContext(
            new DbContextOptionsBuilder()
                .UseCosmos(CosmosTestEnvironment.DefaultConnection, CosmosTestEnvironment.AuthToken, PlaceholderDatabaseName)
                .ReplaceService<ICosmosClientWrapper, OwnedQueryCosmosClientWrapper>()
                .Options);

        var owned = new OwnedQueryReference { Name = "replacement" };
        var ownership = context.Model.FindEntityType(typeof(OwnedQueryReference))!.FindOwnership()!;
        context.Entry(owned).Property(ownership.Properties.Single().Name).CurrentValue = "owner";
        context.Entry(owned).State = EntityState.Added;

        var owners = await context.Set<OwnedQueryOwner>().ToListAsync();
        Assert.Equal(2, owners.Count);
        Assert.Same(owners[0], owners[1]);
        Assert.Same(owned, owners[0].Owned);
        Assert.Equal(EntityState.Added, context.Entry(owned).State);
        var original = Assert.Single(context.ChangeTracker.Entries<OwnedQueryReference>(), e => e.State == EntityState.Deleted);
        Assert.Equal("owned", original.Entity.Name);
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_returns_simple_path_for_scalar_property()
    {
        var entityType = BuildModel().FindEntityType(typeof(Root))!;
        var property = entityType.FindProperty(nameof(Root.Name))!;

        Assert.Equal("/Name", CosmosClientWrapper.GetJsonPropertyPathFromRoot(property));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_quote_escapes_segments_with_special_characters()
    {
        var entityType = BuildModel(eb => eb.Property(e => e.Name).ToJsonProperty("my-name"))
            .FindEntityType(typeof(Root))!;
        var property = entityType.FindProperty(nameof(Root.Name))!;

        Assert.Equal("/\"my-name\"", CosmosClientWrapper.GetJsonPropertyPathFromRoot(property));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_returns_simple_path_for_complex_property_itself()
    {
        var entityType = BuildModel().FindEntityType(typeof(Root))!;
        var owner = entityType.FindComplexProperty(nameof(Root.Owner))!;

        Assert.Equal("/Owner", CosmosClientWrapper.GetJsonPropertyPathFromRoot(owner));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_walks_complex_type_chain()
    {
        var entityType = BuildModel().FindEntityType(typeof(Root))!;
        var nameProperty = entityType.FindComplexProperty(nameof(Root.Owner))!.ComplexType.FindProperty(nameof(Sub.Name))!;

        Assert.Equal("/Owner/Name", CosmosClientWrapper.GetJsonPropertyPathFromRoot(nameProperty));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_returns_collection_path_for_complex_collection_property_itself()
    {
        var entityType = BuildModel().FindEntityType(typeof(Root))!;
        var items = entityType.FindComplexProperty(nameof(Root.Items))!;

        Assert.Equal("/Items/[]", CosmosClientWrapper.GetJsonPropertyPathFromRoot(items));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_walks_complex_collection_chain()
    {
        var entityType = BuildModel().FindEntityType(typeof(Root))!;
        var nameProperty = entityType.FindComplexProperty(nameof(Root.Items))!.ComplexType.FindProperty(nameof(Sub.Name))!;

        Assert.Equal("/Items/[]/Name", CosmosClientWrapper.GetJsonPropertyPathFromRoot(nameProperty));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_escapes_quote_and_backslash_in_complex_property_chain()
    {
        var entityType = BuildModel(eb =>
            {
                eb.ComplexProperty(e => e.Owner).Metadata.SetJsonPropertyName("with\"and\\backslash");
                eb.ComplexProperty(e => e.Owner).Property(s => s.Name).ToJsonProperty("plain");
            })
            .FindEntityType(typeof(Root))!;
        var nameProperty = entityType.FindComplexProperty(nameof(Root.Owner))!.ComplexType.FindProperty(nameof(Sub.Name))!;

        Assert.Equal("/\"with\\\"and\\\\backslash\"/plain", CosmosClientWrapper.GetJsonPropertyPathFromRoot(nameProperty));
    }

    [Fact]
    public void GetJsonPropertyPathFromRoot_escapes_special_chars_in_complex_collection_segment()
    {
        var entityType = BuildModel(eb => eb.ComplexCollection(e => e.Items).Metadata.SetJsonPropertyName("items-list"))
            .FindEntityType(typeof(Root))!;
        var nameProperty = entityType.FindComplexProperty(nameof(Root.Items))!.ComplexType.FindProperty(nameof(Sub.Name))!;

        Assert.Equal("/\"items-list\"/[]/Name", CosmosClientWrapper.GetJsonPropertyPathFromRoot(nameProperty));
    }

    private static IReadOnlyModel BuildModel(Action<EntityTypeBuilder<Root>>? configure = null)
    {
        var modelBuilder = CosmosTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Root>(eb =>
        {
            eb.HasPartitionKey(e => e.PartitionKey);
            eb.ComplexProperty(e => e.Owner);
            eb.ComplexCollection(e => e.Items);
            configure?.Invoke(eb);
        });
        return modelBuilder.FinalizeModel();
    }

    private static ResponseMessage CreateResponseMessage(
        HttpStatusCode statusCode,
        string? content = null,
        string? continuationToken = null)
    {
        var responseMessage = new ResponseMessage(statusCode)
        {
            Content = content is null
                ? Stream.Null
                : new MemoryStream(Encoding.UTF8.GetBytes(content)),
            Diagnostics = new TestCosmosDiagnostics()
        };

        if (continuationToken is not null)
        {
            responseMessage.Headers.Add("x-ms-continuation", continuationToken);
        }

        return responseMessage;
    }

    private sealed class TestFeedIterator(params ResponseMessage[] responses) : FeedIterator
    {
        private readonly Queue<ResponseMessage> _responses = new(responses);

        public int ReadCount { get; private set; }

        public override bool HasMoreResults
            => _responses.Count > 0;

        public override Task<ResponseMessage> ReadNextAsync(CancellationToken cancellationToken = default)
        {
            ReadCount++;
            return Task.FromResult(_responses.Dequeue());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (_responses.Count > 0)
                {
                    _responses.Dequeue().Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }

    private sealed class RetryingPagingCosmosClientWrapper(
        ISingletonCosmosClientWrapper singletonCosmosClientWrapper,
        IDbContextOptions dbContextOptions,
        IExecutionStrategy executionStrategy,
        IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
        : CosmosClientWrapper(singletonCosmosClientWrapper, dbContextOptions, executionStrategy, commandLogger)
    {
        private static int _continuationTokenReadCount;

        public static List<string?> CreateQueryContinuationTokens { get; } = [];

        public static void Reset()
        {
            _continuationTokenReadCount = 0;
            CreateQueryContinuationTokens.Clear();
        }

        public override FeedIterator CreateQuery(
            string containerId,
            CosmosSqlQuery query,
            ISessionTokenStorage sessionTokenStorage,
            string? continuationToken = null,
            QueryRequestOptions? queryRequestOptions = null)
        {
            CreateQueryContinuationTokens.Add(continuationToken);

            return continuationToken switch
            {
                null => new TestFeedIterator(CreateResponseMessage(HttpStatusCode.OK, "{\"Documents\":[\"A\",\"B\"]}", "ct1")),
                "ct1" when _continuationTokenReadCount++ == 0 => new ThrowingFeedIterator(CreateResponseMessage(HttpStatusCode.Gone)),
                "ct1" => new TestFeedIterator(CreateResponseMessage(HttpStatusCode.OK, "{\"Documents\":[\"C\"]}")),
                _ => throw new InvalidOperationException($"Unexpected continuation token '{continuationToken}'.")
            };
        }
    }

    private sealed class OwnedQueryCosmosClientWrapper(
        ISingletonCosmosClientWrapper singletonCosmosClientWrapper,
        IDbContextOptions dbContextOptions,
        IExecutionStrategy executionStrategy,
        IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
        : CosmosClientWrapper(singletonCosmosClientWrapper, dbContextOptions, executionStrategy, commandLogger)
    {
        public override FeedIterator CreateQuery(
            string containerId,
            CosmosSqlQuery query,
            ISessionTokenStorage sessionTokenStorage,
            string? continuationToken = null,
            QueryRequestOptions? queryRequestOptions = null)
            => new TestFeedIterator(
                CreateResponseMessage(
                    HttpStatusCode.OK,
                    query.Query switch
                    {
                        var sql when sql.Contains("\"Owner\" : c", StringComparison.Ordinal) => """
{"Documents":[
  {"Id":"owner","Owner":{"id":"owner","$type":"OwnedQueryOwner","Owned":{"Name":"owned"}},"Owned":{"Name":"owned"}}
]}
""",
                        var sql when sql.StartsWith("SELECT VALUE c", StringComparison.Ordinal) => """
{"Documents":[
  {"id":"owner","$type":"OwnedQueryOwner","Owned":{"Name":"owned"}},
  {"id":"owner","$type":"OwnedQueryOwner","Owned":{"Name":"owned"}}
]}
""",
                        _ => throw new InvalidOperationException($"Unexpected query: {query.Query}")
                    }));
    }

    private sealed class ThrowingFeedIterator(ResponseMessage firstResponse) : FeedIterator
    {
        private bool _hasReturnedResponse;

        public override bool HasMoreResults
            => !_hasReturnedResponse;

        public override Task<ResponseMessage> ReadNextAsync(CancellationToken cancellationToken = default)
        {
            if (_hasReturnedResponse)
            {
                throw new InvalidOperationException("Feed iterator was reused across retry attempts.");
            }

            _hasReturnedResponse = true;
            return Task.FromResult(firstResponse);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_hasReturnedResponse)
            {
                firstResponse.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    private sealed class RetryingExecutionStrategy : IExecutionStrategy
    {
        public bool RetriesOnFailure
            => true;

        public TResult Execute<TState, TResult>(
            TState state,
            Func<DbContext, TState, TResult> operation,
            Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
            => throw new NotSupportedException();

        public async Task<TResult> ExecuteAsync<TState, TResult>(
            TState state,
            Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
            Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
            CancellationToken cancellationToken = default)
        {
            for (var retry = 0; ; retry++)
            {
                try
                {
                    return await operation(null!, state, cancellationToken).ConfigureAwait(false);
                }
                catch (CosmosException e) when (e.StatusCode == HttpStatusCode.Gone && retry == 0)
                {
                }
            }
        }
    }

    private sealed class TestCosmosDiagnostics : CosmosDiagnostics
    {
        public override TimeSpan GetClientElapsedTime()
            => TimeSpan.Zero;

        public override IReadOnlyList<(string regionName, Uri uri)> GetContactedRegions()
            => [];

        public override string ToString()
            => "";
    }

    private sealed class TestDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Root> Roots
            => Set<Root>();
    }

    private sealed class OwnedQueryContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OwnedQueryOwner>().HasPartitionKey(e => e.Id).OwnsOne(e => e.Owned);
        }
    }

    private sealed class OwnedQueryOwner
    {
        public string Id { get; set; } = null!;
        public OwnedQueryReference Owned { get; set; } = null!;
    }

    private sealed class OwnedQueryReference
    {
        public string Name { get; set; } = null!;
    }

    private class Root
    {
        public string Id { get; set; } = null!;
        public string PartitionKey { get; set; } = null!;
        public string Name { get; set; } = null!;
        public Sub Owner { get; set; } = null!;
        public List<Sub> Items { get; set; } = null!;
    }

    private class Sub
    {
        public string Name { get; set; } = null!;
    }
}
