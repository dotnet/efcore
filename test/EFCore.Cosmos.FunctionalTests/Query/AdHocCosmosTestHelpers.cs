// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocCosmosTestHelpers
{
    public static Task CreateCustomEntityHelperAsync(
        Container container,
        string json,
        CancellationToken cancellationToken)
        => new TestCosmosExecutionStrategy().ExecuteAsync(
            async ct =>
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                using var response = await container.CreateItemStreamAsync(
                        stream,
                        PartitionKey.None,
                        requestOptions: null,
                        ct)
                    .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    throw new InvalidOperationException($"Failed to create entity (status code: {response.StatusCode}) for json: {json}");
                }
            }, cancellationToken);
}
