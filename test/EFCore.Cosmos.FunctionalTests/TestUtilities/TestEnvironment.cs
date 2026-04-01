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

    private static CosmosDbContainer _container;
    private static bool _initialized;
    private static readonly SemaphoreSlim _initSemaphore = new(1, 1);

    public static string DefaultConnection { get; private set; } = string.IsNullOrEmpty(Config["DefaultConnection"])
        ? "https://localhost:8081"
        : Config["DefaultConnection"];

    internal static HttpMessageHandler HttpMessageHandler { get; private set; }

    public static async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _initSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_initialized)
            {
                return;
            }

            // If a connection string is specified (env var, config.json...), always use that.
            var configured = Config["DefaultConnection"];
            if (!string.IsNullOrEmpty(configured))
            {
                DefaultConnection = configured;
                _initialized = true;
                return;
            }

            // Try to connect to the default emulator endpoint (e.g. Windows emulator or
            // a manually-started Docker container).
            if (await TryProbeEmulatorAsync("https://localhost:8081").ConfigureAwait(false))
            {
                DefaultConnection = "https://localhost:8081";
                _initialized = true;
                return;
            }

            // Start a testcontainer with the Linux emulator.
            CosmosDbContainer container;
            try
            {
                container = new CosmosDbBuilder("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview")
                    .Build();
                await container.StartAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to start the Cosmos DB emulator testcontainer. "
                    + "Ensure that either the Cosmos DB emulator is running on localhost:8081, "
                    + "or Docker is installed and running, "
                    + "or set the 'Test__Cosmos__DefaultConnection' environment variable to connect to "
                    + "an existing emulator or Cosmos DB instance.",
                    ex);
            }

            _container = container;

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                try
                {
                    _container.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }
                catch
                {
                    // Best-effort cleanup: container may already be stopped or Docker daemon
                    // may have exited before the process exit handler runs.
                }
            };

            DefaultConnection = new UriBuilder(
                Uri.UriSchemeHttp,
                _container.Hostname,
                _container.GetMappedPublicPort(CosmosDbBuilder.CosmosDbPort)).ToString();
            HttpMessageHandler = _container.HttpMessageHandler;

            _initialized = true;
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    private static async Task<bool> TryProbeEmulatorAsync(string endpoint)
    {
        try
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(3) };
            // Any successful response (even 401) means the emulator is up and accepting connections.
            using var response = await client.GetAsync(endpoint).ConfigureAwait(false);
            return true;
        }
        catch
        {
            // Expected: HttpRequestException (connection refused), TaskCanceledException (timeout),
            // or SocketException when the emulator is not running.
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

    public static string EmulatorType => _container != null
        ? "linux"
        : Config["EmulatorType"] ?? (!OperatingSystem.IsWindows() ? "linux" : "");

    public static bool IsLinuxEmulator => IsEmulator
        && EmulatorType.Equals("linux", StringComparison.OrdinalIgnoreCase);
}
