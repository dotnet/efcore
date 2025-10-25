// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

public class CosmosSessionTokensTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    private const string OtherContainerName = "Other";
    protected override string StoreName
        => "CosmosSessionTokensTest";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder) => base.AddOptions(builder).ConfigureWarnings(x => x.Ignore(CosmosEventId.SyncNotSupported));

    [ConditionalFact]
    public virtual async Task SetSessionToken_ThrowsForNonExistentContainer()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        var exception = Assert.Throws<ArgumentException>(() => sessionTokens.SetSessionToken("Not the container name", "0:-1#231"));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("Not the container name") + " (Parameter 'containerName')", exception.Message);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionToken_ThrowsForNonExistentContainer()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        var exception = Assert.Throws<ArgumentException>(() => sessionTokens.AppendSessionToken("Not the container name", "0:-1#231"));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("Not the container name") + " (Parameter 'containerName')", exception.Message);
    }

    [ConditionalFact]
    public virtual async Task GetSessionToken_ThrowsForNonExistentContainer()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        var exception = Assert.Throws<ArgumentException>(() => sessionTokens.GetSessionToken("Not the container name"));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("Not the container name") + " (Parameter 'containerName')", exception.Message);
    }


    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task AppendSessionToken_no_tokens_sets_token(bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var sessionTokens = context.Database.GetSessionTokens();
        if (defaultContainer)
        {
            sessionTokens.AppendSessionToken("0:-1#231");
        }
        else
        {
            sessionTokens.AppendSessionToken(OtherContainerName, "0:-1#231");
        }

        var updatedToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);

        Assert.Equal("0:-1#231", updatedToken);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task AppendSessionToken_append_token_not_present_adds_token(bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        
        using var context = contextFactory.CreateContext();
        if (defaultContainer)
        {
            context.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }
        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        var newToken = "0:-1#231";
        if (defaultContainer)
        {
            sessionTokens.AppendSessionToken(newToken);
        }
        else
        {
            sessionTokens.AppendSessionToken(OtherContainerName, newToken);
        }

        var updatedToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);

        Assert.Equal(initialToken + "," + newToken, updatedToken);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task AppendSessionToken_append_token_already_present_does_not_add_token(bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        if (defaultContainer)
        {
            context.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        var newToken = initialToken;
        if (defaultContainer)
        {
            sessionTokens.AppendSessionToken(newToken);
        }
        else
        {
            sessionTokens.AppendSessionToken(OtherContainerName, newToken);
        }

        var updatedToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);

        Assert.Equal(initialToken, updatedToken);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task AppendSessionToken_multiple_tokens_splits_tokens(bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var sessionTokens = context.Database.GetSessionTokens();
        var newToken = "0:-1#123,1:-1#456";
        var appendix = "0:-1#123";
        if (defaultContainer)
        {
            sessionTokens.AppendSessionToken(newToken);
            sessionTokens.AppendSessionToken(appendix);

        }
        else
        {
            sessionTokens.AppendSessionToken(OtherContainerName, newToken);
            sessionTokens.AppendSessionToken(OtherContainerName, appendix);
        }

        var updatedToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);

        Assert.Equal(newToken, updatedToken);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task SetSessionToken_does_not_append_session_token(bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        if (defaultContainer)
        {
            context.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        var newToken = "0:-1#1,1:-1#1";
        if (defaultContainer)
        {
            sessionTokens.SetSessionToken(newToken);
        }
        else
        {
            sessionTokens.SetSessionToken(OtherContainerName, newToken);
        }

        var updatedToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);

        Assert.Equal(newToken, updatedToken);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task SetSessionToken_null_sets_session_token_null(bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        if (defaultContainer)
        {
            context.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);
        Assert.False(string.IsNullOrWhiteSpace(initialToken));

        if (defaultContainer)
        {
            sessionTokens.SetSessionToken(null);
        }
        else
        {
            sessionTokens.SetSessionToken(OtherContainerName, null);
        }

        var updatedToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);

        Assert.Null(updatedToken);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task GetSessionToken_no_token_returns_null(bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        var initialToken = defaultContainer ? sessionTokens.GetSessionToken() : sessionTokens.GetSessionToken(OtherContainerName);
        Assert.Null(initialToken);
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Query_uses_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

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

        string sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken()!;
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName)!;
        }

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        var newToken = sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue;

        if (defaultContainer)
        {
            sessionTokens.SetSessionToken(newToken);
        }
        else
        {
            sessionTokens.SetSessionToken(OtherContainerName, newToken);
        }

        CosmosException ex;
        if (async)
        {
            if (defaultContainer)
            {
                ex = await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToListAsync());
            }
            else
            {
                ex = await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToListAsync());
            }
        }
        else
        {
            if (defaultContainer)
            {
                ex = Assert.Throws<CosmosException>(() => context.Customers.ToList());
            }
            else
            {
                ex = Assert.Throws<CosmosException>(() => context.OtherContainerCustomers.ToList());
            }
        }

        Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Query_on_new_context_does_not_use_same_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();
        
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

        string sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken()!;
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName)!;
        }

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        var newToken = sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue;

        if (defaultContainer)
        {
            sessionTokens.SetSessionToken(newToken);
        }
        else
        {
            sessionTokens.SetSessionToken(OtherContainerName, newToken);
        }

        if (async)
        {
            if (defaultContainer)
            {
                await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToListAsync());
            }
            else
            {
                await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToListAsync());
            }
        }
        else
        {
            if (defaultContainer)
            {
                Assert.Throws<CosmosException>(() => context.Customers.ToList());
            }
            else
            {
                Assert.Throws<CosmosException>(() => context.OtherContainerCustomers.ToList());
            }
        }

        using var newContext = contextFactory.CreateContext();
        if (async)
        {
            if (defaultContainer)
            {
                await newContext.Customers.ToListAsync();
            }
            else
            {
                await newContext.OtherContainerCustomers.ToListAsync();
            }
        }
        else
        {
            if (defaultContainer)
            {
                newContext.Customers.ToList();
            }
            else
            {
                newContext.OtherContainerCustomers.ToList();
            }
        }
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Query_on_same_newly_pooled_context_does_not_use_same_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        DbContext contextCopy;
        using (var context = contextFactory.CreateContext())
        {
            contextCopy = context;
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

            string sessionToken;
            if (defaultContainer)
            {
                sessionToken = sessionTokens.GetSessionToken()!;
            }
            else
            {
                sessionToken = sessionTokens.GetSessionToken(OtherContainerName)!;
            }

            // Only way we can test this is by setting a session token that will fail the request if used..
            // This will take a couple of seconds to fail
            var newToken = sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue;

            if (defaultContainer)
            {
                sessionTokens.SetSessionToken(newToken);
            }
            else
            {
                sessionTokens.SetSessionToken(OtherContainerName, newToken);
            }

            if (async)
            {
                if (defaultContainer)
                {
                    await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToListAsync());
                }
                else
                {
                    await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToListAsync());
                }
            }
            else
            {
                if (defaultContainer)
                {
                    Assert.Throws<CosmosException>(() => context.Customers.ToList());
                }
                else
                {
                    Assert.Throws<CosmosException>(() => context.OtherContainerCustomers.ToList());
                }
            }
        }

        using var newContext = contextFactory.CreateContext();
        Assert.Same(newContext, contextCopy);
        if (async)
        {
            if (defaultContainer)
            {
                await newContext.Customers.ToListAsync();
            }
            else
            {
                await newContext.OtherContainerCustomers.ToListAsync();
            }
        }
        else
        {
            if (defaultContainer)
            {
                newContext.Customers.ToList();
            }
            else
            {
                newContext.OtherContainerCustomers.ToList();
            }
        }
    }

    [ConditionalTheory]
    [InlineData(true), InlineData(false)]
    public virtual async Task PagingQuery_uses_session_token(bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (defaultContainer)
        {
            context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        }
        else
        {
            context.OtherContainerCustomers.Add(new OtherContainerCustomer { Id = "1", PartitionKey = "1" });
        }

        await context.SaveChangesAsync();

        var sessionTokens = context.Database.GetSessionTokens();

        string sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken()!;
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName)!;
        }

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        var newToken = sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue;

        if (defaultContainer)
        {
            sessionTokens.SetSessionToken(newToken);
        }
        else
        {
            sessionTokens.SetSessionToken(OtherContainerName, newToken);
        }

        CosmosException ex;
        if (defaultContainer)
        {
            ex = await Assert.ThrowsAsync<CosmosException>(() => context.Customers.ToPageAsync(1, null));
        }
        else
        {
            ex = await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.ToPageAsync(1, null));
        }

        Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Shaped_query_uses_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

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

        string sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken()!;
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName)!;
        }

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        var newToken = sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue;

        if (defaultContainer)
        {
            sessionTokens.SetSessionToken(newToken);
        }
        else
        {
            sessionTokens.SetSessionToken(OtherContainerName, newToken);
        }

        CosmosException ex;
        if (async)
        {
            if (defaultContainer)
            {
                ex = await Assert.ThrowsAsync<CosmosException>(() => context.Customers.Select(x => new { x.Id, x.PartitionKey }).ToListAsync());
            }
            else
            {
                ex = await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.Select(x => new { x.Id, x.PartitionKey }).ToListAsync());
            }
        }
        else
        {
            if (defaultContainer)
            {
                ex = Assert.Throws<CosmosException>(() => context.Customers.Select(x => new { x.Id, x.PartitionKey }).ToList());
            }
            else
            {
                ex = Assert.Throws<CosmosException>(() => context.OtherContainerCustomers.Select(x => new { x.Id, x.PartitionKey }).ToList());
            }
        }

        Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Read_item_uses_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

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

        string sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken()!;
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName)!;
        }

        // Only way we can test this is by setting a session token that will fail the request if used..
        // This will take a couple of seconds to fail
        var newToken = sessionToken.Substring(0, sessionToken.IndexOf('#') + 1) + int.MaxValue;

        if (defaultContainer)
        {
            sessionTokens.SetSessionToken(newToken);
        }
        else
        {
            sessionTokens.SetSessionToken(OtherContainerName, newToken);
        }

        CosmosException ex;
        if (async)
        {
            if (defaultContainer)
            {
                ex = await Assert.ThrowsAsync<CosmosException>(() => context.Customers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1"));
            }
            else
            {
                ex = await Assert.ThrowsAsync<CosmosException>(() => context.OtherContainerCustomers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1"));
            }
        }
        else
        {
            if (defaultContainer)
            {
                ex = Assert.Throws<CosmosException>(() => context.Customers.FirstOrDefault(x => x.Id == "1" && x.PartitionKey == "1"));
            }
            else
            {
                ex = Assert.Throws<CosmosException>(() => context.OtherContainerCustomers.FirstOrDefault(x => x.Id == "1" && x.PartitionKey == "1"));
            }
        }

        Assert.Contains("The read session is not available for the input session token.", ex.ResponseBody);
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Query_sets_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (async)
        {
            if (defaultContainer)
            {
                await context.Customers.ToListAsync();
            }
            else
            {
                await context.OtherContainerCustomers.ToListAsync();
            }
        }
        else
        {
            if (defaultContainer)
            {
                _ = context.Customers.ToList();
            }
            else
            {
                _ = context.OtherContainerCustomers.ToList();
            }
        }

        var sessionTokens = context.Database.GetSessionTokens();
        string? sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken();
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName);
        }

        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task PagingQuery_sets_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (defaultContainer)
        {
            await context.Customers.ToPageAsync(1, null);
        }
        else
        {
            await context.OtherContainerCustomers.ToPageAsync(1, null);
        }

        var sessionTokens = context.Database.GetSessionTokens();
        string? sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken();
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName);
        }

        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Shaped_query_sets_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (async)
        {
            if (defaultContainer)
            {
                await context.Customers.Select(x => new { x.Id, x.PartitionKey }).ToListAsync();
            }
            else
            {
                await context.OtherContainerCustomers.Select(x => new { x.Id, x.PartitionKey }).ToListAsync();
            }
        }
        else
        {
            if (defaultContainer)
            {
                _ = context.Customers.Select(x => new { x.Id, x.PartitionKey }).ToList();
            }
            else
            {
                _ = context.OtherContainerCustomers.Select(x => new { x.Id, x.PartitionKey }).ToList();
            }
        }

        var sessionTokens = context.Database.GetSessionTokens();
        string? sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken();
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName);
        }

        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Read_item_sets_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (async)
        {
            if (defaultContainer)
            {
                await context.Customers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1");
            }
            else
            {
                await context.OtherContainerCustomers.FirstOrDefaultAsync(x => x.Id == "1" && x.PartitionKey == "1");
            }
        }
        else
        {
            if (defaultContainer)
            {
                _ = context.Customers.FirstOrDefault(x => x.Id == "1" && x.PartitionKey == "1");
            }
            else
            {
                _ = context.OtherContainerCustomers.FirstOrDefault(x => x.Id == "1" && x.PartitionKey == "1");
            }
        }

        var sessionTokens = context.Database.GetSessionTokens();
        string? sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken();
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName);
        }

        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
    }

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Read_item_enumerable_sets_session_token(bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();
        using var context = contextFactory.CreateContext();

        if (async)
        {
            if (defaultContainer)
            {
                await context.Customers.Where(x => x.Id == "1" && x.PartitionKey == "1").ToListAsync();
            }
            else
            {
                await context.OtherContainerCustomers.Where(x => x.Id == "1" && x.PartitionKey == "1").ToListAsync();
            }
        }
        else
        {
            if (defaultContainer)
            {
                _ = context.Customers.Where(x => x.Id == "1" && x.PartitionKey == "1").ToList();
            }
            else
            {
                _ = context.OtherContainerCustomers.Where(x => x.Id == "1" && x.PartitionKey == "1").ToList();
            }
        }

        var sessionTokens = context.Database.GetSessionTokens();
        string? sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken();
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName);
        }

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
    public virtual async Task Add_sets_session_token(AutoTransactionBehavior autoTransactionBehavior, bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

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
        string? sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken();
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName);
        }

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
    public virtual async Task Add_merges_session_token(AutoTransactionBehavior autoTransactionBehavior, bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        var sessionTokens = context.Database.GetSessionTokens();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

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

        var initialToken = defaultContainer ? sessionTokens.GetSessionToken()! : sessionTokens.GetSessionToken(OtherContainerName)!;

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

        string? sessionToken;
        if (defaultContainer)
        {
            sessionToken = sessionTokens.GetSessionToken();
        }
        else
        {
            sessionToken = sessionTokens.GetSessionToken(OtherContainerName);
        }

        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
        Assert.NotEqual(sessionToken, initialToken);
        Assert.StartsWith(initialToken + ",", sessionToken);
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
    public virtual async Task Delete_merges_session_token(AutoTransactionBehavior autoTransactionBehavior, bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        string initialToken;
        if (defaultContainer)
        {
            var customer = new Customer { Id = "1", PartitionKey = "1" };
            context.Customers.Add(customer);

            if (async)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                context.SaveChanges();
            }

            initialToken = context.Database.GetSessionTokens().GetSessionToken()!;

            context.Remove(customer);

            if (async)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                context.SaveChanges();
            }
        }
        else
        {
            var customer = new OtherContainerCustomer { Id = "1", PartitionKey = "1" };
            context.Add(customer);

            if (async)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                context.SaveChanges();
            }

            initialToken = context.Database.GetSessionTokens().GetSessionToken(OtherContainerName)!;

            context.Remove(customer);

            if (async)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                context.SaveChanges();
            }
        }

        var sessionToken = defaultContainer ? context.Database.GetSessionTokens().GetSessionToken() : context.Database.GetSessionTokens().GetSessionToken(OtherContainerName);
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
        Assert.NotEqual(sessionToken, initialToken);
        Assert.StartsWith(initialToken + ",", sessionToken);
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
    public virtual async Task Update_merges_session_token(AutoTransactionBehavior autoTransactionBehavior, bool async, bool defaultContainer)
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = autoTransactionBehavior;

        string initialToken;
        if (defaultContainer)
        {
            var customer = new Customer { Id = "1", PartitionKey = "1" };
            context.Customers.Add(customer);

            if (async)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                context.SaveChanges();
            }

            initialToken = context.Database.GetSessionTokens().GetSessionToken()!;

            customer.Name = "updated";
        }
        else
        {
            var customer = new OtherContainerCustomer { Id = "1", PartitionKey = "1" };
            context.Add(customer);

            if (async)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                context.SaveChanges();
            }

            initialToken = context.Database.GetSessionTokens().GetSessionToken(OtherContainerName)!;

            customer.Name = "updated";
        }

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var sessionToken = defaultContainer ? context.Database.GetSessionTokens().GetSessionToken() : context.Database.GetSessionTokens().GetSessionToken(OtherContainerName);
        Assert.False(string.IsNullOrWhiteSpace(sessionToken));
        Assert.NotEqual(initialToken, sessionToken);
        Assert.StartsWith(initialToken + ",", sessionToken);
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
        var internalDictionary = sessionTokens.GetType().GetField("_containerSessionTokens", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(sessionTokens)!;
        var internalComposite = internalDictionary.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance)!.GetValue(internalDictionary, new object[] { defaultContainer ? nameof(CosmosSessionTokenContext) : OtherContainerName })!;
        internalComposite.GetType().GetField("_string", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(internalComposite, "invalidtoken");
        internalComposite.GetType().GetField("_isChanged", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(internalComposite, false);

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
        var sessionToken = sessionTokens.GetSessionToken()!;
        // Only way we can test this is by setting a session token that will fail the request if used..
        // Only way to do this for a write is to set an invalid session token..
        var internalDictionary = sessionTokens.GetType().GetField("_containerSessionTokens", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(sessionTokens)!;
        var internalComposite = internalDictionary.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance)!.GetValue(internalDictionary, new object[] { defaultContainer ? nameof(CosmosSessionTokenContext) : OtherContainerName })!;
        internalComposite.GetType().GetField("_string", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(internalComposite, "invalidtoken");
        internalComposite.GetType().GetField("_isChanged", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(internalComposite, false);

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
        var sessionToken = sessionTokens.GetSessionToken()!;
        // Only way we can test this is by setting a session token that will fail the request if used..
        // Only way to do this for a write is to set an invalid session token..
        var internalDictionary = sessionTokens.GetType().GetField("_containerSessionTokens", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(sessionTokens)!;
        var internalComposite = internalDictionary.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance)!.GetValue(internalDictionary, new object[] { defaultContainer ? nameof(CosmosSessionTokenContext) : OtherContainerName })!;
        internalComposite.GetType().GetField("_string", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(internalComposite, "invalidtoken");
        internalComposite.GetType().GetField("_isChanged", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(internalComposite, false);

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
