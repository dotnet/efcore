// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Azure;

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
        if (content is MemoryStream memoryStream)
        {
            return memoryStream.GetBuffer().AsMemory().Slice(0, (int)content.Length);
        }

        // SDK returns a memory stream in most cases, but sometimes it returns its own internal wrapper of a MemoryStream.
        // In that case, we will use CopyTo to get the internal buffer
        memoryStream = new MemoryStream(capacity: (int)content.Length);
        content.CopyTo(memoryStream); // @TODO: Could actually implement a custom stream that stores the buffer retreived from Write from CopyTo, so we don't iterate the data unnecessarily,
                                      // Then we are grabbing the buffer directly from the underlying memory stream in the wrapper
                                      // but that could cause issues if the implementation changed.

        return memoryStream.GetBuffer().AsMemory().Slice(0, (int)content.Length);
    }
}
