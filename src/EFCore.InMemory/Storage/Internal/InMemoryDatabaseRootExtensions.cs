// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class InMemoryDatabaseRootExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IInMemoryStore GetStore(this InMemoryDatabaseRoot root, string name, IInMemoryTableFactory tableFactory)
    {
        var instance = (ConcurrentDictionary<string, IInMemoryStore>)
            LazyInitializer.EnsureInitialized(
                ref root.Instance,
                () => new ConcurrentDictionary<string, IInMemoryStore>());

        return instance.GetOrAdd(name, static (_, f) => new InMemoryStore(f), tableFactory);
    }
}
