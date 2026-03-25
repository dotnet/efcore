// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Testcontainers.CosmosDb;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public static class TestEnvironment
{
    private static readonly string _emulatorAuthToken =
        "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    public static IConfiguration Config { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("cosmosConfig.json", optional: true)
        .AddJsonFile("cosmosConfig.test.json", optional: true)
        .AddEnvironmentVariables()
        .Build()
        .GetSection("Test:Cosmos");

    private static readonly Lazy<(string Connection, CosmosDbContainer Container)> _connectionInfo = new(InitializeConnection);

    public static string DefaultConnection => _connectionInfo.Value.Connection;

    private static CosmosDbContainer Container => _connectionInfo.Value.Container;

    public static bool IsTestContainer => Container != null;

    internal static Func<HttpMessageHandler> HttpMessageHandlerFactory
        => Container != null ? () => Container.HttpMessageHandler : null;

    private static (string Connection, CosmosDbContainer Container) InitializeConnection()
    {
        // If a connection string is specified (env var, config.json...), always use that.
        var configured = Config["DefaultConnection"];
        if (!string.IsNullOrEmpty(configured))
        {
            return (configured, null);
        }

        // Try to connect to the default emulator endpoint.
        if (TryProbeEmulator("https://localhost:8081"))
        {
            return ("https://localhost:8081", null);
        }

        // Try to start a testcontainer with the Linux emulator.
        try
        {
            var container = new CosmosDbBuilder("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview")
                .Build();
            container.StartAsync().GetAwaiter().GetResult();

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                try
                {
                    container.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }
                catch
                {
                    // Ignore errors during container cleanup
                }
            };

            var endpoint = new UriBuilder(
                Uri.UriSchemeHttp,
                container.Hostname,
                container.GetMappedPublicPort(CosmosDbBuilder.CosmosDbPort)).ToString();

            return (endpoint, container);
        }
        catch
        {
            // Docker not available or container failed to start.
            // Fall back to the default endpoint; the connection check will
            // determine whether the emulator is actually reachable.
            return ("https://localhost:8081", null);
        }
    }

    private static bool TryProbeEmulator(string endpoint)
    {
        try
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(3) };
            using var response = client.GetAsync(endpoint).GetAwaiter().GetResult();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string AuthToken { get; } = string.IsNullOrEmpty(Config["AuthToken"])
        ? _emulatorAuthToken
        : Config["AuthToken"];

    public static string ConnectionString => $"AccountEndpoint={DefaultConnection};AccountKey={AuthToken}";

    public static bool UseTokenCredential { get; } = string.Equals(Config["UseTokenCredential"], "true", StringComparison.OrdinalIgnoreCase);

    public static TokenCredential TokenCredential { get; } = new AzureCliCredential(
        new AzureCliCredentialOptions { ProcessTimeout = TimeSpan.FromMinutes(5) });

    public static string SubscriptionId { get; } = Config["SubscriptionId"];

    public static string ResourceGroup { get; } = Config["ResourceGroup"];

    public static AzureLocation AzureLocation { get; } = string.IsNullOrEmpty(Config["AzureLocation"])
        ? AzureLocation.WestUS
        : Enum.Parse<AzureLocation>(Config["AzureLocation"]);

    public static bool IsEmulator => !UseTokenCredential && (AuthToken == _emulatorAuthToken);

    public static bool SkipConnectionCheck { get; } = string.Equals(Config["SkipConnectionCheck"], "true", StringComparison.OrdinalIgnoreCase);

    public static string EmulatorType => IsTestContainer
        ? "linux"
        : Config["EmulatorType"] ?? (!OperatingSystem.IsWindows() ? "linux" : "");

    public static bool IsLinuxEmulator => IsEmulator
        && EmulatorType.Equals("linux", StringComparison.OrdinalIgnoreCase);
}
