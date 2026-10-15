// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Extensions.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class CosmosResponseStreamHelper
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ReadOnlyMemory<byte> ExtractContentAsMemory(Stream content)
    {
        if (content is MemoryStream memoryStream
            && memoryStream.TryGetBuffer(out var segment))
        {
            return segment.AsMemory(
                checked((int)memoryStream.Position),
                checked((int)(memoryStream.Length - memoryStream.Position)));
        }

        // SDK returns a memory stream in most cases, but sometimes it returns its own internal wrapper of a MemoryStream.
        // In that case, copy to a single, exactly-sized buffer.
        var length = checked((int)(content.Length - content.Position));
        if (length == 0)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        var buffer = GC.AllocateUninitializedArray<byte>(length);
        content.ReadExactly(buffer);

        return buffer;
    }
}
