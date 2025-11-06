// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

public abstract class CosmosSessionTokensTestBase(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected const string OtherContainerName = "Other";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    protected override string StoreName => nameof(CosmosSessionTokensTestBase);

    protected abstract SessionTokenManagementMode Mode { get; }

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder) => base.AddOptions(builder).ConfigureWarnings(x => x.Ignore(CosmosEventId.SyncNotSupported));

    protected override TestStore CreateTestStore() => CosmosTestStore.Create(StoreName, (c) => c.SessionTokenManagementMode(Mode));

    public class CosmosSessionTokenContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<OtherContainerCustomer> OtherContainerCustomers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Customer>(
                b =>
                {
                    b.HasKey(c => c.Id);
                    b.Property(c => c.ETag).IsETagConcurrency();
                    b.OwnsMany(x => x.Children);
                    b.HasPartitionKey(c => c.PartitionKey);
                });

            builder.Entity<OtherContainerCustomer>(
                b =>
                {
                    b.HasKey(c => c.Id);
                    b.HasPartitionKey(c => c.PartitionKey);
                    b.ToContainer(OtherContainerName);
                });
        }
    }

    public class Customer
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? ETag { get; set; }

        public string? PartitionKey { get; set; }

        public ICollection<DummyChild> Children { get; } = new HashSet<DummyChild>();
    }

    public class DummyChild
    {
        public string? Id { get; init; }
    }

    public class OtherContainerCustomer
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? PartitionKey { get; set; }
    }
}

