// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class MetadataExtensions
{
    public static IQueryable<TEntity> AsTracking<TEntity>(
        this IQueryable<TEntity> source,
        bool tracking)
        where TEntity : class
        => tracking ? source.AsTracking() : source.AsNoTracking();

    public static IEnumerable<T> NullChecked<T>(this IEnumerable<T> enumerable)
        => enumerable ?? Enumerable.Empty<T>();

    public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
    {
        foreach (var item in @this)
        {
            action(item);
        }
    }
}
