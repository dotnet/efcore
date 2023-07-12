// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class CosmosDbContextOptionsBuilderExtensions
{
    public static CosmosDbContextOptionsBuilder ApplyConfiguration(this CosmosDbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .ExecutionStrategy(d => new TestCosmosExecutionStrategy(d))
            .RequestTimeout(TimeSpan.FromMinutes(20))
            .HttpClientFactory(
                () => new HttpClient(
                    new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    }))
            .ConnectionMode(ConnectionMode.Gateway);

        return optionsBuilder;
    }
}
