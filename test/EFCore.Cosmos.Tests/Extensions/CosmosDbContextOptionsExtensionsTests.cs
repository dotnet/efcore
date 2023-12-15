// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Azure.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;
using Microsoft.EntityFrameworkCore.TestUtilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public class CosmosDbContextOptionsExtensionsTests
{
    [ConditionalFact]
    public void Service_collection_extension_method_can_configure_Cosmos_options()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCosmos<ApplicationDbContext>(
            "Database=Crunchie",
            "Crunchie",
            cosmosOptions =>
            {
                cosmosOptions.IdleTcpConnectionTimeout(new TimeSpan(0, 5, 50));
                cosmosOptions.OpenTcpConnectionTimeout(new TimeSpan(0, 2, 45));
            },
            dbContextOption =>
            {
                dbContextOption.EnableDetailedErrors();
            });

        var services = serviceCollection.BuildServiceProvider(validateScopes: true);

        using (var serviceScope = services
                   .GetRequiredService<IServiceScopeFactory>()
                   .CreateScope())
        {
            var coreOptions = serviceScope.ServiceProvider
                .GetRequiredService<DbContextOptions<ApplicationDbContext>>().GetExtension<CoreOptionsExtension>();

            Assert.True(coreOptions.DetailedErrorsEnabled);

            var cosmosOptions = serviceScope.ServiceProvider
                .GetRequiredService<DbContextOptions<ApplicationDbContext>>().GetExtension<CosmosOptionsExtension>();

            Assert.Equal(new TimeSpan(0, 5, 50), cosmosOptions.IdleTcpConnectionTimeout);
            Assert.Equal(new TimeSpan(0, 2, 45), cosmosOptions.OpenTcpConnectionTimeout);
            Assert.Equal("Database=Crunchie", cosmosOptions.ConnectionString);
            Assert.Equal("Crunchie", cosmosOptions.DatabaseName);
        }
    }

    [ConditionalFact]
    public void Can_create_options_with_valid_values()
    {
        Test(o => o.Region(Regions.EastAsia), o => Assert.Equal(Regions.EastAsia, o.Region));
        // The region will be validated by the Cosmos SDK, because the region list is not constant
        Test(o => o.Region("FakeRegion"), o => Assert.Equal("FakeRegion", o.Region));
        Test(
            o => o.PreferredRegions(new[] { Regions.AustraliaCentral, Regions.EastAsia }),
            o => Assert.Equal(new[] { Regions.AustraliaCentral, Regions.EastAsia }, o.PreferredRegions));
        Test(o => o.ConnectionMode(ConnectionMode.Direct), o => Assert.Equal(ConnectionMode.Direct, o.ConnectionMode));
        Test(o => o.GatewayModeMaxConnectionLimit(3), o => Assert.Equal(3, o.GatewayModeMaxConnectionLimit));
        Test(o => o.MaxRequestsPerTcpConnection(3), o => Assert.Equal(3, o.MaxRequestsPerTcpConnection));
        Test(o => o.MaxTcpConnectionsPerEndpoint(3), o => Assert.Equal(3, o.MaxTcpConnectionsPerEndpoint));
        Test(o => o.LimitToEndpoint(), o => Assert.True(o.LimitToEndpoint));
        Test(o => o.ContentResponseOnWriteEnabled(), o => Assert.True(o.EnableContentResponseOnWrite));

        var webProxy = new WebProxy();
        Test(o => o.WebProxy(webProxy), o => Assert.Same(webProxy, o.WebProxy));
        Test(
            o => o.ExecutionStrategy(d => new CosmosExecutionStrategy(d)),
            o => Assert.IsType<CosmosExecutionStrategy>(o.ExecutionStrategyFactory(null)));
        Test(o => o.RequestTimeout(TimeSpan.FromMinutes(3)), o => Assert.Equal(TimeSpan.FromMinutes(3), o.RequestTimeout));
        Test(
            o => o.OpenTcpConnectionTimeout(TimeSpan.FromMinutes(3)),
            o => Assert.Equal(TimeSpan.FromMinutes(3), o.OpenTcpConnectionTimeout));
        Test(
            o => o.IdleTcpConnectionTimeout(TimeSpan.FromMinutes(3)),
            o => Assert.Equal(TimeSpan.FromMinutes(3), o.IdleTcpConnectionTimeout));
        var httpClientFactory = () => new HttpClient();
        Test(
            o => o.HttpClientFactory(httpClientFactory),
            o => Assert.Same(httpClientFactory, o.HttpClientFactory)
        );
    }

    [ConditionalFact]
    public void Endpoint_and_key_overrides_connection_string()
    {
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                "authKeyOrResourceToken@serviceEndPoint",
                "databaseName2")
            .UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName");

        var extension = options.Options.FindExtension<CosmosOptionsExtension>();

        Assert.Equal("serviceEndPoint", extension.AccountEndpoint);
        Assert.Equal("authKeyOrResourceToken", extension.AccountKey);
        Assert.Null(extension.ConnectionString);
        Assert.Equal("databaseName", extension.DatabaseName);
    }

    [ConditionalFact]
    public void Connection_string_overrides_endpoint_and_key()
    {
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName2")
            .UseCosmos(
                "authKeyOrResourceToken@serviceEndPoint",
                "databaseName");

        var extension = options.Options.FindExtension<CosmosOptionsExtension>();

        Assert.Null(extension.AccountEndpoint);
        Assert.Null(extension.AccountKey);
        Assert.Equal("authKeyOrResourceToken@serviceEndPoint", extension.ConnectionString);
        Assert.Equal("databaseName", extension.DatabaseName);
    }

    [ConditionalFact]
    public async Task Endpoint_and_token_overrides_connection_string()
    {
        await using var testDatabase = CosmosTestStore.Create("NonExisting");

        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                "authKeyOrResourceToken@serviceEndPoint",
                "databaseName2")
            .UseCosmos(
                "serviceEndPoint",
                testDatabase.TokenCredential,
                "databaseName");

        var extension = options.Options.FindExtension<CosmosOptionsExtension>();

        Assert.Equal("serviceEndPoint", extension.AccountEndpoint);
        Assert.Null(extension.AccountKey);
        Assert.Same(testDatabase.TokenCredential, extension.TokenCredential);
        Assert.Null(extension.ConnectionString);
        Assert.Equal("databaseName", extension.DatabaseName);
    }

    [ConditionalFact]
    public async Task Connection_string_overrides_endpoint_and_token()
    {
        await using var testDatabase = CosmosTestStore.Create("NonExisting");

        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                "serviceEndPoint",
                testDatabase.TokenCredential,
                "databaseName2")
            .UseCosmos(
                "authKeyOrResourceToken@serviceEndPoint",
                "databaseName");

        var extension = options.Options.FindExtension<CosmosOptionsExtension>();

        Assert.Null(extension.AccountEndpoint);
        Assert.Null(extension.AccountKey);
        Assert.Null(extension.TokenCredential);
        Assert.Equal("authKeyOrResourceToken@serviceEndPoint", extension.ConnectionString);
        Assert.Equal("databaseName", extension.DatabaseName);
    }

    [ConditionalFact]
    public void Throws_for_invalid_values()
        => Throws<ArgumentOutOfRangeException>(o => o.ConnectionMode((ConnectionMode)958410610));

    private void Test(
        Action<CosmosDbContextOptionsBuilder> cosmosOptionsAction,
        Action<CosmosOptionsExtension> extensionAssert)
    {
        var options = new DbContextOptionsBuilder().UseCosmos(
            cosmosOptionsAction);

        var extension = options
            .Options.FindExtension<CosmosOptionsExtension>();

        extensionAssert(extension);

        var clone = new DbContextOptionsBuilder().UseCosmos(
                cosmosOptionsAction)
            .Options.FindExtension<CosmosOptionsExtension>();

        Assert.Equal(extension.Info.GetServiceProviderHashCode(), clone.Info.GetServiceProviderHashCode());
        Assert.True(extension.Info.ShouldUseSameServiceProvider(clone.Info));
    }

    private void Throws<T>(Action<CosmosDbContextOptionsBuilder> cosmosOptionsAction)
        where T : Exception
        => Assert.Throws<T>(
            () => new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                cosmosOptionsAction));
}
