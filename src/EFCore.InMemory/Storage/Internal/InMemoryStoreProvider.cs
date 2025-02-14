// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryStoreProvider : IInMemoryStoreProvider
{
    private readonly IDbContextOptions _options;
    private readonly IInMemoryDatabaseRootCache _databaseRootCache;
    private readonly IInMemoryTableFactory _tableFactory;
    private IInMemoryStore? _store;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryStoreProvider(IDbContextOptions options, IInMemoryDatabaseRootCache databaseRootCache, IInMemoryTableFactory tableFactory)
    {
        _options = options;
        _databaseRootCache = databaseRootCache;
        _tableFactory = tableFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IInMemoryStore Store
        => LazyInitializer.EnsureInitialized(ref _store, InitializeStore);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IInMemoryStore InitializeStore()
    {
        var extension = _options.Extensions.OfType<InMemoryOptionsExtension>().First();
        var root = extension.DatabaseRoot ?? _databaseRootCache.SharedRoot;
        return root.GetStore(extension.StoreName, _tableFactory);
    }
}
