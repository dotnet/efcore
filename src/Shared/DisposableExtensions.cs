// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.Utilities;

internal static class DisposableExtensions
{
    public static ValueTask DisposeAsyncIfAvailable(this IDisposable? disposable)
    {
        switch (disposable)
        {
            case null:
                return default;
            case IAsyncDisposable asyncDisposable:
                return asyncDisposable.DisposeAsync();
            default:
                disposable.Dispose();
                return default;
        }
    }
}
