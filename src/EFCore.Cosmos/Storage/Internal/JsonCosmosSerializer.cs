// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class JsonCosmosSerializer : CosmosSerializer
{
    /// <inheritdoc />
    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            return typeof(Stream).IsAssignableFrom(typeof(T)) ? (T)(object)stream : JsonSerializer.Deserialize<T>(stream)!;
        }
    }

    /// <inheritdoc />
    public override Stream ToStream<T>(T input)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, input);
        stream.Position = 0;
        return stream;
    }
}
