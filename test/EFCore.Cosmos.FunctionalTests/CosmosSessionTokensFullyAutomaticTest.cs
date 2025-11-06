// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

public class CosmosSessionTokensFullyAutomaticTest(NonSharedFixture fixture) : CosmosSessionTokensTestBase(fixture)
{
    protected override SessionTokenManagementMode Mode => SessionTokenManagementMode.FullyAutomatic;

    [ConditionalFact]
    public virtual async Task GetSessionToken_throws()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(() => context.Database.GetSessionToken());
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, exception.Message);
    }

    [ConditionalFact]
    public virtual async Task UseSessionToken_throws()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(() => context.Database.UseSessionToken("0:-1#231"));
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, exception.Message);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionToken_throws()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(() => context.Database.AppendSessionToken("0:-1#231"));
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, exception.Message);
    }

    [ConditionalFact]
    public virtual async Task GetSessionTokens_throws()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(() => context.Database.GetSessionTokens());
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, exception.Message);
    }

    [ConditionalFact]
    public virtual async Task UseSessionTokens_throws()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(() => context.Database.UseSessionTokens(new Dictionary<string, string>() { { nameof(CosmosSessionTokenContext), "0:-1#231" } }));
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, exception.Message);
    }

    [ConditionalFact]
    public virtual async Task AppendSessionTokens_throws()
    {
        var contextFactory = await InitializeAsync<CosmosSessionTokenContext>();

        using var context = contextFactory.CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(() => context.Database.AppendSessionTokens(new Dictionary<string, string>() { { nameof(CosmosSessionTokenContext), "0:-1#231" } }));
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, exception.Message);
    }
}
