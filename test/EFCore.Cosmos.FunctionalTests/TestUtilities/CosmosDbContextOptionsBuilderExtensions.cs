// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class CosmosDbContextOptionsBuilderExtensions
{
    private static HttpMessageHandler? _handler;
    private static Func<HttpClient>? _httpClientFactory;

    public static CosmosDbContextOptionsBuilder ApplyConfiguration(this CosmosDbContextOptionsBuilder optionsBuilder)
    {
        if (_httpClientFactory == null)
        {
            _handler = TestEnvironment.HttpMessageHandler
                ?? new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            _httpClientFactory = () => new HttpClient(_handler, disposeHandler: false);
        }

        optionsBuilder
            .ExecutionStrategy(d => new TestCosmosExecutionStrategy(d))
            .RequestTimeout(TimeSpan.FromMinutes(20))
            .HttpClientFactory(_httpClientFactory)
            .ConnectionMode(ConnectionMode.Gateway);

        return optionsBuilder;
    }
}