public abstract class CosmosSessionTokensNonFullyAutomaticTestBase(NonSharedFixture fixture) : CosmosSessionTokensTestBase(fixture)
{
    [ConditionalFact]
    public virtual async Task AppendSessionTokens_throws_for_non_existent_container()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var exception = Assert.Throws<InvalidOperationException>(() => context.Database.AppendSessionTokens(new Dictionary<string, string> { { OtherContainerName, "0:-1#231"}, { "Not the container name", "0:-1#231" } }));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("Not the container name"), exception.Message);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionToken_no_token_sets_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        IReadOnlyDictionary<string, string> sessionTokens;
        context.Database.AppendSessionToken("0:-1#231");
        sessionTokens = context.Database.GetSessionTokens();

        Assert.Equal("0:-1#231", sessionTokens[nameof(CosmosSessionTokenContext)]);
        Assert.Equal(nameof(CosmosSessionTokenContext), sessionTokens.First().Key);
        Assert.Equal(sessionTokens[nameof(CosmosSessionTokenContext)], sessionTokens.First().Value);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionTokens_no_tokens_sets_tokens()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        IReadOnlyDictionary<string, string> sessionTokens;

        context.Database.AppendSessionTokens(new Dictionary<string, string> { { OtherContainerName, "0:-1#123" }, { nameof(CosmosSessionTokenContext), "0:-1#231" } });
        sessionTokens = context.Database.GetSessionTokens();

        Assert.Equal("0:-1#123", sessionTokens[OtherContainerName]);
        Assert.Equal("0:-1#231", sessionTokens[nameof(CosmosSessionTokenContext)]);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionToken_append_token_already_present_does_not_add_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var initialToken = context.Database.GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(initialToken));
        context.Database.AppendSessionToken(initialToken);

        var updatedToken = context.Database.GetSessionToken();

        Assert.Equal(initialToken, updatedToken);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionTokens_append_token_already_present_does_not_add_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var initialTokens = context.Database.GetSessionTokens();
        context.Database.AppendSessionTokens(initialTokens!);

        var updatedTokens = context.Database.GetSessionTokens();

        foreach (var pair in updatedTokens)
        {
            Assert.Equal(initialTokens[pair.Key], pair.Value);
        }
    }

    [ConditionalFact]
    public virtual async Task AppendSessionToken_multiple_tokens_splits_tokens()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var sessionTokens = context.Database.GetSessionTokens();
        var newToken = "0:-1#123,1:-1#456";
        var appendix = "0:-1#123";
        context.Database.AppendSessionToken(newToken);
        context.Database.AppendSessionToken(appendix);

        var updatedToken = context.Database.GetSessionToken();

        Assert.Equal(newToken, updatedToken);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionTokens_multiple_tokens_splits_tokens()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var sessionTokens = context.Database.GetSessionTokens();
        var newToken = "0:-1#123,1:-1#456";
        var appendix = "0:-1#123";

        context.Database.AppendSessionTokens(new Dictionary<string, string> { { OtherContainerName, newToken }, { nameof(CosmosSessionTokenContext), newToken } });
        context.Database.AppendSessionTokens(new Dictionary<string, string> { { OtherContainerName, appendix }, { nameof(CosmosSessionTokenContext), appendix } });

        var updatedTokens = context.Database.GetSessionTokens();

        foreach (var pair in updatedTokens)
        {
            Assert.Equal(newToken, pair.Value);
        }
    }

    [ConditionalFact]
    public virtual async Task UseSessionToken_sets_tokens()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var sessionTokens = context.Database.GetSessionTokens();
        var newToken = "0:-1#123,1:-1#456";
        var overwrite = "0:-1#123";

        context.Database.UseSessionToken(newToken);
        context.Database.UseSessionToken(overwrite);

        var updatedToken = context.Database.GetSessionToken();
        Assert.Equal(overwrite, updatedToken);
    }

    [ConditionalFact]
    public virtual async Task UseSessionToken_multiple_tokens_splits_tokens()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var sessionTokens = context.Database.GetSessionTokens();
        var newToken = "0:-1#123,1:-1#456";
        var appendix = "0:-1#123";

        context.Database.UseSessionToken(newToken);
        context.Database.AppendSessionToken(appendix);

        var updatedToken = context.Database.GetSessionToken();
        Assert.Equal(newToken, updatedToken);
    }

    [ConditionalFact]
    public virtual async Task UseSessionTokens_sets_tokens()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var sessionTokens = context.Database.GetSessionTokens();
        var newToken = "0:-1#123,1:-1#456";
        var overwrite = "0:-1#123";

        context.Database.UseSessionTokens(new Dictionary<string, string> { { OtherContainerName, newToken }, { nameof(CosmosSessionTokenContext), newToken } });
        context.Database.UseSessionTokens(new Dictionary<string, string> { { OtherContainerName, overwrite }, { nameof(CosmosSessionTokenContext), overwrite } });

        var updatedTokens = context.Database.GetSessionTokens();

        foreach (var pair in updatedTokens)
        {
            Assert.Equal(overwrite, pair.Value);
        }
    }

    [ConditionalFact]
    public virtual async Task UseSessionTokens_multiple_tokens_splits_tokens()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var sessionTokens = context.Database.GetSessionTokens();
        var newToken = "0:-1#123,1:-1#456";
        var appendix = "0:-1#123";

        context.Database.UseSessionTokens(new Dictionary<string, string> { { OtherContainerName, newToken }, { nameof(CosmosSessionTokenContext), newToken } });
        context.Database.AppendSessionTokens(new Dictionary<string, string> { { OtherContainerName, appendix }, { nameof(CosmosSessionTokenContext), appendix } });

        var updatedTokens = context.Database.GetSessionTokens();

        foreach (var pair in updatedTokens)
        {
            Assert.Equal(newToken, pair.Value);
        }
    }

    [ConditionalFact]
    public virtual async Task GetSessionTokens_no_token_returns_empty()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        Assert.Equal(0, sessionTokens.Count);
    }

    [ConditionalFact]
    public virtual async Task GetSessionToken_no_token_returns_null()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();
        var sessionToken = context.Database.GetSessionToken();
        Assert.Null(sessionToken);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Query_uses_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();
        
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();

        // Only way we can test this is by setting a session token that will fail the request if used..
        var newTokens = sessionTokens.ToDictionary(x => x.Key, x => "invalidtoken");

        context.Database.AppendSessionTokens(newTokens);
        
        CosmosException ex1;
        CosmosException ex2;

        if (async)
        {
            ex1 = await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToListAsync());
            ex2 = await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToListAsync());
        }
        else
        {
            ex1 = Assert.Throws<CosmosException>(() => context.Customers.ToList());
            ex2 = Assert.Throws<CosmosException>(() => context.OtherContainerCustomers.ToList());
        }

        Assert.Contains("The session token provided 'invalidtoken' is invalid", ex1.ResponseBody);
        Assert.Contains("The session token provided 'invalidtoken' is invalid", ex2.ResponseBody);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Query_on_new_context_does_not_use_same_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();

        // Only way we can test this is by setting a session token that will fail the request if used..
        var newTokens = sessionTokens.ToDictionary(x => x.Key, x => "invalidtoken");

        context.Database.AppendSessionTokens(newTokens);

        if (async)
        {
            await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToListAsync());
            await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToListAsync());
        }
        else
        {
            Assert.Throws<CosmosException>(() => context.Customers.ToList());
            Assert.Throws<CosmosException>(() => context.OtherContainerCustomers.ToList());
        }

        using var newContext = contextFactory.CreateContext();
        Assert.NotSame(context, newContext);
        if (async)
        {
            await newContext.Customers.ToListAsync();
            await newContext.OtherContainerCustomers.ToListAsync();
        }
        else
        {
            newContext.Customers.ToList();
            newContext.OtherContainerCustomers.ToList();
        }
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Query_on_same_newly_pooled_context_does_not_use_same_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        DbContext contextCopy;
        using (var context = contextFactory.CreateContext())
        {
            contextCopy = context;
            context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
            context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

            if (async)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                context.SaveChanges();
            }

            var sessionTokens = context.Database.GetSessionTokens();

            // Only way we can test this is by setting a session token that will fail the request if used..
            var newTokens = sessionTokens.ToDictionary(x => x.Key, x => "invalidtoken");

            context.Database.AppendSessionTokens(newTokens);

            if (async)
            {
                await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToListAsync());
                await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToListAsync());
            }
            else
            {
                Assert.Throws<CosmosException>(() => context.Customers.ToList());
                Assert.Throws<CosmosException>(() => context.OtherContainerCustomers.ToList());
            }
        }

        using var newContext = contextFactory.CreateContext();
        Assert.Same(newContext, contextCopy);
        await newContext.Customers.ToListAsync();
        await newContext.OtherContainerCustomers.ToListAsync();
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task PagingQuery_uses_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();

        // Only way we can test this is by setting a session token that will fail the request if used..
        var newTokens = sessionTokens.ToDictionary(x => x.Key, x => "invalidtoken");

        context.Database.AppendSessionTokens(newTokens);

        var ex1 = await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToPageAsync(1, null));
        var ex2 = await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToPageAsync(1, null));

        Assert.Contains("The session token provided 'invalidtoken' is invalid", ex1.ResponseBody);
        Assert.Contains("The session token provided 'invalidtoken' is invalid", ex2.ResponseBody);

    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Shaped_query_uses_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();

        // Only way we can test this is by setting a session token that will fail the request if used..
        var newTokens = sessionTokens.ToDictionary(x => x.Key, x => "invalidtoken");

        context.Database.AppendSessionTokens(newTokens);

        CosmosException ex1;
        CosmosException ex2;
        if (async)
        {
            ex1 = await Assert.ThrowsAsync<CosmosException>(() => context.Customers.Select(x => new { x.Id, x.PartitionKey }).ToListAsync());
            ex2 = await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.Select(x => new { x.Id, x.PartitionKey }).ToListAsync());
        }
        else
        {
            ex1 = Assert.Throws<CosmosException>(() => context.Customers.Select(x => new { x.Id, x.PartitionKey }).ToList());
            ex2 = Assert.Throws<CosmosException>(() => context.OtherContainerCustomers.Select(x => new { x.Id, x.PartitionKey }).ToList());
        }

        Assert.Contains("The session token provided 'invalidtoken' is invalid", ex1.ResponseBody);
        Assert.Contains("The session token provided 'invalidtoken' is invalid", ex2.ResponseBody);

    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Read_item_uses_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();

        // Only way we can test this is by setting a session token that will fail the request if used..
        var newTokens = sessionTokens.ToDictionary(x => x.Key, x => "invalidtoken");

        context.Database.AppendSessionTokens(newTokens);

        CosmosException ex1;
        CosmosException ex2;
        if (async)
        {
            ex1 = await Assert.ThrowsAsync<CosmosException>(() => context.Customers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1"));
            ex2 = await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1"));
        }
        else
        {
            ex1 = Assert.Throws<CosmosException>(() => context.Customers.FirstOrDefault(x => x.Id == "1" && x.PartitionKey == "1"));
            ex2 = Assert.Throws<CosmosException>(() => context.OtherContainerCustomers.FirstOrDefault(x => x.Id == "1" && x.PartitionKey == "1"));
        }

        Assert.Contains("The session token provided 'invalidtoken' is invalid", ex1.ResponseBody);
        Assert.Contains("The session token provided 'invalidtoken' is invalid", ex2.ResponseBody);

    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Query_sets_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (async)
        {
            await context.Customers.ToListAsync();
            await context.OtherContainerCustomers.ToListAsync();
        }
        else
        {
            _ = context.Customers.ToList();
            _ = context.OtherContainerCustomers.ToList();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        Assert.False(string.IsNullOrWhiteSpace(sessionTokens.First().Value));
        Assert.False(string.IsNullOrWhiteSpace(sessionTokens.Last().Value));
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Query_appends_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (async)
        {
            await context.Customers.ToListAsync();
            await context.OtherContainerCustomers.ToListAsync();
        }
        else
        {
            _ = context.Customers.ToList();
            _ = context.OtherContainerCustomers.ToList();
        }

        var initialTokens = context.Database.GetSessionTokens();

        using var otherContext = contextFactory.CreateContext();
        otherContext.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        otherContext.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        if (async)
        {
            await otherContext.SaveChangesAsync();
        }
        else
        {
            otherContext.SaveChanges();
        }

        var otherTokens = otherContext.Database.GetSessionTokens();

        if (async)
        {
            await context.Customers.ToListAsync();
            await context.OtherContainerCustomers.ToListAsync();
        }
        else
        {
            _ = context.Customers.ToList();
            _ = context.OtherContainerCustomers.ToList();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        foreach (var token in sessionTokens)
        {
            var initialToken = initialTokens[token.Key];
            var otherToken = otherTokens[token.Key];
            Assert.Equal(initialToken + "," + otherToken, token.Value);
        }
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Query_same_session_does_not_append_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (async)
        {
            await context.Customers.ToListAsync();
            await context.OtherContainerCustomers.ToListAsync();
        }
        else
        {
            _ = context.Customers.ToList();
            _ = context.OtherContainerCustomers.ToList();
        }

        var initialTokens = context.Database.GetSessionTokens();

        if (async)
        {
            await context.Customers.ToListAsync();
            await context.OtherContainerCustomers.ToListAsync();
        }
        else
        {
            _ = context.Customers.ToList();
            _ = context.OtherContainerCustomers.ToList();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        foreach (var token in sessionTokens)
        {
            var initialToken = initialTokens[token.Key];
            Assert.Equal(initialToken, token.Value);
        }
    }

    [ConditionalFact]
    public virtual async Task PagingQuery_appends_session_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        await context.Customers.ToPageAsync(1, null);
        await context.OtherContainerCustomers.ToPageAsync(1, null);

        var initialTokens = context.Database.GetSessionTokens();

        using var otherContext = contextFactory.CreateContext();
        otherContext.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        otherContext.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        await otherContext.SaveChangesAsync();
        otherContext.SaveChanges();

        var otherTokens = otherContext.Database.GetSessionTokens();

        await context.Customers.ToPageAsync(1, null);
        await context.OtherContainerCustomers.ToPageAsync(1, null);

        var sessionTokens = context.Database.GetSessionTokens();
        foreach (var token in sessionTokens)
        {
            var initialToken = initialTokens[token.Key];
            var otherToken = otherTokens[token.Key];
            Assert.Equal(initialToken + "," + otherToken, token.Value);
        }
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Read_item_appends_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (async)
        {
            await context.Customers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1");
            await context.OtherContainerCustomers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1");
        }
        else
        {
            _ = context.Customers.ToList();
            _ = context.OtherContainerCustomers.ToList();
        }

        var initialTokens = context.Database.GetSessionTokens();

        using var otherContext = contextFactory.CreateContext();
        otherContext.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        otherContext.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        if (async)
        {
            await otherContext.SaveChangesAsync();
        }
        else
        {
            otherContext.SaveChanges();
        }

        var otherTokens = otherContext.Database.GetSessionTokens();

        if (async)
        {
            await context.Customers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1");
            await context.OtherContainerCustomers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1");
        }
        else
        {
            _ = context.Customers.ToList();
            _ = context.OtherContainerCustomers.ToList();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        foreach (var token in sessionTokens)
        {
            var initialToken = initialTokens[token.Key];
            var otherToken = otherTokens[token.Key];
            Assert.Equal(initialToken + "," + otherToken, token.Value);
        }
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task Read_item_enumerable_sets_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (async)
        {
            await context.Customers.Where(x => x.Id == "1" && x.PartitionKey == "1").ToListAsync();
            await context.OtherContainerCustomers.Where(x => x.Id == "1" && x.PartitionKey == "1").ToListAsync();
        }
        else
        {
            _ = context.Customers.ToList();
            _ = context.OtherContainerCustomers.ToList();
        }

        var initialTokens = context.Database.GetSessionTokens();

        using var otherContext = contextFactory.CreateContext();
        otherContext.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        otherContext.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        if (async)
        {
            await otherContext.SaveChangesAsync();
        }
        else
        {
            otherContext.SaveChanges();
        }

        var otherTokens = otherContext.Database.GetSessionTokens();

        if (async)
        {
            await context.Customers.Where(x => x.Id == "1" && x.PartitionKey == "1").ToListAsync();
            await context.OtherContainerCustomers.Where(x => x.Id == "1" && x.PartitionKey == "1").ToListAsync();
        }
        else
        {
            _ = context.Customers.ToList();
            _ = context.OtherContainerCustomers.ToList();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        foreach (var token in sessionTokens)
        {
            var initialToken = initialTokens[token.Key];
            var otherToken = otherTokens[token.Key];
            Assert.Equal(initialToken + "," + otherToken, token.Value);
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Add_AutoTransactionBehavior_never_sets_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        foreach (var sessionToken in sessionTokens.Values)
        {
            Assert.False(string.IsNullOrWhiteSpace(sessionToken));
        }
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Add_AutoTransactionBehavior_always_sets_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;
        if (defaultContainer)
        {
            context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = defaultContainer ? sessionTokens[nameof(CosmosSessionTokenContext)]! : sessionTokens[OtherContainerName]!;
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Add_never_merges_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var initialTokens = context.Database.GetSessionTokens();

        context.Customers.Add(new Customer { Id = "2", PartitionKey = "1" });
        context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "2", PartitionKey = "1" });

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        foreach (var sessionToken in sessionTokens)
        {
            var initialToken = initialTokens[sessionToken.Key];
            Assert.NotEqual(sessionToken.Value, initialToken);
            Assert.StartsWith(initialToken + ",", sessionToken.Value);
            Assert.False(string.IsNullOrWhiteSpace(sessionToken.Value!.Substring(sessionToken.Value.IndexOf(",") + 1)));
        }
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Add_always_merges_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        if (defaultContainer)
        {
            context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var initialTokens = context.Database.GetSessionTokens();

        if (defaultContainer)
        {
            context.Customers.Add(new Customer { Id = "2", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "2", PartitionKey = "1" });
        }

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = defaultContainer ? sessionTokens[nameof(CosmosSessionTokenContext)]! : sessionTokens[OtherContainerName]!;
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Delete_never_merges_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

        var customer = new Customer { Id = "1", PartitionKey = "1" };
        var otherContainerCustomer = new OtherContainerCustomer { Id = "1", PartitionKey = "1" };
        context.Customers.Add(customer);
        context.OtherContainerCustomers.Add(otherContainerCustomer);

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var initialTokens = context.Database.GetSessionTokens();

        context.Customers.Remove(customer);
        context.OtherContainerCustomers.Remove(otherContainerCustomer);

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        foreach (var sessionToken in sessionTokens)
        {
            var initialToken = initialTokens[sessionToken.Key];
            Assert.NotEqual(sessionToken.Value, initialToken);
            Assert.StartsWith(initialToken + ",", sessionToken.Value);
            Assert.False(string.IsNullOrWhiteSpace(sessionToken.Value!.Substring(sessionToken.Value.IndexOf(",") + 1)));
        }
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Delete_always_merges_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        if (defaultContainer)
        {
            context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        context.ChangeTracker.Clear();
        var initialTokens = context.Database.GetSessionTokens();

        if (defaultContainer)
        {
            context.Customers.Remove(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Remove(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = defaultContainer ? sessionTokens[nameof(CosmosSessionTokenContext)]! : sessionTokens[OtherContainerName]!;
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Update_never_merges_session_token(bool async)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

        var customer = new Customer { Id = "1", PartitionKey = "1" };
        var otherContainerCustomer = new OtherContainerCustomer { Id = "1", PartitionKey = "1" };
        context.Customers.Add(customer);
        context.OtherContainerCustomers.Add(otherContainerCustomer);

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var initialTokens = context.Database.GetSessionTokens();

        customer.Name = "updated";
        otherContainerCustomer.Name = "updated";

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        foreach (var sessionToken in sessionTokens)
        {
            var initialToken = initialTokens[sessionToken.Key];
            Assert.NotEqual(sessionToken.Value, initialToken);
            Assert.StartsWith(initialToken + ",", sessionToken.Value);
            Assert.False(string.IsNullOrWhiteSpace(sessionToken.Value!.Substring(sessionToken.Value.IndexOf(",") + 1)));
        }
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Update_always_merges_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        if (defaultContainer)
        {
            context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        context.ChangeTracker.Clear();
        var initialTokens = context.Database.GetSessionTokens();

        if (defaultContainer)
        {
            context.Customers.Update(new Customer { Id = "1", Name = "updated", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Update(new OtherContainerCustomer { Id = "1", Name = "updated", PartitionKey = "1" });
        }

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = defaultContainer ? sessionTokens[nameof(CosmosSessionTokenContext)]! : sessionTokens[OtherContainerName]!;
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
    }

    [ConditionalTheory]
    [InlineData(AutoTransactionBehavior.WhenNeeded, true, true)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, true, false)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, false, true)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, false, false)]
    [InlineData(AutoTransactionBehavior.Never, true, true)]
    [InlineData(AutoTransactionBehavior.Never, true, false)]
    [InlineData(AutoTransactionBehavior.Never, false, true)]
    [InlineData(AutoTransactionBehavior.Never, false, false)]
    [InlineData(AutoTransactionBehavior.Always, true, true)]
    [InlineData(AutoTransactionBehavior.Always, true, false)]
    [InlineData(AutoTransactionBehavior.Always, false, true)]
    [InlineData(AutoTransactionBehavior.Always, false, false)]
    public virtual async Task Add_uses_session_token(AutoTransactionBehavior autoTransactionBehavior, bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        var sessionTokens = context.Database.GetSessionTokens();
        // Only way we can test this is by setting a session token that will fail the request if used..
        // Only way to do this for a write is to set an invalid session token..

        if (defaultContainer)
        {
            context.Database.AppendSessionToken("invalidtoken");
        }
        else
        {
            context.Database.AppendSessionTokens(new Dictionary<string, string> { { OtherContainerName, "invalidtoken" } });
        }

        if (defaultContainer)
        {
            context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        DbUpdateException ex;
        if (async)
        {
            ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
        else
        {
            ex = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        }

        Assert.Contains("The session token provided 'invalidtoken' is invalid.", ((CosmosException)ex.InnerException!).ResponseBody);
    }

    [ConditionalTheory]
    [InlineData(AutoTransactionBehavior.WhenNeeded, true, true)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, true, false)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, false, true)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, false, false)]
    [InlineData(AutoTransactionBehavior.Never, true, true)]
    [InlineData(AutoTransactionBehavior.Never, true, false)]
    [InlineData(AutoTransactionBehavior.Never, false, true)]
    [InlineData(AutoTransactionBehavior.Never, false, false)]
    [InlineData(AutoTransactionBehavior.Always, true, true)]
    [InlineData(AutoTransactionBehavior.Always, true, false)]
    [InlineData(AutoTransactionBehavior.Always, false, true)]
    [InlineData(AutoTransactionBehavior.Always, false, false)]
    public virtual async Task Update_uses_session_token(AutoTransactionBehavior autoTransactionBehavior, bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        var sessionTokens = context.Database.GetSessionTokens();
        // Only way we can test this is by setting a session token that will fail the request if used..
        // Only way to do this for a write is to set an invalid session token..
        if (defaultContainer)
        {
            context.Database.AppendSessionToken("invalidtoken");
        }
        else
        {
            context.Database.AppendSessionTokens(new Dictionary<string, string> { { OtherContainerName, "invalidtoken" } });
        }

        if (defaultContainer)
        {
            context.Customers.Update(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Update(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        DbUpdateException ex;
        if (async)
        {
            ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
        else
        {
            ex = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        }

        Assert.Contains("The session token provided 'invalidtoken' is invalid.", ((CosmosException)ex.InnerException!).ResponseBody);
    }

    [ConditionalTheory]
    [InlineData(AutoTransactionBehavior.WhenNeeded, true, true)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, true, false)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, false, true)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, false, false)]
    [InlineData(AutoTransactionBehavior.Never, true, true)]
    [InlineData(AutoTransactionBehavior.Never, true, false)]
    [InlineData(AutoTransactionBehavior.Never, false, true)]
    [InlineData(AutoTransactionBehavior.Never, false, false)]
    [InlineData(AutoTransactionBehavior.Always, true, true)]
    [InlineData(AutoTransactionBehavior.Always, true, false)]
    [InlineData(AutoTransactionBehavior.Always, false, true)]
    [InlineData(AutoTransactionBehavior.Always, false, false)]
    public virtual async Task Delete_uses_session_token(AutoTransactionBehavior autoTransactionBehavior, bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        var sessionTokens = context.Database.GetSessionTokens();
        // Only way we can test this is by setting a session token that will fail the request if used..
        // Only way to do this for a write is to set an invalid session token..
        if (defaultContainer)
        {
            context.Database.AppendSessionToken("invalidtoken");
        }
        else
        {
            context.Database.AppendSessionTokens(new Dictionary<string, string> { { OtherContainerName, "invalidtoken" } });
        }

        if (defaultContainer)
        {
            context.Customers.Remove(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Remove(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        DbUpdateException ex;
        if (async)
        {
            ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
        else
        {
            ex = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        }

        Assert.Contains("The session token provided 'invalidtoken' is invalid.", ((CosmosException)ex.InnerException!).ResponseBody);
    }
}
