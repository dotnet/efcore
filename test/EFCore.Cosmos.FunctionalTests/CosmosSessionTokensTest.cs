// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage;

namespace Microsoft.EntityFrameworkCore;

public class CosmosSessionTokensTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    private const string OtherContainerName = "Other";
    protected override string StoreName
        => "CosmosSessionTokensTest";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual async Task SetSessionToken_ThrowsForNonExistentContainer()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        var exception = Assert.Throws<ArgumentException>(() => sessionTokens.SetSessionToken("Not the store name", "0:-1#231"));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("Not the store name") + " (Parameter 'containerName')", exception.Message);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionToken_ThrowsForNonExistentContainer()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        var exception = Assert.Throws<ArgumentException>(() => sessionTokens.AppendSessionToken("Not the store name", "0:-1#231"));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("Not the store name") + " (Parameter 'containerName')", exception.Message);
    }

    [ConditionalFact]
    public virtual async Task GetSessionToken_ThrowsForNonExistentContainer()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        var exception = Assert.Throws<ArgumentException>(() => sessionTokens.GetSessionToken("Not the store name"));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("Not the store name") + " (Parameter 'containerName')", exception.Message);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionToken_no_tokens_sets_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var sessionTokens = context.Database.GetSessionTokens();
        sessionTokens.AppendSessionToken("0:-1#231");
        var updatedToken = sessionTokens.GetSessionToken();

        Assert.Equal("0:-1#231", updatedToken);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionToken_append_higher_lsn_same_pkrange_takes_higher_lsn()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = sessionTokens.GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        var newToken = initialToken.Substring(0, initialToken.IndexOf('#') + 1) + "999999";
        sessionTokens.AppendSessionToken(newToken);

        var updatedToken = sessionTokens.GetSessionToken();

        Assert.Equal(newToken, updatedToken);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionToken_append_lower_lsn_same_pkrange_takes_higher_lsn()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = sessionTokens.GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        var newToken = initialToken.Substring(0, initialToken.IndexOf('#') + 1) + "1";
        sessionTokens.AppendSessionToken(newToken);

        var updatedToken = sessionTokens.GetSessionToken();

        Assert.Equal(initialToken, updatedToken);
    }

    [ConditionalTheory, InlineData(AutoTransactionBehavior.WhenNeeded), InlineData(AutoTransactionBehavior.Never), InlineData(AutoTransactionBehavior.Always)]
    public virtual async Task AppendSessionToken_different_pkrange_composites_tokens(AutoTransactionBehavior autoTransactionBehavior)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = sessionTokens.GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        sessionTokens.AppendSessionToken("99:-1#999999");

        var updatedToken = sessionTokens.GetSessionToken();

        Assert.Equal(initialToken + ",99:-1#999999", updatedToken);
    }

    [ConditionalFact]
    public virtual async Task SetSessionToken_does_not_merge_session_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = sessionTokens.GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        sessionTokens.SetSessionToken("0:-1#1");

        var updatedToken = sessionTokens.GetSessionToken();

        Assert.Equal("0:-1#1", updatedToken);
    }

    [ConditionalFact]
    public virtual async Task SetSessionToken_null_sets_session_token_null()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = sessionTokens.GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        sessionTokens.SetSessionToken(null);

        var updatedToken = sessionTokens.GetSessionToken();

        Assert.Null(updatedToken);
    }

    [ConditionalFact]
    public virtual async Task GetSessionToken_no_token_returns_null()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = sessionTokens.GetSessionToken();
        Assert.Null(sessionToken);
    }

    [ConditionalFact]
     // @TODO: Read item and select...
     // @TODO: and sync..
    public virtual async Task Query_uses_session_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = sessionTokens.GetSessionToken()!;

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        sessionTokens.SetSessionToken(sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue);

        var ex = await Assert.ThrowsAsync<Microsoft.Azure.Cosmos.CosmosException>(() => context.Customers.ToListAsync());
        Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
    }

    [ConditionalFact]
     // @TODO: and sync..
    public virtual async Task Shaped_query_uses_session_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = sessionTokens.GetSessionToken()!;
        
        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        sessionTokens.SetSessionToken(sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue);

        var ex = await Assert.ThrowsAsync<Microsoft.Azure.Cosmos.CosmosException>(() => context.Customers.Select(x => new { x.Id, x.PartitionKey }).ToListAsync());
        Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
    }

    [ConditionalFact]
    // @TODO: and sync..
    public virtual async Task Read_item_uses_session_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = sessionTokens.GetSessionToken()!;

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        sessionTokens.SetSessionToken(sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue);

        var ex = await Assert.ThrowsAsync<Microsoft.Azure.Cosmos.CosmosException>(() => context.Customers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1"));
        Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
    }

    [ConditionalTheory, InlineData(AutoTransactionBehavior.WhenNeeded), InlineData(AutoTransactionBehavior.Never), InlineData(AutoTransactionBehavior.Always)]
    // @TODO: and sync..
    public virtual async Task Add_sets_session_token(AutoTransactionBehavior autoTransactionBehavior)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = sessionTokens.GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
    }

    [ConditionalTheory, InlineData(AutoTransactionBehavior.WhenNeeded), InlineData(AutoTransactionBehavior.Never), InlineData(AutoTransactionBehavior.Always)]
    // @TODO: and sync..
    public virtual async Task Delete_merges_session_token(AutoTransactionBehavior autoTransactionBehavior)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        var customer = new Customer { Id = "1", PartitionKey = "1" };
        context.Customers.Add(customer);

        await context.SaveChangesAsync();

        var initialToken = context.Database.GetSessionTokens().GetSessionToken()!;

        context.Remove(customer);
        await context.SaveChangesAsync();

        var sessionToken = context.Database.GetSessionTokens().GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
        Assert.NotEqual(sessionToken, initialToken);
        Assert.StartsWith(initialToken.Substring(0, initialToken.IndexOf('#') + 1), sessionToken);
    }

    [ConditionalTheory, InlineData(AutoTransactionBehavior.WhenNeeded), InlineData(AutoTransactionBehavior.Never), InlineData(AutoTransactionBehavior.Always)]
    // @TODO: and sync..
    public virtual async Task Update_merges_session_token(AutoTransactionBehavior autoTransactionBehavior)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        var customer = new Customer { Id = "1", PartitionKey = "1" };
        context.Customers.Add(customer);

        await context.SaveChangesAsync();

        var initialToken = context.Database.GetSessionTokens().GetSessionToken()!;

        customer.Name = "updated";
        await context.SaveChangesAsync();

        var sessionToken = context.Database.GetSessionTokens().GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
        Assert.NotEqual(initialToken, sessionToken);
        Assert.StartsWith(initialToken.Substring(0, initialToken.IndexOf('#') + 1), sessionToken);
    }

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

        public string? PartitionKey { get; set; }
    }
}
