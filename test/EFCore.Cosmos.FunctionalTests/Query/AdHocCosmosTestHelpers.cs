// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocCosmosTestHelpers
{
    public static async Task CreateCustomEntityHelperAsync(
        Container container,
        string json,
        CancellationToken cancellationToken)
    {
        var response = await container.CreateItemStreamAsync(
                MemoryStream.PublicReadOnly(Encoding.UTF8.GetBytes(json)),
                PartitionKey.None,
                requestOptions: null,
                cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Failed to create entity (status code: {response.StatusCode}) for json: {json}");
        }
    }
}
