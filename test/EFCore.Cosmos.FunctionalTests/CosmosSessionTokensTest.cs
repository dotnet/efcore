// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage;

namespace Microsoft.EntityFrameworkCore;

public class CosmosSessionTokensTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string StoreName
        => "CosmosSessionTokensTest";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual async Task OverwriteSessionToken_ThrowsForNonExistentContainer()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        var exception = Assert.Throws<ArgumentException>(() => sessionTokens.OverwriteSessionToken("Not the store name", "0:-1#231"));
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

    [ConditionalTheory, InlineData(AutoTransactionBehavior.WhenNeeded), InlineData(AutoTransactionBehavior.Never), InlineData(AutoTransactionBehavior.Always)]
    public virtual async Task AppendSessionToken(AutoTransactionBehavior autoTransactionBehavior)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = sessionTokens.GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        sessionTokens.AppendSessionToken("0:-1#231");

        var updatedToken = sessionTokens.GetSessionToken();

        Assert.Equal(initialToken + ",0:-1#231", updatedToken);
    }

    [ConditionalTheory, InlineData(AutoTransactionBehavior.WhenNeeded), InlineData(AutoTransactionBehavior.Never), InlineData(AutoTransactionBehavior.Always)]
    public virtual async Task OverwriteSessionToken(AutoTransactionBehavior autoTransactionBehavior)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = sessionTokens.GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        sessionTokens.OverwriteSessionToken("0:-1#231");

        var updatedToken = sessionTokens.GetSessionToken();

        Assert.Equal("0:-1#231", updatedToken);
    }

    [ConditionalTheory, InlineData(AutoTransactionBehavior.WhenNeeded), InlineData(AutoTransactionBehavior.Never), InlineData(AutoTransactionBehavior.Always)]
    public virtual async Task OverwriteSessionToken_null(AutoTransactionBehavior autoTransactionBehavior)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = sessionTokens.GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        sessionTokens.OverwriteSessionToken(null);

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
    public virtual async Task AppendSessionToken_uses_session_token_list()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = sessionTokens.GetSessionToken()!;

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        sessionTokens.OverwriteSessionToken(sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue);

        var ex = await Assert.ThrowsAsync<Microsoft.Azure.Cosmos.CosmosException>(() => context.Customers.ToListAsync());
        Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
    }

    [ConditionalFact]
     // @TODO: and sync..
    public virtual async Task AppendSessionToken_uses_session_token_select_list()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = sessionTokens.GetSessionToken()!;

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        sessionTokens.OverwriteSessionToken(sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue);

        var ex = await Assert.ThrowsAsync<Microsoft.Azure.Cosmos.CosmosException>(() => context.Customers.Select(x => new { x.Id, x.PartitionKey }).ToListAsync());
        Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
    }

    [ConditionalFact]
    // @TODO: and sync..
    public virtual async Task AppendSessionToken_uses_session_token_read_item()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var sessionToken = sessionTokens.GetSessionToken()!;

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        sessionTokens.OverwriteSessionToken(sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue);

        var ex = await Assert.ThrowsAsync<Microsoft.Azure.Cosmos.CosmosException>(() => context.Customers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1"));
        Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
    }

    [ConditionalTheory, InlineData(AutoTransactionBehavior.WhenNeeded), InlineData(AutoTransactionBehavior.Never), InlineData(AutoTransactionBehavior.Always)]
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
    public virtual async Task Delete_sets_session_token(AutoTransactionBehavior autoTransactionBehavior)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        var customer = new Customer { Id = "1", PartitionKey = "1" };
        context.Customers.Add(customer);

        await context.SaveChangesAsync();

        var initialToken = context.Database.GetSessionTokens().GetSessionToken();

        context.Remove(customer);
        await context.SaveChangesAsync();

        var sessionToken = context.Database.GetSessionTokens().GetSessionToken();
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
        Assert.StartsWith(initialToken + ",", sessionToken);
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
    }

    //[ConditionalTheory, InlineData(AutoTransactionBehavior.WhenNeeded), InlineData(AutoTransactionBehavior.Never), InlineData(AutoTransactionBehavior.Always)]
    //public virtual async Task Update_sets_session_token(AutoTransactionBehavior autoTransactionBehavior)
    //{
    //    var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

    //    using var context = contextFactory.CreateContext();
    //    context.Database.AutoTransactionBehavior = autoTransactionBehavior;

    //    var customer = new Customer { Id = "1", PartitionKey = "1" };
    //    context.Customers.Add(customer);

    //    await context.SaveChangesAsync();

    //    var initialToken = context.Database.GetSessionTokens().Single().Value;

    //    customer.Name = "updated";
    //    await context.SaveChangesAsync();

    //    var sessionTokens = context.Database.GetSessionTokens();
    //    var key = new CosmosContainerPartitionScope("CosmosSessionTokenContext", new Azure.Cosmos.PartitionKey("1"));
    //    Assert.Equal(1, sessionTokens.Count);
    //    Assert.True(sessionTokens.ContainsKey(key));
    //    Assert.NotEmpty(sessionTokens[key]);
    //    Assert.NotEqual(initialToken, sessionTokens[key]);
    //}

    //[ConditionalFact]
    //public virtual async Task Query_with_single_filter_uses_session_token()
    //{
    //    var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

    //    using var context = contextFactory.CreateContext();

    //    var customer = new Customer { Id = "1", PartitionKey = "1" };
    //    context.Customers.Add(customer);

    //    await context.SaveChangesAsync();

    //    await context.Customers.Where(x => x.PartitionKey == "1").ToListAsync();
    //}

    //[ConditionalFact]
    //public virtual async Task Query_with_double_filter_uses_session_token()
    //{
    //    var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

    //    using var context = contextFactory.CreateContext();

    //    var customer = new Customer { Id = "1", PartitionKey = "1" };
    //    context.Customers.Add(customer);

    //    await context.SaveChangesAsync();

    //    await context.Customers.Where(x => x.PartitionKey == "1" || x.PartitionKey == "2").ToListAsync();
    //}


    //[ConditionalFact]
    //public virtual async Task Query_with_composite_partition_key_not_all_properties_does_not_use_sessiontoken()
    //{
    //    var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

    //    using var context = contextFactory.CreateContext();

    //    var customer = new CompositeCustomer { Id = "1", PartitionKey1 = "1", PartitionKey2 = "2" };
    //    context.Add(customer);

    //    await context.SaveChangesAsync();

    //    await context.CompositeCustomers.Where(x => x.PartitionKey1 == "1").ToListAsync();
    //}

    //[ConditionalFact]
    //public virtual async Task Query_with_composite_partition_key_uses_session_token()
    //{
    //    var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

    //    using var context = contextFactory.CreateContext();

    //    var customer = new CompositeCustomer { Id = "1", PartitionKey1 = "1", PartitionKey2 = "2" };
    //    context.Add(customer);

    //    await context.SaveChangesAsync();

    //    await context.CompositeCustomers.Where(x => x.PartitionKey1 == "1" && x.PartitionKey2 == "2").ToListAsync();
    //}


    public class CosmosSessionTokenContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; } = null!;
        // public DbSet<CompositeCustomer> CompositeCustomers { get; set; } = null!;


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

            //builder.Entity<CompositeCustomer>(
            //    b =>
            //    {
            //        b.HasKey(c => c.Id);
            //        b.HasPartitionKey(c => new { c.PartitionKey1, c.PartitionKey2 });
            //        b.ToContainer("composite");
            //    });
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

    //public class CompositeCustomer
    //{
    //    public string? Id { get; set; }

    //    public string? PartitionKey1 { get; set; }
    //    public string? PartitionKey2 { get; set; }
    //}
}
