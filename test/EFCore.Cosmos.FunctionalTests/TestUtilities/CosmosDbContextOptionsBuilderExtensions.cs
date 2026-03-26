// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class CosmosDbContextOptionsBuilderExtensions
{
    private static HttpClient? _httpClient;

    public static CosmosDbContextOptionsBuilder ApplyConfiguration(this CosmosDbContextOptionsBuilder optionsBuilder)
    {
        _httpClient ??= TestEnvironment.HttpMessageHandler != null
            ? new HttpClient(TestEnvironment.HttpMessageHandler, disposeHandler: false)
            : new HttpClient(
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

        optionsBuilder
            .ExecutionStrategy(d => new TestCosmosExecutionStrategy(d))
            .RequestTimeout(TimeSpan.FromMinutes(20))
            .HttpClientFactory(() => _httpClient)
            .ConnectionMode(ConnectionMode.Gateway);

        return optionsBuilder;
    }
}
