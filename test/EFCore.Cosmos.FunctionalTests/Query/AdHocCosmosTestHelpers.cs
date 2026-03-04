// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocCosmosTestHelpers
{
    public static async Task CreateCustomEntityHelperAsync(
        Container container,
        string json,
        CancellationToken cancellationToken)
    {
        var document = JObject.Parse(json);

        var stream = new MemoryStream();
        await using var __ = stream.ConfigureAwait(false);
        var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false);
        await using var ___ = writer.ConfigureAwait(false);
        using var jsonWriter = new JsonTextWriter(writer);

        CosmosClientWrapper.Serializer.Serialize(jsonWriter, document);
        await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

        var response = await container.CreateItemStreamAsync(
                stream,
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
