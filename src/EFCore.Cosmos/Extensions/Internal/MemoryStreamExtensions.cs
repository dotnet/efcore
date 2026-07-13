// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Extensions.Internal;

public static class MemoryStreamExtensions
{
    extension(MemoryStream memoryStream)
    {
        public static MemoryStream PublicReadOnly(byte[] buffer)
            => new(buffer, index: 0, buffer.Length, writable: false, publiclyVisible: true);
    }
}
