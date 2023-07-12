// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.Utilities;

internal static class DisposableExtensions
{
    public static ValueTask DisposeAsyncIfAvailable(this IDisposable? disposable)
    {
        if (disposable != null)
        {
            if (disposable is IAsyncDisposable asyncDisposable)
            {
                return asyncDisposable.DisposeAsync();
            }

            disposable.Dispose();
        }

        return default;
    }
}
