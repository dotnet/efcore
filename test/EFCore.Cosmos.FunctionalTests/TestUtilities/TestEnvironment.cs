// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public static class TestEnvironment
{
    public static IConfiguration Config { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("cosmosConfig.json", optional: true)
        .AddJsonFile("cosmosConfig.test.json", optional: true)
        .AddEnvironmentVariables()
        .Build()
        .GetSection("Test:Cosmos");

    public static string DefaultConnection { get; } = string.IsNullOrEmpty(Config["DefaultConnection"])
        ? "https://localhost:8081"
        : Config["DefaultConnection"];

    public static string AuthToken { get; } = string.IsNullOrEmpty(Config["AuthToken"])
        ? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
        : Config["AuthToken"];

    public static string ConnectionString { get; } = $"AccountEndpoint={DefaultConnection};AccountKey={AuthToken}";

    public static TokenCredential TokenCredential { get; } = new DefaultAzureCredential();

    public static bool IsEmulator { get; } = DefaultConnection.StartsWith("https://localhost:8081", StringComparison.Ordinal);
}
