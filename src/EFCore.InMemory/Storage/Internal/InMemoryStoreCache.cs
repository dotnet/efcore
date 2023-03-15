// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryStoreCache : IInMemoryStoreCache
{
    private readonly IInMemoryTableFactory _tableFactory;
    private readonly ConcurrentDictionary<string, IInMemoryStore> _namedStores;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryStoreCache(
        IInMemoryTableFactory tableFactory,
        IInMemorySingletonOptions? options)
    {
        _tableFactory = tableFactory;

        if (options?.DatabaseRoot != null)
        {
            LazyInitializer.EnsureInitialized(
                ref options.DatabaseRoot.Instance,
                () => new ConcurrentDictionary<string, IInMemoryStore>());

            _namedStores = (ConcurrentDictionary<string, IInMemoryStore>)options.DatabaseRoot.Instance;
        }
        else
        {
            _namedStores = new ConcurrentDictionary<string, IInMemoryStore>();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IInMemoryStore GetStore(string name)
        => _namedStores.GetOrAdd(name, _ => new InMemoryStore(_tableFactory));
}
