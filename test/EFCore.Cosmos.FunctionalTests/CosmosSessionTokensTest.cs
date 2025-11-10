// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore;

public class CosmosSessionTokensTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected const string OtherContainerName = "Other";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    protected override string StoreName => nameof(CosmosSessionTokensTest);

    private bool _mock = true;
    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
    {
        var services = base.AddServices(serviceCollection);

        return _mock
            ? services.Replace(ServiceDescriptor.Singleton<ISessionTokenStorageFactory, TestSessionTokenStorageFactory>())
            : services;
    }

    protected override TestStore CreateTestStore() => CosmosTestStore.Create(StoreName, (cfg) => cfg.SessionTokenManagementMode(Cosmos.Infrastructure.SessionTokenManagementMode.SemiAutomatic));

    private static TestSessionTokenStorage _sessionTokenStorage = null!;

    [ConditionalFact]
    public virtual async Task Can_use_session_tokens_no_mock()
    {
        _mock = false;
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();

        Assert.NotNull(sessionTokens[nameof(CosmosSessionTokenContext)]);
        Assert.NotNull(sessionTokens[OtherContainerName]);

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        var newTokens = sessionTokens.ToDictionary(x => x.Key, x => x.Value!.Substring(0, x.Value.IndexOf('#') + 1) + int.MaxValue);
        context.Database.UseSessionTokens(newTokens!);

        var exes = new List<CosmosException>()
        {
            await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToListAsync()),
            await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToListAsync())
        };

        foreach (var ex in exes)
        {
            Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
        }
    }

    [ConditionalFact]
    public virtual async Task AppendSessionToken_uses_AppendDefaultContainerSessionToken()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var arg = "0:-1#231";
        context.Database.AppendSessionToken(arg);
        Assert.Equal(arg, _sessionTokenStorage.AppendDefaultContainerSessionTokenCalls.Single());
    }

    [ConditionalFact]
    public virtual async Task AppendSessionTokens_uses_AppendSessionTokens()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var arg = new Dictionary<string, string> { { OtherContainerName, "0:-1#123" }, { nameof(CosmosSessionTokenContext), "0:-1#231" } };
        context.Database.AppendSessionTokens(arg);
        Assert.Equal(arg, _sessionTokenStorage.AppendSessionTokensCalls.Single());
    }

    [ConditionalFact]
    public virtual async Task UseSessionToken_uses_SetDefaultContainerSessionToken()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var arg = "0:-1#231";
        context.Database.UseSessionToken(arg);
        Assert.Equal(arg, _sessionTokenStorage.SetDefaultContainerSessionTokenCalls.Single());
    }

    [ConditionalFact]
    public virtual async Task UseSessionTokens_uses_SetSessionTokens()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var arg = new Dictionary<string, string?> { { OtherContainerName, "0:-1#123" }, { nameof(CosmosSessionTokenContext), "0:-1#231" } };
        context.Database.UseSessionTokens(arg);
        Assert.Equal(arg, _sessionTokenStorage.SetSessionTokensCalls.Single());
    }

    [ConditionalFact]
    public virtual async Task GetSessionTokens_uses_GetTrackedSessionTokens()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();
        _sessionTokenStorage.SessionTokens = new Dictionary<string, string?> { { OtherContainerName, "0:-1#123" }, { nameof(CosmosSessionTokenContext), "0:-1#231" } };
        var sessionTokens = context.Database.GetSessionTokens();
        Assert.Equal(_sessionTokenStorage.SessionTokens, sessionTokens);
    }

    [ConditionalFact]
    public virtual async Task Query_uses_session_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        _sessionTokenStorage.SessionTokens = new Dictionary<string, string?> { { OtherContainerName, "invalidtoken" }, { nameof(CosmosSessionTokenContext), "invalidtoken" } };

        var exes = new List<CosmosException>
        {
            await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToListAsync()),
            await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToListAsync())
        };

        foreach (var ex in exes)
        {
            Assert.Contains("The session token provided 'invalidtoken' is invalid", ex.ResponseBody);
        }
    }

    [ConditionalFact]
    public virtual async Task New_context_does_not_use_same_SessionTokenStorage()
    {
        _mock = false;
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();
        context.Database.UseSessionToken("A");

        using var newContext = contextFactory.CreateContext();
        Assert.NotSame(context, newContext);
        Assert.Null(newContext.Database.GetSessionToken());
        Assert.Equal("A", context.Database.GetSessionToken());
        Assert.NotSame(((CosmosDatabaseWrapper)context.GetService<IDatabase>()).SessionTokenStorage, ((CosmosDatabaseWrapper)newContext.GetService<IDatabase>()).SessionTokenStorage);
    }

    [ConditionalFact]
    public virtual async Task Pooled_context_uses_same_SessionTokenStorage()
    {
        _mock = false;

        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        DbContext contextCopy;
        ISessionTokenStorage sessionTokenStorageCopy;
        using (var context = contextFactory.CreateContext())
        {
            contextCopy = context;
            context.Database.UseSessionToken("A");
            sessionTokenStorageCopy = ((CosmosDatabaseWrapper)context.GetService<IDatabase>()).SessionTokenStorage;
        }
        
        using var newContext = contextFactory.CreateContext();

        Assert.Same(newContext, contextCopy);
        Assert.Same(sessionTokenStorageCopy, ((CosmosDatabaseWrapper)newContext.GetService<IDatabase>()).SessionTokenStorage);
        Assert.Null(newContext.Database.GetSessionToken());
    }

    [ConditionalFact]
    public virtual async Task Pooled_context_clears_SessionTokenStorage()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        DbContext contextCopy;
        ISessionTokenStorage sessionTokenStorageCopy;
        using (var context = contextFactory.CreateContext())
        {
            contextCopy = context;
            sessionTokenStorageCopy = ((CosmosDatabaseWrapper)context.GetService<IDatabase>()).SessionTokenStorage;
            _sessionTokenStorage.ClearCalled = false;
        }

        using var newContext = contextFactory.CreateContext();

        Assert.Same(newContext, contextCopy);
        Assert.Same(sessionTokenStorageCopy, ((CosmosDatabaseWrapper)newContext.GetService<IDatabase>()).SessionTokenStorage);
        Assert.True(_sessionTokenStorage.ClearCalled);
    }

    [ConditionalFact]
    public virtual async Task PagingQuery_uses_session_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        _sessionTokenStorage.SessionTokens = new Dictionary<string, string?> { { OtherContainerName, "invalidtoken" }, { nameof(CosmosSessionTokenContext), "invalidtoken" } };

        var exes = new List<CosmosException>()
        {
            await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToPageAsync(1, null)),
            await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToPageAsync(1, null)),
        };

        foreach (var ex in exes)
        {
            Assert.Contains("The session token provided 'invalidtoken' is invalid", ex.ResponseBody);
        }
    }

    [ConditionalFact]
    public virtual async Task Shaped_query_uses_session_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        _sessionTokenStorage.SessionTokens = new Dictionary<string, string?> { { OtherContainerName, "invalidtoken" }, { nameof(CosmosSessionTokenContext), "invalidtoken" } };

        var exes = new List<CosmosException>()
        {
            await Assert.ThrowsAsync<CosmosException>(() => context.Customers.Select(x => new { x.Id, x.PartitionKey }).ToListAsync()),
            await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.Select(x => new { x.Id, x.PartitionKey }).ToListAsync())
        };

        foreach (var ex in exes)
        {
            Assert.Contains("The session token provided 'invalidtoken' is invalid", ex.ResponseBody);
        }
    }

    [ConditionalFact]
    public virtual async Task Read_item_uses_session_token()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        _sessionTokenStorage.SessionTokens = new Dictionary<string, string?> { { OtherContainerName, "invalidtoken" }, { nameof(CosmosSessionTokenContext), "invalidtoken" } };

        var exes = new List<CosmosException>()
        {
            await Assert.ThrowsAsync<CosmosException>(() => context.Customers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1")),
            await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1"))
        };

        foreach (var ex in exes)
        {
            Assert.Contains("The session token provided 'invalidtoken' is invalid", ex.ResponseBody);
        }
    }

    [ConditionalFact]
    public virtual async Task Query_uses_TrackSessionToken()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        await context.Customers.ToListAsync();
        await context.OtherContainerCustomers.ToListAsync();

        Assert.Equal(2, _sessionTokenStorage.TrackSessionTokenCalls.Count);
        var defaultContainerCall = _sessionTokenStorage.TrackSessionTokenCalls.First();
        var otherContainerCall = _sessionTokenStorage.TrackSessionTokenCalls.Last();

        Assert.Equal(nameof(CosmosSessionTokenContext), defaultContainerCall.containerName);
        Assert.NotEmpty(defaultContainerCall.sessionToken);

        Assert.Equal(OtherContainerName, otherContainerCall.containerName);
        Assert.NotEmpty(otherContainerCall.sessionToken);
    }

    [ConditionalFact]
    public virtual async Task PagingQuery_uses_TrackSessionToken()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        await context.Customers.ToPageAsync(1, null);
        await context.OtherContainerCustomers.ToPageAsync(1, null);

        Assert.Equal(2, _sessionTokenStorage.TrackSessionTokenCalls.Count);
        var defaultContainerCall = _sessionTokenStorage.TrackSessionTokenCalls.First();
        var otherContainerCall = _sessionTokenStorage.TrackSessionTokenCalls.Last();

        Assert.Equal(nameof(CosmosSessionTokenContext), defaultContainerCall.containerName);
        Assert.NotEmpty(defaultContainerCall.sessionToken);

        Assert.Equal(OtherContainerName, otherContainerCall.containerName);
        Assert.NotEmpty(otherContainerCall.sessionToken);
    }

    [ConditionalFact]
    public virtual async Task Read_item_uses_TrackSessionToken()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        await context.Customers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1");
        await context.OtherContainerCustomers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1");

        Assert.Equal(2, _sessionTokenStorage.TrackSessionTokenCalls.Count);
        var defaultContainerCall = _sessionTokenStorage.TrackSessionTokenCalls.First();
        var otherContainerCall = _sessionTokenStorage.TrackSessionTokenCalls.Last();

        Assert.Equal(nameof(CosmosSessionTokenContext), defaultContainerCall.containerName);
        Assert.NotEmpty(defaultContainerCall.sessionToken);

        Assert.Equal(OtherContainerName, otherContainerCall.containerName);
        Assert.NotEmpty(otherContainerCall.sessionToken);
    }

    [ConditionalFact]
    public virtual async Task Read_item_enumerable_uses_TrackSessionToken()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        await context.Customers.Where(x => x.Id == "1" && x.PartitionKey == "1").ToListAsync();
        await context.OtherContainerCustomers.Where(x => x.Id == "1" && x.PartitionKey == "1").ToListAsync();

        Assert.Equal(2, _sessionTokenStorage.TrackSessionTokenCalls.Count);
        var defaultContainerCall = _sessionTokenStorage.TrackSessionTokenCalls.First();
        var otherContainerCall = _sessionTokenStorage.TrackSessionTokenCalls.Last();

        Assert.Equal(nameof(CosmosSessionTokenContext), defaultContainerCall.containerName);
        Assert.NotEmpty(defaultContainerCall.sessionToken);

        Assert.Equal(OtherContainerName, otherContainerCall.containerName);
        Assert.NotEmpty(otherContainerCall.sessionToken);
    }

    [ConditionalFact]
    public virtual async Task Add_AutoTransactionBehavior_Never_uses_TrackSessionToken()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });

        await context.SaveChangesAsync();

        Assert.Equal(2, _sessionTokenStorage.TrackSessionTokenCalls.Count);
        var defaultContainerCall = _sessionTokenStorage.TrackSessionTokenCalls.First();
        var otherContainerCall = _sessionTokenStorage.TrackSessionTokenCalls.Last();

        Assert.Equal(nameof(CosmosSessionTokenContext), defaultContainerCall.containerName);
        Assert.NotEmpty(defaultContainerCall.sessionToken);

        Assert.Equal(OtherContainerName, otherContainerCall.containerName);
        Assert.NotEmpty(otherContainerCall.sessionToken);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Add_AutoTransactionBehavior_Always_uses_TrackSessionToken(bool defaultContainer)
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

        await context.SaveChangesAsync();

        Assert.Equal(1, _sessionTokenStorage.TrackSessionTokenCalls.Count);
        var call = _sessionTokenStorage.TrackSessionTokenCalls.First();

        if (defaultContainer)
        {
            Assert.Equal(nameof(CosmosSessionTokenContext), call.containerName);
        }
        else
        {
            Assert.Equal(OtherContainerName, call.containerName);
        }

        Assert.NotEmpty(call.sessionToken);
    }

    [ConditionalFact]
    public virtual async Task Delete_never_uses_TrackSessionToken()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

        var customer = new Customer { Id = "1", PartitionKey = "1" };
        var otherContainerCustomer = new OtherContainerCustomer { Id = "1", PartitionKey = "1" };
        context.Customers.Add(customer);
        context.OtherContainerCustomers.Add(otherContainerCustomer);

        await context.SaveChangesAsync();

        var initialDefaultContainerCall = _sessionTokenStorage.TrackSessionTokenCalls[0];
        var initialOtherContainerCall = _sessionTokenStorage.TrackSessionTokenCalls[1];

        context.Customers.Remove(customer);
        context.OtherContainerCustomers.Remove(otherContainerCustomer);

        await context.SaveChangesAsync();

        Assert.Equal(4, _sessionTokenStorage.TrackSessionTokenCalls.Count);
        var defaultContainerCall = _sessionTokenStorage.TrackSessionTokenCalls[2];
        var otherContainerCall = _sessionTokenStorage.TrackSessionTokenCalls[3];

        Assert.Equal(nameof(CosmosSessionTokenContext), defaultContainerCall.containerName);
        Assert.NotEmpty(defaultContainerCall.sessionToken);

        Assert.Equal(OtherContainerName, otherContainerCall.containerName);
        Assert.NotEmpty(otherContainerCall.sessionToken);

        Assert.Equal(initialDefaultContainerCall.containerName, defaultContainerCall.containerName);
        Assert.Equal(initialOtherContainerCall.containerName, otherContainerCall.containerName);

        Assert.NotEqual(initialDefaultContainerCall.sessionToken, defaultContainerCall.sessionToken);
        Assert.NotEqual(initialOtherContainerCall.sessionToken, otherContainerCall.sessionToken);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Delete_always_uses_TrackSessionToken(bool defaultContainer)
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

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var initialCall = _sessionTokenStorage.TrackSessionTokenCalls[0];

        if (defaultContainer)
        {
            context.Customers.Remove(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Remove(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        await context.SaveChangesAsync();

        Assert.Equal(2, _sessionTokenStorage.TrackSessionTokenCalls.Count);
        var call = _sessionTokenStorage.TrackSessionTokenCalls[1];

        if (defaultContainer)
        {
            Assert.Equal(nameof(CosmosSessionTokenContext), call.containerName);
        }
        else
        {
            Assert.Equal(OtherContainerName, call.containerName);

        }
        Assert.NotEmpty(call.sessionToken);

        Assert.Equal(initialCall.containerName, call.containerName);
        Assert.NotEqual(initialCall.sessionToken, call.sessionToken);
    }

    [ConditionalFact]
    public virtual async Task Update_never_uses_TrackSessionToken()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

        var customer = new Customer { Id = "1", PartitionKey = "1" };
        var otherContainerCustomer = new OtherContainerCustomer { Id = "1", PartitionKey = "1" };
        context.Customers.Add(customer);
        context.OtherContainerCustomers.Add(otherContainerCustomer);

        await context.SaveChangesAsync();

        var initialDefaultContainerCall = _sessionTokenStorage.TrackSessionTokenCalls[0];
        var initialOtherContainerCall = _sessionTokenStorage.TrackSessionTokenCalls[1];

        customer.Name = "updated";
        otherContainerCustomer.Name = "updated";

        await context.SaveChangesAsync();

        Assert.Equal(4, _sessionTokenStorage.TrackSessionTokenCalls.Count);
        var defaultContainerCall = _sessionTokenStorage.TrackSessionTokenCalls[2];
        var otherContainerCall = _sessionTokenStorage.TrackSessionTokenCalls[3];

        Assert.Equal(nameof(CosmosSessionTokenContext), defaultContainerCall.containerName);
        Assert.NotEmpty(defaultContainerCall.sessionToken);

        Assert.Equal(OtherContainerName, otherContainerCall.containerName);
        Assert.NotEmpty(otherContainerCall.sessionToken);

        Assert.Equal(initialDefaultContainerCall.containerName, defaultContainerCall.containerName);
        Assert.Equal(initialOtherContainerCall.containerName, otherContainerCall.containerName);

        Assert.NotEqual(initialDefaultContainerCall.sessionToken, defaultContainerCall.sessionToken);
        Assert.NotEqual(initialOtherContainerCall.sessionToken, otherContainerCall.sessionToken);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public virtual async Task Update_always_uses_TrackSessionToken(bool defaultContainer)
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

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var initialCall = _sessionTokenStorage.TrackSessionTokenCalls[0];

        if (defaultContainer)
        {
            context.Customers.Update(new Customer { Id = "1", Name = "updated", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Update(new OtherContainerCustomer { Id = "1", Name = "updated", PartitionKey = "1" });
        }

        await context.SaveChangesAsync();

        Assert.Equal(2, _sessionTokenStorage.TrackSessionTokenCalls.Count);
        var call = _sessionTokenStorage.TrackSessionTokenCalls[1];

        if (defaultContainer)
        {
            Assert.Equal(nameof(CosmosSessionTokenContext), call.containerName);
        }
        else
        {
            Assert.Equal(OtherContainerName, call.containerName);

        }
        Assert.NotEmpty(call.sessionToken);

        Assert.Equal(initialCall.containerName, call.containerName);
        Assert.NotEqual(initialCall.sessionToken, call.sessionToken);
    }

    [ConditionalTheory]
    [InlineData(AutoTransactionBehavior.WhenNeeded, true)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, false)]
    [InlineData(AutoTransactionBehavior.Never, false)]
    [InlineData(AutoTransactionBehavior.Never, true)]
    [InlineData(AutoTransactionBehavior.Always, false)]
    [InlineData(AutoTransactionBehavior.Always, true)]
    public virtual async Task Add_uses_GetSessionToken(AutoTransactionBehavior autoTransactionBehavior, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        // Only way we can test this is by setting a session token that will fail the request if used..
        // Only way to do this for a write is to set an invalid session token..
        _sessionTokenStorage.SessionTokens = new Dictionary<string, string?> { { defaultContainer ? nameof(CosmosSessionTokenContext) : OtherContainerName, "invalidtoken" } };

        if (defaultContainer)
        {
            context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains("The session token provided 'invalidtoken' is invalid.", ((CosmosException)ex.InnerException!).ResponseBody);
    }

    [ConditionalTheory]
    [InlineData(AutoTransactionBehavior.WhenNeeded, true)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, false)]
    [InlineData(AutoTransactionBehavior.Never, false)]
    [InlineData(AutoTransactionBehavior.Never, true)]
    [InlineData(AutoTransactionBehavior.Always, false)]
    [InlineData(AutoTransactionBehavior.Always, true)]
    public virtual async Task Update_uses_session_token(AutoTransactionBehavior autoTransactionBehavior, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        var sessionTokens = context.Database.GetSessionTokens();
        // Only way we can test this is by setting a session token that will fail the request if used..
        // Only way to do this for a write is to set an invalid session token..
        _sessionTokenStorage.SessionTokens = new Dictionary<string, string?> { { defaultContainer ? nameof(CosmosSessionTokenContext) : OtherContainerName, "invalidtoken" } };

        if (defaultContainer)
        {
            context.Customers.Update(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Update(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains("The session token provided 'invalidtoken' is invalid.", ((CosmosException)ex.InnerException!).ResponseBody);
    }

    [ConditionalTheory]
    [InlineData(AutoTransactionBehavior.WhenNeeded, true)]
    [InlineData(AutoTransactionBehavior.WhenNeeded, false)]
    [InlineData(AutoTransactionBehavior.Never, false)]
    [InlineData(AutoTransactionBehavior.Never, true)]
    [InlineData(AutoTransactionBehavior.Always, false)]
    [InlineData(AutoTransactionBehavior.Always, true)]
    public virtual async Task Delete_uses_session_token(AutoTransactionBehavior autoTransactionBehavior, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        var sessionTokens = context.Database.GetSessionTokens();
        // Only way we can test this is by setting a session token that will fail the request if used..
        // Only way to do this for a write is to set an invalid session token..
        _sessionTokenStorage.SessionTokens = new Dictionary<string, string?> { { defaultContainer ? nameof(CosmosSessionTokenContext) : OtherContainerName, "invalidtoken" } };

        if (defaultContainer)
        {
            context.Customers.Remove(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Remove(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Contains("The session token provided 'invalidtoken' is invalid.", ((CosmosException)ex.InnerException!).ResponseBody);
    }

    private class TestSessionTokenStorageFactory : ISessionTokenStorageFactory
    {
        public ISessionTokenStorage Create(DbContext _)
            => _sessionTokenStorage = new();
    }

    private class TestSessionTokenStorage : ISessionTokenStorage
    {
        public Dictionary<string, string?> SessionTokens { get; set; } = new() { { nameof(CosmosSessionTokenContext), null }, { OtherContainerName, null } };

        public List<string> AppendDefaultContainerSessionTokenCalls { get; set; } = new();
        public List<IReadOnlyDictionary<string, string>> AppendSessionTokensCalls { get; set; } = new();
        public List<string> SetDefaultContainerSessionTokenCalls { get; set; } = new();

        public List<IReadOnlyDictionary<string, string?>> SetSessionTokensCalls { get; set; } = new();
        public List<(string containerName, string sessionToken)> TrackSessionTokenCalls { get; set; } = new();
        public bool ClearCalled { get; set; }



        public void AppendDefaultContainerSessionToken(string sessionToken) => AppendDefaultContainerSessionTokenCalls.Add(sessionToken);

        public void AppendSessionTokens(IReadOnlyDictionary<string, string> sessionTokens) => AppendSessionTokensCalls.Add(sessionTokens);
        public void Clear() => ClearCalled = true;
        public string? GetDefaultContainerTrackedToken() => SessionTokens.FirstOrDefault().Value;
        public string? GetSessionToken(string containerName) => SessionTokens[containerName];
        public IReadOnlyDictionary<string, string?> GetTrackedTokens() => SessionTokens;
        public void SetDefaultContainerSessionToken(string sessionToken) => SetDefaultContainerSessionTokenCalls.Add(sessionToken);
        public void SetSessionTokens(IReadOnlyDictionary<string, string?> sessionTokens) => SetSessionTokensCalls.Add(sessionTokens);
        public void TrackSessionToken(string containerName, string sessionToken) => TrackSessionTokenCalls.Add((containerName, sessionToken));
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

        public string? Name { get; set; }

        public string? PartitionKey { get; set; }
    }
}
